using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectAssistant.EntityModel.Models;

public class MyTask
{
    public MyTask()
    {
    }
    public int Id { get; set; }
    [Required(ErrorMessage = "工作名稱 不可為空白")]
    public string Name { get; set; } = String.Empty;
    [StringLength(2000, ErrorMessage = "描述長度不可超過 2000 字元")]
    public string? Description { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Category { get; set; } = String.Empty; 
    public StatusEnum Status { get; set; }
    public PriorityEnum Priority { get; set; }
    public int CompletionPercentage { get; set; } // 0-100
    public string Owner { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }


    public int ProjectId { get; set; }
    public Project? Project { get; set; }
}
