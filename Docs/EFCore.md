# 第一次 Migration 

```
Add-Migration Init -Project ProjectAssistant.EntityModel -StartupProject ProjectAssistant.Web 
```

* -Context <String>	

  The DbContext class to use. Class name only or fully qualified with namespaces. If this parameter is omitted, EF Core finds the context class. If there are multiple context classes, this parameter is required.
* -Project <String>	

  The target project. If this parameter is omitted, the Default project for Package Manager Console is used as the target project.
* -StartupProject <String>	

  The startup project. If this parameter is omitted, the Startup project in Solution properties is used as the target project.
* -Args <String>	

  Arguments passed to the application.
* -Verbose	

  Show verbose output.

# 更新資料庫

```
Update-Database -Context BackendDBContext -StartupProject ProjectAssistant.Web -Project ProjectAssistant.EntityModel
```

# 套用 Migration 

```
Add-Migration AddAthleteExamine -Context BackendDBContext -Project ProjectAssistant.EntityModel -StartupProject ProjectAssistant.Web
```

```
dotnet ef migrations add AddAthleteExamine --project ProjectAssistant.EntityModel --startup-project ProjectAssistant.Web
```


# 移除 Migration

```
Remove-Migration -Context BackendDBContext -Project ProjectAssistant.EntityModel -StartupProject ProjectAssistant.Web
```

```
dotnet ef migrations remove --project ProjectAssistant.EntityModel --startup-project ProjectAssistant.Web
```

# 套用移轉

```
Script-Migration -Project ProjectAssistant.EntityModel -StartupProject ProjectAssistant.Web
```

