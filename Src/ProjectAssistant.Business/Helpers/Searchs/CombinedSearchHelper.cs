using Microsoft.CognitiveServices.Speech.Transcription;
using ProjectAssistant.EntityModel.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Meeting = ProjectAssistant.EntityModel.Models.Meeting;

namespace ProjectAssistant.Business.Helpers.Searchs;

public class CombinedSearchHelper
{
    #region MyTask 輔助方法

    /// <summary>
    /// 組合 Expression 條件 (AND)
    /// </summary>
    public static Expression<Func<MyTask, bool>> MyTaskCombinePredicates(
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
    public static List<MyTask> MyTaskApplySorting(List<MyTask> MyTasks, string? sortBy, bool descending)
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
            "updatedat" => descending
                ? MyTasks.OrderByDescending(p => p.UpdatedAt).ToList()
                : MyTasks.OrderBy(p => p.UpdatedAt).ToList(),
            _ => MyTasks
        };
    }

    #endregion

    #region Project 輔助方法

    /// <summary>
    /// 組合 Expression 條件 (AND)
    /// </summary>
    public static Expression<Func<Project, bool>> ProjectCombinePredicates(
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
    public static List<Project> ProjectApplySorting(List<Project> projects, string? sortBy, bool descending)
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
            "updatedat" => descending
                ? projects.OrderByDescending(p => p.UpdatedAt).ToList()
                : projects.OrderBy(p => p.UpdatedAt).ToList(),
            _ => projects
        };
    }

    #endregion

    #region Meeting 輔助方法

    /// <summary>
    /// 組合 Expression 條件 (AND)
    /// </summary>
    public static Expression<Func<Meeting, bool>> MeetingCombinePredicates(
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
    public static List<Meeting> MeetingApplySorting(List<Meeting> Meetings, string? sortBy, bool descending)
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
            "createdat" => descending
                ? Meetings.OrderByDescending(p => p.CreatedAt).ToList()
                : Meetings.OrderBy(p => p.CreatedAt).ToList(),
            "updatedat" => descending
                ? Meetings.OrderByDescending(p => p.UpdatedAt).ToList()
                : Meetings.OrderBy(p => p.UpdatedAt).ToList(),
            _ => Meetings
        };
    }

    #endregion
}
