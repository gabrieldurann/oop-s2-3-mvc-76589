using Microsoft.AspNetCore.Identity;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Create roles
        string[] roles = { "Admin", "Faculty", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Create users
        var adminUser = await CreateUserAsync(userManager, "admin@vgc.ie", "Admin123!", "Admin");
        var facultyUser = await CreateUserAsync(userManager, "faculty@vgc.ie", "Faculty123!", "Faculty");
        var student1User = await CreateUserAsync(userManager, "student1@vgc.ie", "Student123!", "Student");
        var student2User = await CreateUserAsync(userManager, "student2@vgc.ie", "Student123!", "Student");

        // Only seed data if branches don't exist yet
        if (db.Branches.Any()) return;

        // Branches
        var dublin = new Branch { Name = "Dublin Branch", Address = "123 O'Connell Street, Dublin 1" };
        var cork = new Branch { Name = "Cork Branch", Address = "45 Patrick Street, Cork" };
        var galway = new Branch { Name = "Galway Branch", Address = "78 Shop Street, Galway" };
        db.Branches.AddRange(dublin, cork, galway);
        await db.SaveChangesAsync();

        // Courses
        var csCourse = new Course
        {
            Name = "Computer Science", BranchId = dublin.Id,
            StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30)
        };
        var businessCourse = new Course
        {
            Name = "Business Studies", BranchId = dublin.Id,
            StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30)
        };
        var artsCourse = new Course
        {
            Name = "Liberal Arts", BranchId = cork.Id,
            StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30)
        };
        var dataCourse = new Course
        {
            Name = "Data Analytics", BranchId = galway.Id,
            StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30)
        };
        db.Courses.AddRange(csCourse, businessCourse, artsCourse, dataCourse);
        await db.SaveChangesAsync();

        // Faculty profile
        var facultyProfile = new FacultyProfile
        {
            IdentityUserId = facultyUser.Id,
            Name = "Dr. Sarah Murphy",
            Email = "faculty@vgc.ie",
            Phone = "087-1234567"
        };
        db.FacultyProfiles.Add(facultyProfile);
        await db.SaveChangesAsync();

        // Assign faculty to Computer Science and Business Studies
        db.FacultyCourseAssignments.AddRange(
            new FacultyCourseAssignment { FacultyProfileId = facultyProfile.Id, CourseId = csCourse.Id },
            new FacultyCourseAssignment { FacultyProfileId = facultyProfile.Id, CourseId = businessCourse.Id }
        );
        await db.SaveChangesAsync();

        // Student profiles
        var studentProfile1 = new StudentProfile
        {
            IdentityUserId = student1User.Id,
            Name = "John O'Brien",
            Email = "student1@vgc.ie",
            Phone = "085-1111111",
            Address = "10 Temple Bar, Dublin 2",
            StudentNumber = "STU-001"
        };
        var studentProfile2 = new StudentProfile
        {
            IdentityUserId = student2User.Id,
            Name = "Emma Kelly",
            Email = "student2@vgc.ie",
            Phone = "085-2222222",
            Address = "22 Grafton Street, Dublin 2",
            StudentNumber = "STU-002"
        };
        db.StudentProfiles.AddRange(studentProfile1, studentProfile2);
        await db.SaveChangesAsync();

        // Enrolments — both students in Computer Science, student1 also in Business
        var enrolment1 = new CourseEnrolment
        {
            StudentProfileId = studentProfile1.Id, CourseId = csCourse.Id,
            EnrolDate = new DateTime(2025, 9, 1), Status = "Active"
        };
        var enrolment2 = new CourseEnrolment
        {
            StudentProfileId = studentProfile2.Id, CourseId = csCourse.Id,
            EnrolDate = new DateTime(2025, 9, 1), Status = "Active"
        };
        var enrolment3 = new CourseEnrolment
        {
            StudentProfileId = studentProfile1.Id, CourseId = businessCourse.Id,
            EnrolDate = new DateTime(2025, 9, 5), Status = "Active"
        };
        db.CourseEnrolments.AddRange(enrolment1, enrolment2, enrolment3);
        await db.SaveChangesAsync();

        // Attendance records — a few weeks for CS enrolments
        for (int week = 1; week <= 4; week++)
        {
            db.AttendanceRecords.Add(new AttendanceRecord
            {
                CourseEnrolmentId = enrolment1.Id,
                Date = new DateTime(2025, 9, 1).AddDays((week - 1) * 7),
                Present = week != 3 // absent week 3
            });
            db.AttendanceRecords.Add(new AttendanceRecord
            {
                CourseEnrolmentId = enrolment2.Id,
                Date = new DateTime(2025, 9, 1).AddDays((week - 1) * 7),
                Present = true
            });
        }
        await db.SaveChangesAsync();

        // Assignments
        var assignment1 = new Assignment
        {
            CourseId = csCourse.Id, Title = "OOP Principles Essay",
            MaxScore = 100, DueDate = new DateTime(2025, 10, 15)
        };
        var assignment2 = new Assignment
        {
            CourseId = csCourse.Id, Title = "MVC Project",
            MaxScore = 100, DueDate = new DateTime(2025, 12, 1)
        };
        var assignment3 = new Assignment
        {
            CourseId = businessCourse.Id, Title = "Business Plan Draft",
            MaxScore = 50, DueDate = new DateTime(2025, 11, 1)
        };
        db.Assignments.AddRange(assignment1, assignment2, assignment3);
        await db.SaveChangesAsync();

        // Assignment results
        db.AssignmentResults.AddRange(
            new AssignmentResult { AssignmentId = assignment1.Id, StudentProfileId = studentProfile1.Id, Score = 82, Feedback = "Well structured argument" },
            new AssignmentResult { AssignmentId = assignment1.Id, StudentProfileId = studentProfile2.Id, Score = 91, Feedback = "Excellent work" },
            new AssignmentResult { AssignmentId = assignment2.Id, StudentProfileId = studentProfile1.Id, Score = 75, Feedback = "Good but needs more tests" },
            new AssignmentResult { AssignmentId = assignment3.Id, StudentProfileId = studentProfile1.Id, Score = 40, Feedback = "Solid draft" }
        );
        await db.SaveChangesAsync();

        // Exams — one released, one provisional
        var exam1 = new Exam
        {
            CourseId = csCourse.Id, Title = "Midterm Exam",
            Date = new DateTime(2025, 11, 15), MaxScore = 100,
            ResultsReleased = true
        };
        var exam2 = new Exam
        {
            CourseId = csCourse.Id, Title = "Final Exam",
            Date = new DateTime(2026, 5, 20), MaxScore = 100,
            ResultsReleased = false // Students cannot see this yet
        };
        db.Exams.AddRange(exam1, exam2);
        await db.SaveChangesAsync();

        // Exam results
        db.ExamResults.AddRange(
            new ExamResult { ExamId = exam1.Id, StudentProfileId = studentProfile1.Id, Score = 78, Grade = "B" },
            new ExamResult { ExamId = exam1.Id, StudentProfileId = studentProfile2.Id, Score = 88, Grade = "A" },
            new ExamResult { ExamId = exam2.Id, StudentProfileId = studentProfile1.Id, Score = 65, Grade = "C" },
            new ExamResult { ExamId = exam2.Id, StudentProfileId = studentProfile2.Id, Score = 72, Grade = "B" }
        );
        await db.SaveChangesAsync();
    }

    private static async Task<IdentityUser> CreateUserAsync(
        UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            await userManager.AddToRoleAsync(user, role);
        }
        return user;
    }
}