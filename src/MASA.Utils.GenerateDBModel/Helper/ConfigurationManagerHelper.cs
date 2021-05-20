using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.IO;

namespace MASA.Utils.GenerateDBModel.Helper
{
    public static class ConfigurationManagerHelper
    {
        public static readonly IConfiguration Configuration;
        static ConfigurationManagerHelper()
        {
            // 读取环境变量
            var provider = new EnvironmentVariablesConfigurationProvider();
            provider.Load();
            provider.TryGet("ASPNETCORE_ENVIRONMENT", out string environmentName);

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile(path: $"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public static string GetValue(string key) => Configuration[key];

        public static T GetValue<T>(string key) => Configuration.GetValue<T>(key);

        public static string GetConnectionString(string name) => Configuration.GetConnectionString(name);
    }
}
