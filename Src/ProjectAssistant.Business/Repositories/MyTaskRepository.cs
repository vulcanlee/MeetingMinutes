using Microsoft.EntityFrameworkCore;
using ProjectAssistant.Business.Helpers.Searchs;
using ProjectAssistant.Dto.Commons;
using ProjectAssistant.EntityModel;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;
using System.Linq.Expressions;

namespace ProjectAssistant.Business.Repositories;

public class MyTaskRepository
{
    private readonly BackendDBContext context;

    public MyTaskRepository(BackendDBContext context)
    {
        this.context = context;
    }

    #region 查詢方法

    /// <summary>
    /// 取得所有工作(包含相關資料)
    /// </summary>
    public async Task<List<MyTask>> GetAllAsync(bool includeRelatedData = false)
    {
        var query = context.MyTask.AsNoTracking().AsQueryable();

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
    public async Task<MyTask?> GetByIdAsync(int id, bool includeRelatedData = false)
    {
        var query = context.MyTask.AsNoTracking().AsQueryable();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Project);
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// 分頁查詢工作
    /// </summary>
    public async Task<(List<MyTask> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<MyTask, bool>>? predicate = null,
        bool includeRelatedData = false)
    {
        var query = context.MyTask.AsNoTracking().AsQueryable();

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
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<PagedResult<MyTask>> GetPagedAsync(
        MyTaskSearchRequestDto request,
        bool includeRelatedData = false)
    {
        var query = context.MyTask.AsNoTracking().AsQueryable();

        #region 建立過濾條件
        Expression<Func<MyTask, bool>>? predicate = null;

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            predicate = p => p.Name.Contains(request.Keyword) ||
                            (p.Description != null && p.Description.Contains(request.Keyword));
        }

        if (request.ProjectId.HasValue)
        {
            predicate = p => p.ProjectId == request.ProjectId.Value;
        }

        if (request.Status.HasValue)
        {
            var statusPredicate = (Expression<Func<MyTask, bool>>)(p => p.Status == request.Status.Value);
            predicate = predicate == null ? statusPredicate : CombinedSearchHelper.MyTaskCombinePredicates(predicate, statusPredicate);
        }

        if (request.Priority.HasValue)
        {
            var priorityPredicate = (Expression<Func<MyTask, bool>>)(p => p.Priority == request.Priority.Value);
            predicate = predicate == null ? priorityPredicate : CombinedSearchHelper.MyTaskCombinePredicates(predicate, priorityPredicate);
        }

        if (request.StartDateFrom.HasValue)
        {
            var datePredicate = (Expression<Func<MyTask, bool>>)(p => p.StartDate >= request.StartDateFrom.Value);
            predicate = predicate == null ? datePredicate : CombinedSearchHelper.MyTaskCombinePredicates(predicate, datePredicate);
        }

        if (request.StartDateTo.HasValue)
        {
            var datePredicate = (Expression<Func<MyTask, bool>>)(p => p.StartDate <= request.StartDateTo.Value);
            predicate = predicate == null ? datePredicate : CombinedSearchHelper.MyTaskCombinePredicates(predicate, datePredicate);
        }

        if (request.CompletionPercentageMin.HasValue)
        {
            var completionPredicate = (Expression<Func<MyTask, bool>>)(p => p.CompletionPercentage >= request.CompletionPercentageMin.Value);
            predicate = predicate == null ? completionPredicate : CombinedSearchHelper.MyTaskCombinePredicates(predicate, completionPredicate);
        }

        if (request.CompletionPercentageMax.HasValue)
        {
            var completionPredicate = (Expression<Func<MyTask, bool>>)(p => p.CompletionPercentage <= request.CompletionPercentageMax.Value);
            predicate = predicate == null ? completionPredicate : CombinedSearchHelper.MyTaskCombinePredicates(predicate, completionPredicate);
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
                "startdate" => request.SortDescending
                    ? query.OrderByDescending(p => p.StartDate)
                    : query.OrderBy(p => p.StartDate),
                "enddate" => request.SortDescending
                    ? query.OrderByDescending(p => p.EndDate)
                    : query.OrderBy(p => p.EndDate),
                "status" => request.SortDescending
                    ? query.OrderByDescending(p => p.Status)
                    : query.OrderBy(p => p.Status),
                "priority" => request.SortDescending
                    ? query.OrderByDescending(p => p.Priority)
                    : query.OrderBy(p => p.Priority),
                "completionpercentage" => request.SortDescending
                    ? query.OrderByDescending(p => p.CompletionPercentage)
                    : query.OrderBy(p => p.CompletionPercentage),
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

        PagedResult<MyTask> pagedResult = new()
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
        var query = context.MyTask.Where(p => p.Name == name);

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
    public async Task<MyTask> AddAsync(MyTask MyTask)
    {
        MyTask.CreatedAt = DateTime.Now;
        MyTask.UpdatedAt = DateTime.Now;

        await context.MyTask.AddAsync(MyTask);
        await context.SaveChangesAsync();

        return MyTask;
    }

    #endregion

    #region 更新方法

    /// <summary>
    /// 更新工作
    /// </summary>
    public async Task<bool> UpdateAsync(MyTask MyTask)
    {
        var existingMyTask = await context.MyTask.FindAsync(MyTask.Id);
        if (existingMyTask == null)
        {
            return false;
        }

        MyTask.UpdatedAt = DateTime.Now;
        MyTask.CreatedAt = existingMyTask.CreatedAt; // 保留原建立時間

        context.Entry(existingMyTask).CurrentValues.SetValues(MyTask);
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
        var MyTask = await context.MyTask.FindAsync(id);
        if (MyTask == null)
        {
            return false;
        }

        context.MyTask.Remove(MyTask);
        await context.SaveChangesAsync();

        return true;
    }

    #endregion
}
