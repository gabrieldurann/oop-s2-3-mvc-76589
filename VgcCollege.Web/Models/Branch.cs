using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Web.Models;

public class Branch
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}