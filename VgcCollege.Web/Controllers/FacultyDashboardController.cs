using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Faculty")]
public class FacultyDashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public FacultyDashboardController(ApplicationDbContext db)
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
            .Include(c => c.Enrolments)
            .Where(c => courseIds.Contains(c.Id))
            .ToListAsync();

        ViewBag.FacultyName = faculty.Name;
        return View(courses);
    }

    public async Task<IActionResult> CourseStudents(int id)
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
            .Where(e => e.CourseId == id)
            .ToListAsync();

        ViewBag.Course = course;
        return View(enrolments);
    }

    public async Task<IActionResult> StudentDetails(int id)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return RedirectToAction("Index", "Home");

        var courseIds = await GetFacultyCourseIdsAsync(faculty.Id);

        // Only allow viewing students enrolled in faculty's courses
        var enrolment = await _db.CourseEnrolments
            .Include(e => e.StudentProfile)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id && courseIds.Contains(e.CourseId));
        if (enrolment == null) return Forbid();

        var student = enrolment.StudentProfile;

        var attendanceRecords = await _db.AttendanceRecords
            .Where(a => a.CourseEnrolmentId == id)
            .OrderBy(a => a.Date)
            .ToListAsync();

        var assignmentResults = await _db.AssignmentResults
            .Include(ar => ar.Assignment)
            .Where(ar => ar.StudentProfileId == student.Id
                         && ar.Assignment.CourseId == enrolment.CourseId)
            .ToListAsync();

        var examResults = await _db.ExamResults
            .Include(er => er.Exam)
            .Where(er => er.StudentProfileId == student.Id
                         && er.Exam.CourseId == enrolment.CourseId)
            .ToListAsync();

        ViewBag.Enrolment = enrolment;
        ViewBag.AttendanceRecords = attendanceRecords;
        ViewBag.AssignmentResults = assignmentResults;
        ViewBag.ExamResults = examResults;

        return View(student);
    }
}