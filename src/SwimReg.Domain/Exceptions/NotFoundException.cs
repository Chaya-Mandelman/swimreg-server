namespace SwimReg.Domain.Exceptions;

// מתורגם ל-404 ב-Middleware
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
