using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMart.Data;

namespace AutoMart.Models
{
    public static class Configure
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService
                <DbContextOptions<ApplicationDbContext>>()))
            {
                // Check if the database already contains at least one role, meaning that the code has been run.
                
                if (context.Roles.Any())
                {
                    return;  
                }

               
                context.Roles.AddRange(
                    new IdentityRole { Id = "f2369e92-e6e9-48f0-a1e6-4e146596c283", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                    new IdentityRole { Id = "5194406e-a6ae-4570-94a8-5af47a8f0468", Name = "Editor", NormalizedName = "Editor".ToUpper() },
                    new IdentityRole { Id = "1f89f81e-95cf-4360-9834-c0214c08d357", Name = "User", NormalizedName = "User".ToUpper() }
                );

                
                var hasher = new PasswordHasher<WebUser>();

               
                context.Users.AddRange(
                    new WebUser
                    {
                        Id = "75c8f3aa-e6f2-40d7-b865-b807e8ec6bb5", // primary key
                        UserName = "admin@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMIN@TEST.COM",
                        Email = "admin@test.com",
                        NormalizedUserName = "ADMIN@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Admin1!")
                    },
                    new WebUser
                    {
                        Id = "13beaf34-24c9-45fc-a5d5-0f747a011392", 
                        UserName = "Editor@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "EDITOR@TEST.COM",
                        Email = "Editor@test.com",
                        NormalizedUserName = "EDITOR@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Editor1!")
                    },
                    new WebUser
                    {
                        Id = "43291cef-b112-4e5f-a4c7-367d2fbffa18", 
                        UserName = "user@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "USER@TEST.COM",
                        Email = "user@test.com",
                        NormalizedUserName = "USER@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "User1!")
                    }
                );

                // ASSOCIATING USER-ROLE
                context.UserRoles.AddRange(
                    new IdentityUserRole<string>
                    {
                        RoleId = "f2369e92-e6e9-48f0-a1e6-4e146596c283",
                        UserId = "75c8f3aa-e6f2-40d7-b865-b807e8ec6bb5"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "5194406e-a6ae-4570-94a8-5af47a8f0468",
                        UserId = "13beaf34-24c9-45fc-a5d5-0f747a011392"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "1f89f81e-95cf-4360-9834-c0214c08d357",
                        UserId = "43291cef-b112-4e5f-a4c7-367d2fbffa18"
                    }
                );

                context.SaveChanges();

            }
        }
    }
}
