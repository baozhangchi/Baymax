using System.Configuration;
using System.Runtime.CompilerServices;

namespace Baymax.Logic
{
    internal static class Config
    {
        public static string ChromeDriverPath { get; set; }

        public static string GetAppSetting([CallerMemberName] string key=null)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
