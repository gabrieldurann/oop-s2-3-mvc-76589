using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminExamsController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminExamsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var exams = await _db.Exams
            .Include(e => e.Course).ThenInclude(c => c.Branch)
            .ToListAsync();
        return View(exams);
    }

    public async Task<IActionResult> Details(int id)
    {
        var exam = await _db.Exams
            .Include(e => e.Course).ThenInclude(c => c.Branch)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null) return NotFound();

        var results = await _db.ExamResults
            .Include(er => er.StudentProfile)
            .Where(er => er.ExamId == id)
            .ToListAsync();

        ViewBag.Results = results;

        return View(exam);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Courses = new SelectList(
            await _db.Courses.Include(c => c.Branch)
                .Select(c => new { c.Id, Display = c.Name + " (" + c.Branch.Name + ")" })
                .ToListAsync(),
            "Id", "Display");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Exam exam)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = new SelectList(
                await _db.Courses.Include(c => c.Branch)
                    .Select(c => new { c.Id, Display = c.Name + " (" + c.Branch.Name + ")" })
                    .ToListAsync(),
                "Id", "Display");
            return View(exam);
        }

        _db.Exams.Add(exam);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var exam = await _db.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        ViewBag.Courses = new SelectList(
            await _db.Courses.Include(c => c.Branch)
                .Select(c => new { c.Id, Display = c.Name + " (" + c.Branch.Name + ")" })
                .ToListAsync(),
            "Id", "Display", exam.CourseId);
        return View(exam);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Exam exam)
    {
        if (id != exam.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = new SelectList(
                await _db.Courses.Include(c => c.Branch)
                    .Select(c => new { c.Id, Display = c.Name + " (" + c.Branch.Name + ")" })
                    .ToListAsync(),
                "Id", "Display", exam.CourseId);
            return View(exam);
        }

        _db.Exams.Update(exam);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRelease(int id)
    {
        var exam = await _db.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        exam.ResultsReleased = !exam.ResultsReleased;
        _db.Exams.Update(exam);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var exam = await _db.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null) return NotFound();
        return View(exam);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var exam = await _db.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        _db.Exams.Remove(exam);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}