using System.Diagnostics;
using NLog;
using SwimReg.API.Errors;

namespace SwimReg.API.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // לוקח מה-header או מייצר Guid, שומר ב-Items ומחזיר ב-header של התשובה
        var correlationId = CorrelationId.GetOrCreate(context);
        var stopwatch = Stopwatch.StartNew();

        // הלוג נכתב כשהתשובה הסתיימה, כדי שה-StatusCode יהיה הסופי (גם אחרי טיפול בחריגה)
        context.Response.OnCompleted(() =>
        {
            using (ScopeContext.PushProperty("CorrelationId", correlationId))
            {
                // לא כותבים headers, query string או גוף בקשה: עלולים להכיל סיסמה או token
                _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms (CorrelationId {CorrelationId})",
                    context.Request.Method, context.Request.Path.Value, context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds, correlationId);
            }
            return Task.CompletedTask;
        });

        // כל לוג שנכתב בהמשך הבקשה יכלול את ה-CorrelationId
        using (ScopeContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
