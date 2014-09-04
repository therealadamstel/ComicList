using ComicList.Lib.Configuration;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicList.ViewModel {
    public class UserComicSelectionModel : ViewModelBase {
        public UserComicSelection Value { get; private set; }
        private Action _saveNotifier;

        public UserComicSelectionModel( UserComicSelection value, Action saveNotifier ) {
            Value = value;
            _saveNotifier = saveNotifier;
        }

        public string TitleText {
            get { return Value.TitleText; }
            set {
                if( Value.TitleText != value ) {
                    Value.TitleText = value;

                    _saveNotifier();
                }
            }
        }

        public int Priority {
            get { return Value.Priority; }
            set {
                if( Value.Priority != value ) {
                    Value.Priority = value;

                    _saveNotifier();
                }
            }
        }

        public string Notes {
            get { return Value.Notes; }
            set {
                if( Value.Notes != value ) {
                    Value.Notes = value;

                    _saveNotifier();
                }
            }
        }
    }
}
