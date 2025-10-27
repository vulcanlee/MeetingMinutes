using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProjectAssistant.Business.Helpers;
using ProjectAssistant.Business.Repositories;
using ProjectAssistant.Dto.Commons;
using ProjectAssistant.Dto.Models;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;
using System.Linq.Expressions;

namespace ProjectAssistant.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecordedMediaFileController : ControllerBase
{
    private readonly ILogger<RecordedMediaFileController> logger;
    private readonly RecordedMediaFileRepository RecordedMediaFileRepository;
    private readonly IMapper mapper;

    public RecordedMediaFileController(ILogger<RecordedMediaFileController> logger,
        RecordedMediaFileRepository RecordedMediaFileRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.RecordedMediaFileRepository = RecordedMediaFileRepository;
        this.mapper = mapper;
    }

    #region 查詢 API

    /// <summary>
    /// 取得所有專案
    /// </summary>
    /// <param name="includeRelatedData">是否包含關聯資料</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<ApiResult<List<RecordedMediaFileDto>>>> GetAll([FromQuery] bool includeRelatedData = false)
    {
        try
        {
            var RecordedMediaFiles = await RecordedMediaFileRepository.GetAllAsync(includeRelatedData);
            var RecordedMediaFileDtos = mapper.Map<List<RecordedMediaFileDto>>(RecordedMediaFiles);
            return Ok(ApiResult<List<RecordedMediaFileDto>>.SuccessResult(RecordedMediaFileDtos, "取得所有專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得所有專案時發生錯誤");
            return StatusCode(500, ApiResult<List<RecordedMediaFileDto>>.ServerErrorResult("取得所有專案時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 根據 ID 取得專案
    /// </summary>
    /// <param name="id">專案 ID</param>
    /// <param name="includeRelatedData">是否包含關聯資料</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResult<RecordedMediaFileDto>>> GetById(int id, [FromQuery] bool includeRelatedData = false)
    {
        try
        {
            var RecordedMediaFile = await RecordedMediaFileRepository.GetByIdAsync(id, includeRelatedData);

            if (RecordedMediaFile == null)
            {
                return NotFound(ApiResult<RecordedMediaFileDto>.NotFoundResult($"找不到 ID 為 {id} 的專案"));
            }

            var RecordedMediaFileDto = mapper.Map<RecordedMediaFileDto>(RecordedMediaFile);
            return Ok(ApiResult<RecordedMediaFileDto>.SuccessResult(RecordedMediaFileDto, "取得專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得專案 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult<RecordedMediaFileDto>.ServerErrorResult("取得專案時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 分頁查詢專案(支援排序、過濾)
    /// </summary>
    /// <param name="request">查詢請求參數</param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResult<PagedResult<RecordedMediaFileDto>>>> Search([FromBody] CommonSearchRequest request)
    {
        try
        {
            // 建立過濾條件
            Expression<Func<RecordedMediaFile, bool>>? predicate = null;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                predicate = p => p.Name.Contains(request.Keyword) ;
            }

            // 執行分頁查詢
            var (items, totalCount) = await RecordedMediaFileRepository.GetPagedAsync(
                request.PageIndex,
                request.PageSize,
                predicate,
                request.IncludeRelatedData
            );

            // 排序
            items = ApplySorting(items, request.SortBy, request.SortDescending);

            // 轉換為 DTO
            var RecordedMediaFileDtos = mapper.Map<List<RecordedMediaFileDto>>(items);

            var result = new PagedResult<RecordedMediaFileDto>
            {
                Items = RecordedMediaFileDtos,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return Ok(ApiResult<PagedResult<RecordedMediaFileDto>>.SuccessResult(result, "搜尋專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜尋專案時發生錯誤");
            return StatusCode(500, ApiResult<PagedResult<RecordedMediaFileDto>>.ServerErrorResult("搜尋專案時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 新增 API

    /// <summary>
    /// 新增專案
    /// </summary>
    /// <param name="RecordedMediaFileDto">專案資料</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<ApiResult<RecordedMediaFileDto>>> Create([FromBody] RecordedMediaFileDto RecordedMediaFileDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<RecordedMediaFileDto>.ValidationError(errors));
            }

            // 檢查專案名稱是否重複
            if (await RecordedMediaFileRepository.ExistsByNameAsync(RecordedMediaFileDto.Name))
            {
                return Conflict(ApiResult<RecordedMediaFileDto>.ConflictResult($"專案名稱 '{RecordedMediaFileDto.Name}' 已存在"));
            }

            // DTO 轉 Entity
            var RecordedMediaFile = mapper.Map<RecordedMediaFile>(RecordedMediaFileDto);
            var createdRecordedMediaFile = await RecordedMediaFileRepository.AddAsync(RecordedMediaFile);

            // Entity 轉 DTO
            var createdRecordedMediaFileDto = mapper.Map<RecordedMediaFileDto>(createdRecordedMediaFile);
            return Ok(ApiResult<RecordedMediaFileDto>.SuccessResult(createdRecordedMediaFileDto, "新增專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "新增專案時發生錯誤");
            return StatusCode(500, ApiResult<RecordedMediaFileDto>.ServerErrorResult("新增專案時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 更新 API

    /// <summary>
    /// 更新專案
    /// </summary>
    /// <param name="id">專案 ID</param>
    /// <param name="RecordedMediaFileDto">專案資料</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResult>> Update(int id, [FromBody] RecordedMediaFileDto RecordedMediaFileDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult.ValidationError(errors));
            }

            if (id != RecordedMediaFileDto.Id)
            {
                return BadRequest(ApiResult.ValidationError("路由 ID 與專案 ID 不符"));
            }

            // 檢查專案名稱是否與其他專案重複
            if (await RecordedMediaFileRepository.ExistsByNameAsync(RecordedMediaFileDto.Name, id))
            {
                return Conflict(ApiResult.ConflictResult($"專案名稱 '{RecordedMediaFileDto.Name}' 已被其他專案使用"));
            }

            // DTO 轉 Entity
            var RecordedMediaFile = mapper.Map<RecordedMediaFile>(RecordedMediaFileDto);
            var success = await RecordedMediaFileRepository.UpdateAsync(RecordedMediaFile);

            if (!success)
            {
                return NotFound(ApiResult.NotFoundResult($"找不到 ID 為 {id} 的專案"));
            }

            return Ok(ApiResult.SuccessResult("更新專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新專案 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult.ServerErrorResult("更新專案時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 刪除 API

    /// <summary>
    /// 刪除專案
    /// </summary>
    /// <param name="id">專案 ID</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResult>> Delete(int id)
    {
        try
        {
            var success = await RecordedMediaFileRepository.DeleteAsync(id);

            if (!success)
            {
                return NotFound(ApiResult.NotFoundResult($"找不到 ID 為 {id} 的專案"));
            }

            return Ok(ApiResult.SuccessResult("刪除專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刪除專案 ID {Id} 時發生錯誤", id);

            // 檢查是否為外鍵約束錯誤
            if (ex.InnerException?.Message.Contains("DELETE statement conflicted") == true)
            {
                return BadRequest(ApiResult.FailureResult("無法刪除此專案,因為有相關的子資料(任務、會議等)存在"));
            }

            return StatusCode(500, ApiResult.ServerErrorResult("刪除專案時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 輔助方法

    /// <summary>
    /// 組合 Expression 條件 (AND)
    /// </summary>
    private Expression<Func<RecordedMediaFile, bool>> CombinePredicates(
        Expression<Func<RecordedMediaFile, bool>> first,
        Expression<Func<RecordedMediaFile, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(RecordedMediaFile), "p");

        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);

        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return Expression.Lambda<Func<RecordedMediaFile, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    /// <summary>
    /// 套用排序
    /// </summary>
    private List<RecordedMediaFile> ApplySorting(List<RecordedMediaFile> RecordedMediaFiles, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return RecordedMediaFiles;
        }

        return sortBy.ToLower() switch
        {
            "name" => descending
                ? RecordedMediaFiles.OrderByDescending(p => p.Name).ToList()
                : RecordedMediaFiles.OrderBy(p => p.Name).ToList(),
            "status" => descending
                ? RecordedMediaFiles.OrderByDescending(p => p.Status).ToList()
                : RecordedMediaFiles.OrderBy(p => p.Status).ToList(),
            "createdat" => descending
                ? RecordedMediaFiles.OrderByDescending(p => p.CreatedAt).ToList()
                : RecordedMediaFiles.OrderBy(p => p.CreatedAt).ToList(),
            _ => RecordedMediaFiles
        };
    }

    #endregion
}

