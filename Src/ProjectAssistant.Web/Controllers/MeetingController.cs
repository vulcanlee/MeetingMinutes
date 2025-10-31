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
public class MeetingController : ControllerBase
{
    private readonly ILogger<MeetingController> logger;
    private readonly MeetingRepository MeetingRepository;
    private readonly IMapper mapper;

    public MeetingController(ILogger<MeetingController> logger,
        MeetingRepository MeetingRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.MeetingRepository = MeetingRepository;
        this.mapper = mapper;
    }

    #region 查詢 API

    /// <summary>
    /// 根據 ID 取得專案
    /// </summary>
    /// <param name="id">專案 ID</param>
    /// <param name="includeRelatedData">是否包含關聯資料</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResult<MeetingDto>>> GetById(int id, [FromQuery] bool includeRelatedData = false)
    {
        try
        {
            var Meeting = await MeetingRepository.GetByIdAsync(id, includeRelatedData);

            if (Meeting == null)
            {
                return NotFound(ApiResult<MeetingDto>.NotFoundResult($"找不到 ID 為 {id} 的專案"));
            }

            var MeetingDto = mapper.Map<MeetingDto>(Meeting);
            return Ok(ApiResult<MeetingDto>.SuccessResult(MeetingDto, "取得專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得專案 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult<MeetingDto>.ServerErrorResult("取得專案時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 分頁查詢會議(支援排序、過濾)
    /// </summary>
    /// <param name="request">查詢請求參數</param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResult<PagedResult<MeetingDto>>>> Search([FromBody] MeetingSearchRequestDto request)
    {
        try
        {
            // 執行分頁查詢
            PagedResult<Meeting> pagedResult = await MeetingRepository.GetPagedAsync(request);
            var MeetingDtos = mapper.Map<List<MeetingDto>>(pagedResult.Items);

            var result = new PagedResult<MeetingDto>
            {
                Items = MeetingDtos,
                TotalCount = pagedResult.TotalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(pagedResult.TotalCount / (double)request.PageSize)
            };

            return Ok(ApiResult<PagedResult<MeetingDto>>.SuccessResult(result, "搜尋會議成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜尋會議時發生錯誤");
            return StatusCode(500, ApiResult<PagedResult<MeetingDto>>.ServerErrorResult("搜尋會議時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 新增 API

    /// <summary>
    /// 新增專案
    /// </summary>
    /// <param name="MeetingDto">專案資料</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<ApiResult<MeetingDto>>> Create([FromBody] MeetingDto MeetingDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<MeetingDto>.ValidationError(errors));
            }

            // 檢查專案名稱是否重複
            if (await MeetingRepository.ExistsByNameAsync(MeetingDto.Name))
            {
                return Conflict(ApiResult<MeetingDto>.ConflictResult($"專案名稱 '{MeetingDto.Name}' 已存在"));
            }

            // DTO 轉 Entity
            var Meeting = mapper.Map<Meeting>(MeetingDto);
            var createdMeeting = await MeetingRepository.AddAsync(Meeting);

            // Entity 轉 DTO
            var createdMeetingDto = mapper.Map<MeetingDto>(createdMeeting);
            return Ok(ApiResult<MeetingDto>.SuccessResult(createdMeetingDto, "新增專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "新增專案時發生錯誤");
            return StatusCode(500, ApiResult<MeetingDto>.ServerErrorResult("新增專案時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 更新 API

    /// <summary>
    /// 更新專案
    /// </summary>
    /// <param name="id">專案 ID</param>
    /// <param name="MeetingDto">專案資料</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResult>> Update(int id, [FromBody] MeetingDto MeetingDto)
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

            if (id != MeetingDto.Id)
            {
                return BadRequest(ApiResult.ValidationError("路由 ID 與專案 ID 不符"));
            }

            // 檢查專案名稱是否與其他專案重複
            if (await MeetingRepository.ExistsByNameAsync(MeetingDto.Name, id))
            {
                return Conflict(ApiResult.ConflictResult($"專案名稱 '{MeetingDto.Name}' 已被其他專案使用"));
            }

            // DTO 轉 Entity
            var Meeting = mapper.Map<Meeting>(MeetingDto);
            var success = await MeetingRepository.UpdateAsync(Meeting);

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
            var success = await MeetingRepository.DeleteAsync(id);

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
    private Expression<Func<Meeting, bool>> CombinePredicates(
        Expression<Func<Meeting, bool>> first,
        Expression<Func<Meeting, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(Meeting), "p");

        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);

        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return Expression.Lambda<Func<Meeting, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    /// <summary>
    /// 套用排序
    /// </summary>
    private List<Meeting> ApplySorting(List<Meeting> Meetings, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return Meetings;
        }

        return sortBy.ToLower() switch
        {
            "name" => descending
                ? Meetings.OrderByDescending(p => p.Name).ToList()
                : Meetings.OrderBy(p => p.Name).ToList(),
            _ => Meetings
        };
    }

    #endregion
}

