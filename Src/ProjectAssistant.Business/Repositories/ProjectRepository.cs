﻿using Microsoft.EntityFrameworkCore;
using ProjectAssistant.Business.Helpers.Searchs;
using ProjectAssistant.Dto.Commons;
using ProjectAssistant.EntityModel;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;
using System.Globalization;
using System.Linq.Expressions;

namespace ProjectAssistant.Business.Repositories;

public class ProjectRepository
{
    private readonly BackendDBContext context;

    public ProjectRepository(BackendDBContext context)
    {
        this.context = context;
    }

    #region 查詢方法

    /// <summary>
    /// 根據 ID 取得專案
    /// </summary>
    public async Task<Project?> GetByIdAsync(int id, bool includeRelatedData = false)
    {
        var query = context.Project.AsNoTracking().AsQueryable();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Task)
                .Include(p => p.GanttChart)
                .Include(p => p.Meeting)
                    .ThenInclude(m => m.ChatHistory)
                .Include(p => p.Meeting)
                    .ThenInclude(m => m.RecordedMediaFile);
        }

        return await query.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PagedResult<Project>> GetPagedAsync(
        ProjectSearchRequestDto request,
        bool includeRelatedData = false)
    {
        var query = context.Project.AsNoTracking().AsQueryable();

        #region 建立過濾條件
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
        #endregion 

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        #region 根據 request.SortBy 及  request.Descending 進行排序
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            query = request.SortBy.ToLower() switch
            {
                "name" => request.SortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "startdate" => request.SortDescending
                    ? query.OrderByDescending(p => p.StartDate)
                    : query.OrderBy(p => p.StartDate),
                "enddate" => request.SortDescending
                    ? query.OrderByDescending(p => p.EndDate)
                    : query.OrderBy(p => p.EndDate),
                "status" => request.SortDescending
                    ? query.OrderByDescending(p => p.Status)
                    : query.OrderBy(p => p.Status),
                "priority" => request.SortDescending
                    ? query.OrderByDescending(p => p.Priority)
                    : query.OrderBy(p => p.Priority),
                "completionpercentage" => request.SortDescending
                    ? query.OrderByDescending(p => p.CompletionPercentage)
                    : query.OrderBy(p => p.CompletionPercentage),
                "createdat" => request.SortDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                _ => query
            };
        }
        #endregion

        var totalCount = await query.CountAsync();

        if (includeRelatedData)
        {
            query = query
                .Include(p => p.Task)
                .Include(p => p.GanttChart)
                .Include(p => p.Meeting);
        }

        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        PagedResult<Project> pagedResult = new()
        {
            Items = items,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return pagedResult;
    }

    /// <summary>
    /// 檢查專案名稱是否存在
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = context.Project.Where(p => p.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    #endregion

    #region 新增方法

    /// <summary>
    /// 新增專案
    /// </summary>
    public async Task<Project> AddAsync(Project project)
    {
        project.CreatedAt = DateTime.Now;
        project.UpdatedAt = DateTime.Now;
        project.GanttChart = null; // 新增專案時不建立甘特圖   

        await context.Project.AddAsync(project);
        await context.SaveChangesAsync();

        return project;
    }

    /// <summary>
    /// 批次新增專案
    /// </summary>
    public async Task<int> AddRangeAsync(List<Project> projects)
    {
        var now = DateTime.Now;
        foreach (var project in projects)
        {
            project.CreatedAt = now;
            project.UpdatedAt = now;
            project.GanttChart = null; // 新增專案時不建立甘特圖   
        }

        await context.Project.AddRangeAsync(projects);
        return await context.SaveChangesAsync();
    }

    #endregion

    #region 更新方法

    /// <summary>
    /// 更新專案
    /// </summary>
    public async Task<bool> UpdateAsync(Project project)
    {
        var existingProject = await context.Project.FindAsync(project.Id);
        if (existingProject == null)
        {
            return false;
        }

        project.UpdatedAt = DateTime.Now;
        project.CreatedAt = existingProject.CreatedAt; // 保留原建立時間
        project.GanttChart = null; // 新增專案時不建立甘特圖   

        context.Entry(existingProject).CurrentValues.SetValues(project);
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 更新專案狀態
    /// </summary>
    public async Task<bool> UpdateStatusAsync(int id, StatusEnum status)
    {
        var project = await context.Project.FindAsync(id);
        if (project == null)
        {
            return false;
        }

        project.Status = status;
        project.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 更新專案完成百分比
    /// </summary>
    public async Task<bool> UpdateCompletionPercentageAsync(int id, int percentage)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "完成百分比必須介於 0 到 100 之間");
        }

        var project = await context.Project.FindAsync(id);
        if (project == null)
        {
            return false;
        }

        project.CompletionPercentage = percentage;
        project.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region 刪除方法

    /// <summary>
    /// 刪除專案
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var project = await context.Project.FindAsync(id);
        if (project == null)
        {
            return false;
        }

        context.Project.Remove(project);
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 批次刪除專案
    /// </summary>
    public async Task<int> DeleteRangeAsync(List<int> ids)
    {
        var projects = await context.Project
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        context.Project.RemoveRange(projects);
        return await context.SaveChangesAsync();
    }

    #endregion

    #region 統計方法

    /// <summary>
    /// 取得專案總數
    /// </summary>
    public async Task<int> GetCountAsync(Expression<Func<Project, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await context.Project.CountAsync();
        }

        return await context.Project.CountAsync(predicate);
    }

    #endregion
}
