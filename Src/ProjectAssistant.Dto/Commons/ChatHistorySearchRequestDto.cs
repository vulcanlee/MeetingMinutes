using ProjectAssistant.Share.Enums;

namespace ProjectAssistant.Dto.Commons;

/// <summary>
/// 專案搜尋請求參數
/// </summary>
public class ChatHistorySearchRequestDto : SearchRequestBaseDto
{
    /// <summary>
    /// 專案代碼
    /// </summary>
    public int? MeetingId { get; set; }

}
