using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SchoolEvents.Data.Seed;

public static class IdentitySeed
{
    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Teacher", "Student" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedRolesAndUsersAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

       
        if (!await roleManager.RoleExistsAsync("Teacher"))
            await roleManager.CreateAsync(new IdentityRole("Teacher"));

        var teacherEmail = "teacher@school.com";
        var teacher = await userManager.FindByEmailAsync(teacherEmail);
        if (teacher == null)
        {
            teacher = new IdentityUser { UserName = teacherEmail, Email = teacherEmail, EmailConfirmed = true };
            await userManager.CreateAsync(teacher, "Teacher#123");
        }

        if (!await userManager.IsInRoleAsync(teacher, "Teacher"))
        {
            await userManager.AddToRoleAsync(teacher, "Teacher");
        }

      
        if (!await roleManager.RoleExistsAsync("Student"))
            await roleManager.CreateAsync(new IdentityRole("Student"));

        var studentEmail = "student@school.com";
        var student = await userManager.FindByEmailAsync(studentEmail);
        if (student == null)
        {
            student = new IdentityUser { UserName = studentEmail, Email = studentEmail, EmailConfirmed = true };
            await userManager.CreateAsync(student, "Student#123");
        }

        if (!await userManager.IsInRoleAsync(student, "Student"))
        {
            await userManager.AddToRoleAsync(student, "Student");
        }
    }
}
