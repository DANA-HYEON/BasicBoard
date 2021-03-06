using BasicBoard.Data;
using BasicBoard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity.UI.Services;
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
            //검색 필요
            services.AddTransient<IEmailSender, MailKitEmailSender>();

            //email
            services.Configure<MailKitEmailSenderOptions>(options =>
            {
                options.Host_Address = "smtp.naver.com";
                options.Host_Port = 587;
                options.Host_UserName = "dada4202";
                options.Host_Password = "dlekgus";
                options.Sender_Email = "dada4202@naver.com";
                options.Sender_Name = "basiccboard";
            });

            //Session - 서비스에 등록s
            services.AddSession();

            //Web API 미들웨어
            services.AddControllersWithViews();

            var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

            //https://dev.mysql.com/doc/dev/connector-net/8.0/html/M_Microsoft_EntityFrameworkCore_MySQLDbContextOptionsExtensions_UseMySQL.htm
            services.AddDbContext<BasicboardDbContext>(options =>
            options.UseMySql(Configuration.GetConnectionString("BasicboardDbContext"), serverVersion, mySqlOptionsAction: sqlOptions =>
            {   //DB 연결에 실패할 경우 재시도 설정
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            })
            .EnableSensitiveDataLogging() //응용 프로그램 데이터를 예외 메시지, 로깅 등에 포함할 수 있도록 함
            .EnableDetailedErrors() //저장소 쿼리 결과를 처리 하는 동안 발생 하는 데이터 값 예외를 처리할 때 자세한 오류를 사용
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

            app.UseAuthorization();

            //이 Application 에서 사용하겠다 선언
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
