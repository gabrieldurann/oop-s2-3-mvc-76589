using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Faculty")]
public class FacultyGradebookController : Controller
{
    private readonly ApplicationDbContext _db;

    public FacultyGradebookController(ApplicationDbContext db)
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

        var assignments = await _db.Assignments
            .Include(a => a.Results).ThenInclude(r => r.StudentProfile)
            .Where(a => a.CourseId == id)
            .ToListAsync();

        ViewBag.Course = course;
        return View(assignments);
    }

    public async Task<IActionResult> EnterResult(int id)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return RedirectToAction("Index", "Home");

        var assignment = await _db.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (assignment == null) return NotFound();

        var courseIds = await GetFacultyCourseIdsAsync(faculty.Id);
        if (!courseIds.Contains(assignment.CourseId)) return Forbid();

        // Get students enrolled in this course who don't have a result yet
        var existingStudentIds = await _db.AssignmentResults
            .Where(ar => ar.AssignmentId == id)
            .Select(ar => ar.StudentProfileId)
            .ToListAsync();

        var students = await _db.CourseEnrolments
            .Include(e => e.StudentProfile)
            .Where(e => e.CourseId == assignment.CourseId && !existingStudentIds.Contains(e.StudentProfileId))
            .Select(e => e.StudentProfile)
            .ToListAsync();

        ViewBag.Assignment = assignment;
        ViewBag.Students = new SelectList(students, "Id", "Name");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterResult(int id, int studentProfileId, int score, string? feedback)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return RedirectToAction("Index", "Home");

        var assignment = await _db.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (assignment == null) return NotFound();

        var courseIds = await GetFacultyCourseIdsAsync(faculty.Id);
        if (!courseIds.Contains(assignment.CourseId)) return Forbid();

        // Validate score
        if (score < 0 || score > assignment.MaxScore)
        {
            ModelState.AddModelError("", $"Score must be between 0 and {assignment.MaxScore}.");
        }

        // Check duplicate
        var exists = await _db.AssignmentResults
            .AnyAsync(ar => ar.AssignmentId == id && ar.StudentProfileId == studentProfileId);
        if (exists)
        {
            ModelState.AddModelError("", "This student already has a result for this assignment.");
        }

        if (!ModelState.IsValid)
        {
            var existingStudentIds = await _db.AssignmentResults
                .Where(ar => ar.AssignmentId == id)
                .Select(ar => ar.StudentProfileId)
                .ToListAsync();

            var students = await _db.CourseEnrolments
                .Include(e => e.StudentProfile)
                .Where(e => e.CourseId == assignment.CourseId && !existingStudentIds.Contains(e.StudentProfileId))
                .Select(e => e.StudentProfile)
                .ToListAsync();

            ViewBag.Assignment = assignment;
            ViewBag.Students = new SelectList(students, "Id", "Name");
            return View();
        }

        _db.AssignmentResults.Add(new AssignmentResult
        {
            AssignmentId = id,
            StudentProfileId = studentProfileId,
            Score = score,
            Feedback = feedback
        });
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Course), new { id = assignment.CourseId });
    }

    public async Task<IActionResult> EditResult(int id)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return RedirectToAction("Index", "Home");

        var result = await _db.AssignmentResults
            .Include(ar => ar.Assignment).ThenInclude(a => a.Course)
            .Include(ar => ar.StudentProfile)
            .FirstOrDefaultAsync(ar => ar.Id == id);
        if (result == null) return NotFound();

        var courseIds = await GetFacultyCourseIdsAsync(faculty.Id);
        if (!courseIds.Contains(result.Assignment.CourseId)) return Forbid();

        return View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditResult(int id, int score, string? feedback)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return RedirectToAction("Index", "Home");

        var result = await _db.AssignmentResults
            .Include(ar => ar.Assignment)
            .FirstOrDefaultAsync(ar => ar.Id == id);
        if (result == null) return NotFound();

        var courseIds = await GetFacultyCourseIdsAsync(faculty.Id);
        if (!courseIds.Contains(result.Assignment.CourseId)) return Forbid();

        if (score < 0 || score > result.Assignment.MaxScore)
        {
            ModelState.AddModelError("", $"Score must be between 0 and {result.Assignment.MaxScore}.");
            result = await _db.AssignmentResults
                .Include(ar => ar.Assignment).ThenInclude(a => a.Course)
                .Include(ar => ar.StudentProfile)
                .FirstOrDefaultAsync(ar => ar.Id == id);
            return View(result);
        }

        result.Score = score;
        result.Feedback = feedback;
        _db.AssignmentResults.Update(result);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Course), new { id = result.Assignment.CourseId });
    }
}