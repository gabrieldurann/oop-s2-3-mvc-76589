using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminBranchesController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminBranchesController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var branches = await _db.Branches.ToListAsync();
        return View(branches);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Branch branch)
    {
        if (!ModelState.IsValid) return View(branch);

        _db.Branches.Add(branch);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var branch = await _db.Branches.FindAsync(id);
        if (branch == null) return NotFound();
        return View(branch);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Branch branch)
    {
        if (id != branch.Id) return NotFound();
        if (!ModelState.IsValid) return View(branch);

        _db.Branches.Update(branch);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var branch = await _db.Branches.FindAsync(id);
        if (branch == null) return NotFound();
        return View(branch);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var branch = await _db.Branches.FindAsync(id);
        if (branch == null) return NotFound();

        _db.Branches.Remove(branch);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}