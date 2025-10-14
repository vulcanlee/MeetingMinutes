using System.ComponentModel.DataAnnotations;

namespace ProjectAssistant.EntityModel.Models;

public class RoleView
{
    public RoleView()
    {
    }
    public int Id { get; set; }
    [Required(ErrorMessage = "名稱 不可為空白")]
    public string Name { get; set; }
    [Required(ErrorMessage = "頁面可視權限 Json 不可為空白")]
    public string PermissionJson { get; set; }
    public ICollection<MyUserRoleView> MyUserRoleView { get; set; } = new List<MyUserRoleView>();
}
