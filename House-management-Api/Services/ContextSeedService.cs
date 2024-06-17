using House_management_Api.Data;
using House_management_Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace House_management_Api.Services
{
    
    public class ContextSeedService(Context_identity context,
        UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        private readonly Context_identity _context = context;
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task InitializeContextAsync()
        {
            if(_context.Database.GetPendingMigrationsAsync().GetAwaiter().GetResult().Any()) 
            {
                //apply any pending migrations into our database
                await _context.Database.MigrateAsync();
            }

            if(!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.AdminRole });
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.ManagerRole });
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.RentRole });

                if(!_userManager.Users.AnyAsync().GetAwaiter().GetResult())
                {
                    var admin = new User
                    {
                        FirstName = "Admin",
                        LastName = "Admin",
                        UserName = SD.AdminUserName,
                        Email = SD.AdminUserName,
                        EmailConfirmed = true
                    };

                    await _userManager.CreateAsync(admin, "123456");
                    await _userManager.AddToRolesAsync(admin, [SD.AdminRole, SD.ManagerRole, SD.RentRole]);
                    await _userManager.AddClaimsAsync(admin,
                         [
                            new(ClaimTypes.Email, admin.Email),
                            new(ClaimTypes.Email, admin.LastName)
                         ]);

                    var manager = new User
                    {
                        FirstName = "Manager",
                        LastName = "Manager",
                        UserName = "manager@example.com",
                        Email = "manager@example.com",
                        EmailConfirmed = true
                    };

                    await _userManager.CreateAsync(manager, "123456");
                    await _userManager.AddToRolesAsync(manager, [SD.ManagerRole]);
                    await _userManager.AddClaimsAsync(manager,
                    [
                        new(ClaimTypes.Email, manager.Email),
                        new(ClaimTypes.Email, manager.LastName)

                    ]);


                    var rent = new User
                    {
                        FirstName = "Rent",
                        LastName = "Rent",
                        UserName = "rent@example.com",
                        Email = "rent@example.com",
                        EmailConfirmed = true
                    };

                    await _userManager.CreateAsync(rent, "123456");
                    await _userManager.AddToRolesAsync(rent, [ SD.RentRole]);
                    await _userManager.AddClaimsAsync(rent,
                    [
                        new(ClaimTypes.Email, rent.Email),
                        new(ClaimTypes.Email, rent.LastName)

                    ]);

                    var viprent = new User
                    {
                        FirstName = "Vip Rent",
                        LastName = "Vip Rent",
                        UserName = "viprent@example.com",
                        Email = "viprent@example.com",
                        EmailConfirmed = true
                    };

                    await _userManager.CreateAsync(viprent, "123456");
                    await _userManager.AddToRolesAsync(viprent, [SD.RentRole]);
                    await _userManager.AddClaimsAsync(viprent,
                    [
                        new(ClaimTypes.Email, viprent.Email),
                        new(ClaimTypes.Email, viprent.LastName)

                    ]);


                }

            }

        }

    }
}
