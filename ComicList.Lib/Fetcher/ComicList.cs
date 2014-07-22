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

namespace ComicList.Lib.Fetcher {
    public class DatedComicList {
        private List<Func<ComicEntry, bool>> _filters;

        public DateTime Date { get; set; }
        public string Title { get; set; }
        public List<ComicEntry> Comics { get; set; }

        public DatedComicList() {
            Comics = new List<ComicEntry>();
            _filters = new List<Func<ComicEntry, bool>>();
        }

        public void AddFilter( Func<ComicEntry, bool> filter ) {
            _filters.Add( filter );
        }

        public void AddShouldBefirstPrintFilter() {
            AddFilter( entry => entry.Title.IndexOf( "printing", StringComparison.OrdinalIgnoreCase ) == -1 || entry.Title.IndexOf( "1st print" ) > -1 );
        }

        public void OmitVariantCovers() {
            AddFilter( entry => entry.Title.IndexOf( "variant", StringComparison.OrdinalIgnoreCase ) == -1 );
            AddFilter( RestrictToRegularCoverIfItExists );
        }

        public void AddPublisherFilter( IEnumerable<string> whitelistPublishers ) {
            if( whitelistPublishers.Any() ) {
                AddFilter( entry => whitelistPublishers.Any( x => x.Equals( entry.Publisher, StringComparison.OrdinalIgnoreCase ) ) );
            }
        }

        private bool RestrictToRegularCoverIfItExists( ComicEntry entry ) {
            var sameTitleComics = Comics.Where( e => e.SeriesTitle.Equals( entry.SeriesTitle, StringComparison.OrdinalIgnoreCase ) ).ToList();
            if( sameTitleComics.Count > 1 ) {
                if( sameTitleComics.Where( x => x.IssueNumber.ToLower().Contains( "regular cover" ) ).Any() ) {
                    return entry.IssueNumber.ToLower().Contains( "regular cover" );
                }
                else if( sameTitleComics.Where( x => x.IssueNumber.ToLower().Contains( "cover a" ) ).Any() ) {
                    return entry.IssueNumber.ToLower().Contains( "cover a" );
                }
            }

            return true;
        }

        public void ClearFilters() {
            _filters.Clear();
        }

        public IEnumerable<ComicEntry> GetEntries() {
            var entries = Comics.ToList();
            _filters.ForEach( f => entries = entries.Where( f ).ToList() );

            return entries;
        }
    }
}
