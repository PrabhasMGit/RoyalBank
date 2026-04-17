using Microsoft.EntityFrameworkCore;
using RoyalBank.Models;

namespace RoyalBank.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            // Create default admin user if not exists
            if (!await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
            {
                context.Users.Add(new User
                {
                    Username     = "admin@gmail.com",
                    Password     = "Admin@123",
                    HashPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role         = UserRole.Admin,
                    CustomerId   = null
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
