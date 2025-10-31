using Microsoft.EntityFrameworkCore;
using ProjectAssistant.Dto.Commons;
using ProjectAssistant.EntityModel;
using ProjectAssistant.EntityModel.Models;
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
    /// 根據 ID 取得甘特圖
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
    /// 分頁查詢甘特圖
    /// </summary>
    public async Task<PagedResult<GanttChart>> GetPagedAsync(
        GanttChartSearchRequestDto request,
        bool includeRelatedData = false)
    {
        var query = context.GanttChart.AsNoTracking().AsQueryable();

        #region 建立過濾條件
        Expression<Func<GanttChart, bool>>? predicate = null;

        if (request.ProjectId.HasValue)
        {
            predicate = p => p.ProjectId == request.ProjectId.Value;
        }

        #endregion 

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

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

        PagedResult<GanttChart> pagedResult = new()
        {
            Items = items,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return pagedResult;
    }

    #endregion

    #region 新增方法

    /// <summary>
    /// 新增甘特圖
    /// </summary>
    public async Task<GanttChart> AddAsync(GanttChart GanttChart)
    {
        GanttChart.CreatedAt = DateTime.Now;
        GanttChart.UpdatedAt = DateTime.Now;
        GanttChart.Project = null; // 避免更新關聯資料

        await context.GanttChart.AddAsync(GanttChart);
        await context.SaveChangesAsync();

        return GanttChart;
    }

    #endregion

    #region 更新方法

    /// <summary>
    /// 更新甘特圖
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
        GanttChart.Project = null; // 避免更新關聯資料

        context.Entry(existingGanttChart).CurrentValues.SetValues(GanttChart);
        await context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region 刪除方法

    /// <summary>
    /// 刪除甘特圖
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
