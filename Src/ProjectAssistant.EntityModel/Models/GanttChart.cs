using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectAssistant.EntityModel.Models;

public class GanttChart
{
    public GanttChart()
    {
    }
    public int Id { get; set; }
    public string? Content { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }


    public int ProjectId { get; set; }
    public Project? Project { get; set; }
}
