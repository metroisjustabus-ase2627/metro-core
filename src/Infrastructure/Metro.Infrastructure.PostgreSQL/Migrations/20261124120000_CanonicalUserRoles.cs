using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metro.Infrastructure.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CanonicalUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "User" SET "Role" = 'Staff' WHERE LOWER("Role") IN ('master', 'admin', 'staff');
                UPDATE "User" SET "Role" = 'Passenger' WHERE LOWER("Role") IN ('client', 'user', 'passenger');
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_Role",
                table: "User",
                sql: "\"Role\" IN ('Passenger', 'Staff')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_User_Role",
                table: "User");

            migrationBuilder.Sql("""
                UPDATE "User" SET "Role" = 'master' WHERE "Role" = 'Staff';
                UPDATE "User" SET "Role" = 'client' WHERE "Role" = 'Passenger';
                """);
        }
    }
}
