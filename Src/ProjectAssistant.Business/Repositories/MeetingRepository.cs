using Microsoft.EntityFrameworkCore;
using ProjectAssistant.Dto.Commons;
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

    public async Task<PagedResult<Meeting>> GetPagedAsync(
        MeetingSearchRequestDto request,
        bool includeRelatedData = false)
    {
        var query = context.Meeting.AsNoTracking().AsQueryable();

        #region 建立過濾條件
        Expression<Func<Meeting, bool>>? predicate = null;

        if (request.ProjectId.HasValue)
        {
            predicate = p => p.ProjectId == request.ProjectId.Value;
        }

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            predicate = p => p.Name.Contains(request.Keyword) ||
                            (p.Description != null && p.Description.Contains(request.Keyword));
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
                .Include(p => p.Project)
                ;
        }

        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        PagedResult<Meeting> pagedResult = new()
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
        Meeting.Project = null; // 避免新增時一併新增 Project 資料

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
        Meeting.Project = null; // 避免更新關聯資料

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
