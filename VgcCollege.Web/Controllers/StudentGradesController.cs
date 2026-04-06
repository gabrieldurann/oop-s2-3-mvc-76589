using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentGradesController : Controller
{
    private readonly ApplicationDbContext _db;

    public StudentGradesController(ApplicationDbContext db)
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

        // Verify student is enrolled in this course
        var enrolment = await _db.CourseEnrolments
            .Include(e => e.Course).ThenInclude(c => c.Branch)
            .FirstOrDefaultAsync(e => e.StudentProfileId == student.Id && e.CourseId == id);
        if (enrolment == null) return Forbid();

        var assignmentResults = await _db.AssignmentResults
            .Include(ar => ar.Assignment)
            .Where(ar => ar.StudentProfileId == student.Id && ar.Assignment.CourseId == id)
            .ToListAsync();

        // Only show released exam results
        var examResults = await _db.ExamResults
            .Include(er => er.Exam)
            .Where(er => er.StudentProfileId == student.Id
                         && er.Exam.CourseId == id
                         && er.Exam.ResultsReleased)
            .ToListAsync();

        // Count provisional exams so student knows there are unreleased results
        var provisionalCount = await _db.ExamResults
            .Include(er => er.Exam)
            .CountAsync(er => er.StudentProfileId == student.Id
                              && er.Exam.CourseId == id
                              && !er.Exam.ResultsReleased);

        ViewBag.Course = enrolment.Course;
        ViewBag.AssignmentResults = assignmentResults;
        ViewBag.ExamResults = examResults;
        ViewBag.ProvisionalCount = provisionalCount;

        return View();
    }
}