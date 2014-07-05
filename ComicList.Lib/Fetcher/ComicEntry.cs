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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace ComicList.Lib.Fetcher {
    public class ComicEntry {
        public DateTime ReleaseDate { get; set; }
        public string Publisher { get; set; }
        public string Title { get; set; }
        public string Price { get; set; }
        public string Url { get; set; }

        public string SeriesTitle {
            get {
                if( Title.IndexOf( "#" ) > -1 ) {
                    return Title.Substring( 0, Title.IndexOf( "#" ) );
                }
                return Title;
            }
        }

        public string IssueNumber {
            get {
                if( Title.IndexOf( "#" ) > -1 ) {
                    return Title.Substring( Title.IndexOf( "#" ) + 1 );
                }
                return Title;
            }
        }

        public class Map : CsvClassMap<ComicEntry> {
            public Map() {
                Map( m => m.ReleaseDate ).Name( "RELEASE DATE" );
                Map( m => m.Publisher ).Name( "PUBLISHER" );
                Map( m => m.Title ).Name( "TITLE" );
                Map( m => m.Price ).Name( "PRICE" );
            }
        }
    }
}
