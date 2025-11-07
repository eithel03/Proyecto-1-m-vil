// Services/TaskDataService.cs
using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.ApplicationModel.DataTransfer; // ShareFile / Share
using MiAgendaUTN.Models;

namespace MiAgendaUTN.Services
{
    public class TaskDataService
    {
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly string _filePath;
        private readonly SemaphoreSlim _ioLock = new(1, 1);

        public TaskDataService()
        {
            var folder = FileSystem.AppDataDirectory;
            Directory.CreateDirectory(folder); // asegura la carpeta
            _filePath = Path.Combine(folder, "tasks.json");
        }

        /// <summary>Carga tareas desde JSON local. Si no existe, devuelve colección vacía.</summary>
        public async Task<ObservableCollection<TaskModel>> LoadTasksAsync()
        {
            await _ioLock.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                    return new ObservableCollection<TaskModel>();

                using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var tasks = await JsonSerializer.DeserializeAsync<ObservableCollection<TaskModel>>(stream, _jsonOpts);
                return tasks ?? new ObservableCollection<TaskModel>();
            }
            catch (Exception ex)
            {
                // Log suave; no romper la app
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar tareas: {ex}");
                await SafeAlert("Error", "No se pudieron cargar las tareas.");
                return new ObservableCollection<TaskModel>();
            }
            finally
            {
                _ioLock.Release();
            }
        }

        /// <summary>Guarda tareas a JSON local con escritura atómica.</summary>
        public async Task SaveTasksAsync(ObservableCollection<TaskModel> tasks)
        {
            await _ioLock.WaitAsync();
            try
            {
                // escritura atómica: a .tmp y luego reemplazo
                var tmp = _filePath + ".tmp";

                await using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
                    await JsonSerializer.SerializeAsync(fs, tasks, _jsonOpts);

                // Reemplaza el archivo final (si existe, lo sobreescribe de forma segura)
                if (File.Exists(_filePath))
                    File.Delete(_filePath);

                File.Move(tmp, _filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al guardar tareas: {ex}");
                await SafeAlert("Error", "No se pudieron guardar las tareas.");
            }
            finally
            {
                _ioLock.Release();
            }
        }

        /// <summary>Comparte el JSON actual con el cuadro de compartir del SO.</summary>
        public async Task ExportTasksJsonAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    await SafeAlert("Aviso", "No hay datos para exportar.");
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
                System.Diagnostics.Debug.WriteLine($"❌ Error al exportar JSON: {ex}");
                await SafeAlert("Error", "No se pudo exportar el archivo JSON.");
            }
        }

        // ------ utilidades ------
        private static Task SafeAlert(string title, string msg)
        {
            // Evita excepción si aún no hay MainPage (p.ej. en tests/arranque)
            var page = Application.Current?.MainPage;
            return page != null
                ? page.DisplayAlert(title, msg, "OK")
                : Task.CompletedTask;
        }
    }
}
