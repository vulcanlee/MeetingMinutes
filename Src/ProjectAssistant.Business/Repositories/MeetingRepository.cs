using Microsoft.EntityFrameworkCore;
using ProjectAssistant.EntityModel;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;
using System.Linq.Expressions;

namespace ProjectAssistant.Business.Repositories;

public class MeetingRepository
{
    private readonly BackendDBContext context;

    public MeetingRepository(BackendDBContext context)
    {
        this.context = context;
    }

    #region 查詢方法

    /// <summary>
    /// 取得所有工作(包含相關資料)
    /// </summary>
    public async Task<List<Meeting>> GetAllAsync(bool includeRelatedData = false)
    {
        var query = context.Meeting.AsNoTracking().AsQueryable();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Project);
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// 根據 ID 取得工作
    /// </summary>
    public async Task<Meeting?> GetByIdAsync(int id, bool includeRelatedData = false)
    {
        var query = context.Meeting.AsNoTracking().AsQueryable();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Project);
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// 根據條件查詢工作
    /// </summary>
    public async Task<List<Meeting>> GetByConditionAsync(
        Expression<Func<Meeting, bool>> predicate,
        bool includeRelatedData = false)
    {
        var query = context.Meeting.AsNoTracking().Where(predicate);

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Project);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// 分頁查詢工作
    /// </summary>
    public async Task<(List<Meeting> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<Meeting, bool>>? predicate = null,
        bool includeRelatedData = false)
    {
        var query = context.Meeting.AsNoTracking().AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Project);
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
        var query = context.Meeting.Where(p => p.Name == name);

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
    public async Task<Meeting> AddAsync(Meeting Meeting)
    {
        Meeting.CreatedAt = DateTime.Now;
        Meeting.UpdatedAt = DateTime.Now;

        await context.Meeting.AddAsync(Meeting);
        await context.SaveChangesAsync();

        return Meeting;
    }

    #endregion

    #region 更新方法

    /// <summary>
    /// 更新工作
    /// </summary>
    public async Task<bool> UpdateAsync(Meeting Meeting)
    {
        var existingMeeting = await context.Meeting.FindAsync(Meeting.Id);
        if (existingMeeting == null)
        {
            return false;
        }

        Meeting.UpdatedAt = DateTime.Now;
        Meeting.CreatedAt = existingMeeting.CreatedAt; // 保留原建立時間

        context.Entry(existingMeeting).CurrentValues.SetValues(Meeting);
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
        var Meeting = await context.Meeting.FindAsync(id);
        if (Meeting == null)
        {
            return false;
        }

        context.Meeting.Remove(Meeting);
        await context.SaveChangesAsync();

        return true;
    }

    #endregion
}
