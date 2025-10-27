using Microsoft.EntityFrameworkCore;
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
    /// 取得所有工作(包含相關資料)
    /// </summary>
    public async Task<List<ChatHistory>> GetAllAsync(bool includeRelatedData = false)
    {
        var query = context.ChatHistory.AsNoTracking().AsQueryable();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Meeting);
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

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

    /// <summary>
    /// 根據條件查詢工作
    /// </summary>
    public async Task<List<ChatHistory>> GetByConditionAsync(
        Expression<Func<ChatHistory, bool>> predicate,
        bool includeRelatedData = false)
    {
        var query = context.ChatHistory.AsNoTracking().Where(predicate);

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Meeting);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// 分頁查詢工作
    /// </summary>
    public async Task<(List<ChatHistory> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<ChatHistory, bool>>? predicate = null,
        bool includeRelatedData = false)
    {
        var query = context.ChatHistory.AsNoTracking().AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Meeting);
        }

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
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
