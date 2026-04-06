using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentDashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public StudentDashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    private async Task<StudentProfile?> GetCurrentStudentAsync()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return await _db.StudentProfiles.FirstOrDefaultAsync(s => s.IdentityUserId == userId);
    }

    public async Task<IActionResult> Index()
    {
        var student = await GetCurrentStudentAsync();
        if (student == null) return RedirectToAction("Index", "Home");

        var enrolments = await _db.CourseEnrolments
            .Include(e => e.Course).ThenInclude(c => c.Branch)
            .Where(e => e.StudentProfileId == student.Id)
            .ToListAsync();

        ViewBag.Student = student;
        return View(enrolments);
    }

    public async Task<IActionResult> Profile()
    {
        var student = await GetCurrentStudentAsync();
        if (student == null) return RedirectToAction("Index", "Home");
        return View(student);
    }
}