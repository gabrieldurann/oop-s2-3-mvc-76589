using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminEnrolmentsController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminEnrolmentsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var enrolments = await _db.CourseEnrolments
            .Include(e => e.StudentProfile)
            .Include(e => e.Course).ThenInclude(c => c.Branch)
            .ToListAsync();
        return View(enrolments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var enrolment = await _db.CourseEnrolments
            .Include(e => e.StudentProfile)
            .Include(e => e.Course).ThenInclude(c => c.Branch)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (enrolment == null) return NotFound();

        var attendanceRecords = await _db.AttendanceRecords
            .Where(a => a.CourseEnrolmentId == id)
            .OrderBy(a => a.Date)
            .ToListAsync();

        var assignmentResults = await _db.AssignmentResults
            .Include(ar => ar.Assignment)
            .Where(ar => ar.StudentProfileId == enrolment.StudentProfileId
                         && ar.Assignment.CourseId == enrolment.CourseId)
            .ToListAsync();

        var examResults = await _db.ExamResults
            .Include(er => er.Exam)
            .Where(er => er.StudentProfileId == enrolment.StudentProfileId
                         && er.Exam.CourseId == enrolment.CourseId)
            .ToListAsync();

        ViewBag.AttendanceRecords = attendanceRecords;
        ViewBag.AssignmentResults = assignmentResults;
        ViewBag.ExamResults = examResults;

        return View(enrolment);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Students = new SelectList(await _db.StudentProfiles.ToListAsync(), "Id", "Name");
        ViewBag.Courses = new SelectList(
            await _db.Courses.Include(c => c.Branch)
                .Select(c => new { c.Id, Display = c.Name + " (" + c.Branch.Name + ")" })
                .ToListAsync(),
            "Id", "Display");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseEnrolment enrolment)
    {
        var exists = await _db.CourseEnrolments
            .AnyAsync(e => e.StudentProfileId == enrolment.StudentProfileId && e.CourseId == enrolment.CourseId);

        if (exists)
        {
            ModelState.AddModelError("", "This student is already enrolled in this course.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Students = new SelectList(await _db.StudentProfiles.ToListAsync(), "Id", "Name");
            ViewBag.Courses = new SelectList(
                await _db.Courses.Include(c => c.Branch)
                    .Select(c => new { c.Id, Display = c.Name + " (" + c.Branch.Name + ")" })
                    .ToListAsync(),
                "Id", "Display");
            return View(enrolment);
        }

        enrolment.EnrolDate = DateTime.Now;
        enrolment.Status = "Active";
        _db.CourseEnrolments.Add(enrolment);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var enrolment = await _db.CourseEnrolments
            .Include(e => e.StudentProfile)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (enrolment == null) return NotFound();
        return View(enrolment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string status)
    {
        var enrolment = await _db.CourseEnrolments.FindAsync(id);
        if (enrolment == null) return NotFound();

        enrolment.Status = status;
        _db.CourseEnrolments.Update(enrolment);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var enrolment = await _db.CourseEnrolments
            .Include(e => e.StudentProfile)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (enrolment == null) return NotFound();
        return View(enrolment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var enrolment = await _db.CourseEnrolments.FindAsync(id);
        if (enrolment == null) return NotFound();

        _db.CourseEnrolments.Remove(enrolment);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}