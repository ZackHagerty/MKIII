using System;
using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            var userId = resultContext.HttpContext.User.GetUserId();
            Console.WriteLine(userId);
            var uow = resultContext.HttpContext.RequestServices.GetService<IUserRepository>();
            var user = await uow.GetUserByIdAsync(userId);
            var timeUtc = DateTime.UtcNow;
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            user.LastActive = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

        }
    }
}