# SwimReg – ניהול הרשמה לחוגי שחייה

שרת ASP.NET Core Web API (.NET 8) לניהול הרשמה לחוגי שחייה, עם PostgreSQL, EF Core, JWT ו-NLog.

## 1. תיאור המערכת והתפקידים

המערכת מנהלת בריכות, קורסי שחייה והרשמות של מתאמנים לקורסים.

| תפקיד | מה מותר |
|---|---|
| **Student** (מתאמן) | צפייה בקורסים עם סינון ודפדוף, הרשמה לקורס, צפייה בהרשמות שלו, ביטול הרשמה שלו. ההרשמה הפתוחה (`/api/auth/register`) יוצרת תמיד Student. |
| **Instructor** (מדריך) | צפייה בתלמידים הרשומים לקורסים שלו בלבד. |
| **Admin** (מנהל) | CRUD על בריכות וקורסים, צפייה בכל ההרשמות עם סינון ודפדוף, ביטול כל הרשמה. |

## 2. המשאב המוגבל ו-Optimistic Concurrency

**המשאב:** המקומות בקורס (`MaxParticipants`). כל קורס שומר גם את `CurrentCount`, מספר הנרשמים.

**איפה נוצרת התחרות:** שני מתאמנים שולחים `POST /api/registrations` לאותו קורס, כשנשאר בו מקום אחד בלבד.
שתי הבקשות טוענות את הקורס, שתיהן רואות `CurrentCount < MaxParticipants`, ושתיהן מנסות להירשם.

**איך מטפלים בזה:**
- **xmin:** המאפיין `Course.Version` (uint) ממופה ב-Fluent API עם `IsRowVersion()` לעמודת המערכת `xmin` של PostgreSQL.
  PostgreSQL משנה אותה בכל עדכון של השורה. EF Core מוסיף לכל `UPDATE` תנאי `WHERE xmin = <הערך שנטען>`.
  לא נוצרת עמודה רגילה בשם Version.
- **טרנזקציה אחת:** `RegistrationService.RegisterAsync` מעלה את `CurrentCount` ומוסיף `Registration`,
  ואז קורא **פעם אחת** ל-`SaveChangesAsync`. ה-UPDATE של הקורס וה-INSERT של ההרשמה רצים באותה טרנזקציה.
  אם ה-UPDATE נכשל בגלל xmin, גם ההרשמה לא נשמרת. ביטול הרשמה עובד באותה צורה (DELETE + `CurrentCount--`).
- **409:** הבקשה השנייה מקבלת `DbUpdateConcurrencyException`. ה-Repository מתרגם אותה ל-`ConcurrencyConflictException`
  (חריגה של ה-Domain, כדי ש-Application לא יכיר את EF). ה-Service כותב לוג Warning, וה-Middleware מחזיר
  `409` עם ההודעה "המקום נתפס בינתיים, נסה שוב". אם הלקוח מנסה שוב, הוא יקבל "הקורס מלא" (גם 409).
- **בדיקת Integration:** ב-`tests/SwimReg.Tests/Integration/ConcurrencyTests.cs`, מול PostgreSQL אמיתי (Testcontainers):
  - `Concurrency_TwoContextsUpdateSameCourse_SecondThrows`: שני DbContext מעדכנים את אותו קורס. השני מקבל `DbUpdateConcurrencyException`.
  - `Register_WhenConflict_NoRegistrationSaved`: דרך ה-Service וה-Repository האמיתיים. מוכיחה שאחרי התנגשות לא נשמרה הרשמה והמכסה לא השתנתה.

## 3. ארכיטקטורה (Clean Architecture)

```
SwimReg.API  ──►  SwimReg.Infrastructure  ──►  SwimReg.Application  ──►  SwimReg.Domain
     └──────────────────────────────────────────────►┘
```

| פרויקט | תוכן | תלוי ב- |
|---|---|---|
| `SwimReg.Domain` | ישויות, Enums, Exceptions | כלום |
| `SwimReg.Application` | DTOs, Interfaces, Services, AutoMapper Profile, JwtSettings. לא מכיר EF Core. | Domain |
| `SwimReg.Infrastructure` | DbContext, Fluent API, Migrations, Seed, Repositories, TokenService, PasswordHasher | Application |
| `SwimReg.API` | Controllers, Middleware, Program.cs, קונפיגורציה, nlog.config, Dockerfile | Application, Infrastructure |
| `SwimReg.Tests` | `Unit` (Moq) ו-`Integration` (Testcontainers) | Application, Infrastructure |

כל שכבה חושפת extension לרישום: `AddApplication()` ו-`AddInfrastructure(configuration)`.

## 4. הרצה מקומית

