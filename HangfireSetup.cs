using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
namespace QiYeWeiXinNotify
{
    /// <summary>
    ///     任务调度
    /// </summary>
    public static class HangfireSetup
    {
        private static IConfigurationRoot _configuration;
        public static IConfigurationRoot Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                    _configuration = builder.Build();
                }
                return _configuration;
            }
            set
            {
                _configuration = value;
            }
        }
        public static void AddHangfireSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.AddHangfire(configuration => configuration
                //.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)//此方法 只初次创建数据库使用即可
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(HangfireSetup.Configuration["HangFire:Connection"], new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true // Migration to Schema 7 is required

                    ////TransactionIsolationLevel = (System.Data.IsolationLevel?)IsolationLevel.ReadCommitted, //事务隔离级别。默认是读取已提交
                    //QueuePollInterval = TimeSpan.FromSeconds(15), //- 作业队列轮询间隔。默认值为15秒。
                    //JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    //CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    //PrepareSchemaIfNecessary = false, // 如果设置为true，则创建数据库表。默认是true
                    //DashboardJobListLimit = 50000,
                    //TransactionTimeout = TimeSpan.FromMinutes(1),
                    ////TablesPrefix = "Hangfire"
                }));
            services.AddHangfireServer(opt =>
            {
                opt.Queues = new[] { "default","msg", "test" }; //队列名称，只能为小写
                opt.WorkerCount = Environment.ProcessorCount * 5; //并发任务
                opt.ServerName = "HangfireServer"; //代表服务名称
            });
        }
    }
}