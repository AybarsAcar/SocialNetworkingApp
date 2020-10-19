using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using API.Extentions;
using API.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace API.Helpers
{
  public class LogUserActivity : IAsyncActionFilter
  {
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      var resultContext = await next();

      // return early if the user is not authenticated
      if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

      var userId = resultContext.HttpContext.User.GetUserId();

      // get access to our repo with service locator pattern
      var repo = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();
      var user = await repo.GetUserByIdAsync(userId);

      user.LastActive = DateTime.Now;
      await repo.SaveAllAsync();
    }
  }
}