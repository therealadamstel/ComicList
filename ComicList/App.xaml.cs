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

using NLog;
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ComicList {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public App() {
        }

        protected override void OnStartup( StartupEventArgs e ) {
            base.OnStartup( e );

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e ) {
            LogUnhandledException( e.ExceptionObject as Exception );
        }

        void Dispatcher_UnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
            LogUnhandledException( e.Exception );
        }

        private void LogUnhandledException( Exception ex ) {
            if( ex != null ) {
                _logger.Log( LogLevel.Error, "Unhandled exception", ex );
            }
        }
    }
}
