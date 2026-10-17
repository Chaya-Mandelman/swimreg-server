namespace SwimReg.Domain.Exceptions;

// מתורגם ל-403 ב-Middleware
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
