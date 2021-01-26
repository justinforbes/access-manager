using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Lithnet.AccessManager.Server;
using Lithnet.AccessManager.Service.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Web;

[assembly: InternalsVisibleTo("Lithnet.AccessManager.Test")]
namespace Lithnet.AccessManager.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "setup")
            {
                Setup.Process(args);
            }
            else
            {
                RegistryProvider registryProvider = new RegistryProvider(false);
                SetupNLog(registryProvider);

                List<Task> tasks = new List<Task>();

                foreach (var host in CreateHosts(args, registryProvider))
                {
                    tasks.Add(host.Build().RunAsync());
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        public static IEnumerable<IHostBuilder> CreateHosts(string[] args, RegistryProvider registryProvider)
        {
            bool safeStart = args.Any(t => string.Equals(t, "/safeStart", StringComparison.OrdinalIgnoreCase));

            if (safeStart || !registryProvider.IsConfigured)
            {
                yield return Host.CreateDefaultBuilder().ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<UnconfiguredHost>();
                })
                    .UseWindowsService()
                    .ConfigureAccessManagerLogging();
            }
            else
            {
                yield return CreateHttpRedirectorHostBuilder(args);
                yield return CreateHttpsHostBuilder(args);
            }
        }

        private static IHostBuilder CreateHostBuilderCommon(string[] args)
        {
            var host = new HostBuilder();

            host.UseContentRoot(Directory.GetCurrentDirectory());

            host.ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables(prefix: "DOTNET_");
                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            });

            host.UseNLog();

            host.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.ConfigureAppSettings();

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            });

            host.ConfigureAccessManagerLogging();

            host.UseDefaultServiceProvider((context, options) =>
            {
                var isDevelopment = context.HostingEnvironment.IsDevelopment();
                options.ValidateScopes = isDevelopment;
                options.ValidateOnBuild = isDevelopment;
            });

            host.UseWindowsService();

            return host;
        }

        private static IHostBuilder CreateHttpsHostBuilder(string[] args)
        {
            var host = CreateHostBuilderCommon(args);

            host.ConfigureWebHostDefaults(webBuilder =>
            {
                var httpsysConfig = new ConfigurationBuilder().ConfigureAppSettings().Build();

                webBuilder.UseHttpSys(httpsysConfig);
                webBuilder.UseStartup<Startup>();
            });

            return host;
        }

        private static IHostBuilder CreateHttpRedirectorHostBuilder(string[] args)
        {
            var host = CreateHostBuilderCommon(args);

            host.ConfigureWebHostDefaults(webBuilder =>
            {
                var httpsysConfig = new ConfigurationBuilder().ConfigureAppSettings().Build();

                webBuilder.UseHttpSysHttpRedirector(httpsysConfig);
                webBuilder.UseStartup<HttpRedirectHostStartup>();
            });

            return host;
        }

        private static void SetupNLog(RegistryProvider registryProvider)
        {
            var configuration = new NLog.Config.LoggingConfiguration();

            var jitWorkerLog = new NLog.Targets.FileTarget("access-manager-jitworker")
            {
                FileName = Path.Combine(registryProvider.LogPath, "access-manager-jit-worker.log"),
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Date,
                MaxArchiveFiles = registryProvider.RetentionDays,
                Layout = "${longdate}|${level:uppercase=true:padding=5}|${logger}|${message}${onexception:inner=${newline}${exception:format=ToString}}"
            };

            var serviceLog = new NLog.Targets.FileTarget("access-manager-service")
            {
                FileName = Path.Combine(registryProvider.LogPath, "access-manager-service.log"),
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Date,
                MaxArchiveFiles = registryProvider.RetentionDays,
                Layout = "${longdate}|${level:uppercase=true:padding=5}|${logger}|${message}${onexception:inner=${newline}${exception:format=ToString}}"
            };

            configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, jitWorkerLog, "Lithnet.AccessManager.Server.Workers.JitGroupWorker", true);
            configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, serviceLog);

            NLog.LogManager.Configuration = configuration;
        }
    }
}
