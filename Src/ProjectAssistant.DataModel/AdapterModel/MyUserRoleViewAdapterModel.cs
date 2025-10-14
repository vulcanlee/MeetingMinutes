namespace ProjectAssistant.EntityModel.Models;

public class MyUserRoleViewAdapterModel
{
    public MyUserRoleViewAdapterModel()
    {
    }
    public int Id { get; set; }
    public int MyUserId { get; set; } 
    public MyUserAdapterModel MyUser { get; set; } 
    public int RoleViewId { get; set; } 
    public RoleViewAdapterModel RoleView { get; set; } 
}
