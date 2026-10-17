using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Text.Json.Serialization;

namespace SwimReg.API.Errors;

// מבנה JSON אחיד לכל שגיאה: { statusCode, message, correlationId, errors? }
public class ErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;

    // שגיאות לפי שדה (רק בשגיאות ולידציה)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; init; }

    // UnicodeRanges.All: עברית נכתבת כמו שהיא ולא כ-ה
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static ErrorResponse Create(HttpContext context, int statusCode, string message, IDictionary<string, string[]>? errors = null) =>
        new()
        {
            StatusCode = statusCode,
            Message = message,
            CorrelationId = SwimReg.API.Errors.CorrelationId.GetOrCreate(context),
            Errors = errors
        };

    // כתיבה ישירה לתשובה: משמש את ה-Middleware ואת אירועי ה-JWT
    public static Task WriteAsync(HttpContext context, int statusCode, string message, IDictionary<string, string[]>? errors = null)
    {
        var body = Create(context, statusCode, message, errors);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        return context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}
