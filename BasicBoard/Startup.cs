using BasicBoard.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicBoard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //DI
            //Identity
            //Session - ���񽺿� ���
            services.AddSession();
            //Web API �̵����
            services.AddControllersWithViews();

            //��Ű 
            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.EventsType = typeof(CustomCookieAuthenticationEvents);
            });

            //�۾��� ���� �� ���� �ش� Ŭ������ ȣ���Ѵ�
            services.AddScoped<CustomCookieAuthenticationEvents>();

            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

            //https://dev.mysql.com/doc/dev/connector-net/8.0/html/M_Microsoft_EntityFrameworkCore_MySQLDbContextOptionsExtensions_UseMySQL.htm
            services.AddDbContext<BasicboardDbContext>(options =>
            options.UseMySql(Configuration.GetConnectionString("BasicboardDbContext"), serverVersion, mySqlOptionsAction: sqlOptions =>
            {   //DB ���ῡ ������ ��� ��õ� ����
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            })
            .EnableSensitiveDataLogging() //���� ���α׷� �����͸� ���� �޽���, �α� � ������ �� �ֵ��� ��
            .EnableDetailedErrors() //����� ���� ����� ó�� �ϴ� ���� �߻� �ϴ� ������ �� ���ܸ� ó���� �� �ڼ��� ������ ���
            );

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            //�� Application ���� ����ϰڴ� ����
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
