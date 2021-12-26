using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace QiYeWeiXinNotify
{
    //任务调度中间件
    public static class HangfireMiddleware
    {
       

        public static void UseHangfireMiddleware(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            //app.UseHangfireServer(); //配置服务//ConfigureOptions()
            app.UseHangfireDashboard(HangfireSetup.Configuration["HangFire:PathMatch"], HfAuthor()); //配置面板
            BackgroundJob.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));
            var companyService = ServiceLocator.Instance.GetService<ICompanyRepository>();
            HangfireService(); //配置各个任务
        }
        /// <summary>
        /// 配置账号模板信息
        /// </summary>
        /// <returns></returns>
        public static DashboardOptions HfAuthor()
        {
            var filter = new BasicAuthAuthorizationFilter(
                new BasicAuthAuthorizationFilterOptions
                {
                    SslRedirect = false,
                    RequireSsl = false,
                    LoginCaseSensitive = false,
                    Users = new[]
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login =HangfireSetup.Configuration["HangFire:Login"], //可视化的登陆账号
                            PasswordClear =HangfireSetup.Configuration["HangFire:PasswordClear"]  //可视化的密码
                        }
                    }
                });
            return new DashboardOptions
            {
                Authorization = new[] { filter }
            };
        }
        #region 配置服务
        public static void HangfireService()
        {
            //// "0 0 1 * * ? " 每天凌晨一点执行
            //RecurringJob.AddOrUpdate(() => Console.WriteLine("{0} Recurring job completed successfully!", DateTime.Now.ToString()), "0 0 1 * * ? ", TimeZoneInfo.Local);
            //// "0 0 7 * * ? " 每天早上七点执行定时任务
            //RecurringJob.AddOrUpdate(() => Console.WriteLine("{0} Recurring job completed successfully!", DateTime.Now.ToString()), "0 0 7 * * ? ",
            //    TimeZoneInfo.Local); 
            var companyService = ServiceLocator.Instance.GetService<ICompanyRepository>();
            //每隔5秒执行一次：*/5 * * * * ?
            //RecurringJob.AddOrUpdate("msg",() => companyService.GetCompanies(), "*/5 * * * * ?",TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate("msg", () => companyService.GetHangFireStates(), "0 */3 * * * ?", TimeZoneInfo.Local);
            //RecurringJob.AddOrUpdate("msg", () => companyService.SendMessage(), "0 */30 * * * ?", TimeZoneInfo.Local);

            ////支持基于队列的任务处理：任务执行不是同步的，而是放到一个持久化队列中，以便马上把请求控制权返回给调用者。
            //var jobId = BackgroundJob.Enqueue(() => WriteLog("队列任务执行了！"));

            ////延迟任务执行：不是马上调用方法，而是设定一个未来时间点再来执行，延迟作业仅执行一次
            //var jobId = BackgroundJob.Schedule（() => WriteLog("一天后的延迟任务执行了！"),TimeSpan.FromDays(1));//一天后执行该任务

            ////循环任务执行：一行代码添加重复执行的任务，其内置了常见的时间循环模式，也可基于CRON表达式来设定复杂的模式。【用的比较的多】
            //RecurringJob.AddOrUpdate(() => WriteLog("每分钟执行任务！"), Cron.Minutely); //注意最小单位是分钟

            ////延续性任务执行：类似于.NET中的Task,可以在第一个任务执行完之后紧接着再次执行另外的任务
            //BackgroundJob.ContinueWith(jobId, () => WriteLog("连续任务！"));
        }
        #endregion
    }
}
