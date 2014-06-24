using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using CsvHelper.Configuration;

namespace ComicList.Fetcher {
    public class ListFetch {
        private string _url = "http://feeds.feedburner.com/ncrl";
        private List<ComicList> _comicLists;
        private List<Func<ComicEntry, bool>> _filters;

        public ListFetch() {
            _filters = new List<Func<ComicEntry, bool>>();
            _comicLists = new List<ComicList>();
        }

        public IEnumerable<ComicList> GetLists() {
            return _comicLists;
        }

        public IEnumerable<ComicEntry> GetEntries( ComicList list ) {
            var entries = list.Comics.ToList();
            _filters.ForEach( f => entries = entries.Where( f ).ToList() );

            return entries;
        }

        public void AddFilter( Func<ComicEntry, bool> filter ) {
            _filters.Add( filter );
        }

        public void AddShouldBefirstPrintFilter() {
            AddFilter( entry => entry.Title.IndexOf( "printing", StringComparison.OrdinalIgnoreCase ) == -1 );
        }

        public void OmitVariantCovers() {
            AddFilter( entry => entry.Title.IndexOf( "variant", StringComparison.OrdinalIgnoreCase ) == -1 );
        }

        public void ClearFilters() {
            _filters.Clear();
        }

        public void Fetch() {
            _comicLists.Clear();

            XmlDocument document = new XmlDocument();
            XmlNamespaceManager mgr = new XmlNamespaceManager( document.NameTable );
            mgr.AddNamespace( "atom", "http://www.w3.org/2005/Atom" );

            document.Load( _url );

            foreach( XmlElement entry in document.SelectNodes( "//atom:entry", mgr ) ) {
                ComicList list = new ComicList() {
                    Date = DateTime.Parse( entry.SelectSingleNode( "atom:published", mgr ).InnerText ),
                    Title = entry.SelectSingleNode( "atom:title", mgr ).InnerText
                };

                var content = ReadFormattedContent( mgr, entry );

                using( CsvHelper.CsvReader csvReader = CreateCsvReader( content ) ) {
                    list.Comics.AddRange( csvReader.GetRecords<ComicEntry>() );
                }

                _comicLists.Add( list );
            }
        }

        private static CsvHelper.CsvReader CreateCsvReader( string content ) {
            StringReader stringReader = new StringReader( content );
            CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader( stringReader );
            csvReader.Configuration.RegisterClassMap( new ComicEntry.Map() );
            return csvReader;
        }

        private static string ReadFormattedContent( XmlNamespaceManager mgr, XmlElement entry ) {
            var content = entry.SelectSingleNode( "atom:content", mgr ).InnerXml;
            int csvStart = content.IndexOf( "RELEASE DATE,PUBLISHER,TITLE,PRICE" );
            int csvEnd = content.IndexOf( "&lt;/p&gt;", csvStart );
            content = content.Substring( csvStart, csvEnd - csvStart );
            content = content.Replace( "&lt;br /&gt;", "" );
            content = HttpUtility.HtmlDecode( content );
            return content;
        }
    }
}
