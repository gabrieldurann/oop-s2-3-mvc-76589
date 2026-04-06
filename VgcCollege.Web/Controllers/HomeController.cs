using Microsoft.AspNetCore.Mvc;

namespace VgcCollege.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "AdminBranches");
        if (User.IsInRole("Faculty"))
            return RedirectToAction("Index", "FacultyDashboard");
        if (User.IsInRole("Student"))
            return RedirectToAction("Index", "StudentDashboard");

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View("~/Views/Shared/AccessDenied.cshtml");
    }

    [Route("Home/Error/{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        if (statusCode == 404)
        {
            ViewData["Title"] = "Page Not Found";
            ViewData["Message"] = "The page you're looking for doesn't exist.";
        }
        else
        {
            ViewData["Title"] = "Error";
            ViewData["Message"] = "An unexpected error occurred.";
        }
        return View("StatusCode");
    }
}