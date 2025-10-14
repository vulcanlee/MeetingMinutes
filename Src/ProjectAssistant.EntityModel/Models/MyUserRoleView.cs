namespace ProjectAssistant.EntityModel.Models;

public class MyUserRoleView
{
    public MyUserRoleView()
    {
    }
    public int Id { get; set; }
    public int MyUserId { get; set; } 
    public MyUser MyUser { get; set; } 
    public int RoleViewId { get; set; } 
    public RoleView RoleView { get; set; } 
}
