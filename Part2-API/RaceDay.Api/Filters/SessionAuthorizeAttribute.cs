using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Filters;

public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;
    public SessionAuthorizeAttribute(params string[] roles) => _roles = roles;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userId = context.HttpContext.Session.GetInt32("UserId");
        var role = context.HttpContext.Session.GetString("Role");

        if (userId == null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Please log in." });
            return;
        }
        if (_roles.Length > 0 && !_roles.Contains(role))
            context.Result = new ObjectResult(new { message = "Your role cannot access this." }) { StatusCode = 403 };
    }
}