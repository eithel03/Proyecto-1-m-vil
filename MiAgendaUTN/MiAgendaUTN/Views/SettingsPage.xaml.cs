using MiAgendaUTN.Services;
using MiAgendaUTN.ViewModels;

namespace MiAgendaUTN.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly TaskDataService _data = new();
        private readonly PdfExportService _pdf = new();

        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = new SettingsViewModel();
        }

        private async void OnExportJsonClicked(object sender, EventArgs e)
            => await _data.ExportTasksJsonAsync();

        private async void OnExportPdfClicked(object sender, EventArgs e)
        {
            var tasks = await _data.LoadTasksAsync();
            await _pdf.ExportToPdfAsync(tasks);
        }
    }
}
