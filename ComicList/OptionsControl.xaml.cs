using ComicList.ViewModel;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for OptionsControl.xaml
    /// </summary>
    public partial class OptionsControl : UserControl {
        public event EventHandler Close = delegate { };

        public OptionsControl() {
            InitializeComponent();
        }

        private void Close_Click( object sender, RoutedEventArgs e ) {
            Close( this, EventArgs.Empty );
        }

        private void UserControl_Loaded( object sender, RoutedEventArgs e ) {
            ( (OptionsViewModel) DataContext ).Close += ( o, args ) => Close( this, EventArgs.Empty );
        }
    }
}
