using NLog;
using SwimReg.API.Errors;
using SwimReg.Domain.Exceptions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SwimReg.API.Middleware;

// ראשון ב-pipeline: תופס כל חריגה שלא טופלה ומתרגם אותה לקוד HTTP ול-JSON אחיד
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // אם השגיאה קרתה לפני CorrelationIdMiddleware, המזהה ייווצר כאן
            var correlationId = CorrelationId.GetOrCreate(context);

            var (statusCode, message) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
                ConcurrencyConflictException => (StatusCodes.Status409Conflict, "המקום נתפס בינתיים, נסה שוב"),
                ConflictException => (StatusCodes.Status409Conflict, ex.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, ex.Message),
                BusinessValidationException => (StatusCodes.Status400BadRequest, ex.Message),
                // token בלי ה-claims הנדרשים: בעיית זהות, לא שגיאת שרת
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "נדרשת התחברות עם token תקין"),
                // הלקוח ניתק באמצע הבקשה: אין למי להחזיר תשובה, וזה לא כשל של השרת
                OperationCanceledException when context.RequestAborted.IsCancellationRequested => (499, "הבקשה בוטלה"),
                // כל השאר: 500 בלי לחשוף פרטים פנימיים ללקוח
                _ => (StatusCodes.Status500InternalServerError, "אירעה שגיאה פנימית בשרת")
            };

            // רמות הלוג: Error רק לכשל אמיתי (500). חריגה עסקית שטופלה (4xx) היא Warning,
            // וביטול ע"י הלקוח הוא Information. התנגשות על מקום כבר נרשמה כ-Warning גם ב-Service
            var level = statusCode switch
            {
                StatusCodes.Status500InternalServerError => LogLevel.Error,
                499 => LogLevel.Information,
                _ => LogLevel.Warning
            };

            using (ScopeContext.PushProperty("CorrelationId", correlationId))
            {
                if (level == LogLevel.Error)
                    _logger.LogError(ex, "Unhandled exception for {Method} {Path} (CorrelationId {CorrelationId})",
                        context.Request.Method, context.Request.Path.Value, correlationId);
                else
                    // בלי ה-stack trace: זה מקרה צפוי, מספיקים הסוג וההודעה
                    _logger.Log(level, "Handled {ExceptionType} for {Method} {Path} → {StatusCode}: {Message} (CorrelationId {CorrelationId})",
                        ex.GetType().Name, context.Request.Method, context.Request.Path.Value, statusCode, ex.Message, correlationId);
            }

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            await ErrorResponse.WriteAsync(context, statusCode, message);
        }
    }
}
