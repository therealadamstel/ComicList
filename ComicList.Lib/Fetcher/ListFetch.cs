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
using System.Xml.Linq;
using System.Net;

namespace ComicList.Lib.Fetcher {
    public class ListFetch {
        private string _url = "http://feeds.feedburner.com/ncrl";
        private List<DatedComicList> _comicLists;

        public ListFetch() {
            _comicLists = new List<DatedComicList>();
        }

        public IEnumerable<DatedComicList> GetLists() {
            return _comicLists;
        }

        public async Task Fetch() {
            _comicLists.Clear();

            string xml = await ReadXmlFromComicListDotCom();

            XmlDocument document = new XmlDocument();
            XmlNamespaceManager mgr = new XmlNamespaceManager( document.NameTable );
            mgr.AddNamespace( "atom", "http://www.w3.org/2005/Atom" );
            document.LoadXml( xml );

            foreach( XmlElement entry in document.SelectNodes( "//atom:entry", mgr ) ) {
                DatedComicList list = new DatedComicList() {
                    Date = DateTime.Parse( entry.SelectSingleNode( "atom:published", mgr ).InnerText ),
                    Title = entry.SelectSingleNode( "atom:title", mgr ).InnerText
                };
                if( list.Title.StartsWith( "ComicList: " ) ) {
                    list.Title = list.Title.Substring( "ComicList: ".Length );
                }
                if( list.Title.EndsWith( " (CSV)" ) ) {
                    list.Title = list.Title.Substring( 0, list.Title.Length - " (CSV)".Length );
                }

                var content = ReadFormattedContent( mgr, entry );

                using( CsvHelper.CsvReader csvReader = CreateCsvReader( content ) ) {
                    list.Comics.AddRange( csvReader.GetRecords<ComicEntry>() );
                }

                _comicLists.Add( list );
            }
        }

        private async Task<string> ReadXmlFromComicListDotCom() {
            var webClient = new WebClient();
            var stream = await webClient.OpenReadTaskAsync( new Uri( _url ) ); // use await so we're not waiting for the connect
            StreamReader reader = new StreamReader( stream );
            string xml = await reader.ReadToEndAsync(); // use await in case the server isn't buffering
            reader.Close();
            webClient.Dispose();
            return xml;
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