**דרישות:** .NET 8 SDK, PostgreSQL 14 ומעלה, ו-Docker (רק לבדיקות ה-Integration).

1. **התקנת PostgreSQL:** מתקינים מ-https://www.postgresql.org/download/ (או מריצים `docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=<סיסמה> postgres:16`).
   אפשר ליצור בסיס נתונים בשם `swimreg`, או לתת ל-EF ליצור אותו.

2. **סודות בפיתוח (User Secrets):** מתוך תיקיית השורש:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=swimreg;Username=postgres;Password=<הסיסמה שלך>" --project src/SwimReg.API
   ```
   ```bash
   dotnet user-secrets set "Jwt:Key" "<מחרוזת אקראית של 32 תווים לפחות>" --project src/SwimReg.API
   ```

3. **יצירת בסיס הנתונים:** כלי `dotnet-ef` מוגדר כ-local tool:
   ```bash
   dotnet tool restore
   ```
   ```bash
   dotnet ef database update --project src/SwimReg.Infrastructure --startup-project src/SwimReg.API
   ```
   (השלב לא חובה: האפליקציה מריצה `Database.MigrateAsync()` בעלייה.)

4. **הרצה:**
   ```bash
   dotnet run --project src/SwimReg.API --launch-profile https
   ```
   Swagger: `https://localhost:7254/swagger`

5. **בדיקות:**
   ```bash
   dotnet test
   ```
   > בדיקות ה-Integration מרימות PostgreSQL בקונטיינר עם Testcontainers, ולכן **דורשות ש-Docker יפעל**.
   > בלי Docker רק בדיקות ה-Unit יעברו.

## 5. משתמשי דמו

| תפקיד | אימייל | סיסמה |
|---|---|---|
| Admin | admin@swimreg.com | `Admin123!` |
| Instructor (Id 2) | dana@swimreg.com | `Instructor123!` |
| Instructor (Id 3) | yossi@swimreg.com | `Instructor123!` |
| Student (Id 4) | noa@swimreg.com | `Student123!` |
| Student (Id 5) | itay@swimreg.com | `Student123!` |

בנתוני ה-Seed יש 2 בריכות, 4 קורסים ו-2 הרשמות.
קורס 4 ("שיעור פרטי") מוגדר עם `MaxParticipants = 1` ומקום אחד פנוי, כדי להדגים בקלות קורס מלא.

## 6. Endpoints והרשאות

| Method | Path | הרשאה | תשובות |
|---|---|---|---|
| POST | `/api/auth/register` | פתוח | 201 + Location / 400 / 409 |
| POST | `/api/auth/login` | פתוח | 200 / 400 / 401 |
| GET | `/api/users/{id}` | המשתמש עצמו או Admin | 200 / 403 / 404 |
| GET | `/api/courses?level=&poolId=&sortBy=&page=&pageSize=` | מחובר | 200 / 400 |
| GET | `/api/courses/{id}` | מחובר | 200 / 404 |
| POST | `/api/courses` | Admin | 201 + Location / 400 |
| PUT | `/api/courses/{id}` | Admin | 204 / 400 / 404 / 409 |
| DELETE | `/api/courses/{id}` | Admin | 204 / 404 / 409 |
| GET | `/api/pools` | מחובר | 200 |
| GET | `/api/pools/{id}` | מחובר | 200 / 404 |
| POST | `/api/pools` | Admin | 201 + Location / 400 |
| PUT | `/api/pools/{id}` | Admin | 204 / 404 |
| DELETE | `/api/pools/{id}` | Admin | 204 / 404 / 409 |
| POST | `/api/registrations` (גוף: `{ "courseId": 4 }`) | Student | 201 + Location / 404 / 409 |
| GET | `/api/registrations?courseId=&page=&pageSize=` | Student (רק שלו), Admin (הכל) | 200 |
| GET | `/api/registrations/{id}` | הבעלים או Admin | 200 / 403 / 404 |
| DELETE | `/api/registrations/{id}` | הבעלים או Admin | 204 / 403 / 404 / 409 |
| GET | `/api/instructors` | Admin | 200 |
| GET | `/api/instructors/{id}/students` | Instructor (רק ה-id שלו), Admin | 200 / 403 |

- `sortBy`: `name` / `level` / `day` / `startTime`. `page` ≥ 1, `pageSize` בין 1 ל-50.
- 401: token חסר, לא תקין או שפג תוקפו. 403: מחובר בלי הרשאה.
- כל שגיאה מוחזרת באותו מבנה: `{ "statusCode", "message", "correlationId", "errors"? }`.
  המזהה חוזר גם ב-header בשם `X-Correlation-Id`.
