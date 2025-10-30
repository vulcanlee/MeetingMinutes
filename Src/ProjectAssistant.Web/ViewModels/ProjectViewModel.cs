using AutoMapper;
using Microsoft.AspNetCore.Components.Forms;
using ProjectAssistant.AdapterModels;
using ProjectAssistant.Business.Helpers;
using ProjectAssistant.Business.Repositories;
using ProjectAssistant.DataModel.Systems;
using ProjectAssistant.Dto.Commons;
using ProjectAssistant.EntityModel.Models;

namespace ProjectAssistant.Web.ViewModels;

public class ProjectViewModel
{

    #region Field 欄位
    bool isNewRecordMode;
    private readonly ILogger<ProjectViewModel> logger;
    private readonly IMapper mapper;
    public Action OnChanged { get; set; }
    #endregion

    #region Property 屬性
    public readonly ProjectRepository CurrentService;
    public List<ProjectAdapterModel> Datas { get; set; } = new();
    public ProjectAdapterModel CurrentRecord { get; set; } = new();
    public bool IsShowEditRecord { get; set; } = false;
    public EditContext LocalEditContext { get; set; }
    public string EditRecordTitle { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 7;
    public int Total { get; set; } = 1;

    public ConfirmModalModel ConfirmModal { get; set; } = new();
    public MessageModalModel MessageModal { get; set; } = new();

    public List<SelectItemModel> SelectItemsStatus { get; set; } = new();
    public List<SelectItemModel> SelectItemsPriority { get; set; } = new();
    public SelectItemModel SelectValueStatus { get; set; } = new();
    public SelectItemModel SelectValuePriority { get; set; } = new();

    public string AddOrEditTitle
    {
        get
        {
            return isNewRecordMode == true ? "新增" : "修改";
        }
    }

    #endregion

    #region 建構式
    public ProjectViewModel(ILogger<ProjectViewModel> logger,
        IMapper mapper,
        ProjectRepository projectRepository)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.CurrentService = projectRepository;

        SelectItemsStatus = SelectItemHelper.BuildStatus();
        SelectItemsPriority = SelectItemHelper.BuildPriority();
    }

    #endregion

    #region Method 方法

    #region CRUD
    public async Task GetAllAsync()
    {
        var projects = await CurrentService.GetAllAsync();
        Datas = mapper.Map<List<ProjectAdapterModel>>(projects);
    }

    public async Task GetPageAsync()
    {
        ProjectSearchRequestDto request = new ProjectSearchRequestDto()
        {
            PageIndex = this.PageIndex,
            PageSize = this.PageSize,
        };

        PagedResult<Project> pagedResult = await CurrentService.GetPagedAsync(request);

        Datas = mapper.Map<List<ProjectAdapterModel>>(pagedResult.Items);
        Total = pagedResult.TotalCount;
    }

    #endregion

    #region 修改紀錄對話窗的按鈕事件
    public void OnAddNewRecord()
    {
        CurrentRecord = new ProjectAdapterModel();
        isNewRecordMode = true;
        IsShowEditRecord = true;
        EditRecordTitle = $"{AddOrEditTitle} 紀錄";

        CurrentRecord.StartDate = DateTime.Now;
        CurrentRecord.EndDate = DateTime.Now.AddMonths(6);
        var foundItem = SelectItemsStatus.FirstOrDefault();
        if (foundItem != null)
        {
            SelectValueStatus = foundItem;
        }
        foundItem = SelectItemsPriority.FirstOrDefault();
        if (foundItem != null)
        {
            SelectValuePriority = foundItem;
        }
    }

    public void OnEditRecord(ProjectAdapterModel record)
    {
        CurrentRecord = mapper.Map<ProjectAdapterModel>(record);
        isNewRecordMode = false;
        IsShowEditRecord = true;
        EditRecordTitle = $"{AddOrEditTitle} 紀錄";

        var foundItem = SelectItemsStatus.FirstOrDefault(x => x.Value == CurrentRecord.Status.ToString());
        if (foundItem != null)
        {
            SelectValueStatus = foundItem;
        }
        foundItem = SelectItemsPriority.FirstOrDefault(x => x.Value == CurrentRecord.Priority.ToString());
        if (foundItem != null)
        {
            SelectValuePriority = foundItem;
        }
    }

    public async Task OnDeleteRecordAsync(ProjectAdapterModel record)
    {
        var confirmResult = await ConfirmModal.ShowAsync("警告", $"請再度確認，確定要 刪除 {record.Name} 這筆紀錄嗎?");
        if (confirmResult == false)
            return;

        CurrentRecord = mapper.Map<ProjectAdapterModel>(record);
        var verifyRecordResult = await CurrentService.DeleteAsync(record.Id);
        if (verifyRecordResult == false)
        {
            var taskMessage = MessageModal.ShowAsync("錯誤通知", $"要進行刪除 {record.Name} 這筆紀錄，發生錯誤");
            OnChanged?.Invoke();
            await taskMessage;
            return;
        }
        await GetPageAsync();
        OnChanged?.Invoke();
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
            var confirmResult = await ConfirmModal.ShowAsync("通知", $"確定要 {AddOrEditTitle} 這筆紀錄嗎?");
            if (confirmResult == false)
                return;

            if (isNewRecordMode == true)
            {
                var verifyRecordResult = await CurrentService.AddAsync(record);
            }
            else
            {
                var verifyRecordResult = await CurrentService.UpdateAsync(record);
            }
            IsShowEditRecord = false;
            await GetPageAsync();
            OnChanged?.Invoke();
        }
    }
    #endregion

    #region 其他
    #region 事件
    public async Task OnTableChange(AntDesign.TableModels.QueryModel<ProjectAdapterModel> args)
    {
        PageIndex = args.PageIndex;
        await GetPageAsync();
    }

    public void OnPriorityChange(SelectItemModel value)
    {
        if (Enum.TryParse<ProjectAssistant.Share.Enums.PriorityEnum>(value.Value, true, out var priority))
        {
            CurrentRecord.Priority = priority;
        }
    }

    public void OnStatusChange(SelectItemModel value)
    {
        if (Enum.TryParse<ProjectAssistant.Share.Enums.StatusEnum>(value.Value, true, out var status))
        {
            CurrentRecord.Status = status;
        }
    }
    #endregion

    #endregion
    #endregion
}
