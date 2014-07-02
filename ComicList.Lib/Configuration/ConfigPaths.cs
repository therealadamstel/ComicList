//Weekly Comic Listing
//Copyright (C) 2014  Adam Sills

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
