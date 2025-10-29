using ProjectAssistant.Share.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectAssistant.AdapterModels;

public class SelectItemModel
{
    public SelectItemModel()
    {
    }
    public string Key { get; set; } = String.Empty;
    public string Value { get; set; } = String.Empty;
}
