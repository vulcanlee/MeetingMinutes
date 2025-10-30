﻿using ProjectAssistant.Share.Enums;

namespace ProjectAssistant.Dto.Commons;

/// <summary>
/// 專案搜尋請求參數
/// </summary>
public class SearchRequestBaseDto
{
    /// <summary>
    /// 頁碼 (從 1 開始)
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每頁筆數
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 關鍵字搜尋 (名稱、描述)
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 排序欄位 (name, startdate, enddate, status, priority, completionpercentage, createdat)
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 是否降冪排序
    /// </summary>
    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// 是否包含關聯資料
    /// </summary>
    public bool IncludeRelatedData { get; set; } = false;
}
