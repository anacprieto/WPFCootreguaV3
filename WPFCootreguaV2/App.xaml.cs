using System.Windows;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.Peripherals;

namespace WPFCootreguaV2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.Exit += AppExit;
            this.DispatcherUnhandledException += OnUnhandledException;

        }
        private async void AppExit(object? sender, ExitEventArgs e)
        {
#if !NO_PERIPHERALS
#else
            await VideoRecorder.Stop();
#endif
            EventLogger.SaveLog(EventType.Info, $"La aplicación se ha cerrado manualmente con codigo: {e.ApplicationExitCode}");


        }
        private async void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Muestra un mensaje de error
            MessageBox.Show("Ha ocurrido un error: " + e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#if !NO_PERIPHERALS
#else
            await VideoRecorder.Stop();
#endif
            EventLogger.SaveLog(EventType.FatalError, $"Ocurrió un error fatal en la aplicación, excepción no manejada: {e.Exception.Message}", e.Exception);


            e.Handled = false;
        }
    }
}
