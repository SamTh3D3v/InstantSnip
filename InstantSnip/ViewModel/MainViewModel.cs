﻿using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using InstantSnip.Helpers;
using InstantSnip.Views;
using Application = System.Windows.Application;
using Point = System.Drawing.Point;

namespace InstantSnip.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        #region Properties

        public string MainActionIcon
        {
            get { return _mainActionIcon; }
            set
            {
                _mainActionIcon = value;
                RaisePropertyChanged("MainActionIcon");
            }
        }

        public ScreeShotView ScreenShotWindow { get; set; }

        public WindowState WindowState
        {
            get { return _windowState; }
            set
            {
                _windowState = value;
                RaisePropertyChanged("WindowState");
            }
        }

        #endregion

        #region RelayCommands

        public RelayCommand CloseApplication
        {
            get;
            private set;
        }

        public RelayCommand StartSnipping
        {
            get;
            private set;
        }

        public RelayCommand RestartSnipping
        {
            get;
            private set;
        }

        public RelayCommand SaveSnipping
        {
            get; 
            private set;
        }

        public RelayCommand MainAction
        {
            get
            {
                return _mainAction;
            }
            private set
            {
                _mainAction = value;
                RaisePropertyChanged("MainAction");
            }
        }

        public RelayCommand<System.Windows.Input.KeyEventArgs> KeyDown { get; private set; }

        #endregion
        
        public MainViewModel()
        {
            InitRelayCommands();
            MessengerSubscriber();
            MainActionIcon = StartSnippingData;
        }

        #region HelperMethods

        private void InitRelayCommands()
        {
            
            InitializeStartSnippingRelayCommand();
            InitializeRestartSnippingRelayCommand();
            InitializeSaveSnipRelayCommand();
            InitializeCloseApplicationRelayCommand();
            InitiliazeKeyDownRelayCommand();


            MainAction = StartSnipping;
        }

        private void InitializeCloseApplicationRelayCommand()
        {
            CloseApplication = new RelayCommand(() => Application.Current.Shutdown());
        }        
        private void InitializeStartSnippingRelayCommand()
        {
            StartSnipping = new RelayCommand(() =>
                                             {
                                                 ViewsAccessibility.GetCorresponingWindow(this).Hide();
                                                 var bitmap = CaptureSnipping();
                                                 ScreenShotWindow = new ScreeShotView();
                                                 ScreenShotWindow.Show();
                                                 Messenger.Default.Send(bitmap);
                                             });
        }
        private void InitializeRestartSnippingRelayCommand()
        {
            RestartSnipping = new RelayCommand(() =>
            {
                ScreenShotWindow.Close();
                SetMainAction(StartSnipping, StartSnippingData);
            });
        }
        private void InitializeSaveSnipRelayCommand()
        {
            SaveSnipping = new RelayCommand(() =>
                                            {
                                                Messenger.Default.Send(SnippingState.Saved);
                                                SetMainAction(StartSnipping, StartSnippingData);

                                            });
        }
        private void InitiliazeKeyDownRelayCommand()
        {
            KeyDown = new RelayCommand<System.Windows.Input.KeyEventArgs>(keyEventArgs =>
                                                     {
                                                         if (keyEventArgs.Key == Key.Escape)
                                                         {
                                                              WindowState = WindowState.Minimized;
                                                         }
                                                     });
        }


        private void MessengerSubscriber()
        {
            Messenger.Default.Register<SnippingState>(this, snippingState =>
                                                         {
                                                             switch (snippingState)
                                                             {
                                                                 case SnippingState.Begin:
                                                                     ViewsAccessibility.GetCorresponingWindow(this).Show();
                                                                     SetMainAction(StartSnipping, StartSnippingData);
                                                                     break;
                                                                 case SnippingState.SelectionStarted:
                                                                     ViewsAccessibility.GetCorresponingWindow(this).Hide();
                                                                     break;
                                                                 case SnippingState.SelectionFinished:
                                                                     ViewsAccessibility.GetCorresponingWindow(this).Show();
                                                                     SetMainAction(SaveSnipping, SaveSnippingData);
                                                                     break;
                                                             }
                                                         });
        }

        private void SetMainAction(RelayCommand action, string data)
        {
                MainAction = action;
                MainActionIcon = data;
        }

        private Bitmap CaptureSnipping()
        {
            var screen = Screen.PrimaryScreen;
            var bitmap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new Point(screen.Bounds.Left, screen.Bounds.Top),
                    new Point(0, 0), screen.Bounds.Size);
            }
            return bitmap;
        }

        #endregion

        #region Fields

        private const string StartSnippingData = "M37.130002,45.231999L47.366002,45.231999 47.366002,47.789999 37.130002,47.789999z M16.656001,45.231999L26.893,45.231999 26.893,47.789999 16.656001,47.789999z M61.441411,37.940998L64.000001,37.940998 64.000001,47.789997 52.483998,47.789997 52.483998,45.231471 61.441411,45.231471z M2.581253,36.362995L2.5871734,45.320074 9.877011,45.314875 9.8790004,47.874795 0.029904366,47.879994 0.022000313,36.364296z M61.418999,20.240999L63.979001,20.240999 63.979001,30.476999 61.418999,30.476999z M0,18.572998L2.5589999,18.572998 2.5589999,28.809999 0,28.809999z M54.151101,0L64.000001,0.044281006 63.949898,11.559999 61.390702,11.548298 61.4297,2.591217 54.14,2.5600281z M37.963002,0L48.198998,0 48.198998,2.5599976 37.963002,2.5599976z M17.49,0L27.727,0 27.727,2.5599976 17.49,2.5599976z M0.044281006,0L11.56,0.050811768 11.548899,2.6095428 2.5919122,2.5704689 2.5592221,9.8610001 0,9.8492489z";
        private const string SaveSnippingData = "M8.1099597,36.94997L8.1099597,41.793968 39.213959,41.793968 39.213959,36.94997z M12.42,0.049999889L18.4,0.049999889 18.4,12.252 12.42,12.252z M0,0L7.9001866,0 7.9001866,14.64218 39.210766,14.64218 39.210766,0 47.401001,0 47.401001,47.917 0,47.917z";


        private string _mainActionIcon;
        private RelayCommand _mainAction;
        private WindowState _windowState;

        #endregion
    }
}