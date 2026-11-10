using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Error() => View();

    public IActionResult StatusPage(int code)
    {
        ViewData["Code"] = code;
        ViewData["Heading"] = code switch
        {
            404 => "Page not found",
            403 => "Access denied",
            _ => "Something went wrong"
        };
        return View();
    }
}