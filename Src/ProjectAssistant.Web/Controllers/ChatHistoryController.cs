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
public class ChatHistoryController : ControllerBase
{
    private readonly ILogger<ChatHistoryController> logger;
    private readonly ChatHistoryRepository ChatHistoryRepository;
    private readonly IMapper mapper;

    public ChatHistoryController(ILogger<ChatHistoryController> logger,
        ChatHistoryRepository ChatHistoryRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.ChatHistoryRepository = ChatHistoryRepository;
        this.mapper = mapper;
    }

    #region 查詢 API

    /// <summary>
    /// 取得所有專案
    /// </summary>
    /// <param name="includeRelatedData">是否包含關聯資料</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<ApiResult<List<ChatHistoryDto>>>> GetAll([FromQuery] bool includeRelatedData = false)
    {
        try
        {
            var ChatHistorys = await ChatHistoryRepository.GetAllAsync(includeRelatedData);
            var ChatHistoryDtos = mapper.Map<List<ChatHistoryDto>>(ChatHistorys);
            return Ok(ApiResult<List<ChatHistoryDto>>.SuccessResult(ChatHistoryDtos, "取得所有專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得所有專案時發生錯誤");
            return StatusCode(500, ApiResult<List<ChatHistoryDto>>.ServerErrorResult("取得所有專案時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 根據 ID 取得專案
    /// </summary>
    /// <param name="id">專案 ID</param>
    /// <param name="includeRelatedData">是否包含關聯資料</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResult<ChatHistoryDto>>> GetById(int id, [FromQuery] bool includeRelatedData = false)
    {
        try
        {
            var ChatHistory = await ChatHistoryRepository.GetByIdAsync(id, includeRelatedData);

            if (ChatHistory == null)
            {
                return NotFound(ApiResult<ChatHistoryDto>.NotFoundResult($"找不到 ID 為 {id} 的專案"));
            }

            var ChatHistoryDto = mapper.Map<ChatHistoryDto>(ChatHistory);
            return Ok(ApiResult<ChatHistoryDto>.SuccessResult(ChatHistoryDto, "取得專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得專案 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult<ChatHistoryDto>.ServerErrorResult("取得專案時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 分頁查詢專案(支援排序、過濾)
    /// </summary>
    /// <param name="request">查詢請求參數</param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResult<PagedResult<ChatHistoryDto>>>> Search([FromBody] CommonSearchRequest request)
    {
        try
        {
            // 建立過濾條件
            Expression<Func<ChatHistory, bool>>? predicate = null;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                predicate = p => p.Name.Contains(request.Keyword);
            }

            // 執行分頁查詢
            var (items, totalCount) = await ChatHistoryRepository.GetPagedAsync(
                request.PageIndex,
                request.PageSize,
                predicate,
                request.IncludeRelatedData
            );

            // 排序
            items = ApplySorting(items, request.SortBy, request.SortDescending);

            // 轉換為 DTO
            var ChatHistoryDtos = mapper.Map<List<ChatHistoryDto>>(items);

            var result = new PagedResult<ChatHistoryDto>
            {
                Items = ChatHistoryDtos,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return Ok(ApiResult<PagedResult<ChatHistoryDto>>.SuccessResult(result, "搜尋專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜尋專案時發生錯誤");
            return StatusCode(500, ApiResult<PagedResult<ChatHistoryDto>>.ServerErrorResult("搜尋專案時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 新增 API

    /// <summary>
    /// 新增專案
    /// </summary>
    /// <param name="ChatHistoryDto">專案資料</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<ApiResult<ChatHistoryDto>>> Create([FromBody] ChatHistoryDto ChatHistoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<ChatHistoryDto>.ValidationError(errors));
            }

            // 檢查專案名稱是否重複
            if (await ChatHistoryRepository.ExistsByNameAsync(ChatHistoryDto.Name))
            {
                return Conflict(ApiResult<ChatHistoryDto>.ConflictResult($"專案名稱 '{ChatHistoryDto.Name}' 已存在"));
            }

            // DTO 轉 Entity
            var ChatHistory = mapper.Map<ChatHistory>(ChatHistoryDto);
            var createdChatHistory = await ChatHistoryRepository.AddAsync(ChatHistory);

            // Entity 轉 DTO
            var createdChatHistoryDto = mapper.Map<ChatHistoryDto>(createdChatHistory);
            return Ok(ApiResult<ChatHistoryDto>.SuccessResult(createdChatHistoryDto, "新增專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "新增專案時發生錯誤");
            return StatusCode(500, ApiResult<ChatHistoryDto>.ServerErrorResult("新增專案時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 更新 API

    /// <summary>
    /// 更新專案
    /// </summary>
    /// <param name="id">專案 ID</param>
    /// <param name="ChatHistoryDto">專案資料</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResult>> Update(int id, [FromBody] ChatHistoryDto ChatHistoryDto)
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

            if (id != ChatHistoryDto.Id)
            {
                return BadRequest(ApiResult.ValidationError("路由 ID 與專案 ID 不符"));
            }

            // 檢查專案名稱是否與其他專案重複
            if (await ChatHistoryRepository.ExistsByNameAsync(ChatHistoryDto.Name, id))
            {
                return Conflict(ApiResult.ConflictResult($"專案名稱 '{ChatHistoryDto.Name}' 已被其他專案使用"));
            }

            // DTO 轉 Entity
            var ChatHistory = mapper.Map<ChatHistory>(ChatHistoryDto);
            var success = await ChatHistoryRepository.UpdateAsync(ChatHistory);

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
            var success = await ChatHistoryRepository.DeleteAsync(id);

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
    private Expression<Func<ChatHistory, bool>> CombinePredicates(
        Expression<Func<ChatHistory, bool>> first,
        Expression<Func<ChatHistory, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(ChatHistory), "p");

        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);

        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return Expression.Lambda<Func<ChatHistory, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    /// <summary>
    /// 套用排序
    /// </summary>
    private List<ChatHistory> ApplySorting(List<ChatHistory> ChatHistorys, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return ChatHistorys;
        }

        return sortBy.ToLower() switch
        {
            "name" => descending
                ? ChatHistorys.OrderByDescending(p => p.Name).ToList()
                : ChatHistorys.OrderBy(p => p.Name).ToList(),
            "createdat" => descending
                ? ChatHistorys.OrderByDescending(p => p.CreatedAt).ToList()
                : ChatHistorys.OrderBy(p => p.CreatedAt).ToList(),
            _ => ChatHistorys
        };
    }

    #endregion
}

