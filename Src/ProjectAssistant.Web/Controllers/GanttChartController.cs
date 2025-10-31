using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProjectAssistant.Business.Helpers;
using ProjectAssistant.Business.Repositories;
using ProjectAssistant.Dto.Commons;
using ProjectAssistant.Dto.Models;
using ProjectAssistant.EntityModel.Models;
using System.Linq.Expressions;

namespace ProjectAssistant.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GanttChartController : ControllerBase
{
    private readonly ILogger<GanttChartController> logger;
    private readonly GanttChartRepository GanttChartRepository;
    private readonly IMapper mapper;

    public GanttChartController(ILogger<GanttChartController> logger,
        GanttChartRepository GanttChartRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.GanttChartRepository = GanttChartRepository;
        this.mapper = mapper;
    }

    #region 查詢 API

    /// <summary>
    /// 根據 ID 取得會議聊天
    /// </summary>
    /// <param name="id">會議聊天 ID</param>
    /// <param name="includeRelatedData">是否包含關聯資料</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResult<GanttChartDto>>> GetById(int id, [FromQuery] bool includeRelatedData = false)
    {
        try
        {
            var GanttChart = await GanttChartRepository.GetByIdAsync(id, includeRelatedData);

            if (GanttChart == null)
            {
                return NotFound(ApiResult<GanttChartDto>.NotFoundResult($"找不到 ID 為 {id} 的會議聊天"));
            }

            var GanttChartDto = mapper.Map<GanttChartDto>(GanttChart);
            return Ok(ApiResult<GanttChartDto>.SuccessResult(GanttChartDto, "取得會議聊天成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得會議聊天 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult<GanttChartDto>.ServerErrorResult("取得會議聊天時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 分頁查詢甘特圖(支援排序、過濾)
    /// </summary>
    /// <param name="request">查詢請求參數</param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResult<PagedResult<GanttChartDto>>>> Search([FromBody] GanttChartSearchRequestDto request)
    {
        try
        {
            // 執行分頁查詢
            PagedResult<GanttChart> pagedResult = await GanttChartRepository.GetPagedAsync(request);
            var GanttChartDtos = mapper.Map<List<GanttChartDto>>(pagedResult.Items);

            var result = new PagedResult<GanttChartDto>
            {
                Items = GanttChartDtos,
                TotalCount = pagedResult.TotalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(pagedResult.TotalCount / (double)request.PageSize)
            };

            return Ok(ApiResult<PagedResult<GanttChartDto>>.SuccessResult(result, "搜尋甘特圖成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜尋甘特圖時發生錯誤");
            return StatusCode(500, ApiResult<PagedResult<GanttChartDto>>.ServerErrorResult("搜尋甘特圖時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 新增 API

    /// <summary>
    /// 新增會議聊天
    /// </summary>
    /// <param name="GanttChartDto">會議聊天資料</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<ApiResult<GanttChartDto>>> Create([FromBody] GanttChartDto GanttChartDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<GanttChartDto>.ValidationError(errors));
            }

            // DTO 轉 Entity
            var GanttChart = mapper.Map<GanttChart>(GanttChartDto);
            var createdGanttChart = await GanttChartRepository.AddAsync(GanttChart);

            // Entity 轉 DTO
            var createdGanttChartDto = mapper.Map<GanttChartDto>(createdGanttChart);
            return Ok(ApiResult<GanttChartDto>.SuccessResult(createdGanttChartDto, "新增會議聊天成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "新增會議聊天時發生錯誤");
            return StatusCode(500, ApiResult<GanttChartDto>.ServerErrorResult("新增會議聊天時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 更新 API

    /// <summary>
    /// 更新會議聊天
    /// </summary>
    /// <param name="id">會議聊天 ID</param>
    /// <param name="GanttChartDto">會議聊天資料</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResult>> Update(int id, [FromBody] GanttChartDto GanttChartDto)
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

            if (id != GanttChartDto.Id)
            {
                return BadRequest(ApiResult.ValidationError("路由 ID 與會議聊天 ID 不符"));
            }

            // DTO 轉 Entity
            var GanttChart = mapper.Map<GanttChart>(GanttChartDto);
            var success = await GanttChartRepository.UpdateAsync(GanttChart);

            if (!success)
            {
                return NotFound(ApiResult.NotFoundResult($"找不到 ID 為 {id} 的會議聊天"));
            }

            return Ok(ApiResult.SuccessResult("更新會議聊天成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新會議聊天 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult.ServerErrorResult("更新會議聊天時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 刪除 API

    /// <summary>
    /// 刪除會議聊天
    /// </summary>
    /// <param name="id">會議聊天 ID</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResult>> Delete(int id)
    {
        try
        {
            var success = await GanttChartRepository.DeleteAsync(id);

            if (!success)
            {
                return NotFound(ApiResult.NotFoundResult($"找不到 ID 為 {id} 的會議聊天"));
            }

            return Ok(ApiResult.SuccessResult("刪除會議聊天成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刪除會議聊天 ID {Id} 時發生錯誤", id);

            // 檢查是否為外鍵約束錯誤
            if (ex.InnerException?.Message.Contains("DELETE statement conflicted") == true)
            {
                return BadRequest(ApiResult.FailureResult("無法刪除此會議聊天,因為有相關的子資料(任務、會議等)存在"));
            }

            return StatusCode(500, ApiResult.ServerErrorResult("刪除會議聊天時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 輔助方法

    /// <summary>
    /// 組合 Expression 條件 (AND)
    /// </summary>
    private Expression<Func<GanttChart, bool>> CombinePredicates(
        Expression<Func<GanttChart, bool>> first,
        Expression<Func<GanttChart, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(GanttChart), "p");

        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);

        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return Expression.Lambda<Func<GanttChart, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    /// <summary>
    /// 套用排序
    /// </summary>
    private List<GanttChart> ApplySorting(List<GanttChart> GanttCharts, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return GanttCharts;
        }

        return sortBy.ToLower() switch
        {
            "createdat" => descending
                ? GanttCharts.OrderByDescending(p => p.CreatedAt).ToList()
                : GanttCharts.OrderBy(p => p.CreatedAt).ToList(),
            _ => GanttCharts
        };
    }

    #endregion
}

