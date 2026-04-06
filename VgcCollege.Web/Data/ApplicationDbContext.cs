using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<FacultyProfile> FacultyProfiles => Set<FacultyProfile>();
    public DbSet<FacultyCourseAssignment> FacultyCourseAssignments => Set<FacultyCourseAssignment>();
    public DbSet<CourseEnrolment> CourseEnrolments => Set<CourseEnrolment>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentResult> AssignmentResults => Set<AssignmentResult>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Ensure one faculty assignment per course per faculty member
        builder.Entity<FacultyCourseAssignment>()
            .HasIndex(fca => new { fca.FacultyProfileId, fca.CourseId })
            .IsUnique();

        // Ensure one enrolment per student per course
        builder.Entity<CourseEnrolment>()
            .HasIndex(ce => new { ce.StudentProfileId, ce.CourseId })
            .IsUnique();

        // Ensure one result per assignment per student
        builder.Entity<AssignmentResult>()
            .HasIndex(ar => new { ar.AssignmentId, ar.StudentProfileId })
            .IsUnique();

        // Ensure one result per exam per student
        builder.Entity<ExamResult>()
            .HasIndex(er => new { er.ExamId, er.StudentProfileId })
            .IsUnique();
    }
}