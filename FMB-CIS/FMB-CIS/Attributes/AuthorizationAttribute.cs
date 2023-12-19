using System.Security.Claims;
using FMB_CIS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services.Utilities;

public class RequiresAccessAttribute : ActionFilterAttribute
{
    public string allowedAccessRights { get; set; }
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var c = filterContext.Controller as Controller;

        var userAccessRights = ((ClaimsIdentity)c.User.Identity).FindFirst("accessRights").Value;

        if (AccessRightsUtilities.IsInAccessRights(userAccessRights, allowedAccessRights))
        {

        }
        else
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
