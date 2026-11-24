using Microsoft.EntityFrameworkCore;
using Metro.Domain.Users;
using Metro.Domain.Users.Entities;
using Metro.Infrastructure.PostgreSQL.Contexts;

namespace Metro.Infrastructure.PostgreSQL.Initializers
{
    public static class PostgreSQLInitializer
    {
        public static void MigrateDatabase(this PostgreSQLContext context)
        {
            context.Database.Migrate();
        }

        public static void SeedStaffUser(this PostgreSQLContext context, string email, string hashedPassword)
        {
            if (context.Users.Any(u => u.Email == email || u.Role == UserRoles.Staff))
            {
                return;
            }

            var staff = new User(
                name: "Operational Staff",
                email: email,
                document: string.Empty,
                phone: string.Empty,
                password: hashedPassword,
                role: UserRoles.Staff,
                isActive: true
            );

            context.Users.Add(staff);
            context.SaveChanges();
        }
    }
}
