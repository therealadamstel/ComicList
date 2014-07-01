using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicList.Fetcher {
    public class ComicList {
        private List<Func<ComicEntry, bool>> _filters;

        public DateTime Date { get; set; }
        public string Title { get; set; }
        public List<ComicEntry> Comics { get; set; }

        public ComicList() {
            Comics = new List<ComicEntry>();
            _filters = new List<Func<ComicEntry, bool>>();
        }

        public void AddFilter( Func<ComicEntry, bool> filter ) {
            _filters.Add( filter );
        }

        public void AddShouldBefirstPrintFilter() {
            AddFilter( entry => entry.Title.IndexOf( "printing", StringComparison.OrdinalIgnoreCase ) == -1 );
        }

        public void OmitVariantCovers() {
            AddFilter( entry => entry.Title.IndexOf( "variant", StringComparison.OrdinalIgnoreCase ) == -1 );
            AddFilter( RestrictToRegularCoverIfItExists );
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
                else {
                    return entry == sameTitleComics.First();
                }
            }
            else {
                return true;
            }
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
