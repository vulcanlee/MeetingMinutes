using Microsoft.EntityFrameworkCore;
using ProjectAssistant.Dto.Commons;
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
    /// 根據 ID 取得會議影音檔
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
    /// 分頁查詢會議影音檔
    /// </summary>
    public async Task<PagedResult<RecordedMediaFile>> GetPagedAsync(
        RecordedMediaFileSearchRequestDto request,
        bool includeRelatedData = false)
    {
        var query = context.RecordedMediaFile.AsNoTracking().AsQueryable();

        #region 建立過濾條件
        Expression<Func<RecordedMediaFile, bool>>? predicate = null;

        if (request.MeetingId.HasValue)
        {
            predicate = p => p.MeetingId == request.MeetingId.Value;
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
                .Include(p => p.Meeting)
                ;
        }

        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        PagedResult<RecordedMediaFile> pagedResult = new()
        {
            Items = items,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return pagedResult;
    }

    /// <summary>
    /// 檢查會議影音檔名稱是否存在
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
    /// 新增會議影音檔
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
    /// 更新會議影音檔
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
    /// 刪除會議影音檔
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
