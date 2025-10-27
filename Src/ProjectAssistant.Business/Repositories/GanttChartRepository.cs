using Microsoft.EntityFrameworkCore;
using ProjectAssistant.EntityModel;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;
using System.Linq.Expressions;

namespace ProjectAssistant.Business.Repositories;

public class GanttChartRepository
{
    private readonly BackendDBContext context;

    public GanttChartRepository(BackendDBContext context)
    {
        this.context = context;
    }

    #region 查詢方法

    /// <summary>
    /// 取得所有工作(包含相關資料)
    /// </summary>
    public async Task<List<GanttChart>> GetAllAsync(bool includeRelatedData = false)
    {
        var query = context.GanttChart.AsNoTracking().AsQueryable();

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
    public async Task<GanttChart?> GetByIdAsync(int id, bool includeRelatedData = false)
    {
        var query = context.GanttChart.AsNoTracking().AsQueryable();

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
    public async Task<List<GanttChart>> GetByConditionAsync(
        Expression<Func<GanttChart, bool>> predicate,
        bool includeRelatedData = false)
    {
        var query = context.GanttChart.AsNoTracking().Where(predicate);

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
    public async Task<(List<GanttChart> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<GanttChart, bool>>? predicate = null,
        bool includeRelatedData = false)
    {
        var query = context.GanttChart.AsNoTracking().AsQueryable();

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

    #endregion

    #region 新增方法

    /// <summary>
    /// 新增工作
    /// </summary>
    public async Task<GanttChart> AddAsync(GanttChart GanttChart)
    {
        GanttChart.CreatedAt = DateTime.Now;
        GanttChart.UpdatedAt = DateTime.Now;

        await context.GanttChart.AddAsync(GanttChart);
        await context.SaveChangesAsync();

        return GanttChart;
    }

    #endregion

    #region 更新方法

    /// <summary>
    /// 更新工作
    /// </summary>
    public async Task<bool> UpdateAsync(GanttChart GanttChart)
    {
        var existingGanttChart = await context.GanttChart.FindAsync(GanttChart.Id);
        if (existingGanttChart == null)
        {
            return false;
        }

        GanttChart.UpdatedAt = DateTime.Now;
        GanttChart.CreatedAt = existingGanttChart.CreatedAt; // 保留原建立時間

        context.Entry(existingGanttChart).CurrentValues.SetValues(GanttChart);
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
        var GanttChart = await context.GanttChart.FindAsync(id);
        if (GanttChart == null)
        {
            return false;
        }

        context.GanttChart.Remove(GanttChart);
        await context.SaveChangesAsync();

        return true;
    }

    #endregion
}
