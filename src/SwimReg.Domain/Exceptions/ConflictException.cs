namespace SwimReg.Domain.Exceptions;

// מתורגם ל-409 ב-Middleware
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
