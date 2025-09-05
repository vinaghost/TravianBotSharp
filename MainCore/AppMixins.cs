using MainCore.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveMarbles.Extensions.Hosting.AppServices;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Splat.Microsoft.Extensions.DependencyInjection;

[assembly: Behaviors(
    typeof(AccountDataLoggingBehavior<,>),
    typeof(TaskNameLoggingBehavior<,>),
    typeof(CommandLoggingBehavior<,>),
    typeof(ErrorLoggingBehavior<,>),
    typeof(AccountTaskBehavior<,>),
    typeof(VillageTaskBehavior<,>)
)]

namespace MainCore
{
    public static class AppMixins
    {
        private static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureServices(services =>
            {
                services.AddSerilog(c => c
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Filter.ByExcluding("SourceContext like 'ReactiveUI.POCOObservableForProperty' and Contains(@m, 'WhenAny will only return a single value')")
                    .WriteTo.Map("Account", "Other", (acc, wt) =>
                    {
                        wt.File(new ExpressionTemplate("[{@t:HH:mm:ss} {@l:u3}{#if SourceContext is not null} ({SourceContext}){#end}] {@m}\n{@x}"),
                            path: $"./logs/log-{acc}-.txt",
                            rollingInterval: RollingInterval.Day);
                        wt.LogSink();
                    })
                    .Enrich.FromLogContext());
            });

        private const string _connectionString = "DataSource=TBS.db;Cache=Shared";

        private static IHostBuilder ConfigureDbContext(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddDbContext<AppDbContext>(options => options
                    .EnableSensitiveDataLogging(hostContext.HostingEnvironment.IsDevelopment())
                    .UseSqlite(_connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
            });

        public static IHostBuilder ConfigureServices(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureServices(services =>
            {
                services.AddMainCore();
                services.AddValidatorsFromAssembly(typeof(AppMixins).Assembly, ServiceLifetime.Singleton);
                services.AddMainCoreBehaviors();
                services.AddMainCoreHandlers();

                services.AddScoped<IBrowser>(sp =>
                {
                    var dataService = sp.GetRequiredService<IDataService>();
                    if (dataService.AccountId == AccountId.Empty) throw new InvalidOperationException("AccountId is empty");
                    var browserManager = sp.GetRequiredService<IBrowserManager>();
                    var logger = Log
                        .ForContext("Account", dataService.AccountData)
                        .ForContext("AccountId", dataService.AccountId);
                    var browser = browserManager.Get(dataService.AccountId);
                    browser.Logger = logger;
                    return browser;
                });
            });

        private static IHostBuilder ConfigureSplatForMicrosoftDependencyResolver(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureServices((serviceCollection) =>
            {
                serviceCollection.UseMicrosoftDependencyResolver();
                var resolver = Locator.CurrentMutable;
                resolver.InitializeSplat();
                resolver.InitializeReactiveUI();
            });

        public static IHostBuilder GetHostBuilder()
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureSplatForMicrosoftDependencyResolver()
                .ConfigureSingleInstance(builder =>
                {
                    builder.MutexId = "{hcmmn304-1975-4a2d-afed-615d4a318283}";
                    builder.WhenNotFirstInstance = (hostingEnvironment, logger) =>
                    {
                        logger.LogWarning("Application {0} already running.", hostingEnvironment.ApplicationName);
                    };
                })
                .ConfigureLogging()
                .ConfigureDbContext()
                .ConfigureServices();
            return hostBuilder;
        }
    }
}
