using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectAssistant.EntityModel.Models;

public class Meeting
{
    public Meeting()
    {
    }
    public int Id { get; set; }
    [Required(ErrorMessage = "會議主題名稱 不可為空白")]
    public string Name { get; set; } = String.Empty;
    [StringLength(2000, ErrorMessage = "描述長度不可超過 2000 字元")]
    public string? Description { get; set; } = String.Empty;
    public string? Participants { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }


    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public ICollection<ChatHistory> ChatHistory { get; set; } = new List<ChatHistory>();
    public ICollection<RecordedMediaFile> RecordedMediaFile { get; set; } = new List<RecordedMediaFile>();

}
