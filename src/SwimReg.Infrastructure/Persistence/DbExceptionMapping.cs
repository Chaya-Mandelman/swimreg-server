using Microsoft.EntityFrameworkCore;
using Npgsql;
using SwimReg.Domain.Exceptions;

namespace SwimReg.Infrastructure.Persistence;

// מתרגם חריגות של EF Core / PostgreSQL לחריגות של ה-Domain,
// כדי ששכבת Application לא תכיר את EF
internal static class DbExceptionMapping
{
    public static async Task SaveChangesMappedAsync(this DbContext context, CancellationToken ct)
    {
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // ה-xmin של השורה השתנה מאז שנטענה: מישהו אחר עדכן אותה בינתיים
            throw new ConcurrencyConflictException("המקום נתפס בינתיים, נסה שוב", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            // 23503: ניסיון למחוק שורה שיש שורות אחרות שמצביעות עליה
            throw new ConflictException("לא ניתן למחוק: קיימים נתונים קשורים");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // 23505: הפרת אינדקס ייחודי (למשל הרשמה כפולה שנשלחה במקביל)
            throw new ConflictException("הרשומה כבר קיימת");
        }
    }
}
