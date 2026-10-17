namespace SwimReg.Domain.Exceptions;

// מוגדרת ב-Domain כדי ש-Application לא יכיר את DbUpdateConcurrencyException של EF Core.
// ה-Repository תופס את חריגת EF וזורק את זו במקומה. מתורגמת ל-409.
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message, Exception? inner = null) : base(message, inner) { }
}
