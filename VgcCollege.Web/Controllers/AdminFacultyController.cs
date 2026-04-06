using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminFacultyController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminFacultyController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var faculty = await _db.FacultyProfiles
            .Include(f => f.CourseAssignments).ThenInclude(ca => ca.Course)
            .ToListAsync();
        return View(faculty);
    }

    public async Task<IActionResult> Details(int id)
    {
        var faculty = await _db.FacultyProfiles
            .Include(f => f.CourseAssignments).ThenInclude(ca => ca.Course).ThenInclude(c => c.Branch)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (faculty == null) return NotFound();
        return View(faculty);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FacultyProfile faculty)
    {
        if (!ModelState.IsValid) return View(faculty);

        _db.FacultyProfiles.Add(faculty);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var faculty = await _db.FacultyProfiles.FindAsync(id);
        if (faculty == null) return NotFound();
        return View(faculty);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FacultyProfile faculty)
    {
        if (id != faculty.Id) return NotFound();
        if (!ModelState.IsValid) return View(faculty);

        _db.FacultyProfiles.Update(faculty);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AssignCourse(int id)
    {
        var faculty = await _db.FacultyProfiles.FindAsync(id);
        if (faculty == null) return NotFound();

        var assignedCourseIds = await _db.FacultyCourseAssignments
            .Where(fca => fca.FacultyProfileId == id)
            .Select(fca => fca.CourseId)
            .ToListAsync();

        ViewBag.FacultyName = faculty.Name;
        ViewBag.FacultyId = faculty.Id;
        ViewBag.Courses = new SelectList(
            await _db.Courses.Include(c => c.Branch)
                .Where(c => !assignedCourseIds.Contains(c.Id))
                .Select(c => new { c.Id, Display = c.Name + " (" + c.Branch.Name + ")" })
                .ToListAsync(),
            "Id", "Display");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignCourse(int id, int courseId)
    {
        var exists = await _db.FacultyCourseAssignments
            .AnyAsync(fca => fca.FacultyProfileId == id && fca.CourseId == courseId);

        if (!exists)
        {
            _db.FacultyCourseAssignments.Add(new FacultyCourseAssignment
            {
                FacultyProfileId = id,
                CourseId = courseId
            });
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCourseAssignment(int id, int assignmentId)
    {
        var assignment = await _db.FacultyCourseAssignments.FindAsync(assignmentId);
        if (assignment == null) return NotFound();

        _db.FacultyCourseAssignments.Remove(assignment);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var faculty = await _db.FacultyProfiles
            .Include(f => f.CourseAssignments).ThenInclude(ca => ca.Course)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (faculty == null) return NotFound();
        return View(faculty);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var faculty = await _db.FacultyProfiles.FindAsync(id);
        if (faculty == null) return NotFound();

        _db.FacultyProfiles.Remove(faculty);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}