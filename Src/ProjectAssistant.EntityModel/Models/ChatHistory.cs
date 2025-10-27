using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectAssistant.EntityModel.Models;

public class ChatHistory
{
    public ChatHistory()
    {
    }
    public int Id { get; set; }
    [Required(ErrorMessage = "聊天主題 不可為空白")]
    public string Name { get; set; } = String.Empty;
    public string? Content { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }


    public int MeetingId { get; set; }
    public Meeting? Meeting { get; set; }
}
