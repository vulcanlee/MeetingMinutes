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

public class RoleViewService
{
    #region 欄位與屬性
    private readonly BackendDBContext context;

    public IMapper Mapper { get; }
    public IConfiguration Configuration { get; }
    public ILogger<RoleViewService> Logger { get; }
    #endregion

    #region 建構式
    public RoleViewService(BackendDBContext context, IMapper mapper,
        IConfiguration configuration, ILogger<RoleViewService> logger)
    {
        this.context = context;
        Mapper = mapper;
        Configuration = configuration;
        Logger = logger;
    }
    #endregion

    #region CRUD 服務
    public async Task<DataRequestResult<RoleViewAdapterModel>> GetAsync(DataRequest dataRequest)
    {
        List<RoleViewAdapterModel> data = new();
        DataRequestResult<RoleViewAdapterModel> result = new();
        var DataSource = context.RoleView
            .AsNoTracking();

        #region 進行搜尋動作
        if (!string.IsNullOrWhiteSpace(dataRequest.Search))
        {
            DataSource = DataSource
            .Where(x => x.Name.Contains(dataRequest.Search));
        }
        #endregion

        #region 進行排序動作
        #endregion

        #region 進行分頁
        // 取得記錄總數量，將要用於分頁元件面板使用
        result.Count = DataSource.Cast<RoleView>().Count();
        DataSource = DataSource.Skip(dataRequest.Skip);
        if (dataRequest.Take != 0)
        {
            DataSource = DataSource.Take(dataRequest.Take);
        }
        #endregion

        #region 在這裡進行取得資料與與額外屬性初始化
        List<RoleViewAdapterModel> adapterModelObjects =
            Mapper.Map<List<RoleViewAdapterModel>>(DataSource);

        foreach (var adapterModelItem in adapterModelObjects)
        {
            await OhterDependencyData(adapterModelItem);
        }
        #endregion

        result.Result = adapterModelObjects;
        await Task.Yield();
        return result;
    }

    public async Task<RoleViewAdapterModel> GetAsync(int id)

    {
        RoleView? RoleView = await context.RoleView
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        RoleView item = RoleView;
        if (item != null)
        {
            RoleViewAdapterModel result = Mapper.Map<RoleViewAdapterModel>(item);
            await OhterDependencyData(result);
            return result;
        }
        else
        {
            return new RoleViewAdapterModel() {};
        }
    }

    public async Task<bool> AddAsync(RoleViewAdapterModel paraObject)
    {
        try
        {
            RoleView itemParameter = Mapper.Map<RoleView>(paraObject);

            CleanTrackingHelper.Clean<RoleView>(context);
            await context.RoleView
                .AddAsync(itemParameter);
            await context.SaveChangesAsync();
            CleanTrackingHelper.Clean<RoleView>(context);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "新增記錄發生例外異常");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(RoleViewAdapterModel paraObject)
    {
        try
        {
            RoleView itemData = Mapper.Map<RoleView>(paraObject);
            itemData.MyUserRoleView = null;

            CleanTrackingHelper.Clean<RoleView>(context);
            RoleView item = await context.RoleView
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == paraObject.Id);
            if (item == null)
            {
                return false;
            }
            else
            {
                CleanTrackingHelper.Clean<RoleView>(context);
                context.Entry(itemData).State = EntityState.Modified;
                await context.SaveChangesAsync();
                CleanTrackingHelper.Clean<RoleView>(context);
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
            CleanTrackingHelper.Clean<RoleView>(context);
            RoleView item = await context.RoleView
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return false;
            }
            else
            {
                CleanTrackingHelper.Clean<RoleView>(context);
                context.Entry(item).State = EntityState.Deleted;
                await context.SaveChangesAsync();
                CleanTrackingHelper.Clean<RoleView>(context);
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
    async Task OhterDependencyData(RoleViewAdapterModel data)
    {
        await Task.Yield();
    }
    #endregion
}
