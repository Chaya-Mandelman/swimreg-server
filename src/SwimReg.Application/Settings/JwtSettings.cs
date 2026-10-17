namespace SwimReg.Application.Settings;

// נטען מהקונפיגורציה (סעיף Jwt). Key מגיע מ-User Secrets או ממשתנה סביבה בלבד
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
