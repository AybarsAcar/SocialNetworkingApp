using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace API.Extentions
{
  public static class IdentityServiceExtentions
  {
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
      services.AddIdentityCore<AppUser>(opt =>
      {
        opt.Password.RequireNonAlphanumeric = true;
      })
        .AddRoles<AppRole>()
        .AddRoleManager<RoleManager<AppRole>>()
        .AddSignInManager<SignInManager<AppUser>>()
        .AddRoleValidator<RoleValidator<AppRole>>()
        .AddEntityFrameworkStores<DataContext>();

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(opt =>
     {
       opt.TokenValidationParameters = new TokenValidationParameters
       {
         ValidateIssuerSigningKey = true,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
         ValidateIssuer = false,
         ValidateAudience = false
       };

       //  options for web sockets // hubs end points
       opt.Events = new JwtBearerEvents
       {
         OnMessageReceived = context =>
         {
           var accessToken = context.Request.Query["access_token"];

           var path = context.HttpContext.Request.Path;

           if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
           {
             context.Token = accessToken;
           }

           return Task.CompletedTask;
         }
       };
     });

      services.AddAuthorization(opt =>
      {
        opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
        opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
      });

      return services;
    }
  }
}