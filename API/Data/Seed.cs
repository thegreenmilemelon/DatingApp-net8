using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    // public static async Task SeedUsers(DataContext context)
    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        // if (await context.Users.AnyAsync()) return;
        if (await userManager.Users.AnyAsync()) return;
        var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

        if (users == null) return;

        var roles  = new List<AppRole>
        {
            new  AppRole{Name = "Member"},
            new() {Name = "Admin"},
            new() {Name = "Moderator"}
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }
        

        foreach (var user in users)
        {
            //using var hmac = new HMACSHA512();
            //UserName is given by the Identity in upper case, so no need to convert
            // user.UserName = user.UserName.ToLower();

            // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
            // user.PasswordSalt = hmac.Key;
            // context.Users.Add(user);


            //creating uer in database with a password
            //and at the same time saves to the database
            user.Photos.First().IsApproved = true;
            user.UserName = user.UserName!.ToLower();
            await userManager.CreateAsync(user, "Pa$$w0rd");

            await userManager.AddToRoleAsync(user, "Member");
        }

        var admin = new AppUser{
            UserName = "admin",
            KnownAs = "Admin",
            Gender = "",
            City = "",
            Country = ""
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
        //await context.SaveChangesAsync();

    }
}
