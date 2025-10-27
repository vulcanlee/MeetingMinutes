using Microsoft.EntityFrameworkCore;
using ProjectAssistant.EntityModel;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;
using System.Linq.Expressions;

namespace ProjectAssistant.Business.Repositories;

public class RecordedMediaFileRepository
{
    private readonly BackendDBContext context;

    public RecordedMediaFileRepository(BackendDBContext context)
    {
        this.context = context;
    }

    #region 查詢方法

    /// <summary>
    /// 取得所有工作(包含相關資料)
    /// </summary>
    public async Task<List<RecordedMediaFile>> GetAllAsync(bool includeRelatedData = false)
    {
        var query = context.RecordedMediaFile.AsNoTracking().AsQueryable();

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
    public async Task<RecordedMediaFile?> GetByIdAsync(int id, bool includeRelatedData = false)
    {
        var query = context.RecordedMediaFile.AsNoTracking().AsQueryable();

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
    public async Task<List<RecordedMediaFile>> GetByConditionAsync(
        Expression<Func<RecordedMediaFile, bool>> predicate,
        bool includeRelatedData = false)
    {
        var query = context.RecordedMediaFile.AsNoTracking().Where(predicate);

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
    public async Task<(List<RecordedMediaFile> Items, int TotalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<RecordedMediaFile, bool>>? predicate = null,
        bool includeRelatedData = false)
    {
        var query = context.RecordedMediaFile.AsNoTracking().AsQueryable();

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
        var query = context.RecordedMediaFile.Where(p => p.Name == name);

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
    public async Task<RecordedMediaFile> AddAsync(RecordedMediaFile RecordedMediaFile)
    {
        RecordedMediaFile.CreatedAt = DateTime.Now;
        RecordedMediaFile.UpdatedAt = DateTime.Now;

        await context.RecordedMediaFile.AddAsync(RecordedMediaFile);
        await context.SaveChangesAsync();

        return RecordedMediaFile;
    }

    #endregion

    #region 更新方法

    /// <summary>
    /// 更新工作
    /// </summary>
    public async Task<bool> UpdateAsync(RecordedMediaFile RecordedMediaFile)
    {
        var existingRecordedMediaFile = await context.RecordedMediaFile.FindAsync(RecordedMediaFile.Id);
        if (existingRecordedMediaFile == null)
        {
            return false;
        }

        RecordedMediaFile.UpdatedAt = DateTime.Now;
        RecordedMediaFile.CreatedAt = existingRecordedMediaFile.CreatedAt; // 保留原建立時間

        context.Entry(existingRecordedMediaFile).CurrentValues.SetValues(RecordedMediaFile);
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
        var RecordedMediaFile = await context.RecordedMediaFile.FindAsync(id);
        if (RecordedMediaFile == null)
        {
            return false;
        }

        context.RecordedMediaFile.Remove(RecordedMediaFile);
        await context.SaveChangesAsync();

        return true;
    }

    #endregion
}
