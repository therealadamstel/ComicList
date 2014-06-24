using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicList.Fetcher {
    public class ComicList {
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public List<ComicEntry> Comics { get; set; }

        public ComicList() {
            Comics = new List<ComicEntry>();
        }
    }
}
