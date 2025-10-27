using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectAssistant.Dto.Models;

public class GanttChartDto
{
    public GanttChartDto()
    {
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; } = String.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("projectId")]
    public int ProjectId { get; set; }

    [JsonPropertyName("project")]
    public ProjectDto? Project { get; set; }
}
