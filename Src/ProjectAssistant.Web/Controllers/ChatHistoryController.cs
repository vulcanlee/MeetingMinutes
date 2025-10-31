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
    /// 根據 ID 取得會議聊天
    /// </summary>
    /// <param name="id">會議聊天 ID</param>
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
                return NotFound(ApiResult<ChatHistoryDto>.NotFoundResult($"找不到 ID 為 {id} 的會議聊天"));
            }

            var ChatHistoryDto = mapper.Map<ChatHistoryDto>(ChatHistory);
            return Ok(ApiResult<ChatHistoryDto>.SuccessResult(ChatHistoryDto, "取得會議聊天成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得會議聊天 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult<ChatHistoryDto>.ServerErrorResult("取得會議聊天時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 分頁查詢會議聊天(支援排序、過濾)
    /// </summary>
    /// <param name="request">查詢請求參數</param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResult<PagedResult<ChatHistoryDto>>>> Search([FromBody] ChatHistorySearchRequestDto request)
    {
        try
        {
            // 執行分頁查詢
            PagedResult<ChatHistory> pagedResult = await ChatHistoryRepository.GetPagedAsync(request);
            var ChatHistoryDtos = mapper.Map<List<ChatHistoryDto>>(pagedResult.Items);

            var result = new PagedResult<ChatHistoryDto>
            {
                Items = ChatHistoryDtos,
                TotalCount = pagedResult.TotalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(pagedResult.TotalCount / (double)request.PageSize)
            };

            return Ok(ApiResult<PagedResult<ChatHistoryDto>>.SuccessResult(result, "搜尋會議聊天成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜尋會議聊天時發生錯誤");
            return StatusCode(500, ApiResult<PagedResult<ChatHistoryDto>>.ServerErrorResult("搜尋會議聊天時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 新增 API

    /// <summary>
    /// 新增會議聊天
    /// </summary>
    /// <param name="ChatHistoryDto">會議聊天資料</param>
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

            // 檢查會議聊天名稱是否重複
            if (await ChatHistoryRepository.ExistsByNameAsync(ChatHistoryDto.Name))
            {
                return Conflict(ApiResult<ChatHistoryDto>.ConflictResult($"會議聊天名稱 '{ChatHistoryDto.Name}' 已存在"));
            }

            // DTO 轉 Entity
            var ChatHistory = mapper.Map<ChatHistory>(ChatHistoryDto);
            var createdChatHistory = await ChatHistoryRepository.AddAsync(ChatHistory);

            // Entity 轉 DTO
            var createdChatHistoryDto = mapper.Map<ChatHistoryDto>(createdChatHistory);
            return Ok(ApiResult<ChatHistoryDto>.SuccessResult(createdChatHistoryDto, "新增會議聊天成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "新增會議聊天時發生錯誤");
            return StatusCode(500, ApiResult<ChatHistoryDto>.ServerErrorResult("新增會議聊天時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 更新 API

    /// <summary>
    /// 更新會議聊天
    /// </summary>
    /// <param name="id">會議聊天 ID</param>
    /// <param name="ChatHistoryDto">會議聊天資料</param>
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
                return BadRequest(ApiResult.ValidationError("路由 ID 與會議聊天 ID 不符"));
            }

            // 檢查會議聊天名稱是否與其他會議聊天重複
            if (await ChatHistoryRepository.ExistsByNameAsync(ChatHistoryDto.Name, id))
            {
                return Conflict(ApiResult.ConflictResult($"會議聊天名稱 '{ChatHistoryDto.Name}' 已被其他會議聊天使用"));
            }

            // DTO 轉 Entity
            var ChatHistory = mapper.Map<ChatHistory>(ChatHistoryDto);
            var success = await ChatHistoryRepository.UpdateAsync(ChatHistory);

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
            var success = await ChatHistoryRepository.DeleteAsync(id);

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

