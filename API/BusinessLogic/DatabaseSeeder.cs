using JiraTrack.Models.Entities;
using JiraTrack.Repository;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.BusinessLogic;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
            return;

        var admin = new User
        {
            Email = "admin@jiratrack.com",
            UserName = "admin",
            PasswordHash = PasswordHasher.HashPassword("Admin@123"),
            FirstName = "System",
            LastName = "Administrator",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = 1 });
        await context.SaveChangesAsync();
    }
}
