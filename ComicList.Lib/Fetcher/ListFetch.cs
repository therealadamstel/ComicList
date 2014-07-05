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

            string htmlLastWeek = await lastWeekTask;
            string htmlThisWeek = await thisWeekTask;
            string htmlNextWeek = await nextWeekTask;

            _comicLists.Add( ParseComicList( htmlLastWeek ) );
            _comicLists.Add( ParseComicList( htmlThisWeek ) );
            _comicLists.Add( ParseComicList( htmlNextWeek ) );
        }

        private DatedComicList ParseComicList( string html ) {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml( html );

            string title = doc.DocumentNode.SelectSingleNode( "//h3[@class='bTitle']" ).InnerText;
            if( title.StartsWith( "ComicList: " ) ) {
                title = title.Substring( "ComicList: ".Length );
            }
            if( title.EndsWith( "(1 Week Out)" ) ) {
                title = title.Substring( 0, title.Length - "(1 Week Out)".Length );
            }
            title = title.Trim();

            DatedComicList list = new DatedComicList();
            list.Title = title;
            list.Date = DateTime.Parse( Regex.Match(title, @"\d{2}/\d{2}/\d{4}").Value);

            foreach( var publisherNode in doc.DocumentNode.SelectNodes( "//p/a[starts-with(@href, 'http://www.shareasale.com/')]/b/u" ) ) {
                string publisher = publisherNode.InnerText;

                HtmlNode firstParent = publisherNode.Ancestors( "p" ).First();
                foreach( var comicNode in firstParent.SelectNodes( "a[starts-with(@href, 'http://www.shareasale.com/')]" ).Skip(1) ) {
                    string href = comicNode.Attributes["href"].Value;
                    string comicTitle = comicNode.InnerText;

                    ComicEntry entry = new ComicEntry() {
                        Title = comicTitle,
                        Publisher = publisher,
                        ReleaseDate = list.Date,
                        Price = "",
                        Url = HttpUtility.HtmlDecode( href )
                    };
                    list.Comics.Add( entry );   
                }
            }
            
            return list;
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
