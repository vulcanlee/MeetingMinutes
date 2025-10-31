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
public class MyTaskController : ControllerBase
{
    private readonly ILogger<MyTaskController> logger;
    private readonly MyTaskRepository MyTaskRepository;
    private readonly IMapper mapper;

    public MyTaskController(ILogger<MyTaskController> logger,
        MyTaskRepository MyTaskRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.MyTaskRepository = MyTaskRepository;
        this.mapper = mapper;
    }

    #region 查詢 API

    /// <summary>
    /// 根據 ID 取得工作
    /// </summary>
    /// <param name="id">工作 ID</param>
    /// <param name="includeRelatedData">是否包含關聯資料</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResult<MyTaskDto>>> GetById(int id, [FromQuery] bool includeRelatedData = false)
    {
        try
        {
            var MyTask = await MyTaskRepository.GetByIdAsync(id, includeRelatedData);

            if (MyTask == null)
            {
                return NotFound(ApiResult<MyTaskDto>.NotFoundResult($"找不到 ID 為 {id} 的工作"));
            }

            var MyTaskDto = mapper.Map<MyTaskDto>(MyTask);
            return Ok(ApiResult<MyTaskDto>.SuccessResult(MyTaskDto, "取得工作成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得工作 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult<MyTaskDto>.ServerErrorResult("取得工作時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 分頁查詢工作(支援排序、過濾)
    /// </summary>
    /// <param name="request">查詢請求參數</param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResult<PagedResult<MyTaskDto>>>> Search([FromBody] MyTaskSearchRequestDto request)
    {
        try
        {
            // 執行分頁查詢
            PagedResult<MyTask> pagedResult = await MyTaskRepository.GetPagedAsync(request);
            var MyTaskDtos = mapper.Map<List<MyTaskDto>>(pagedResult.Items);

            var result = new PagedResult<MyTaskDto>
            {
                Items = MyTaskDtos,
                TotalCount = pagedResult.TotalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(pagedResult.TotalCount / (double)request.PageSize)
            };

            return Ok(ApiResult<PagedResult<MyTaskDto>>.SuccessResult(result, "搜尋工作成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜尋工作時發生錯誤");
            return StatusCode(500, ApiResult<PagedResult<MyTaskDto>>.ServerErrorResult("搜尋工作時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 新增 API

    /// <summary>
    /// 新增工作
    /// </summary>
    /// <param name="MyTaskDto">工作資料</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<ApiResult<MyTaskDto>>> Create([FromBody] MyTaskDto MyTaskDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<MyTaskDto>.ValidationError(errors));
            }

            // 檢查工作名稱是否重複
            if (await MyTaskRepository.ExistsByNameAsync(MyTaskDto.Name))
            {
                return Conflict(ApiResult<MyTaskDto>.ConflictResult($"工作名稱 '{MyTaskDto.Name}' 已存在"));
            }

            // DTO 轉 Entity
            var MyTask = mapper.Map<MyTask>(MyTaskDto);
            var createdMyTask = await MyTaskRepository.AddAsync(MyTask);

            // Entity 轉 DTO
            var createdMyTaskDto = mapper.Map<MyTaskDto>(createdMyTask);
            return Ok(ApiResult<MyTaskDto>.SuccessResult(createdMyTaskDto, "新增工作成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "新增工作時發生錯誤");
            return StatusCode(500, ApiResult<MyTaskDto>.ServerErrorResult("新增工作時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 更新 API

    /// <summary>
    /// 更新工作
    /// </summary>
    /// <param name="id">工作 ID</param>
    /// <param name="MyTaskDto">工作資料</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResult>> Update(int id, [FromBody] MyTaskDto MyTaskDto)
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

            if (id != MyTaskDto.Id)
            {
                return BadRequest(ApiResult.ValidationError("路由 ID 與工作 ID 不符"));
            }

            // 檢查工作名稱是否與其他工作重複
            if (await MyTaskRepository.ExistsByNameAsync(MyTaskDto.Name, id))
            {
                return Conflict(ApiResult.ConflictResult($"工作名稱 '{MyTaskDto.Name}' 已被其他工作使用"));
            }

            // DTO 轉 Entity
            var MyTask = mapper.Map<MyTask>(MyTaskDto);
            var success = await MyTaskRepository.UpdateAsync(MyTask);

            if (!success)
            {
                return NotFound(ApiResult.NotFoundResult($"找不到 ID 為 {id} 的工作"));
            }

            return Ok(ApiResult.SuccessResult("更新工作成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新工作 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult.ServerErrorResult("更新工作時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 刪除 API

    /// <summary>
    /// 刪除工作
    /// </summary>
    /// <param name="id">工作 ID</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResult>> Delete(int id)
    {
        try
        {
            var success = await MyTaskRepository.DeleteAsync(id);

            if (!success)
            {
                return NotFound(ApiResult.NotFoundResult($"找不到 ID 為 {id} 的工作"));
            }

            return Ok(ApiResult.SuccessResult("刪除工作成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刪除工作 ID {Id} 時發生錯誤", id);

            // 檢查是否為外鍵約束錯誤
            if (ex.InnerException?.Message.Contains("DELETE statement conflicted") == true)
            {
                return BadRequest(ApiResult.FailureResult("無法刪除此工作,因為有相關的子資料(任務、會議等)存在"));
            }

            return StatusCode(500, ApiResult.ServerErrorResult("刪除工作時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 輔助方法

    /// <summary>
    /// 組合 Expression 條件 (AND)
    /// </summary>
    private Expression<Func<MyTask, bool>> CombinePredicates(
        Expression<Func<MyTask, bool>> first,
        Expression<Func<MyTask, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(MyTask), "p");

        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);

        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return Expression.Lambda<Func<MyTask, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    /// <summary>
    /// 套用排序
    /// </summary>
    private List<MyTask> ApplySorting(List<MyTask> MyTasks, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return MyTasks;
        }

        return sortBy.ToLower() switch
        {
            "name" => descending
                ? MyTasks.OrderByDescending(p => p.Name).ToList()
                : MyTasks.OrderBy(p => p.Name).ToList(),
            "startdate" => descending
                ? MyTasks.OrderByDescending(p => p.StartDate).ToList()
                : MyTasks.OrderBy(p => p.StartDate).ToList(),
            "enddate" => descending
                ? MyTasks.OrderByDescending(p => p.EndDate).ToList()
                : MyTasks.OrderBy(p => p.EndDate).ToList(),
            "status" => descending
                ? MyTasks.OrderByDescending(p => p.Status).ToList()
                : MyTasks.OrderBy(p => p.Status).ToList(),
            "priority" => descending
                ? MyTasks.OrderByDescending(p => p.Priority).ToList()
                : MyTasks.OrderBy(p => p.Priority).ToList(),
            "completionpercentage" => descending
                ? MyTasks.OrderByDescending(p => p.CompletionPercentage).ToList()
                : MyTasks.OrderBy(p => p.CompletionPercentage).ToList(),
            "createdat" => descending
                ? MyTasks.OrderByDescending(p => p.CreatedAt).ToList()
                : MyTasks.OrderBy(p => p.CreatedAt).ToList(),
            _ => MyTasks
        };
    }

    #endregion
}

