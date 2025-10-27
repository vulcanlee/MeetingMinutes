using AutoMapper;
using Microsoft.AspNetCore.Components.Forms;
using ProjectAssistant.AdapterModels;
using ProjectAssistant.Business.Repositories;
using ProjectAssistant.Business.Services.Database;
using ProjectAssistant.EntityModel.Models;

namespace ProjectAssistant.Web.ViewModels;

public class ProjectViewModel
{

    #region Field 欄位
    bool isNewRecordMode;
    private readonly ILogger<ProjectViewModel> logger;
    private readonly IMapper mapper;

    #endregion

    #region Property 屬性
    public readonly ProjectRepository CurrentService;
    public List<ProjectAdapterModel> Datas { get; set; } = new();
    public ProjectAdapterModel CurrentRecord { get; set; } = new();
    public bool IsShowEditRecord { get; set; } = false;
    public EditContext LocalEditContext { get; set; }
    public string EditRecordTitle { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int Total { get; set; } = 1;

    #endregion

    #region 建構式
    public ProjectViewModel(ILogger<ProjectViewModel> logger,
        IMapper mapper,
        ProjectRepository projectRepository)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.CurrentService = projectRepository;
    }

    #endregion

    #region Method 方法

    #region CRUD
    public async Task GetAllAsync()
    {
        var projects = await CurrentService.GetAllAsync();
        Datas = mapper.Map<List<ProjectAdapterModel>>(projects);
    }

    #endregion

    #region 修改紀錄對話窗的按鈕事件
    public void OnAddNewRecord()
    {
        CurrentRecord = new ProjectAdapterModel();
        isNewRecordMode = true;
        IsShowEditRecord = true;
        EditRecordTitle = "新增紀錄";
    }

    public void OnEditRecord(ProjectAdapterModel record)
    {
        CurrentRecord = mapper.Map<ProjectAdapterModel>(record);
        isNewRecordMode = false;
        IsShowEditRecord = true;
        EditRecordTitle = "修改紀錄";
    }

    public void OnEditContestChanged(EditContext context)
    {
        LocalEditContext = context;
    }

    public void OnRecordEditCancel()
    {
        IsShowEditRecord = false;
    }

    public async Task OnRecordEditConfirm()
    {
        #region 進行 Form Validation 檢查驗證作業
        if (LocalEditContext.Validate() == false)
        {
            return;
        }
        #endregion

        #region 檢查資料完整性
        if (isNewRecordMode == true)
        {
        }
        else
        {
        }
        #endregion

        if (IsShowEditRecord == true)
        {
            var record = mapper.Map<Project>(CurrentRecord);
            if (isNewRecordMode == true)
            {
                var verifyRecordResult = await CurrentService.AddAsync(record);
            }
            else
            {
                var verifyRecordResult = await CurrentService.UpdateAsync(record);
            }
            IsShowEditRecord = false;
        }
    }
    #endregion

    #region 其他
    public async Task OnTableChange(AntDesign.TableModels.QueryModel<ProjectAdapterModel> args)
    {
    }

    #endregion
    #endregion
}
