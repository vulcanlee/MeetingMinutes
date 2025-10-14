using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjectAssistant.Business.Helpers;
using ProjectAssistant.DataModel.Systems;
using ProjectAssistant.EntityModel;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Helpers;
using System.Text.Json;

namespace ProjectAssistant.Business.Services.Database;

public class MyUserService
{
    #region 欄位與屬性
    private readonly BackendDBContext context;

    public IMapper Mapper { get; }
    public IConfiguration Configuration { get; }
    public ILogger<MyUserService> Logger { get; }
    #endregion

    #region 建構式
    public MyUserService(BackendDBContext context, IMapper mapper,
        IConfiguration configuration, ILogger<MyUserService> logger)
    {
        this.context = context;
        Mapper = mapper;
        Configuration = configuration;
        Logger = logger;
    }
    #endregion

    public async Task TestAsync()
    {
        var myUser = await context.MyUser
           .AsNoTracking()
           .Include(x => x.MyUserRoleView)
           .ThenInclude(a => a.RoleView)
           .ToListAsync();
    }

    public async Task Test2Async()
    {
        RoleView roleView1 = new RoleView()
        { Name = "Admin", PermissionJson="{}" };
        RoleView roleView2 = new RoleView()
        { Name = "User", PermissionJson = "{}" };
        context.RoleView.Add(roleView1);
        context.RoleView.Add(roleView2);
        await context.SaveChangesAsync();

        MyUser myUser = new MyUser() { Name = "系統管理員", Account = "admin", Password = "admin", Status = true, Email="E", IsAdmin=false, RoleJson = "{}" };
        context.MyUser.Add(myUser);
        await context.SaveChangesAsync();
        MyUserRoleView myUserRoleView1 = new MyUserRoleView()
        { MyUserId = myUser.Id, RoleViewId = roleView1.Id };
        MyUserRoleView myUserRoleView2 = new MyUserRoleView()
        { MyUserId = myUser.Id, RoleViewId = roleView2.Id };
        context.MyUserRoleView.Add(myUserRoleView1);
        context.MyUserRoleView.Add(myUserRoleView2);
        await context.SaveChangesAsync();
    }

    #region CRUD 服務
    public async Task<DataRequestResult<MyUserAdapterModel>> GetAsync(DataRequest dataRequest)
    {
        List<MyUserAdapterModel> data = new();
        DataRequestResult<MyUserAdapterModel> result = new();
        var DataSource = context.MyUser
            .Include(x => x.MyUserRoleView)
            .ThenInclude(a => a.RoleView)
            .AsNoTracking();

        #region 進行搜尋動作
        if (!string.IsNullOrWhiteSpace(dataRequest.Search))
        {
            DataSource = DataSource
            .Where(x => x.Name.Contains(dataRequest.Search) ||
            x.Account.Contains(dataRequest.Search));
        }
        #endregion

        #region 進行排序動作
        #endregion

        #region 進行分頁
        // 取得記錄總數量，將要用於分頁元件面板使用
        result.Count = DataSource.Cast<MyUser>().Count();
        DataSource = DataSource.Skip(dataRequest.Skip);
        if (dataRequest.Take != 0)
        {
            DataSource = DataSource.Take(dataRequest.Take);
        }
        #endregion

        #region 在這裡進行取得資料與與額外屬性初始化
        List<MyUserAdapterModel> adapterModelObjects =
            Mapper.Map<List<MyUserAdapterModel>>(DataSource);

        foreach (var adapterModelItem in adapterModelObjects)
        {
            await OhterDependencyData(adapterModelItem);
        }
        #endregion

        result.Result = adapterModelObjects;
        await Task.Yield();
        return result;
    }

    public async Task<MyUserAdapterModel> GetAsync(int id)

    {
        MyUser? myUser = await context.MyUser
            .AsNoTracking()
            .Include(x => x.MyUserRoleView)
            .ThenInclude(a => a.RoleView)
            .FirstOrDefaultAsync(x => x.Id == id);
        MyUser item = myUser;
        if (item != null)
        {
            MyUserAdapterModel result = Mapper.Map<MyUserAdapterModel>(item);
            await OhterDependencyData(result);
            return result;
        }
        else
        {
            return new MyUserAdapterModel() { Status = false };
        }
    }

    public async Task<bool> AddAsync(MyUserAdapterModel paraObject)
    {
        try
        {
            MyUser itemParameter = Mapper.Map<MyUser>(paraObject);

            CleanTrackingHelper.Clean<MyUser>(context);
            await context.MyUser
                .AddAsync(itemParameter);
            await context.SaveChangesAsync();
            CleanTrackingHelper.Clean<MyUser>(context);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "新增記錄發生例外異常");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(MyUserAdapterModel paraObject)
    {
        try
        {
            MyUser itemData = Mapper.Map<MyUser>(paraObject);
            itemData.MyUserRoleView = null;

            CleanTrackingHelper.Clean<MyUser>(context);
            MyUser item = await context.MyUser
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == paraObject.Id);
            if (item == null)
            {
                return false;
            }
            else
            {
                CleanTrackingHelper.Clean<MyUser>(context);
                context.Entry(itemData).State = EntityState.Modified;
                await context.SaveChangesAsync();
                CleanTrackingHelper.Clean<MyUser>(context);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "修改記錄發生例外異常");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            CleanTrackingHelper.Clean<MyUser>(context);
            MyUser item = await context.MyUser
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return false;
            }
            else
            {
                CleanTrackingHelper.Clean<MyUser>(context);
                context.Entry(item).State = EntityState.Deleted;
                await context.SaveChangesAsync();
                CleanTrackingHelper.Clean<MyUser>(context);
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "刪除記錄發生例外異常");
            return false;
        }
    }
    #endregion

    #region CRUD 的限制條件檢查
    #endregion

    #region 其他服務方法
    async Task OhterDependencyData(MyUserAdapterModel data)
    {
        await Task.Yield();
    }
    #endregion
}
