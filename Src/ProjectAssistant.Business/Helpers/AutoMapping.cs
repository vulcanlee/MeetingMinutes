using AutoMapper;
using ProjectAssistant.AdapterModels;
using ProjectAssistant.Dto.Models;
using ProjectAssistant.EntityModel.Models;

namespace ProjectAssistant.Business.Helpers;

public class AutoMapping : Profile
{
    public AutoMapping()
    {
        #region Blazor AdapterModel

        #region GanttChart
        CreateMap<GanttChart, GanttChartAdapterModel>();
        CreateMap<GanttChartAdapterModel, GanttChart>();
        CreateMap<GanttChart, GanttChartDto>();
        CreateMap<GanttChartDto, GanttChart>();
        #endregion

        #region ChatHistory
        CreateMap<ChatHistory, ChatHistoryAdapterModel>();
        CreateMap<ChatHistoryAdapterModel, ChatHistory>();
        CreateMap<ChatHistory, ChatHistoryDto>();
        CreateMap<ChatHistoryDto, ChatHistory>();
        #endregion

        #region RecordedMediaFile
        CreateMap<RecordedMediaFile, RecordedMediaFileAdapterModel>();
        CreateMap<RecordedMediaFileAdapterModel, RecordedMediaFile>();
        CreateMap<RecordedMediaFile, RecordedMediaFileDto>();
        CreateMap<RecordedMediaFileDto, RecordedMediaFile>();
        #endregion

        #region Meeting
        CreateMap<Meeting, MeetingAdapterModel>();
        CreateMap<MeetingAdapterModel, Meeting>();
        CreateMap<Meeting, MeetingDto>();
        CreateMap<MeetingDto, Meeting>();
        #endregion

        #region MyTask
        CreateMap<MyTask, MyTaskAdapterModel>();
        CreateMap<MyTaskAdapterModel, MyTask>();
        CreateMap<MyTask, MyTaskDto>();
        CreateMap<MyTaskDto, MyTask>();
        #endregion

        #region Project
        CreateMap<Project, ProjectAdapterModel>();
        CreateMap<ProjectAdapterModel, Project>();
        CreateMap<Project, ProjectDto>();
        CreateMap<ProjectDto, Project>();
        #endregion

        #region RoleView
        CreateMap<RoleView, RoleViewAdapterModel>();
        CreateMap<RoleViewAdapterModel, RoleView>();
        #endregion

        #region MyUser
        CreateMap<MyUser, MyUserAdapterModel>();
        CreateMap<MyUserAdapterModel, MyUser>();
        #endregion
        #endregion

        #region MyUserRoleView
        CreateMap<MyUserRoleView, MyUserRoleViewAdapterModel>();
        CreateMap<MyUserRoleViewAdapterModel, MyUserRoleView>();
        #endregion
    }
}
