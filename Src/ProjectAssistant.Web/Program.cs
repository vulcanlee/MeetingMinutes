using AntDesign;
using Microsoft.EntityFrameworkCore;
using ProjectAssistant.Business.Helpers;
using ProjectAssistant.Business.Services.Database;
using ProjectAssistant.Business.Services.Options;
using ProjectAssistant.DataModel.Models.Configurations;
using ProjectAssistant.EntityModel;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Helpers;
using ProjectAssistant.Web.Components;
using ProjectAssistant.Web.ViewModels;

namespace ProjectAssistant.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddAntDesign();


            #region EF Core 宣告
            var ctmsSettings = builder.Configuration
                .GetSection(nameof(SystemSettings))
                .Get<SystemSettings>();
            var SQLiteDefaultConnection = ctmsSettings.ConnectionStrings.SQLiteDefaultConnection;

            builder.Services.AddDbContext<BackendDBContext>(options =>
                options.UseSqlite(SQLiteDefaultConnection),
                ServiceLifetime.Scoped);
            #endregion


            #region 註冊專案客制用的服務

            #region Repository
            builder.Services.AddTransient<RoleViewService>();
            builder.Services.AddTransient<MyUserService>();
            #endregion

            #region ViewModel
            builder.Services.AddTransient<CounterViewModel>();
            #endregion

            #region Other 其他
            builder.Services.AddTransient<SystemSettingsConfigurationService>();
            #endregion

            #endregion

            #region 加入設定強型別注入宣告
            builder.Services.Configure<SystemSettings>(builder.Configuration
                .GetSection(nameof(SystemSettings)));
            #endregion

            #region AutoMapper 使用的宣告
            builder.Services.AddAutoMapper(c => c.AddProfile<AutoMapping>());
            #endregion

            var app = builder.Build();

            #region 資料庫的 Migration
            using var scope = app.Services.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<BackendDBContext>();
            dbContext.Database.Migrate();
            #endregion

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
