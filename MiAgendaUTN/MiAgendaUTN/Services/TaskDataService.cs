using System.Collections.ObjectModel;
using System.Text.Json;
using MiAgendaUTN.Models;

namespace MiAgendaUTN.Services
{
    public class TaskDataService
    {
        private readonly string _filePath;

        public TaskDataService()
        {
            string folder = FileSystem.AppDataDirectory;
            _filePath = Path.Combine(folder, "tasks.json");
        }

        public async Task<ObservableCollection<TaskModel>> LoadTasksAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new ObservableCollection<TaskModel>();

                using var stream = File.OpenRead(_filePath);
                var tasks = await JsonSerializer.DeserializeAsync<ObservableCollection<TaskModel>>(stream);
                return tasks ?? new ObservableCollection<TaskModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al cargar tareas: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudieron cargar las tareas.", "OK");
                return new ObservableCollection<TaskModel>();
            }
        }

        public async Task SaveTasksAsync(ObservableCollection<TaskModel> tasks)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                using var stream = File.Create(_filePath);
                await JsonSerializer.SerializeAsync(stream, tasks, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar tareas: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudieron guardar las tareas.", "OK");
            }
        }

        // ⬇️ NUEVO: compartir/exportar el JSON actual
        public async Task ExportTasksJsonAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    await Application.Current.MainPage.DisplayAlert("Aviso", "No hay datos para exportar.", "OK");
                    return;
                }

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Exportar Mis Tareas (JSON)",
                    File = new ShareFile(_filePath)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al exportar JSON: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo exportar el archivo JSON.", "OK");
            }
        }
    }
}
