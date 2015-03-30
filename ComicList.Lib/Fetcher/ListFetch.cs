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
using HtmlAgilityPack;
using System.Text.RegularExpressions;

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

            string baseUrl = "http://www.comiclist.com/index.php/newreleases/";

            var lastWeekTask = ReadWeekListHtml( baseUrl + "last-week" );
            var thisWeekTask = ReadWeekListHtml( baseUrl + "this-week" );
            var nextWeekTask = ReadWeekListHtml( baseUrl + "next-week" );

            // Read the HTML feeds for next week and this/last week links
            string htmlLastWeek = await lastWeekTask;
            string htmlThisWeek = await thisWeekTask;
            string htmlNextWeek = await nextWeekTask;

            var lastWeekFromHtml = ParseComicListHtml( htmlLastWeek );
            var thisWeekFromHtml = ParseComicListHtml( htmlThisWeek );

            // Immediately add next week
            _comicLists.Add( ParseComicListHtml( htmlNextWeek ) );

            // Read the CSV feeds for this week and last week
            string rssXml = await ReadXmlFromComicListDotCom( _url );

            XmlDocument document = new XmlDocument();
            XmlNamespaceManager mgr = new XmlNamespaceManager( document.NameTable );
            mgr.AddNamespace( "atom", "http://www.w3.org/2005/Atom" );
            document.LoadXml( rssXml );

            foreach( XmlElement entry in document.SelectNodes( "//atom:entry", mgr ) ) {
                DatedComicList list = new DatedComicList() {
                    Title = entry.SelectSingleNode( "atom:title", mgr ).InnerText
                };
                list.Title = CleanComicListTitle( list.Title );
                list.Date = ReadDateFromTitle( list.Title );
                var content = ReadFormattedContent( mgr, entry );

                using( CsvHelper.CsvReader csvReader = CreateCsvReader( content ) ) {
                    list.Comics.AddRange( csvReader.GetRecords<ComicEntry>().Where( x => x.IsValid ) );
                }

                if( list.Date == lastWeekFromHtml.Date ) {
                    ApplyUrlsToMatchingIssues( lastWeekFromHtml, list );
                }
                else if( list.Date == thisWeekFromHtml.Date ) {
                    ApplyUrlsToMatchingIssues( thisWeekFromHtml, list );
                }
                _comicLists.Add( list );
            }
        }

        private DateTime ReadDateFromTitle( string title ) {
            Regex regex = new Regex( @"\d{1,2}/\d{1,2}/\d{4}" );
            var match = regex.Match( title );
            return DateTime.Parse( match.Value );
        }

        private void ApplyUrlsToMatchingIssues( DatedComicList thisWeekFromHtml, DatedComicList match ) {
            foreach( var entry in thisWeekFromHtml.Comics ) {
                var matchedEntry = match.Comics.Where( x => x.Title == entry.Title ).FirstOrDefault();
                if( matchedEntry != null ) {
                    matchedEntry.Url = entry.Url;
                }
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

        private async Task<string> ReadXmlFromComicListDotCom(string url) {
            var webClient = new WebClient();
            var stream = await webClient.OpenReadTaskAsync( new Uri( url ) ); // use await so we're not waiting for the connect
            StreamReader reader = new StreamReader( stream );
            string xml = await reader.ReadToEndAsync(); // use await in case the server isn't buffering
            reader.Close();
            webClient.Dispose();
            return xml;
        }

        private DatedComicList ParseComicListHtml( string html ) {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionAutoCloseOnEnd = true;
            doc.LoadHtml( html );

            string title = doc.DocumentNode.SelectSingleNode( "//h3[@class='bTitle']" ).InnerText;
            title = CleanComicListTitle( title );

            DatedComicList list = new DatedComicList();
            list.Title = title;
            list.Date = DateTime.Parse( Regex.Match(title, @"\d{2}/\d{2}/\d{4}").Value);

            foreach( var publisherNode in doc.DocumentNode.SelectNodes( "//p/a/b/u" ) ) {
                string publisher = publisherNode.InnerText;

                HtmlNode firstParent = publisherNode.Ancestors( "p" ).First();
                foreach( var comicNode in firstParent.SelectNodes( "a" ).Skip( 1 ) ) {
                    string href = comicNode.Attributes["href"].Value;
                    string comicTitle = comicNode.InnerText;

                    ComicEntry entry = new ComicEntry() {
                        Title = comicTitle.Trim(),
                        Publisher = publisher.Trim(),
                        ReleaseDate = list.Date,
                        Price = "",
                        Url = HttpUtility.HtmlDecode( href )
                    };
                    list.Comics.Add( entry );   
                }
            }
            
            return list;
        }

        private static string CleanComicListTitle( string title ) {
            if( title.StartsWith( "ComicList: " ) ) {
                title = title.Substring( "ComicList: ".Length );
            }
            if( title.EndsWith( "(1 Week Out)" ) ) {
                title = title.Substring( 0, title.Length - "(1 Week Out)".Length );
            }
            if( title.EndsWith( "(2 Weeks Out)" ) ) {
                title = title.Substring( 0, title.Length - "(2 Weeks Out)".Length );
            }
            if( title.EndsWith( "(CSV)" ) ) {
                title = title.Substring( 0, title.Length - "(CSV)".Length );
            }
            title = title.Trim();
            return title;
        }

        private async Task<string> ReadWeekListHtml( string url ) {
            var webClient = new WebClient();
            var stream = await webClient.OpenReadTaskAsync( new Uri( url ) ); // use await so we're not waiting for the connect
            StreamReader reader = new StreamReader( stream );
            string xml = await reader.ReadToEndAsync(); // use await in case the server isn't buffering
            reader.Close();
            webClient.Dispose();
            return xml;
        }
    }
}
