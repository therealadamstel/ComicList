
namespace ComicList.ViewModel {
    public class SelectableEntity<T> {
        public T Entity { get; private set; }
        public bool IsSelected { get; set; }

        public SelectableEntity( T entity, bool isSelected ) {
            this.Entity = entity;
            this.IsSelected = isSelected;
        }
    }
}
