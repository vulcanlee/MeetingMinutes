using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectAssistant.Dto.Models;

public class MeetingDto
{
    public MeetingDto()
    {
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "會議主題名稱 不可為空白")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [StringLength(2000, ErrorMessage = "描述長度不可超過 2000 字元")]
    [JsonPropertyName("description")]
    public string? Description { get; set; } = String.Empty;

    [JsonPropertyName("participants")]
    public string? Participants { get; set; } = String.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("projectId")]
    public int ProjectId { get; set; }

    [JsonPropertyName("project")]
    public ProjectDto? Project { get; set; }

    public List<ChatHistoryDto> ChatHistory { get; set; } = new List<ChatHistoryDto>();
    public List<RecordedMediaFileDto> RecordedMediaFile { get; set; } = new List<RecordedMediaFileDto>();
}
