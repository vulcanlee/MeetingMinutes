using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectAssistant.Dto.Models;

public class RecordedMediaFileDto
{
    public RecordedMediaFileDto()
    {
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "名稱 不可為空白")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = String.Empty;

    [JsonPropertyName("saveFileName")]
    public string SaveFileName { get; set; } = String.Empty;

    [JsonPropertyName("status")]
    public ConvertStatusEnum Status { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("meetingId")]
    public int MeetingId { get; set; }

    [JsonPropertyName("meeting")]
    public MeetingDto? Meeting { get; set; }
}
