using System.Security.Claims;
using FMB_CIS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services.Services;
using Services.Utilities;

public class UserActivatedAttribute : ActionFilterAttribute
{
        private readonly UserService _userService;
        public UserActivatedAttribute(UserService userService)
        {
            _userService = userService;
        }
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var c = filterContext.Controller as Controller;

        var uid = Convert.ToInt32(((ClaimsIdentity)c.User.Identity).FindFirst("userID").Value);

        bool userStatus = _userService.IsAdminActivated(uid);

        if (userStatus != true)
        {
            RedirectToDashboard(filterContext);
        }
    }

    private void RedirectToLogin(ActionExecutingContext filterContext)
    {
        var redirectTarget = new RouteValueDictionary
    {
        {"action", "Login"},
        {"controller", "Account"}
    };

        filterContext.Result = new RedirectToRouteResult(redirectTarget);
    }

    private void RedirectToDashboard(ActionExecutingContext filterContext)
    {
        var redirectTarget = new RouteValueDictionary
    {
        {"action", "Index"},
        {"controller", "Dashboard"}
    };

        filterContext.Result = new RedirectToRouteResult(redirectTarget);
    }
}
