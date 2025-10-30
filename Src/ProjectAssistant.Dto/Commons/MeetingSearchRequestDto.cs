using ProjectAssistant.Share.Enums;

namespace ProjectAssistant.Dto.Commons;

/// <summary>
/// 專案搜尋請求參數
/// </summary>
public class MeetingSearchRequestDto : SearchRequestBaseDto
{
    /// <summary>
    /// 專案代碼
    /// </summary>
    public int? ProjectId { get; set; }

}
