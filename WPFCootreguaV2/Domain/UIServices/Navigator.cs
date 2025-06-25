using System;
using System.Windows;
using System.Windows.Controls;
using WPFCootreguaV2.Modals;

namespace WPFCootreguaV2.Domain.UIServices
{

    /*
    public class Navigator
    {
        // Patron de Diseño Singleton
        private static Navigator? _instance;
        private MainWindow? _mainWindow;
        private ModalWindow? _currentModal; // Incluso se puede hacer con una Cola de modales

        private Navigator() { }

        public static Navigator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Navigator();
                return _instance;
            }
        }

        public void Init(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void NavigateTo(UserControl view)
        {

            if (_mainWindow == null)
            {
                throw new Exception("El navegador de la aplicación no ha sido inicializado en la ventana principal");
            }

            if (_mainWindow.Dispatcher.CheckAccess())
            {
                _mainWindow.MainContainer.Content = view;
                return;
            }

            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.MainContainer.Content = view;
            });

        }

        public bool ShowModal(string msg, ModalType type)
        {
            bool result = false;

            ModalViewModel model = new ModalViewModel
            {
                Title = "Estimado Cliente: ",
                Message = msg,
                TypeModal = type,
            };

            Application.Current.Dispatcher.Invoke(delegate
            {
                _currentModal = new ModalWindow(model);
                _currentModal.ShowDialog();
                if (_currentModal.DialogResult.HasValue)
                {
                    result = _currentModal.DialogResult.Value;
                }
            });
            return result;
        }

        public ModalWindow? ShowLoadModal(string msg)
        {
            ModalWindow? loadWindow = null;

            ModalViewModel model = new ModalViewModel
            {
                Title = "Estimado Cliente: ",
                Message = msg,
                TypeModal = new LoadModal(),
            };

            Application.Current.Dispatcher.Invoke(delegate
            {
                loadWindow = new ModalWindow(model);
                loadWindow.Show();
            });


            return loadWindow;
        }

        public void CloseModal() => Application.Current.Dispatcher.Invoke(delegate
        {
            if (_currentModal != null)
            {
                _currentModal.Close();
                _currentModal = null;
            }
        });
    }*/

    public class Navigator
    {
        // Patron de Diseño Singleton
        private static Navigator? _instance;
        private MainWindow? _mainWindow;
        private ModalWindow? _currentModal;
        private ModalWindow? _loadingModal; // Modal de carga separado

        private Navigator() { }

        public static Navigator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Navigator();
                return _instance;
            }
        }

        public void Init(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void NavigateTo(UserControl view)
        {
            if (_mainWindow == null)
            {
                throw new Exception("El navegador de la aplicación no ha sido inicializado en la ventana principal");
            }

            if (_mainWindow.Dispatcher.CheckAccess())
            {
                _mainWindow.MainContainer.Content = view;
                return;
            }

            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.MainContainer.Content = view;
            });
        }

        public bool ShowModal(string msg, ModalType type)
        {
            bool result = false;
            ModalViewModel model = new ModalViewModel
            {
                Title = "Estimado Cliente: ",
                Message = msg,
                TypeModal = type,
            };

            Application.Current.Dispatcher.Invoke(delegate
            {
                _currentModal = new ModalWindow(model);
                _currentModal.ShowDialog();
                if (_currentModal.DialogResult.HasValue)
                {
                    result = _currentModal.DialogResult.Value;
                }
                _currentModal = null; // Limpiar referencia después de cerrar
            });

            return result;
        }

        public void ShowLoadModal(string msg)
        {
            ModalViewModel model = new ModalViewModel
            {
                Title = "Estimado Cliente: ",
                Message = msg,
                TypeModal = new LoadModal(),
            };

            Application.Current.Dispatcher.Invoke(delegate
            {
                // Cerrar modal de carga anterior si existe
                if (_loadingModal != null)
                {
                    _loadingModal.Close();
                }

                _loadingModal = new ModalWindow(model);
                _loadingModal.Show();
            });
        }

        public void CloseLoadModal()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                if (_loadingModal != null)
                {
                    _loadingModal.Close();
                    _loadingModal = null;
                }
            });
        }

        public void CloseModal()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                if (_currentModal != null)
                {
                    _currentModal.Close();
                    _currentModal = null;
                }
            });
        }
    }
}
