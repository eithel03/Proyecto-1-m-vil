namespace MiAgendaUTN
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // ✅ Registrar la página para navegación por ruta
            Routing.RegisterRoute(nameof(Views.TaskFormPage), typeof(Views.TaskFormPage));
        }

    }
}
