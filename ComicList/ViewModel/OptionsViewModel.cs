using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ComicList.ViewModel {
    public class OptionsViewModel : ViewModelBase {
        public event EventHandler Close = delegate { };

        private string _catalogPath;

        public string CatalogPath {
            get { return _catalogPath; }
            set { _catalogPath = value; RaisePropertyChanged( () => CatalogPath ); }
        }

        public ICommand SaveCommand { get { return new RelayCommand( SaveSettings ); } }
        public ICommand BrowseCommand { get { return new RelayCommand( BrowseForFolder ); } }
        public ICommand ClearCommand { get { return new RelayCommand( Clear ); } }

        public OptionsViewModel() {
            var mainViewModel = SimpleIoc.Default.GetInstance<MainViewModel>();
            CatalogPath = mainViewModel.SystemSettings.OverrideSettingsFolderPath;
        }
        
        private void BrowseForFolder() {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if( dlg.ShowDialog() ?? false ) {
                CatalogPath = dlg.SelectedPath;
            }
        }

        private void Clear() {
            CatalogPath = null;
        }

        private void SaveSettings() {
            var mainViewModel = SimpleIoc.Default.GetInstance<MainViewModel>();
            if( CatalogPath != mainViewModel.SystemSettings.OverrideSettingsFolderPath ) {
                mainViewModel.SystemSettings.MoveCatalog( CatalogPath );
                mainViewModel.Load();
            }

            Close( this, EventArgs.Empty );
        }
    }
}
