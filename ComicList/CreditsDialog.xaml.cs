using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComicList {
    /// <summary>
    /// Interaction logic for CreditsDialog.xaml
    /// </summary>
    public partial class CreditsDialog : UserControl {
        public event EventHandler Close = delegate { };

        public CreditsDialog() {
            InitializeComponent();
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            Close( this, EventArgs.Empty );
        }

        private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e ) {
            Process.Start( e.Uri.ToString() );
        }
    }
}
