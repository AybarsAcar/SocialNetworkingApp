using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Extentions;
using API.Interfaces;
using API.MIddleware;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API
{
  public class Startup
  {
    private readonly IConfiguration _config;
    public Startup(IConfiguration config)
    {
      this._config = config;
    }

    public void ConfigureServices(IServiceCollection services)
    {

      // our custom add application services method that inject application dependencies
      services.AddApplicationServices(_config);

      // our custom add application services method that inject identity dependencies
      services.AddIdentityServices(_config);

      services.AddControllers();

      services.AddCors();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseMiddleware<ExceptionMiddleware>();

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseCors(
        policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200")
      );

      app.UseAuthentication();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
