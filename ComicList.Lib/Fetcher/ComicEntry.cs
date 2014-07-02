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
