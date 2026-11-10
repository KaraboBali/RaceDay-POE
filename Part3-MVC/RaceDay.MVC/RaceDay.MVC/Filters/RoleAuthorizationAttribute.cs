using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Filters;

// Blocks pages for people who are not logged in, or who have the wrong role.
public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;
    public RoleAuthorizeAttribute(params string[] roles) => _roles = roles;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.IsLoggedIn())
        {
            var factory = context.HttpContext.RequestServices.GetService(typeof(ITempDataDictionaryFactory)) as ITempDataDictionaryFactory;
            var tempData = factory?.GetTempData(context.HttpContext);
            if (tempData != null) tempData["Error"] = "Please log in to continue.";

            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (_roles.Length > 0 && !_roles.Contains(session.CurrentRole()))
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
    }
}