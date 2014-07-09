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
using ComicList.Lib.Fetcher;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicList.Lib.Configuration {
    public class SystemSettings : ObservableObject {
        public string OverrideSettingsFolderPath { get; set; }

        [JsonIgnore]
        public SystemCatalog Catalog { get; private set; }

        [JsonIgnore]
        [Obsolete("Use Catalog.ComicLists now", true)]
        public ObservableCollection<DatedComicList> ComicLists {
            get;
            set;
        }

        [JsonIgnore]
        [Obsolete( "Use UserComicSelection.ComicLists now", true )]
        public ObservableCollection<UserComicSelection> UserComicSelection {
            get;
            set;
        }

        public SystemSettings() {
        }

        public void MoveCatalog( string directory ) {
            if( directory != OverrideSettingsFolderPath ) {
                // If the catalog already exists at the target path, we should NOT overwrite it
                if( File.Exists( ConfigPaths.GetCatalogFilePath( directory ) ) == false ) {
                    // Move the existing file first
                    File.Move( ConfigPaths.GetCatalogFilePath( OverrideSettingsFolderPath ), ConfigPaths.GetCatalogFilePath( directory ) );
                }
                // Now save settings, but don't save the catalog because we've already handled that
                OverrideSettingsFolderPath = directory;
                Save( false );
            }
        }

        public void Save() {
            Save( true );
        }

        public void Save(bool saveCatalog) {
            string filePath = ConfigPaths.GetConfigurationFilePath();
            if( !Directory.Exists( Path.GetDirectoryName( filePath ) ) ) {
                Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
            }

            using( var textWriter = new StreamWriter( filePath, false ) ) {
                using( var jsonWriter = new JsonTextWriter( textWriter ) ) {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize( jsonWriter, this );
                }
            }

            if( saveCatalog )
                Catalog.Save( OverrideSettingsFolderPath );
        }

        public static SystemSettings Load() {
            if( File.Exists( ConfigPaths.GetOldConfigurationFilePath() ) ) {
                // The old settings file exists, copy it to the two new files
                File.Copy( ConfigPaths.GetOldConfigurationFilePath(), ConfigPaths.GetConfigurationFilePath() );
                File.Move( ConfigPaths.GetOldConfigurationFilePath(), ConfigPaths.GetCatalogFilePath( null ) );
            }

            SystemSettings settings;
            string filePath = ConfigPaths.GetConfigurationFilePath();
            if( File.Exists( filePath ) ) {
                using( var textReader = File.OpenText( filePath ) ) {
                    var jsonReader = new JsonTextReader( textReader );
                    JsonSerializer serializer = new JsonSerializer();
                    settings = serializer.Deserialize<SystemSettings>( jsonReader );
                }
            }
            else {
                settings = new SystemSettings();
            }

            settings.Catalog = SystemCatalog.Load( settings.OverrideSettingsFolderPath );

            return settings;
        }
    }
}
