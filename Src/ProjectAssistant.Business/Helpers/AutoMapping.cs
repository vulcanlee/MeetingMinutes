using AutoMapper;
using ProjectAssistant.EntityModel.Models;

namespace ProjectAssistant.Business.Helpers;

public class AutoMapping : Profile
{
    public AutoMapping()
    {
        #region Blazor AdapterModel

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
