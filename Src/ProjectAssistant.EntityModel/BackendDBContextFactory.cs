using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectAssistant.EntityModel;

public class BackendDBContextFactory : IDesignTimeDbContextFactory<BackendDBContext>
{
    public BackendDBContext CreateDbContext(string[] args)
    {
        // 讀取 appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = string.Empty;
        connectionString = configuration
                .GetSection("SystemSettings")
                .GetSection("ConnectionStrings")
                .GetSection("SQLiteDefaultConnection")
                .Value!; // 修正為使用 Value 屬性來取得設定值

        var optionsBuilder = new DbContextOptionsBuilder<BackendDBContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new BackendDBContext(optionsBuilder.Options);
    }
}