- רמות לוג: `Error` רק לשגיאת שרת (500), `Warning` לחריגה עסקית שטופלה (400/403/404/409, כולל התנגשות על מקום),
  `Information` לזרימה רגילה ולכל בקשה, ו-`Debug` (רק ב-Development) לפרטי הקורס שנטען לפני הרשמה.

> Swagger פעיל ב-Production לצורך הדגמה בפרויקט לימודי בלבד. במערכת אמיתית מכבים אותו או מגנים עליו.
> (ההגדרה: `Swagger:Enabled` ב-`appsettings.json`.)

## 7. פריסה ל-Render

**הדרך המהירה (Blueprint):** בשורש ה-repo יש `render.yaml`. ב-Render: **New → Blueprint**, בוחרים את ה-repository,
ו-Render יוצר את בסיס הנתונים ואת השרת, מחבר ביניהם ומייצר `Jwt__Key` אקראי.
את `Cors__AllowedOrigins__0` ממלאים כשיש כתובת לקוח. השרת מקבל גם כתובת בפורמט `postgres://` וממיר אותה בעצמו.

הדרך הידנית:

### א. PostgreSQL
1. ב-Render: **New → PostgreSQL**. בוחרים שם (למשל `swimreg-db`) ואזור, ויוצרים.
2. מעתיקים את ה-**External** או ה-**Internal Database URL**. ה-Internal מתאים כשהשרת רץ ב-Render באותו אזור.
3. **הרשאות:** המשתמש שב-connection string צריך הרשאה ליצור ולשנות טבלאות, כי ה-Migrations רצות בעליית האפליקציה.
   המשתמש הראשי שמסופק עם ה-PostgreSQL של Render מתאים לכך.

**המרת הכתובת לפורמט של Npgsql:** Render נותן כתובת בפורמט:
```
postgres://USER:PASSWORD@HOST:PORT/DATABASE
```
Npgsql צריך את הפורמט:
```
Host=HOST;Port=PORT;Database=DATABASE;Username=USER;Password=PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```
דוגמה: `postgres://swim:abc123@dpg-xyz-a.frankfurt-postgres.render.com/swimreg_db` הופך ל-
`Host=dpg-xyz-a.frankfurt-postgres.render.com;Port=5432;Database=swimreg_db;Username=swim;Password=abc123;SSL Mode=Require;Trust Server Certificate=true`
(כשאין פורט בכתובת, הוא 5432.)

### ב. Web Service מ-Docker
1. דוחפים את הקוד ל-GitHub (ראו הפקודות בסוף הקובץ).
2. ב-Render: **New → Web Service**, ומחברים את ה-repository `swimreg-server`.
3. **Language:** Docker.
4. **Dockerfile Path:** `./src/SwimReg.API/Dockerfile`. **Docker Build Context Directory:** `.` (שורש ה-repo).
5. Render מעביר את הפורט במשתנה `PORT`, והקונטיינר מאזין עליו (`ASPNETCORE_HTTP_PORTS=${PORT}`, ברירת מחדל 8080).
   ה-HTTPS מסתיים ב-Reverse Proxy של Render, ולכן `UseHttpsRedirection` פעיל רק בפיתוח.

### ג. משתני סביבה (Environment)
| מפתח | ערך |
|---|---|
| `ConnectionStrings__DefaultConnection` | ה-connection string בפורמט Npgsql (מסעיף א) |
| `Jwt__Key` | מחרוזת אקראית ארוכה (32 תווים לפחות) |
| `Cors__AllowedOrigins__0` | כתובת הלקוח, למשל `https://swimreg-client.onrender.com` |

בפריסה הראשונה ה-Migrations וה-Seed רצים אוטומטית. בלוג אמורה להופיע השורה `Migrations applied`.
אם בסיס הנתונים לא זמין, נרשמת שגיאה והאפליקציה לא עולה.

## 8. כתובות הפריסה

- כתובת השרת: `https://swimreg-api.onrender.com`
- Swagger: `https://swimreg-api.onrender.com/swagger`
- כתובת הלקוח: `https://swimreg-client.onrender.com`

## 9. הלקוח

- Repository של הלקוח: `https://github.com/Chaya-Mandelman/swimreg-client`

---

### פקודות Git ו-GitHub
```bash
git add .
```
```bash
git commit -m "SwimReg server: initial version"
```
עם GitHub CLI (יוצר repository בשם `swimreg-server` ודוחף אליו):
```bash
gh repo create swimreg-server --public --source=. --remote=origin --push
```
או בלי GitHub CLI: יוצרים repository ריק בשם `swimreg-server` באתר GitHub, ואז:
```bash
git remote add origin https://github.com/<user>/swimreg-server.git
```
```bash
git push -u origin main
```
