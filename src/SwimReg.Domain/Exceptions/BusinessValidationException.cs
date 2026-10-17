namespace SwimReg.Domain.Exceptions;

// הפרת כלל עסקי. מתורגם ל-400 ב-Middleware
public class BusinessValidationException : Exception
{
    public BusinessValidationException(string message) : base(message) { }
}
