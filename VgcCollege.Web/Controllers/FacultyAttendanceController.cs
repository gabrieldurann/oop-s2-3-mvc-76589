using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Faculty")]
public class FacultyAttendanceController : Controller
{
    private readonly ApplicationDbContext _db;

    public FacultyAttendanceController(ApplicationDbContext db)
    {
        _db = db;
    }

    private async Task<FacultyProfile?> GetCurrentFacultyAsync()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return await _db.FacultyProfiles.FirstOrDefaultAsync(f => f.IdentityUserId == userId);
    }

    private async Task<List<int>> GetFacultyCourseIdsAsync(int facultyProfileId)
    {
        return await _db.FacultyCourseAssignments
            .Where(fca => fca.FacultyProfileId == facultyProfileId)
            .Select(fca => fca.CourseId)
            .ToListAsync();
    }

    public async Task<IActionResult> Index()
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return RedirectToAction("Index", "Home");

        var courseIds = await GetFacultyCourseIdsAsync(faculty.Id);

        var courses = await _db.Courses
            .Include(c => c.Branch)
            .Where(c => courseIds.Contains(c.Id))
            .ToListAsync();

        return View(courses);
    }

    public async Task<IActionResult> Course(int id)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return RedirectToAction("Index", "Home");

        var courseIds = await GetFacultyCourseIdsAsync(faculty.Id);
        if (!courseIds.Contains(id)) return Forbid();

        var course = await _db.Courses
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();

        var enrolments = await _db.CourseEnrolments
            .Include(e => e.StudentProfile)
            .Where(e => e.CourseId == id && e.Status == "Active")
            .ToListAsync();

        ViewBag.Course = course;
        return View(enrolments);
    }

    public async Task<IActionResult> MarkAttendance(int id)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return RedirectToAction("Index", "Home");

        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        var courseIds = await GetFacultyCourseIdsAsync(faculty.Id);
        if (!courseIds.Contains(id)) return Forbid();

        var enrolments = await _db.CourseEnrolments
            .Include(e => e.StudentProfile)
            .Where(e => e.CourseId == id && e.Status == "Active")
            .ToListAsync();

        ViewBag.Course = course;
        ViewBag.Today = DateTime.Today;
        return View(enrolments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAttendance(int id, DateTime date, Dictionary<int, bool> attendance)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return RedirectToAction("Index", "Home");

        var courseIds = await GetFacultyCourseIdsAsync(faculty.Id);
        if (!courseIds.Contains(id)) return Forbid();

        var enrolments = await _db.CourseEnrolments
            .Where(e => e.CourseId == id && e.Status == "Active")
            .ToListAsync();

        foreach (var enrolment in enrolments)
        {
            // Check if attendance already exists for this date
            var existing = await _db.AttendanceRecords
                .FirstOrDefaultAsync(a => a.CourseEnrolmentId == enrolment.Id && a.Date == date.Date);

            if (existing != null)
            {
                existing.Present = attendance.ContainsKey(enrolment.Id) && attendance[enrolment.Id];
                _db.AttendanceRecords.Update(existing);
            }
            else
            {
                _db.AttendanceRecords.Add(new AttendanceRecord
                {
                    CourseEnrolmentId = enrolment.Id,
                    Date = date.Date,
                    Present = attendance.ContainsKey(enrolment.Id) && attendance[enrolment.Id]
                });
            }
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Course), new { id });
    }
}