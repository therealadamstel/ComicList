using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicList.Lib {
    public static class ObservableCollectionExtensions {
        public static void Sort<T, TKey>( this ObservableCollection<T> collection, Func<T, TKey> sortFieldSelector ) {
            var list = collection.OrderBy( sortFieldSelector ).ToList();

            collection.Clear();

            foreach( var entry in list ) {
                collection.Add( entry );
            }
        }
    }
}
