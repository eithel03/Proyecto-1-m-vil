namespace MiAgendaUTN
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                Console.WriteLine($"⚠️ Unhandled: {ex}");
                MainThread.BeginInvokeOnMainThread(async () =>
                    await Current!.MainPage.DisplayAlert("Error inesperado", ex.Message, "OK"));
            };
        }
    }
}
