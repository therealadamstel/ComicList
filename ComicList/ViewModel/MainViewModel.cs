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
using System.Collections.Generic;

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
        private bool _showPublisherFilter;
        private SystemSettings _systemSettings;
        private DatedComicList _selectedComicList;
        private string _filterText;
        private string _addTitleText;
        private ObservableCollection<ComicEntry> _weeklyComics;
        private ObservableCollection<ComicEntry> _myComics;

        public SystemSettings SystemSettings { get { return _systemSettings; } }

        public IEnumerable<SelectableEntity<string>> AvailablePublishers {
            get;
            set;
        }

        public bool ShowPublisherFilter {
            get { return _showPublisherFilter; }
            set {
                _showPublisherFilter = value;
                RaisePropertyChanged( () => ShowPublisherFilter );
            }
        }

        public string AddTitleText {
            get { return _addTitleText; }
            set { _addTitleText = value; RaisePropertyChanged( () => AddTitleText ); }
        }

        public string FilterText {
            get { return _filterText; }
            set { _filterText = value; RaisePropertyChanged( () => FilterText ); }
        }

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
        public ObservableCollection<ComicEntry> WeeklyComics {
            get { return _weeklyComics; }
            set {
                _weeklyComics = value;
                RaisePropertyChanged( () => WeeklyComics );
            }
        }
        public ObservableCollection<ComicEntry> MyComics {
            get { return _myComics; }
            set {
                _myComics = value;
                RaisePropertyChanged( () => MyComics );
            }
        }
        public ObservableCollection<string> PersonalComicList { get; set; }
        public CollectionView GroupedMyComics { get; set; }
        public CollectionView GroupedWeeklyComics { get; set; }
        public ICommand LoadWeeklyComicsCommand { get { return new RelayCommand( LoadWeeklyComics ); } }
        public ICommand AddMyComicCommand { get { return new RelayCommand<ComicEntry>( AddMyComic ); } }
        public ICommand AddMyTitleCommand { get { return new RelayCommand( AddMyTitle ); } }
        public ICommand RemoveMyComicCommand { get { return new RelayCommand<string>( RemoveMyComic ); } }
        public ICommand SaveMyTitlesCommand { get { return new RelayCommand( SaveMyTitles ); } }
        public ICommand RefreshCurrentListCommand { get { return new RelayCommand( LoadComicEntries ); } }
        public ICommand ViewComicCommand { get { return new RelayCommand<ComicEntry>( ViewComic, CanViewComic ); } }
        public ICommand SavePublisherFilterCommand { get { return new RelayCommand( SavePublisherFilter ); } }
        public ICommand ShowPublisherFilterCommand {
            get {
                return new RelayCommand( () => {
                    AvailablePublishers = ( from cl in SystemSettings.Catalog.ComicLists
                                            from c in cl.Comics
                                            select c.Publisher )
                       .Union( SystemSettings.Catalog.WhitelistPublishers )
                       .Distinct()
                       .OrderBy( x => x )
                       .Select( x => new SelectableEntity<string>( x, SystemSettings.Catalog.WhitelistPublishers.Contains( x ) ) )
                       .ToList();

                    RaisePropertyChanged( () => AvailablePublishers );
                    ShowPublisherFilter = true;
                } ); 
            }
        }
        public ICommand ClearFilterTextCommand {
            get {
                return new RelayCommand( () => {
                    FilterText = "";
                    LoadComicEntries();
                } );
            }
        }
        public bool OmitVariantCovers { get; set; }
        public bool FirstPrintOnly { get; set; }
        public bool FilterPublishers { get; set; }

        public MainViewModel() {
            this.OmitVariantCovers = true;
            this.FirstPrintOnly = true;
            this.FilterPublishers = true;

            this.WeeklyComics = new ObservableCollection<ComicEntry>();
            LoadGroupedWeeklyComics();

            this.MyComics = new ObservableCollection<ComicEntry>();
            LoadGroupedMyComics();

            Load();
        }

        private void LoadGroupedMyComics() {
            PropertyGroupDescription groupDescription = new PropertyGroupDescription( "Publisher" );
            this.GroupedMyComics = (CollectionView) CollectionViewSource.GetDefaultView( this.MyComics );
            this.GroupedMyComics.GroupDescriptions.Add( groupDescription );
            RaisePropertyChanged( () => GroupedMyComics );
        }

        private void LoadGroupedWeeklyComics() {
            PropertyGroupDescription groupDescription = new PropertyGroupDescription( "Publisher" );
            this.GroupedWeeklyComics = (CollectionView) CollectionViewSource.GetDefaultView( this.WeeklyComics );
            this.GroupedWeeklyComics.GroupDescriptions.Add( groupDescription );
            RaisePropertyChanged( () => GroupedWeeklyComics );
        }

        public void Load() {
            this._systemSettings = SystemSettings.Load();

            this.FilterPublishers = _systemSettings.Catalog.FilterPublishers;
            this.OmitVariantCovers = _systemSettings.Catalog.OmitVariantCovers;
            this.FirstPrintOnly = _systemSettings.Catalog.ShowFirstPrintOnly;

            this.WeeklyComics.Clear();
            this.MyComics.Clear();

            LoadPersonalList();
            LoadSavedLists();
        }

        private void LoadSavedLists() {
            if( ComicLists == null )
                ComicLists = new ObservableCollection<Lib.Fetcher.DatedComicList>();
            else
                ComicLists.Clear();

            foreach( var list in _systemSettings.Catalog.ComicLists.OrderByDescending( x => x.Date ) ) {
                ComicLists.Add( list );
            }
        }

        private void LoadPersonalList() {
            if( PersonalComicList == null )
                PersonalComicList = new ObservableCollection<string>();
            PersonalComicList.Clear();

            foreach( var userComicSelection in _systemSettings.Catalog.UserComicSelection ) {
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

        private void AddMyTitle() {
            AddMyTitle( AddTitleText );
            AddTitleText = "";
        }

        private void AddMyTitle( string title ) {
            _systemSettings.Catalog.AddUserComicSelection( new UserComicSelection() { TitleText = title } );
            _systemSettings.Save();

            LoadPersonalList();
            FilterComicsByPersonalizedList();
        }

        private void RemoveMyComic( string title ) {
            _systemSettings.Catalog.RemoveUserComicSelection( new UserComicSelection() { TitleText = title } );
            _systemSettings.Save();

            LoadPersonalList();
            FilterComicsByPersonalizedList();
        }

        private void FilterComicsByPersonalizedList() {
            var query = from entry in WeeklyComics
                        join myComic in PersonalComicList on entry.SeriesTitle.ToLower().Trim() equals myComic.ToLower().Trim()
                        select entry;

            MyComics = new ObservableCollection<ComicEntry>( query );
            LoadGroupedMyComics();
        }

        private async Task LoadComics() {
            var listFetch = new ListFetch();
            await listFetch.Fetch();

            SelectedComicList = null;

            ComicLists.Clear();
            foreach( var list in listFetch.GetLists() ) {
                _systemSettings.Catalog.AddComicList( list );
            }
            _systemSettings.Save();

            LoadSavedLists();
        }

        private void LoadComicEntries() {
            if( SelectedComicList == null ) {
                WeeklyComics.Clear();
            }
            else {
                if( FilterPublishers != SystemSettings.Catalog.FilterPublishers
                    || OmitVariantCovers != SystemSettings.Catalog.OmitVariantCovers
                    || FirstPrintOnly != SystemSettings.Catalog.ShowFirstPrintOnly ) {
                    SystemSettings.Catalog.FilterPublishers = FilterPublishers;
                    SystemSettings.Catalog.OmitVariantCovers = OmitVariantCovers;
                    SystemSettings.Catalog.ShowFirstPrintOnly = FirstPrintOnly;
                    SystemSettings.Save();
                }

                SelectedComicList.ClearFilters();
                if( FilterPublishers )
                    SelectedComicList.AddPublisherFilter( SystemSettings.Catalog.WhitelistPublishers );
                if( FirstPrintOnly )
                    SelectedComicList.AddShouldBefirstPrintFilter();
                if( OmitVariantCovers )
                    SelectedComicList.OmitVariantCovers();
                if( !string.IsNullOrEmpty( FilterText ) )
                    SelectedComicList.AddFilter( entry => entry.Title.ToLower().Contains( FilterText.ToLower() ) );

                WeeklyComics = new ObservableCollection<ComicEntry>( SelectedComicList.GetEntries() );
                LoadGroupedWeeklyComics();
            }

            FilterComicsByPersonalizedList();
        }

        private void SavePublisherFilter() {
            var selectedPublishers = AvailablePublishers.Where( x => x.IsSelected ).Select( x => x.Entity ).ToList();
            SystemSettings.Catalog.SetWhitelistPublishers( selectedPublishers );
            SystemSettings.Save();
            ShowPublisherFilter = false;
        }
    }
}