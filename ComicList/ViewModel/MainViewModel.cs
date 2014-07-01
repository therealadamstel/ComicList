using ComicList.Fetcher;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

namespace ComicList.ViewModel {
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase {
        private bool _isLoadingComics;

        public bool IsLoadingComics {
            get { return _isLoadingComics; }
            set { _isLoadingComics = value; RaisePropertyChanged( () => IsLoadingComics ); }
        }

        public ObservableCollection<ComicEntry> WeeklyComics { get; set; }
        public ObservableCollection<ComicEntry> MyComics { get; set; }
        public ObservableCollection<string> PersonalComicList { get; set; }
        public CollectionView GroupedMyComics { get; set; }
        public CollectionView GroupedWeeklyComics { get; set; }
        public ICommand LoadWeeklyComicsCommand { get { return new RelayCommand( LoadWeeklyComics ); } }
        public ICommand AddMyComicCommand { get { return new RelayCommand<ComicEntry>( AddMyComic ); } }
        public ICommand AddMyTitleCommand { get { return new RelayCommand<string>( AddMyTitle ); } }
        public ICommand RemoveMyComicCommand { get { return new RelayCommand<string>( RemoveMyComic ); } }
        public ICommand SaveMyTitlesCommand { get { return new RelayCommand( SaveMyTitles ); } }
        public bool OmitVariantCovers { get; set; }
        public bool FirstPrintOnly { get; set; }

        public MainViewModel() {
            this.OmitVariantCovers = true;
            this.FirstPrintOnly = true;
            this.WeeklyComics = new ObservableCollection<ComicEntry>();
            this.GroupedWeeklyComics = (CollectionView) CollectionViewSource.GetDefaultView( this.WeeklyComics );

            PropertyGroupDescription groupDescription = new PropertyGroupDescription( "Publisher" );
            this.GroupedWeeklyComics.GroupDescriptions.Add( groupDescription );

            this.MyComics = new ObservableCollection<ComicEntry>();
            this.GroupedMyComics = (CollectionView) CollectionViewSource.GetDefaultView( this.MyComics );
            this.GroupedMyComics.GroupDescriptions.Add( groupDescription );

            LoadPersonalList();
        }

        private void LoadPersonalList() {
            if( PersonalComicList == null )
                PersonalComicList = new ObservableCollection<string>();
            PersonalComicList.Clear();

            if( Properties.Settings.Default.MyComicTitles != null ) {
                foreach( string title in Properties.Settings.Default.MyComicTitles ) {
                    PersonalComicList.Add( title );
                }
            }
        }

        public async void LoadWeeklyComics() {
            IsLoadingComics = true;

            await LoadComics();

            FilterComicsByPersonalizedList();

            IsLoadingComics = false;
        }

        private void AddMyComic(ComicEntry entry) {
            AddMyTitle( entry.SeriesTitle );
        }

        private void SaveMyTitles() {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Text Files (*.txt)|*.txt";
            if( saveDialog.ShowDialog() ?? false ) {
                string path = saveDialog.FileName;
                using( var writer = new StreamWriter( path, false ) ) {
                    foreach( var entry in MyComics.OrderBy( x => x.SeriesTitle ) ) {
                        writer.WriteLine( entry.SeriesTitle + " #" + entry.IssueNumber );
                    }
                }
            }
        }

        private void AddMyTitle( string title ) {
            if( Properties.Settings.Default.MyComicTitles == null )
                Properties.Settings.Default.MyComicTitles = new StringCollection();

            if( !Properties.Settings.Default.MyComicTitles.Cast<string>().Any( x => x.Equals( title, System.StringComparison.OrdinalIgnoreCase ) ) ) {
                Properties.Settings.Default.MyComicTitles.Add( title );
                Properties.Settings.Default.Save();

                LoadPersonalList();
                FilterComicsByPersonalizedList();
            }
        }

        private void RemoveMyComic( string title ) {
            if( Properties.Settings.Default.MyComicTitles.Cast<string>().Any( x => x.Equals( title, System.StringComparison.OrdinalIgnoreCase ) ) ) {
                var item = Properties.Settings.Default.MyComicTitles.Cast<string>().Where( x => x.ToLower() == title.ToLower() ).First();
                Properties.Settings.Default.MyComicTitles.Remove( item );
                Properties.Settings.Default.Save();

                LoadPersonalList();
                FilterComicsByPersonalizedList();
            }
        }

        private void FilterComicsByPersonalizedList() {
            var query = from entry in WeeklyComics
                        join myComic in PersonalComicList on entry.SeriesTitle.ToLower().Trim() equals myComic.ToLower().Trim()
                        select entry;
            MyComics.Clear();
            foreach( var match in query ) {
                MyComics.Add( match );
            }
        }

        private async Task LoadComics() {
            var listFetch = new ListFetch();
            await listFetch.Fetch();

            var list = listFetch.GetLists().First();

            if( FirstPrintOnly )
                list.AddShouldBefirstPrintFilter();
            if( OmitVariantCovers )
                list.OmitVariantCovers();

            var comics = list.GetEntries();

            WeeklyComics.Clear();
            foreach( var comic in comics ) {
                WeeklyComics.Add( comic );
            }
        }
    }
}