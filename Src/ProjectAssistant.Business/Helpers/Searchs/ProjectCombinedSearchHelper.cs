﻿using ProjectAssistant.EntityModel.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ProjectAssistant.Business.Helpers.Searchs;

public class ProjectCombinedSearchHelper
{
    #region 輔助方法

    /// <summary>
    /// 組合 Expression 條件 (AND)
    /// </summary>
    public static Expression<Func<Project, bool>> CombinePredicates(
        Expression<Func<Project, bool>> first,
        Expression<Func<Project, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(Project), "p");

        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);

        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return Expression.Lambda<Func<Project, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    /// <summary>
    /// 套用排序
    /// </summary>
    public static List<Project> ApplySorting(List<Project> projects, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return projects;
        }

        return sortBy.ToLower() switch
        {
            "name" => descending
                ? projects.OrderByDescending(p => p.Name).ToList()
                : projects.OrderBy(p => p.Name).ToList(),
            "startdate" => descending
                ? projects.OrderByDescending(p => p.StartDate).ToList()
                : projects.OrderBy(p => p.StartDate).ToList(),
            "enddate" => descending
                ? projects.OrderByDescending(p => p.EndDate).ToList()
                : projects.OrderBy(p => p.EndDate).ToList(),
            "status" => descending
                ? projects.OrderByDescending(p => p.Status).ToList()
                : projects.OrderBy(p => p.Status).ToList(),
            "priority" => descending
                ? projects.OrderByDescending(p => p.Priority).ToList()
                : projects.OrderBy(p => p.Priority).ToList(),
            "completionpercentage" => descending
                ? projects.OrderByDescending(p => p.CompletionPercentage).ToList()
                : projects.OrderBy(p => p.CompletionPercentage).ToList(),
            "createdat" => descending
                ? projects.OrderByDescending(p => p.CreatedAt).ToList()
                : projects.OrderBy(p => p.CreatedAt).ToList(),
            _ => projects
        };
    }

    #endregion
}
