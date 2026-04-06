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

        // Unique constraints
        builder.Entity<FacultyCourseAssignment>()
            .HasIndex(fca => new { fca.FacultyProfileId, fca.CourseId })
            .IsUnique();

        builder.Entity<CourseEnrolment>()
            .HasIndex(ce => new { ce.StudentProfileId, ce.CourseId })
            .IsUnique();

        builder.Entity<AssignmentResult>()
            .HasIndex(ar => new { ar.AssignmentId, ar.StudentProfileId })
            .IsUnique();

        builder.Entity<ExamResult>()
            .HasIndex(er => new { er.ExamId, er.StudentProfileId })
            .IsUnique();

        // Cascade deletes — faculty deletion removes their course assignments only
        builder.Entity<FacultyCourseAssignment>()
            .HasOne(fca => fca.FacultyProfile)
            .WithMany(f => f.CourseAssignments)
            .HasForeignKey(fca => fca.FacultyProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Student deletion removes their enrolments
        builder.Entity<CourseEnrolment>()
            .HasOne(ce => ce.StudentProfile)
            .WithMany(s => s.Enrolments)
            .HasForeignKey(ce => ce.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enrolment deletion removes attendance records
        builder.Entity<AttendanceRecord>()
            .HasOne(a => a.CourseEnrolment)
            .WithMany()
            .HasForeignKey(a => a.CourseEnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Student deletion removes their assignment results
        builder.Entity<AssignmentResult>()
            .HasOne(ar => ar.StudentProfile)
            .WithMany(s => s.AssignmentResults)
            .HasForeignKey(ar => ar.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Student deletion removes their exam results
        builder.Entity<ExamResult>()
            .HasOne(er => er.StudentProfile)
            .WithMany(s => s.ExamResults)
            .HasForeignKey(er => er.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Course deletion removes its assignments
        builder.Entity<Assignment>()
            .HasOne(a => a.Course)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Course deletion removes its exams
        builder.Entity<Exam>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Exams)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}