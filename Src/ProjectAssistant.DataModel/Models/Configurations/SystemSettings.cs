using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectAssistant.DataModel.Models.Configurations;

public class SystemSettings
{
    public string SyncfusionLicenseKey { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
    public SystemInformation SystemInformation { get; set; }
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
