using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MiAgendaUTN.Models;
using MiAgendaUTN.Services;

namespace MiAgendaUTN.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private readonly TaskDataService _dataService;
        public ObservableCollection<TaskModel> Tasks { get; set; } = new();

        private TaskModel? _selectedTask;
        public TaskModel? SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                if (value != null)
                {
                    Title = value.Title;
                    Description = value.Description;
                    DueDate = value.DueDate;
                }
                OnPropertyChanged();
            }
        }

        private string? _title;
        public string? Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private DateTime _dueDate = DateTime.Now;
        public DateTime DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(); }
        }

        // Comandos expuestos para la vista
        public ICommand AddOrUpdateTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectForEditCommand { get; }
        public ICommand ClearSelectionCommand { get; }

        public TaskViewModel()
        {
            _dataService = new TaskDataService();

            AddOrUpdateTaskCommand = new Command(async () => await AddOrUpdateTaskAsync());
            DeleteTaskCommand = new Command<TaskModel>(async (t) => await DeleteTaskAsync(t));
            SelectForEditCommand = new Command<TaskModel>((t) => SelectForEdit(t));
            ClearSelectionCommand = new Command(() => ClearForm());

            // Cargar desde JSON al iniciar la instancia
            _ = LoadAsync();
        }

        // Cargar tareas desde JSON
        public async Task LoadAsync()
        {
            var loaded = await _dataService.LoadTasksAsync();
            Tasks = new ObservableCollection<TaskModel>(loaded);
            OnPropertyChanged(nameof(Tasks));
        }

        private async Task AddOrUpdateTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El título es obligatorio.", "OK");
                return;
            }

            if (SelectedTask == null)
            {
                // Crear nuevo
                var newTask = new TaskModel
                {
                    Title = Title,
                    Description = Description,
                    DueDate = DueDate,
                    IsCompleted = false
                };
                Tasks.Add(newTask);
            }
            else
            {
                // Actualizar existente
                var ex = Tasks.FirstOrDefault(t => t.Id == SelectedTask.Id);
                if (ex != null)
                {
                    ex.Title = Title;
                    ex.Description = Description;
                    ex.DueDate = DueDate;
                }
            }

            await _dataService.SaveTasksAsync(Tasks);

            // Mensaje y limpieza del formulario
            await Application.Current.MainPage.DisplayAlert("Éxito",
               SelectedTask == null ? "Tarea guardada correctamente." : "Tarea actualizada correctamente.", "OK");

            // 🔁 Refrescar vista: volver a la lista de tareas
            await Shell.Current.GoToAsync("..");

            ClearForm();

        }

        private async void SelectForEdit(TaskModel task)
        {
            if (task == null) return;

            SelectedTask = task;

            // 🔁 Navegar a la página de edición con los datos cargados
            await Shell.Current.GoToAsync(nameof(Views.TaskFormPage), true, new Dictionary<string, object>
            {
                ["TaskToEdit"] = task
            });
        }


        private async Task DeleteTaskAsync(TaskModel task)
        {
            if (task == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar", $"¿Eliminar la tarea '{task.Title}'?", "Sí", "No");
            if (!confirm) return;

            Tasks.Remove(task);
            await _dataService.SaveTasksAsync(Tasks);
        }

        private void ClearForm()
        {
            SelectedTask = null;
            Title = string.Empty;
            Description = string.Empty;
            DueDate = DateTime.Now;
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(DueDate));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
