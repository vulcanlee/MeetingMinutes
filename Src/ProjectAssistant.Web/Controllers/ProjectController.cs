using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProjectAssistant.Business.Helpers;
using ProjectAssistant.Business.Helpers.Searchs;
using ProjectAssistant.Business.Repositories;
using ProjectAssistant.Dto.Commons;
using ProjectAssistant.Dto.Models;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;
using System.Linq.Expressions;

namespace ProjectAssistant.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectController : ControllerBase
{
    private readonly ILogger<ProjectController> logger;
    private readonly ProjectRepository projectRepository;
    private readonly IMapper mapper;

    public ProjectController(ILogger<ProjectController> logger,
        ProjectRepository projectRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.projectRepository = projectRepository;
        this.mapper = mapper;
    }

    #region 查詢 API

    /// <summary>
    /// 取得所有專案
    /// </summary>
    /// <param name="includeRelatedData">是否包含關聯資料</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<ApiResult<List<ProjectDto>>>> GetAll([FromQuery] bool includeRelatedData = false)
    {
        try
        {
            var projects = await projectRepository.GetAllAsync(includeRelatedData);
            var projectDtos = mapper.Map<List<ProjectDto>>(projects);
            return Ok(ApiResult<List<ProjectDto>>.SuccessResult(projectDtos, "取得所有專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得所有專案時發生錯誤");
            return StatusCode(500, ApiResult<List<ProjectDto>>.ServerErrorResult("取得所有專案時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 根據 ID 取得專案
    /// </summary>
    /// <param name="id">專案 ID</param>
    /// <param name="includeRelatedData">是否包含關聯資料</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResult<ProjectDto>>> GetById(int id, [FromQuery] bool includeRelatedData = false)
    {
        try
        {
            var project = await projectRepository.GetByIdAsync(id, includeRelatedData);

            if (project == null)
            {
                return NotFound(ApiResult<ProjectDto>.NotFoundResult($"找不到 ID 為 {id} 的專案"));
            }

            var projectDto = mapper.Map<ProjectDto>(project);
            return Ok(ApiResult<ProjectDto>.SuccessResult(projectDto, "取得專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得專案 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult<ProjectDto>.ServerErrorResult("取得專案時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 分頁查詢專案(支援排序、過濾)
    /// </summary>
    /// <param name="request">查詢請求參數</param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResult<PagedResult<ProjectDto>>>> Search([FromBody] ProjectSearchRequestDto request)
    {
        try
        {
            // 建立過濾條件
            Expression<Func<Project, bool>>? predicate = null;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                predicate = p => p.Name.Contains(request.Keyword) ||
                                (p.Description != null && p.Description.Contains(request.Keyword));
            }

            if (!string.IsNullOrEmpty(request.Owner))
            {
                var ownerPredicate = (Expression<Func<Project, bool>>)(p => p.Owner == request.Owner);
                predicate = predicate == null ? ownerPredicate : CombinedSearchHelper.ProjectCombinePredicates(predicate, ownerPredicate);
            }

            if (request.Status.HasValue)
            {
                var statusPredicate = (Expression<Func<Project, bool>>)(p => p.Status == request.Status.Value);
                predicate = predicate == null ? statusPredicate : CombinedSearchHelper.ProjectCombinePredicates(predicate, statusPredicate);
            }

            if (request.Priority.HasValue)
            {
                var priorityPredicate = (Expression<Func<Project, bool>>)(p => p.Priority == request.Priority.Value);
                predicate = predicate == null ? priorityPredicate : CombinedSearchHelper.ProjectCombinePredicates(predicate, priorityPredicate);
            }

            if (request.StartDateFrom.HasValue)
            {
                var datePredicate = (Expression<Func<Project, bool>>)(p => p.StartDate >= request.StartDateFrom.Value);
                predicate = predicate == null ? datePredicate : CombinedSearchHelper.ProjectCombinePredicates(predicate, datePredicate);
            }

            if (request.StartDateTo.HasValue)
            {
                var datePredicate = (Expression<Func<Project, bool>>)(p => p.StartDate <= request.StartDateTo.Value);
                predicate = predicate == null ? datePredicate : CombinedSearchHelper.ProjectCombinePredicates(predicate, datePredicate);
            }

            if (request.CompletionPercentageMin.HasValue)
            {
                var completionPredicate = (Expression<Func<Project, bool>>)(p => p.CompletionPercentage >= request.CompletionPercentageMin.Value);
                predicate = predicate == null ? completionPredicate : CombinedSearchHelper.ProjectCombinePredicates(predicate, completionPredicate);
            }

            if (request.CompletionPercentageMax.HasValue)
            {
                var completionPredicate = (Expression<Func<Project, bool>>)(p => p.CompletionPercentage <= request.CompletionPercentageMax.Value);
                predicate = predicate == null ? completionPredicate : CombinedSearchHelper.ProjectCombinePredicates(predicate, completionPredicate);
            }

            // 執行分頁查詢
            var (items, totalCount) = await projectRepository.GetPagedAsync(
                request.PageIndex,
                request.PageSize,
                predicate,
                request.IncludeRelatedData
            );

            // 排序
            items = CombinedSearchHelper.ProjectApplySorting(items, request.SortBy, request.SortDescending);

            // 轉換為 DTO
            var projectDtos = mapper.Map<List<ProjectDto>>(items);

            var result = new PagedResult<ProjectDto>
            {
                Items = projectDtos,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return Ok(ApiResult<PagedResult<ProjectDto>>.SuccessResult(result, "搜尋專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜尋專案時發生錯誤");
            return StatusCode(500, ApiResult<PagedResult<ProjectDto>>.ServerErrorResult("搜尋專案時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 新增 API

    /// <summary>
    /// 新增專案
    /// </summary>
    /// <param name="projectDto">專案資料</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<ApiResult<ProjectDto>>> Create([FromBody] ProjectDto projectDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<ProjectDto>.ValidationError(errors));
            }

            // 檢查專案名稱是否重複
            if (await projectRepository.ExistsByNameAsync(projectDto.Name))
            {
                return Conflict(ApiResult<ProjectDto>.ConflictResult($"專案名稱 '{projectDto.Name}' 已存在"));
            }

            // DTO 轉 Entity
            var project = mapper.Map<Project>(projectDto);
            var createdProject = await projectRepository.AddAsync(project);

            // Entity 轉 DTO
            var createdProjectDto = mapper.Map<ProjectDto>(createdProject);
            return Ok(ApiResult<ProjectDto>.SuccessResult(createdProjectDto, "新增專案成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "新增專案時發生錯誤");
            return StatusCode(500, ApiResult<ProjectDto>.ServerErrorResult("新增專案時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 更新 API

    /// <summary>
    /// 更新專案
    /// </summary>
    /// <param name="id">專案 ID</param>
    /// <param name="projectDto">專案資料</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResult>> Update(int id, [FromBody] ProjectDto projectDto)
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

            if (id != projectDto.Id)
            {
                return BadRequest(ApiResult.ValidationError("路由 ID 與專案 ID 不符"));
            }

            // 檢查專案名稱是否與其他專案重複
            if (await projectRepository.ExistsByNameAsync(projectDto.Name, id))
            {
                return Conflict(ApiResult.ConflictResult($"專案名稱 '{projectDto.Name}' 已被其他專案使用"));
            }

            // DTO 轉 Entity
            var project = mapper.Map<Project>(projectDto);
            var success = await projectRepository.UpdateAsync(project);

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
            var success = await projectRepository.DeleteAsync(id);

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

}

