using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminCoursesController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminCoursesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _db.Courses.Include(c => c.Branch).ToListAsync();
        return View(courses);
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await _db.Courses
            .Include(c => c.Branch)
            .Include(c => c.Enrolments).ThenInclude(e => e.StudentProfile)
            .Include(c => c.FacultyAssignments).ThenInclude(fa => fa.FacultyProfile)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        return View(course);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Branches = new SelectList(await _db.Branches.ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Branches = new SelectList(await _db.Branches.ToListAsync(), "Id", "Name");
            return View(course);
        }

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        ViewBag.Branches = new SelectList(await _db.Branches.ToListAsync(), "Id", "Name", course.BranchId);
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course course)
    {
        if (id != course.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Branches = new SelectList(await _db.Branches.ToListAsync(), "Id", "Name", course.BranchId);
            return View(course);
        }

        _db.Courses.Update(course);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var course = await _db.Courses.Include(c => c.Branch).FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound();
        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return NotFound();

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}