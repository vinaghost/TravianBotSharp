using FluentValidation;
using MainCore.Common;
using MainCore.Infrasturecture.Persistence;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace MainCore
{
    public static class DependencyInjection
    {
        private const string _connectionString = "DataSource=TBS.db;Cache=Shared";

        public static IServiceCollection AddCoreServices(this IServiceCollection services, ServerEnums server)
        {
            services.AddDbContextFactory<AppDbContext>(
                options => options
#if DEBUG
                    .EnableSensitiveDataLogging()
#endif
                    .UseSqlite(_connectionString)
            );

            services
                .AutoRegister(server)
                .AddValidator()
                .AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<AppDbContext>();
                });
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
                .AddTransient<IValidator<FarmListSettingInput>, FarmListSettingInputValidator>()
                .AddTransient<IValidator<ResourceBuildInput>, ResourceBuildInputValidator>();
            return services;
        }

        public static IServiceProvider Setup() => Setup(Constants.Server);

        public static IServiceProvider Setup(ServerEnums server)
        {
            var host = Host.CreateDefaultBuilder()
               .ConfigureServices((_context, services) =>
               {
                   services.UseMicrosoftDependencyResolver();
                   var resolver = Locator.CurrentMutable;
                   resolver.InitializeSplat();
                   resolver.InitializeReactiveUI();

                   services
                       .AddCoreServices(server);
               })
               .UseDefaultServiceProvider(config =>
               {
                   config.ValidateOnBuild = true;
               })
               .Build();
            var container = host.Services;
            container.UseMicrosoftDependencyResolver();
            return container;
        }
    }
}