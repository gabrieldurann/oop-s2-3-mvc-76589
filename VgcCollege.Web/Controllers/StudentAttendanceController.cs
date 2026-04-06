using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentAttendanceController : Controller
{
    private readonly ApplicationDbContext _db;

    public StudentAttendanceController(ApplicationDbContext db)
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

        return View(enrolments);
    }

    public async Task<IActionResult> Course(int id)
    {
        var student = await GetCurrentStudentAsync();
        if (student == null) return RedirectToAction("Index", "Home");

        // Verify student is enrolled
        var enrolment = await _db.CourseEnrolments
            .Include(e => e.Course).ThenInclude(c => c.Branch)
            .FirstOrDefaultAsync(e => e.StudentProfileId == student.Id && e.CourseId == id);
        if (enrolment == null) return Forbid();

        var attendanceRecords = await _db.AttendanceRecords
            .Where(a => a.CourseEnrolmentId == enrolment.Id)
            .OrderBy(a => a.Date)
            .ToListAsync();

        ViewBag.Course = enrolment.Course;
        ViewBag.AttendanceRecords = attendanceRecords;

        return View();
    }
}