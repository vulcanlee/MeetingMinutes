using Microsoft.EntityFrameworkCore;
using ProjectAssistant.EntityModel;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;
using System.Linq.Expressions;

namespace ProjectAssistant.Business.Repositories;

public class ProjectRepository
{
    private readonly BackendDBContext context;

    public ProjectRepository(BackendDBContext context)
    {
        this.context = context;
    }

    #region 查詢方法

    /// <summary>
    /// 取得所有專案(包含相關資料)
    /// </summary>
    public async Task<List<Project>> GetAllAsync(bool includeRelatedData = false)
    {
        var query = context.Project.AsNoTracking().AsQueryable();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Task)
                .Include(p => p.GanttChart)
                .Include(p => p.Meeting);
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// 根據 ID 取得專案
    /// </summary>
    public async Task<Project?> GetByIdAsync(int id, bool includeRelatedData = false)
    {
        var query = context.Project.AsNoTracking().AsQueryable();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Task)
                .Include(p => p.GanttChart)
                .Include(p => p.Meeting)
                    .ThenInclude(m => m.ChatHistory)
                .Include(p => p.Meeting)
                    .ThenInclude(m => m.RecordedMediaFile);
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// 根據條件查詢專案
    /// </summary>
    public async Task<List<Project>> GetByConditionAsync(
        Expression<Func<Project, bool>> predicate,
        bool includeRelatedData = false)
    {
        var query = context.Project.AsNoTracking().Where(predicate);

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Task)
                .Include(p => p.GanttChart)
                .Include(p => p.Meeting);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// 分頁查詢專案
    /// </summary>
    public async Task<(List<Project> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<Project, bool>>? predicate = null,
        bool includeRelatedData = false)
    {
        var query = context.Project.AsNoTracking().AsQueryable();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Task)
                .Include(p => p.GanttChart)
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
    /// 檢查專案名稱是否存在
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = context.Project.Where(p => p.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    #endregion

    #region 新增方法

    /// <summary>
    /// 新增專案
    /// </summary>
    public async Task<Project> AddAsync(Project project)
    {
        project.CreatedAt = DateTime.Now;
        project.UpdatedAt = DateTime.Now;

        await context.Project.AddAsync(project);
        await context.SaveChangesAsync();

        return project;
    }

    /// <summary>
    /// 批次新增專案
    /// </summary>
    public async Task<int> AddRangeAsync(List<Project> projects)
    {
        var now = DateTime.Now;
        foreach (var project in projects)
        {
            project.CreatedAt = now;
            project.UpdatedAt = now;
        }

        await context.Project.AddRangeAsync(projects);
        return await context.SaveChangesAsync();
    }

    #endregion

    #region 更新方法

    /// <summary>
    /// 更新專案
    /// </summary>
    public async Task<bool> UpdateAsync(Project project)
    {
        var existingProject = await context.Project.FindAsync(project.Id);
        if (existingProject == null)
        {
            return false;
        }

        project.UpdatedAt = DateTime.Now;
        project.CreatedAt = existingProject.CreatedAt; // 保留原建立時間

        context.Entry(existingProject).CurrentValues.SetValues(project);
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 更新專案狀態
    /// </summary>
    public async Task<bool> UpdateStatusAsync(int id, StatusEnum status)
    {
        var project = await context.Project.FindAsync(id);
        if (project == null)
        {
            return false;
        }

        project.Status = status;
        project.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 更新專案完成百分比
    /// </summary>
    public async Task<bool> UpdateCompletionPercentageAsync(int id, int percentage)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "完成百分比必須介於 0 到 100 之間");
        }

        var project = await context.Project.FindAsync(id);
        if (project == null)
        {
            return false;
        }

        project.CompletionPercentage = percentage;
        project.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region 刪除方法

    /// <summary>
    /// 刪除專案
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var project = await context.Project.FindAsync(id);
        if (project == null)
        {
            return false;
        }

        context.Project.Remove(project);
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 批次刪除專案
    /// </summary>
    public async Task<int> DeleteRangeAsync(List<int> ids)
    {
        var projects = await context.Project
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        context.Project.RemoveRange(projects);
        return await context.SaveChangesAsync();
    }

    #endregion

    #region 統計方法

    /// <summary>
    /// 取得專案總數
    /// </summary>
    public async Task<int> GetCountAsync(Expression<Func<Project, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await context.Project.CountAsync();
        }

        return await context.Project.CountAsync(predicate);
    }

    #endregion
}
