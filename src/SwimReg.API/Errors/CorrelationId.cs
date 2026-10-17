namespace SwimReg.API.Errors;

public static class CorrelationId
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    // מחזיר את המזהה של הבקשה. אם עוד לא נקבע (שגיאה מוקדמת, לפני CorrelationIdMiddleware),
    // לוקח אותו מה-header או מייצר חדש, ושומר כדי שכל המשך הבקשה ישתמש באותו מזהה
    public static string GetOrCreate(HttpContext context)
    {
        if (context.Items.TryGetValue(ItemKey, out var existing) && existing is string id)
            return id;

        var header = context.Request.Headers[HeaderName].ToString();
        id = IsValid(header) ? header : Guid.NewGuid().ToString();

        context.Items[ItemKey] = id;
        if (!context.Response.HasStarted)
            context.Response.Headers[HeaderName] = id;
        return id;
    }

    // מקבלים מזהה מהלקוח רק אם הוא קצר ובטוח, כדי שלא יכניסו ללוג טקסט זדוני
    private static bool IsValid(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 64 && value.All(c => char.IsLetterOrDigit(c) || c == '-');
}
