using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Web.Models;

public class CourseEnrolment
{
    public int Id { get; set; }

    public int StudentProfileId { get; set; }
    public StudentProfile StudentProfile { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [DataType(DataType.Date)]
    public DateTime EnrolDate { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active"; // Active, Withdrawn, Completed
}