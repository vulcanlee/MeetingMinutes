using ProjectAssistant.Business.Services.Database;

namespace ProjectAssistant.Web.ViewModels;

public class CounterViewModel
{

    #region Field 欄位
    private readonly ILogger<CounterViewModel> logger;
    private readonly MyUserService myUserService;

    #endregion

    #region Property 屬性

    #endregion

    #region 建構式
    public CounterViewModel(ILogger<CounterViewModel> logger,
        MyUserService myUserService)
    {
        this.logger = logger;
        this.myUserService = myUserService;
    }

    public async Task OnDatabaseTestAsync()
    {
        await myUserService.TestAsync();    
        logger.LogInformation("Database Test");
    }

    #endregion
}
