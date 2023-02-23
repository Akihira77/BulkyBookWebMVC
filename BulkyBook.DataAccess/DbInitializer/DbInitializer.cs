using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInitializer;

public class DbInitializer : IDbInitializer
{
    private readonly UserManager<IdentityUser> _userManager;

    // Add dependency injection
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public DbInitializer(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }
    public async Task Initialize()
    {
        // migrations if they are not applied
        try
        {
            if (_db.Database.GetPendingMigrations().Any())
            {
                _db.Database.Migrate();
            }
        } catch (Exception ex)
        {

        }

        // create roles if they are not created
        if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
        {
            // GetAwaiter().GetResult();
            // Create Custom Role
            await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
            await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
            await _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi));
            await _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp));

            // if roles are not created, then we will create admin user as well

            await _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "admin@dotnetmastery.com",
                Email = "admin@dotnetmastery.com",
                Name = "Andika Wahyu",
                PhoneNumber = "082380539018",
                StreetAddress = "Test 123 Ave",
                State = "IL0",
                PostalCode = "31463",
                City = "Lampung"
            }, "andikawp#00");

            var user = _db.ApplicationUsers
                            .FirstOrDefault(u => u.Email == "admin@dotnetmastery.com");

            await _userManager.AddToRoleAsync(user, SD.Role_Admin);
        }
    }
}
