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
        private ObservableCollection<UserComicSelection> _userComicSelection;
        private ObservableCollection<DatedComicList> _comicLists;

        public ObservableCollection<DatedComicList> ComicLists {
            get { return _comicLists; }
            set { _comicLists = value; }
        }

        public ObservableCollection<UserComicSelection> UserComicSelection {
            get { return _userComicSelection; }
            set { _userComicSelection = value; }
        }

        public SystemSettings() {
            _userComicSelection = new ObservableCollection<UserComicSelection>();
            _comicLists = new ObservableCollection<DatedComicList>();
        }

        public void AddComicList( DatedComicList comicList ) {
            var existing = _comicLists.Where( x => x.Date == comicList.Date ).SingleOrDefault();
            if( existing == null ) {
                _comicLists.Add( comicList );
            }
            else {
                existing.Comics = comicList.Comics;
            }

            // Remove old lists
            foreach( var oldList in _comicLists.OrderByDescending( x => x.Date ).Skip( 10 ).ToArray() ) {
                _comicLists.Remove( oldList );
            }
        }

        public void AddUserComicSelection( UserComicSelection value ) {
            var existing = FindUserComicSelection( value.TitleText );
            if( existing == null ) {
                _userComicSelection.Add( value );
            }
        }

        public void RemoveUserComicSelection( UserComicSelection value ) {
            var existing = FindUserComicSelection( value.TitleText );
            if( existing != null ) {
                _userComicSelection.Remove( existing );
            }
        }

        private UserComicSelection FindUserComicSelection( string titleText ) {
            return _userComicSelection.Where(
                x => x.TitleText.Equals( titleText, StringComparison.OrdinalIgnoreCase ) ).SingleOrDefault();
        }

        public void Save() {
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
        }

        public static SystemSettings Load() {
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

            return settings;
        }
    }
}
