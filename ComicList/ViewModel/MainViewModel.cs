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
using ComicList.Lib.Fetcher;
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
using ComicList.Lib.Configuration;
using System.Diagnostics;
using System.Web;

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
        private SystemSettings _systemSettings;
        private DatedComicList _selectedComicList;

        public DatedComicList SelectedComicList {
            get { return _selectedComicList; }
            set { 
                _selectedComicList = value; 
                RaisePropertyChanged( () => SelectedComicList );
                LoadComicEntries();
            }
        }

        public bool IsLoadingComics {
            get { return _isLoadingComics; }
            set { _isLoadingComics = value; RaisePropertyChanged( () => IsLoadingComics ); }
        }

        public ObservableCollection<ComicList.Lib.Fetcher.DatedComicList> ComicLists { get; set; }
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
        public ICommand RefreshCurrentListCommand { get { return new RelayCommand( LoadComicEntries ); } }
        public ICommand FilterCurrentListCommand { get { return new RelayCommand<string>( LoadComicEntries ); } }
        public ICommand ViewComicCommand { get { return new RelayCommand<ComicEntry>( ViewComic, CanViewComic ); } }
        public bool OmitVariantCovers { get; set; }
        public bool FirstPrintOnly { get; set; }

        public MainViewModel() {
            this._systemSettings = SystemSettings.Load();

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
            LoadSavedLists();
        }

        private void LoadSavedLists() {
            if( ComicLists == null )
                ComicLists = new ObservableCollection<Lib.Fetcher.DatedComicList>();
            else
                ComicLists.Clear();

            foreach( var list in _systemSettings.ComicLists.OrderByDescending( x => x.Date ) ) {
                ComicLists.Add( list );
            }
        }

        private void LoadPersonalList() {
            if( PersonalComicList == null )
                PersonalComicList = new ObservableCollection<string>();
            PersonalComicList.Clear();

            foreach( var userComicSelection in _systemSettings.UserComicSelection ) {
                PersonalComicList.Add( userComicSelection.TitleText );
            }
        }

        public async void LoadWeeklyComics() {
            IsLoadingComics = true;

            await LoadComics();

            FilterComicsByPersonalizedList();

            IsLoadingComics = false;
        }

        private bool CanViewComic( ComicEntry entry ) {
            return !string.IsNullOrWhiteSpace( entry.Url );
        }

        private void ViewComic( ComicEntry entry ) {
            Process.Start( entry.Url );
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
            _systemSettings.AddUserComicSelection( new UserComicSelection() { TitleText = title } );
            _systemSettings.Save();

            LoadPersonalList();
            FilterComicsByPersonalizedList();
        }

        private void RemoveMyComic( string title ) {
            _systemSettings.RemoveUserComicSelection( new UserComicSelection() { TitleText = title } );
            _systemSettings.Save();

            LoadPersonalList();
            FilterComicsByPersonalizedList();
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

            SelectedComicList = null;

            ComicLists.Clear();
            foreach( var list in listFetch.GetLists() ) {
                _systemSettings.AddComicList( list );
            }
            _systemSettings.Save();

            LoadSavedLists();
        }

        private void LoadComicEntries()
        {
            LoadComicEntries(null);
        }

        private void LoadComicEntries(string filter) {
            if( SelectedComicList == null ) {
                WeeklyComics.Clear();
            }
            else {
                SelectedComicList.ClearFilters();
                if( FirstPrintOnly )
                    SelectedComicList.AddShouldBefirstPrintFilter();
                if( OmitVariantCovers )
                    SelectedComicList.OmitVariantCovers();
                if (!string.IsNullOrEmpty(filter))
                    SelectedComicList.AddFilter(entry => entry.Title.ToLower().Contains(filter.ToLower()));

                WeeklyComics.Clear();
                foreach( var comic in SelectedComicList.GetEntries() ) {
                    WeeklyComics.Add( comic );
                }
            }

            FilterComicsByPersonalizedList();
        }
    }
}