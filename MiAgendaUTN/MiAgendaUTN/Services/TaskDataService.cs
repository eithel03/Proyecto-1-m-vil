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
            if (!File.Exists(_filePath))
                return new ObservableCollection<TaskModel>();

            using var stream = File.OpenRead(_filePath);
            var tasks = await JsonSerializer.DeserializeAsync<ObservableCollection<TaskModel>>(stream);
            return tasks ?? new ObservableCollection<TaskModel>();
        }

        public async Task SaveTasksAsync(ObservableCollection<TaskModel> tasks)
        {
            using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, tasks, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
