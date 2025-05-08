using MainCore.Commands.Behaviors;
using MainCore.Tasks.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Templates;
using Splat.Microsoft.Extensions.DependencyInjection;
using System.Reflection;

[assembly: Behaviors(
    typeof(CommandLoggingBehavior<,>),
    typeof(AccountTaskBehavior<,>),
    typeof(VillageTaskBehavior<,>)
)]

namespace MainCore
{
    public static class DependencyInjection
    {
        private const string _connectionString = "DataSource=TBS.db;Cache=Shared";

        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(
                options => options
#if DEBUG
                    .EnableSensitiveDataLogging()
#endif
                    .UseSqlite(_connectionString)
            );

            services
                .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton)
                .AddMainCore()
                .AddMainCoreBehaviors()
                .AddMainCoreHandlers();

            services
                .AddScoped<IChromeBrowser>(sp =>
                {
                    var dataService = sp.GetRequiredService<IDataService>();
                    if (dataService.AccountId == AccountId.Empty) throw new InvalidOperationException("AccountId is empty");
                    var chromeManager = sp.GetRequiredService<IChromeManager>();
                    return chromeManager.Get(dataService.AccountId);
                })
                .AddScoped<ILogger>(sp =>
                {
                    var dataService = sp.GetRequiredService<IDataService>();
                    var accountId = dataService.AccountId;
                    if (accountId == AccountId.Empty) return Log.Logger;

                    var logService = sp.GetRequiredService<ILogService>();
                    if (logService.Loggers.ContainsKey(accountId)) return logService.Loggers[accountId];
                    var context = sp.GetRequiredService<AppDbContext>();
                    var account = context.Accounts
                        .Where(x => x.Id == accountId.Value)
                        .First();

                    var uri = new Uri(account.Server);
                    var logger = Log.ForContext("Account", $"{account.Username}_{uri.Host}")
                                    .ForContext("AccountId", accountId);
                    logService.Loggers.Add(accountId, logger);
                    logger.Information("===============> Current version: {Version} <===============", GetVersion());

                    return logger;
                });

            return services;
        }

        private static string GetVersion()
        {
            var versionAssembly = Assembly.GetExecutingAssembly().GetName().Version;
            var version = new Version(versionAssembly.Major, versionAssembly.Minor, versionAssembly.Build);
            return $"{version}";
        }

        public static IServiceProvider Setup()
        {
            var host = Host.CreateDefaultBuilder()
               .ConfigureServices((_context, services) =>
               {
                   services.UseMicrosoftDependencyResolver();
                   var resolver = Locator.CurrentMutable;
                   resolver.InitializeSplat();
                   resolver.InitializeReactiveUI();
                   services.AddCoreServices();
                   services.AddSerilog(c =>
                   {
                       c.Filter.ByExcluding("SourceContext like 'ReactiveUI.POCOObservableForProperty' and Contains(@m, 'WhenAny will only return a single value')");
                       c.WriteTo.Map("Account", "Other", (acc, wt) =>
                       {
                           wt.File(new ExpressionTemplate("[{@t:HH:mm:ss} {@l:u3}{#if SourceContext is not null} ({SourceContext}){#end}] {@m}\n{@x}"),
                               path: $"./logs/log-{acc}-.txt",
                               rollingInterval: RollingInterval.Day);
                           wt.LogSink();
                       });
                   });
               })
               .UseDefaultServiceProvider(config =>
               {
                   config.ValidateOnBuild = true;
                   config.ValidateScopes = true;
               })
               .Build();

            var container = host.Services;

            container.UseMicrosoftDependencyResolver();
            return container;
        }
    }
}