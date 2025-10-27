using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace ProjectAssistant.EntityModel.Models;

public class RecordedMediaFile
{
    public RecordedMediaFile()
    {
    }
    public int Id { get; set; }
    [Required(ErrorMessage = "名稱 不可為空白")]
    public string Name { get; set; } = String.Empty;
    public string FileName { get; set; } = String.Empty;
    public string SaveFileName { get; set; } = String.Empty;
    public ConvertStatusEnum Status { get; set; }
    public string? Content { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }


    public int MeetingId { get; set; }
    public Meeting? Meeting { get; set; }
}
