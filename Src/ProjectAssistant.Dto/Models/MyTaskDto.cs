using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectAssistant.Dto.Models;

public class MyTaskDto
{
    public MyTaskDto()
    {
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "工作名稱 不可為空白")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [StringLength(2000, ErrorMessage = "描述長度不可超過 2000 字元")]
    [JsonPropertyName("description")]
    public string? Description { get; set; } = String.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = String.Empty;

    [JsonPropertyName("status")]
    public StatusEnum Status { get; set; }

    [JsonPropertyName("priority")]
    public PriorityEnum Priority { get; set; }

    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; } // 0-100

    [JsonPropertyName("owner")]
    public string Owner { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("projectId")]
    public int ProjectId { get; set; }

    [JsonPropertyName("project")]
    public ProjectDto? Project { get; set; }
}
