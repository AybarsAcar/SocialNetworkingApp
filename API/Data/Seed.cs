using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class Seed
  {
    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
      // check if there are any users in the Users table
      if (await userManager.Users.AnyAsync()) return;

      var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");

      // serialise the json data into a list of users
      var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

      if (users == null) return;

      var roles = new List<AppRole>
      {
        new AppRole{Name = "Member"},
        new AppRole{Name = "Admin"},
        new AppRole{Name = "Moderator"}
      };

      foreach (var role in roles)
      {
        await roleManager.CreateAsync(role);
      }

      // create the users
      foreach (var user in users)
      {
        // add the username and the password props to each user
        user.UserName = user.UserName.ToLower();

        await userManager.CreateAsync(user, "Pa$$w0rd");

        await userManager.AddToRoleAsync(user, "Member");
      }

      // create the admin
      var admin = new AppUser
      {
        UserName = "adming"
      };

      await userManager.CreateAsync(admin, "Pa$$w0rd");
      await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
    }
  }
}