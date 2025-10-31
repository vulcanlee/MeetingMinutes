using Microsoft.EntityFrameworkCore;
using ProjectAssistant.Dto.Commons;
using ProjectAssistant.EntityModel;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;
using System.Linq.Expressions;

namespace ProjectAssistant.Business.Repositories;

public class ChatHistoryRepository
{
    private readonly BackendDBContext context;

    public ChatHistoryRepository(BackendDBContext context)
    {
        this.context = context;
    }

    #region 查詢方法

    /// <summary>
    /// 根據 ID 取得工作
    /// </summary>
    public async Task<ChatHistory?> GetByIdAsync(int id, bool includeRelatedData = false)
    {
        var query = context.ChatHistory.AsNoTracking().AsQueryable();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Meeting);
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PagedResult<ChatHistory>> GetPagedAsync(
    ChatHistorySearchRequestDto request,
    bool includeRelatedData = false)
    {
        var query = context.ChatHistory.AsNoTracking().AsQueryable();

        #region 建立過濾條件
        Expression<Func<ChatHistory, bool>>? predicate = null;

        if (request.MeetingId.HasValue)
        {
            predicate = p => p.MeetingId == request.MeetingId.Value;
        }

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            predicate = p => p.Name.Contains(request.Keyword);
        }

        #endregion 

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        #region 根據 request.SortBy 及  request.Descending 進行排序
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            query = request.SortBy.ToLower() switch
            {
                "name" => request.SortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "createdat" => request.SortDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                _ => query
            };
        }
        #endregion

        var totalCount = await query.CountAsync();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Meeting)
                ;
        }

        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        PagedResult<ChatHistory> pagedResult = new()
        {
            Items = items,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return pagedResult;
    }

    /// <summary>
    /// 檢查工作名稱是否存在
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = context.ChatHistory.Where(p => p.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    #endregion

    #region 新增方法

    /// <summary>
    /// 新增工作
    /// </summary>
    public async Task<ChatHistory> AddAsync(ChatHistory ChatHistory)
    {
        ChatHistory.CreatedAt = DateTime.Now;
        ChatHistory.UpdatedAt = DateTime.Now;

        await context.ChatHistory.AddAsync(ChatHistory);
        await context.SaveChangesAsync();

        return ChatHistory;
    }

    #endregion

    #region 更新方法

    /// <summary>
    /// 更新工作
    /// </summary>
    public async Task<bool> UpdateAsync(ChatHistory ChatHistory)
    {
        var existingChatHistory = await context.ChatHistory.FindAsync(ChatHistory.Id);
        if (existingChatHistory == null)
        {
            return false;
        }

        ChatHistory.UpdatedAt = DateTime.Now;
        ChatHistory.CreatedAt = existingChatHistory.CreatedAt; // 保留原建立時間

        context.Entry(existingChatHistory).CurrentValues.SetValues(ChatHistory);
        await context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region 刪除方法

    /// <summary>
    /// 刪除工作
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var ChatHistory = await context.ChatHistory.FindAsync(id);
        if (ChatHistory == null)
        {
            return false;
        }

        context.ChatHistory.Remove(ChatHistory);
        await context.SaveChangesAsync();

        return true;
    }

    #endregion
}
