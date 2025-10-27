using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectAssistant.Dto.Models;

public class ChatHistoryDto
{
    public ChatHistoryDto()
    {
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "聊天主題 不可為空白")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; } = String.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("meetingId")]
    public int MeetingId { get; set; }

    [JsonPropertyName("meeting")]
    public MeetingDto? Meeting { get; set; }
}
