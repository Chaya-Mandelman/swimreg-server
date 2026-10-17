using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SwimReg.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseConcurrencyAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Courses",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.InsertData(
                table: "Pools",
                columns: new[] { "Id", "Address", "Name" },
                values: new object[,]
                {
                    { 1, "רחוב הים 10, תל אביב", "בריכה עירונית מרכז" },
                    { 2, "שדרות הנשיא 5, חיפה", "בריכת הצפון" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, "admin@swimreg.com", "מנהלת המערכת", "$2a$11$NJPE9ZQnaC8qKWCRjlbxIO94jAtlu726auVvB5YYKf4oWkSPJsvb6", "Admin" },
                    { 2, "dana@swimreg.com", "דנה כהן", "$2a$11$E/DSeS0ZC5TtghA5BMvGGOdXD9fm5AuyNgB0uHIPISD2xtAn/tlt2", "Instructor" },
                    { 3, "yossi@swimreg.com", "יוסי לוי", "$2a$11$E/DSeS0ZC5TtghA5BMvGGOdXD9fm5AuyNgB0uHIPISD2xtAn/tlt2", "Instructor" },
                    { 4, "noa@swimreg.com", "נועה פרץ", "$2a$11$OgQlFlNG1zTNgxgyDYjYs.Za3Mh4cEghzjC3wSogeSTBg0Jv9M7S.", "Student" },
                    { 5, "itay@swimreg.com", "איתי מזרחי", "$2a$11$OgQlFlNG1zTNgxgyDYjYs.Za3Mh4cEghzjC3wSogeSTBg0Jv9M7S.", "Student" }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "CurrentCount", "DayOfWeek", "InstructorId", "Level", "MaxParticipants", "Name", "PoolId", "StartTime" },
                values: new object[,]
                {
                    { 1, 1, 0, 2, 0, 10, "שחייה למתחילים", 1, new TimeOnly(17, 0, 0) },
                    { 2, 0, 1, 2, 1, 8, "שחייה מתקדמים-בינוני", 1, new TimeOnly(18, 30, 0) },
                    { 3, 1, 3, 3, 2, 12, "אימון מתקדמים", 2, new TimeOnly(19, 0, 0) },
                    { 4, 0, 4, 3, 0, 1, "שיעור פרטי", 2, new TimeOnly(16, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Registrations",
                columns: new[] { "Id", "CourseId", "RegisteredAt", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 9, 1, 8, 0, 0, 0, DateTimeKind.Utc), 4 },
                    { 2, 3, new DateTime(2026, 9, 2, 9, 30, 0, 0, DateTimeKind.Utc), 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Registrations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Registrations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Pools",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Pools",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Courses");
        }
    }
}
