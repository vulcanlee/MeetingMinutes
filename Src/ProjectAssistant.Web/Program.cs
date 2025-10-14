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


            #region EF Core �ŧi
            var ctmsSettings = builder.Configuration
                .GetSection(nameof(SystemSettings))
                .Get<SystemSettings>();
            var SQLiteDefaultConnection = ctmsSettings.ConnectionStrings.SQLiteDefaultConnection;

            builder.Services.AddDbContext<BackendDBContext>(options =>
                options.UseSqlite(SQLiteDefaultConnection),
                ServiceLifetime.Scoped);
            #endregion


            #region ���U�M�׫Ȩ�Ϊ��A��

            #region Repository
            builder.Services.AddTransient<RoleViewService>();
            builder.Services.AddTransient<MyUserService>();
            #endregion

            #region ViewModel
            builder.Services.AddTransient<CounterViewModel>();
            #endregion

            #region Other ��L
            builder.Services.AddTransient<SystemSettingsConfigurationService>();
            #endregion

            #endregion

            #region �[�J�]�w�j���O�`�J�ŧi
            builder.Services.Configure<SystemSettings>(builder.Configuration
                .GetSection(nameof(SystemSettings)));
            #endregion

            #region AutoMapper �ϥΪ��ŧi
            builder.Services.AddAutoMapper(c => c.AddProfile<AutoMapping>());
            #endregion

            var app = builder.Build();

            #region ��Ʈw�� Migration
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
