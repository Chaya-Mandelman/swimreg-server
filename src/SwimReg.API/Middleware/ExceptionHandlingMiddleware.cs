using NLog;
using SwimReg.API.Errors;
using SwimReg.Domain.Exceptions;

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

            using (ScopeContext.PushProperty("CorrelationId", correlationId))
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path} (CorrelationId {CorrelationId})",
                    context.Request.Method, context.Request.Path.Value, correlationId);
            }

            if (context.Response.HasStarted)
                throw;

            var (statusCode, message) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
                ConcurrencyConflictException => (StatusCodes.Status409Conflict, "המקום נתפס בינתיים, נסה שוב"),
                ConflictException => (StatusCodes.Status409Conflict, ex.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, ex.Message),
                BusinessValidationException => (StatusCodes.Status400BadRequest, ex.Message),
                // כל השאר: 500 בלי לחשוף פרטים פנימיים ללקוח
                _ => (StatusCodes.Status500InternalServerError, "אירעה שגיאה פנימית בשרת")
            };

            context.Response.Clear();
            await ErrorResponse.WriteAsync(context, statusCode, message);
        }
    }
}
