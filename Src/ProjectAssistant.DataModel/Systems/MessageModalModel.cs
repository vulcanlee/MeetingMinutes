namespace ProjectAssistant.DataModel.Systems;

public class MessageModalModel
{
    public bool IsVisible { get; set; } = false;
    public string MaxBodyHeight { get; set; } = "200px";
    public string Width { get; set; } = "70%";
    public string Title { get; set; } = "警告";
    public string Body { get; set; } = "確認要刪除這筆紀錄嗎？";
    public Func<bool, Task> ConfirmDelegate { get; set; }
    public TaskCompletionSource<bool> TaskCompletionSource { get; set; }
    public void Show(string width, string height, string title, string body,
        Func<bool, Task> confirmDelegate = null)
    {
        TaskCompletionSource = new();
        ConfirmDelegate = confirmDelegate;
        TaskCompletionSource = null;
        MaxBodyHeight = height;
        Width = width;
        Title = title;
        Body = body;
        IsVisible = true;
    }

    public Task<bool> ShowAsync(string title, string body, string width = "70%", string height = "80vh", Func<bool, Task> confirmDelegate = null)
    {
        TaskCompletionSource = new();
        ConfirmDelegate = confirmDelegate;
        TaskCompletionSource = new TaskCompletionSource<bool>();
        MaxBodyHeight = height;
        Width = width;
        Title = title;
        Body = body;
        IsVisible = true;
        return TaskCompletionSource.Task;
    }

    public void Hidden()
    {
        IsVisible = false;
    }

    public bool HiddenShow(bool choise)
    {
        Hidden();
        return choise;
    }

    public Task HiddenAsync(bool choise)
    {
        if (TaskCompletionSource != null)
            TaskCompletionSource.SetResult(choise);
        Hidden();
        return Task.CompletedTask;
    }
}
