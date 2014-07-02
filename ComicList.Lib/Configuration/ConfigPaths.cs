using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicList.Lib.Configuration {
    static class ConfigPaths {
        public static string GetConfigurationRootPath() {
            string path = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
            path = Path.Combine( path, "ComicListApp", "Configuration" );

            return path;
        }

        public static string GetConfigurationFilePath() {
            return Path.Combine( GetConfigurationRootPath(), "Settings.json" );
        }

        public static string GetListCachePath() {
            return Path.Combine( GetConfigurationRootPath(), "Lists" );
        }
    }
}
