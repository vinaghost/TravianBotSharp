using FluentValidation;
using MainCore.Notification;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Serilog;
using Serilog.Templates;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Serilog;

namespace MainCore
{
    public static class DependencyInjection
    {
        private const string _connectionString = "DataSource=TBS.db;Cache=Shared";

        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddPooledDbContextFactory<AppDbContext>(
                options => options
#if DEBUG
                    .EnableSensitiveDataLogging()
#endif
                    .UseSqlite(_connectionString)
            );

            services
                .AddMainCore()
                .AddValidator()
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<AppDbContext>();
                });
            services
                .AddTransient(typeof(Publisher<>))
                .AddMainCoreHandlers(ServiceLifetime.Transient);

            return services;
        }

        public static IServiceCollection AddValidator(this IServiceCollection services)
        {
            // Validators
            services
                .AddTransient<IValidator<AccountInput>, AccountInputValidator>()
                .AddTransient<IValidator<AccessInput>, AccessInputValidator>()
                .AddTransient<IValidator<AccountSettingInput>, AccountSettingInputValidator>()
                .AddTransient<IValidator<VillageSettingInput>, VillageSettingInputValidator>()
                .AddTransient<IValidator<NormalBuildInput>, NormalBuildInputValidator>()
                .AddTransient<IValidator<ResourceBuildInput>, ResourceBuildInputValidator>();
            return services;
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

                   Log.Logger = new LoggerConfiguration()
                        .Filter.ByExcluding("SourceContext like 'ReactiveUI.POCOObservableForProperty' and Contains(@m, 'WhenAny will only return a single value')")
                        .WriteTo.Map("Account", "Other", (acc, wt) =>
                        {
                            wt.File(new ExpressionTemplate("[{@t:HH:mm:ss} {@l:u3}{#if SourceContext is not null} ({SourceContext}){#end}] {@m}\n{@x}"),
                                path: $"./logs/log-{acc}-.txt",
                                rollingInterval: RollingInterval.Day);
                            wt.LogSink();
                        })
                        .CreateLogger();

                   resolver.UseSerilogFullLogger();
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