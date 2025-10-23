using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace ProjectAssistant.DataModel.Models.Configurations;

public class SystemSettings
{
    public string SyncfusionLicenseKey { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
    public SystemInformation SystemInformation { get; set; }
    public AILicenseKey AILicenseKey { get; set; }
}

public class AILicenseKey
{
    public string AzureOpenAIEndPoint { get; set; }
    public string AzureOpenAIKey { get; set; }
    public string AzureOpenAIModelName { get; set; }
    public string AzureStorageContainerName { get; set; }
    public string AzureStorageAccountName { get; set; }
    public string AzureStorageConnectionString { get; set; }
    public string AzureSpeechServiceSubscriptionKey { get; set; }
    public string AzureSpeechServiceEndPoint { get; set; }

}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; }
    public string SQLiteDefaultConnection { get; set; }

}
public class SystemInformation
{
    public string SystemVersion { get; set; }
    public string SystemName { get; set; }
    public string SystemDescription { get; set; }
}
