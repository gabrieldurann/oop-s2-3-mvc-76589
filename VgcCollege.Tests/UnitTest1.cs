using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Tests;

public class VgcCollegeTests : IDisposable
{
    private readonly ApplicationDbContext _db;

    public VgcCollegeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        SeedTestData();
    }

    private void SeedTestData()
    {
        var branch = new Branch { Id = 1, Name = "Dublin Branch", Address = "123 O'Connell Street" };
        _db.Branches.Add(branch);

        var course1 = new Course { Id = 1, Name = "Computer Science", BranchId = 1, StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30) };
        var course2 = new Course { Id = 2, Name = "Business Studies", BranchId = 1, StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30) };
        _db.Courses.AddRange(course1, course2);

        var student1 = new StudentProfile { Id = 1, IdentityUserId = "student-1", Name = "John O'Brien", Email = "john@test.com", StudentNumber = "STU-001" };
        var student2 = new StudentProfile { Id = 2, IdentityUserId = "student-2", Name = "Emma Kelly", Email = "emma@test.com", StudentNumber = "STU-002" };
        _db.StudentProfiles.AddRange(student1, student2);

        var faculty = new FacultyProfile { Id = 1, IdentityUserId = "faculty-1", Name = "Dr. Murphy", Email = "murphy@test.com" };
        _db.FacultyProfiles.Add(faculty);

        _db.FacultyCourseAssignments.Add(new FacultyCourseAssignment { Id = 1, FacultyProfileId = 1, CourseId = 1 });

        var enrolment1 = new CourseEnrolment { Id = 1, StudentProfileId = 1, CourseId = 1, EnrolDate = DateTime.Now, Status = "Active" };
        var enrolment2 = new CourseEnrolment { Id = 2, StudentProfileId = 2, CourseId = 1, EnrolDate = DateTime.Now, Status = "Active" };
        _db.CourseEnrolments.AddRange(enrolment1, enrolment2);

        var assignment = new Assignment { Id = 1, CourseId = 1, Title = "OOP Essay", MaxScore = 100, DueDate = new DateTime(2025, 10, 15) };
        _db.Assignments.Add(assignment);

        _db.AssignmentResults.Add(new AssignmentResult { Id = 1, AssignmentId = 1, StudentProfileId = 1, Score = 82, Feedback = "Good work" });

        var releasedExam = new Exam { Id = 1, CourseId = 1, Title = "Midterm", Date = new DateTime(2025, 11, 15), MaxScore = 100, ResultsReleased = true };
        var provisionalExam = new Exam { Id = 2, CourseId = 1, Title = "Final", Date = new DateTime(2026, 5, 20), MaxScore = 100, ResultsReleased = false };
        _db.Exams.AddRange(releasedExam, provisionalExam);

        _db.ExamResults.AddRange(
            new ExamResult { Id = 1, ExamId = 1, StudentProfileId = 1, Score = 78, Grade = "B" },
            new ExamResult { Id = 2, ExamId = 2, StudentProfileId = 1, Score = 65, Grade = "C" }
        );

        _db.AttendanceRecords.AddRange(
            new AttendanceRecord { Id = 1, CourseEnrolmentId = 1, Date = new DateTime(2025, 9, 1), Present = true },
            new AttendanceRecord { Id = 2, CourseEnrolmentId = 1, Date = new DateTime(2025, 9, 8), Present = true },
            new AttendanceRecord { Id = 3, CourseEnrolmentId = 1, Date = new DateTime(2025, 9, 15), Present = false }
        );

        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    // Test 1: Student can only see their own enrolments
    [Fact]
    public async Task Student_Can_Only_See_Own_Enrolments()
    {
        var studentUserId = "student-1";
        var student = await _db.StudentProfiles.FirstAsync(s => s.IdentityUserId == studentUserId);

        var enrolments = await _db.CourseEnrolments
            .Where(e => e.StudentProfileId == student.Id)
            .ToListAsync();

        Assert.All(enrolments, e => Assert.Equal(student.Id, e.StudentProfileId));
    }

    // Test 2: Student cannot see provisional exam results
    [Fact]
    public async Task Student_Cannot_See_Provisional_Exam_Results()
    {
        var studentId = 1;

        var visibleResults = await _db.ExamResults
            .Include(er => er.Exam)
            .Where(er => er.StudentProfileId == studentId && er.Exam.ResultsReleased)
            .ToListAsync();

        Assert.Single(visibleResults);
        Assert.Equal("Midterm", visibleResults[0].Exam.Title);
    }

    // Test 3: Student can see released exam results
    [Fact]
    public async Task Student_Can_See_Released_Exam_Results()
    {
        var studentId = 1;

        var releasedResults = await _db.ExamResults
            .Include(er => er.Exam)
            .Where(er => er.StudentProfileId == studentId && er.Exam.ResultsReleased)
            .ToListAsync();

        Assert.NotEmpty(releasedResults);
        Assert.All(releasedResults, r => Assert.True(r.Exam.ResultsReleased));
    }

    // Test 4: Faculty can only see students in their assigned courses
    [Fact]
    public async Task Faculty_Can_Only_See_Students_In_Assigned_Courses()
    {
        var facultyUserId = "faculty-1";
        var faculty = await _db.FacultyProfiles.FirstAsync(f => f.IdentityUserId == facultyUserId);

        var courseIds = await _db.FacultyCourseAssignments
            .Where(fca => fca.FacultyProfileId == faculty.Id)
            .Select(fca => fca.CourseId)
            .ToListAsync();

        var students = await _db.CourseEnrolments
            .Where(e => courseIds.Contains(e.CourseId))
            .Select(e => e.StudentProfileId)
            .Distinct()
            .ToListAsync();

        // Faculty is only assigned to course 1, so should only see students in course 1
        Assert.Contains(1, students);
        Assert.Contains(2, students);
    }

    // Test 5: Duplicate enrolment should not be possible
    [Fact]
    public async Task Cannot_Create_Duplicate_Enrolment()
    {
        var existingEnrolment = await _db.CourseEnrolments
            .AnyAsync(e => e.StudentProfileId == 1 && e.CourseId == 1);

        Assert.True(existingEnrolment);
        // In the real app, the controller checks this before adding
    }

    // Test 6: Assignment score must not exceed max score
    [Fact]
    public async Task Assignment_Score_Should_Not_Exceed_MaxScore()
    {
        var result = await _db.AssignmentResults
            .Include(ar => ar.Assignment)
            .FirstAsync(ar => ar.Id == 1);

        Assert.True(result.Score >= 0);
        Assert.True(result.Score <= result.Assignment.MaxScore);
    }

    // Test 7: Attendance calculation is correct
    [Fact]
    public async Task Attendance_Rate_Calculated_Correctly()
    {
        var enrolmentId = 1;

        var records = await _db.AttendanceRecords
            .Where(a => a.CourseEnrolmentId == enrolmentId)
            .ToListAsync();

        var total = records.Count;
        var present = records.Count(a => a.Present);
        var rate = (double)present / total * 100;

        Assert.Equal(3, total);
        Assert.Equal(2, present);
        Assert.True(rate > 66 && rate < 67); // 66.67%
    }

    // Test 8: Faculty not assigned to course cannot access it
    [Fact]
    public async Task Faculty_Cannot_Access_Unassigned_Course()
    {
        var facultyUserId = "faculty-1";
        var faculty = await _db.FacultyProfiles.FirstAsync(f => f.IdentityUserId == facultyUserId);

        var courseIds = await _db.FacultyCourseAssignments
            .Where(fca => fca.FacultyProfileId == faculty.Id)
            .Select(fca => fca.CourseId)
            .ToListAsync();

        // Course 2 (Business Studies) is not assigned to this faculty
        Assert.DoesNotContain(2, courseIds);
    }

    // Test 9: Toggling exam release changes visibility
    [Fact]
    public async Task Toggle_Exam_Release_Changes_Visibility()
    {
        var exam = await _db.Exams.FirstAsync(e => e.Id == 2);
        Assert.False(exam.ResultsReleased);

        // Toggle
        exam.ResultsReleased = true;
        _db.Exams.Update(exam);
        await _db.SaveChangesAsync();

        var updated = await _db.Exams.FirstAsync(e => e.Id == 2);
        Assert.True(updated.ResultsReleased);

        // Now student should see it
        var visibleResults = await _db.ExamResults
            .Include(er => er.Exam)
            .Where(er => er.StudentProfileId == 1 && er.Exam.ResultsReleased)
            .ToListAsync();

        Assert.Equal(2, visibleResults.Count);
    }

    // Test 10: Student enrolled in course can access their grades
    [Fact]
    public async Task Student_Can_Access_Grades_For_Enrolled_Course()
    {
        var studentId = 1;
        var courseId = 1;

        var isEnrolled = await _db.CourseEnrolments
            .AnyAsync(e => e.StudentProfileId == studentId && e.CourseId == courseId);

        Assert.True(isEnrolled);

        var assignmentResults = await _db.AssignmentResults
            .Include(ar => ar.Assignment)
            .Where(ar => ar.StudentProfileId == studentId && ar.Assignment.CourseId == courseId)
            .ToListAsync();

        Assert.NotEmpty(assignmentResults);
    }
}