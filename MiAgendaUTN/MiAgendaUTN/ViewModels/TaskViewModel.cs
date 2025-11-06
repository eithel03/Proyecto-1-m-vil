using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MiAgendaUTN.Models;
using MiAgendaUTN.Services;

namespace MiAgendaUTN.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private readonly TaskDataService _dataService;

        // Lista principal que ya usas en la vista
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

        // ======= NUEVO: Panel / Dashboard =======

        public int PendingCount { get; private set; }
        public int DueTodayCount { get; private set; }
        public int CompletedCount { get; private set; }

        private TaskModel? _nextTask;
        public TaskModel? NextTask
        {
            get => _nextTask;
            private set { _nextTask = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TaskModel> UpcomingTasks { get; } = new();

        // ======= Comandos expuestos para la vista =======
        public ICommand AddOrUpdateTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectForEditCommand { get; }
        public ICommand ClearSelectionCommand { get; }

        // NUEVO: Botón "Agregar tarea" del panel
        public ICommand GoToNuevaTareaCommand { get; }

        public TaskViewModel()
        {
            _dataService = new TaskDataService();

            AddOrUpdateTaskCommand = new Command(async () => await AddOrUpdateTaskAsync());
            DeleteTaskCommand = new Command<TaskModel>(async (t) => await DeleteTaskAsync(t));
            SelectForEditCommand = new Command<TaskModel>((t) => SelectForEdit(t));
            ClearSelectionCommand = new Command(() => ClearForm());

            GoToNuevaTareaCommand = new Command(async () =>
            {
                // Abre el formulario de creación/edición que ya usas
                await Shell.Current.GoToAsync(nameof(Views.TaskFormPage), true);
            });

            // Cargar desde JSON al iniciar la instancia
            _ = LoadAsync();
        }

        // Cargar tareas desde JSON
        public async Task LoadAsync()
        {
            var loaded = await _dataService.LoadTasksAsync();
            Tasks = new ObservableCollection<TaskModel>(loaded);
            OnPropertyChanged(nameof(Tasks));

            // Hook para actualizar panel automáticamente
            HookTasksCollectionChanged();
            RecomputeDashboard();
        }

        private async Task AddOrUpdateTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El título es obligatorio.", "OK");
                return;
            }

            // Validación: la fecha no puede ser anterior a hoy
            if (DueDate.Date < DateTime.Now.Date)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "La fecha límite no puede ser anterior a hoy.", "OK");
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

            await Application.Current.MainPage.DisplayAlert("Éxito",
                SelectedTask == null ? "Tarea guardada correctamente." : "Tarea actualizada correctamente.", "OK");

            await Shell.Current.GoToAsync("..");
            ClearForm();

            // Asegurar recálculo inmediato (aunque el Hook también lo hará)
            RecomputeDashboard();
        }

        private async void SelectForEdit(TaskModel task)
        {
            if (task == null) return;

            SelectedTask = task;

            // Navegar a la página de edición con los datos cargados
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

            RecomputeDashboard();
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

        // ======= Dashboard: recálculo y suscripciones =======

        private void RecomputeDashboard()
        {
            var all = Tasks?.ToList() ?? new List<TaskModel>();
            var today = DateTime.Now.Date;

            PendingCount = all.Count(t => !t.IsCompleted);
            DueTodayCount = all.Count(t => !t.IsCompleted && t.DueDate.Date == today);
            CompletedCount = all.Count(t => t.IsCompleted);

            NextTask = all
                .Where(t => !t.IsCompleted)
                .OrderBy(t => t.DueDate)
                .FirstOrDefault();

            UpcomingTasks.Clear();
            foreach (var t in all
                .Where(t => !t.IsCompleted)
                .OrderBy(t => t.DueDate)
                .Take(3))
            {
                UpcomingTasks.Add(t);
            }

            OnPropertyChanged(nameof(PendingCount));
            OnPropertyChanged(nameof(DueTodayCount));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(NextTask));
        }

        private void HookTasksCollectionChanged()
        {
            if (Tasks == null) return;

            // Suscribir cambios en la colección
            Tasks.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (TaskModel item in e.NewItems)
                        item.PropertyChanged += OnTaskItemPropertyChanged;
                }
                if (e.OldItems != null)
                {
                    foreach (TaskModel item in e.OldItems)
                        item.PropertyChanged -= OnTaskItemPropertyChanged;
                }

                RecomputeDashboard();
            };

            // Suscribir a los ya existentes
            foreach (var item in Tasks)
                item.PropertyChanged += OnTaskItemPropertyChanged;
        }

        private void OnTaskItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Cualquier cambio en una tarea (IsCompleted, Title, DueDate, etc.) recalcula el panel
            RecomputeDashboard();
        }

        // ======= Notificación de cambios =======

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
