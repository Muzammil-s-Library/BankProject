using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BankProject.Data;
using BankProject.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
namespace BankProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DbcontextConnection") ?? throw new InvalidOperationException("Connection string 'DbcontextConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
         
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
 .AddEntityFrameworkStores<ApplicationDbContext>()
 .AddDefaultTokenProviders();  // <-- Add this to register token providers

            builder.Services.AddScoped<IEmailSender, SendEmail>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSession();
            builder.Services.AddSignalR(); 

            var app = builder.Build();
            CreateAdminUser(app);
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.MapRazorPages();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication(); // Add this line
   

            app.UseAuthorization();


            app.UseEndpoints(endpoints => {

            endpoints.MapHub<ChatHub>("/ChatHub");
            
            });
            app.MapControllerRoute(
                  name: "areas",
                  pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


            app.MapControllerRoute(
               name: "default",
                pattern: "{Area=Client}/{controller=Client}/{action=Index}/{id?}");
            


            app.Run();
        }
        private static void CreateAdminUser(WebApplication app)
        {
            var scope = app.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Check if the Admin role exists, if not create it
            var roleExist = roleManager.RoleExistsAsync("Admin").Result;
            if (!roleExist)
            {
                var role = new IdentityRole("Admin");
                var result = roleManager.CreateAsync(role).Result;
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException("Could not create the Admin role.");
                }
            }

            // Check if the Admin user exists by email (case insensitive)
            var adminUser = userManager.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == "admin@gmail.com".ToLower()).Result;
            if (adminUser == null)
            {
                // Create the Admin user
                adminUser = new ApplicationUser
                {
                    UserName = "admin@gmail.com", // Ensure username is unique
                    Email = "admin@gmail.com",
                    FullName = "Admin",
                    EmailConfirmed = true
                };

                var userResult = userManager.CreateAsync(adminUser, "AdminPassword123!").Result;
                if (!userResult.Succeeded)
                {
                    throw new InvalidOperationException("Could not create the admin user.");
                }
            }
            else
            {
                // Handle the case where the email exists with a different case or username
                if (adminUser.UserName.ToLower() != "admin@gmail.com".ToLower())
                {
                    throw new InvalidOperationException("A user with this email already exists with a different username.");
                }
            }

            // Add the Admin role to the user if not already added
            var isInRole = userManager.IsInRoleAsync(adminUser, "Admin").Result;
            if (!isInRole)
            {
                var addRoleResult = userManager.AddToRoleAsync(adminUser, "Admin").Result;
                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException("Could not add the Admin role to the user.");
                }
            }
        }


    }
}
