using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Web.Models;

public class Assignment
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int MaxScore { get; set; }

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    public ICollection<AssignmentResult> Results { get; set; } = new List<AssignmentResult>();
}