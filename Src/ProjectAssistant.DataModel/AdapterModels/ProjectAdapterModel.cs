using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectAssistant.AdapterModels;

public class ProjectAdapterModel
{
    public ProjectAdapterModel()
    {
    }
    public int Id { get; set; }
    [Required(ErrorMessage = "專案名稱 不可為空白")]
    public string Name { get; set; }
    [StringLength(2000, ErrorMessage = "描述長度不可超過 2000 字元")]
    public string? Description { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public StatusEnum Status { get; set; }
    public PriorityEnum Priority { get; set; }
    public int CompletionPercentage { get; set; } // 0-100
    public string Owner { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<MyTaskAdapterModel> Task { get; set; } = new List<MyTaskAdapterModel>();
    public GanttChartAdapterModel GanttChart { get; set; }
    public List<MeetingAdapterModel> Meeting { get; set; } = new List<MeetingAdapterModel>();
}
