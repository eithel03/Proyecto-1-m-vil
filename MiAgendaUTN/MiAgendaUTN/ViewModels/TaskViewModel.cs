using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using MiAgendaUTN.Models;
using MiAgendaUTN.Services;

namespace MiAgendaUTN.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private readonly TaskDataService _dataService;

        // ===== Colecciones que usa la vista =====
        public ObservableCollection<TaskModel> Tasks { get; set; } = new();
        public ObservableCollection<TaskModel> UpcomingTasks { get; } = new();
        public ObservableCollection<TaskModel> CompletedTasks { get; } = new();

        // ===== Campos de edición / formulario =====
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
        public string? Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string? _description;
        public string? Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private DateTime _dueDate = DateTime.Now;
        public DateTime DueDate { get => _dueDate; set { _dueDate = value; OnPropertyChanged(); } }

        // ===== KPI / Dashboard =====
        public int PendingCount { get; private set; }
        public int DueTodayCount { get; private set; }
        public int CompletedCount { get; private set; }

        private TaskModel? _nextTask;
        public TaskModel? NextTask
        {
            get => _nextTask;
            private set { _nextTask = value; OnPropertyChanged(); }
        }

        // ===== Comandos para la vista =====
        public ICommand AddOrUpdateTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectForEditCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand ToggleDoneCommand { get; }            // ✅ Finalizar / Reabrir
        public ICommand GoToNuevaTareaCommand { get; }        // Botón Nueva tarea

        public TaskViewModel()
        {
            _dataService = new TaskDataService();

            AddOrUpdateTaskCommand = new Command(async () => await AddOrUpdateTaskAsync());
            DeleteTaskCommand = new Command<TaskModel>(async t => await DeleteTaskAsync(t));
            SelectForEditCommand = new Command<TaskModel>(SelectForEdit);
            ClearSelectionCommand = new Command(ClearForm);
            ToggleDoneCommand = new Command<TaskModel>(async t => await ToggleDoneAsync(t));
            GoToNuevaTareaCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.TaskFormPage), true));

            // Cargar datos al crear el VM
            _ = LoadAsync();
        }

        // ===== Carga desde JSON =====
        public async Task LoadAsync()
        {
            var loaded = await _dataService.LoadTasksAsync();

            // Reemplazo la colección para simplificar (y aviso a la vista)
            Tasks = new ObservableCollection<TaskModel>(loaded);
            OnPropertyChanged(nameof(Tasks));

            HookTasksCollectionChanged();
            ReorderTasksInPlace();
            RecomputeDashboard();
        }

        // ===== Crear / Actualizar =====
        private async Task AddOrUpdateTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El título es obligatorio.", "OK");
                return;
            }

            if (DueDate.Date < DateTime.Now.Date)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "La fecha límite no puede ser anterior a hoy.", "OK");
                return;
            }

            if (SelectedTask == null)
            {
                var newTask = new TaskModel
                {
                    Title = Title,
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                    DueDate = DueDate,
                    IsCompleted = false
                };
                Tasks.Add(newTask);
            }
            else
            {
                var ex = Tasks.FirstOrDefault(t => t.Id == SelectedTask.Id);
                if (ex != null)
                {
                    ex.Title = Title;
                    ex.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
                    ex.DueDate = DueDate;
                }
            }

            ReorderTasksInPlace();
            await _dataService.SaveTasksAsync(Tasks);

            await Application.Current.MainPage.DisplayAlert("Éxito",
                SelectedTask == null ? "Tarea guardada correctamente." : "Tarea actualizada correctamente.", "OK");

            await Shell.Current.GoToAsync("..");
            ClearForm();
            RecomputeDashboard();
        }

        // ===== Seleccionar para editar =====
        private async void SelectForEdit(TaskModel task)
        {
            if (task == null) return;

            SelectedTask = task;

            await Shell.Current.GoToAsync(nameof(Views.TaskFormPage), true, new Dictionary<string, object>
            {
                ["TaskToEdit"] = task
            });
        }

        // ===== Eliminar =====
        private async Task DeleteTaskAsync(TaskModel task)
        {
            if (task == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirmar", $"¿Eliminar la tarea '{task.Title}'?", "Sí", "No");
            if (!confirm) return;

            // Quitar de la lista principal por Id
            var toRemove = Tasks.FirstOrDefault(t => t.Id == task.Id);
            if (toRemove != null)
                Tasks.Remove(toRemove);

            // Quitar también de vistas derivadas (si estuviera)
            var up = UpcomingTasks.FirstOrDefault(t => t.Id == task.Id);
            if (up != null) UpcomingTasks.Remove(up);

            var done = CompletedTasks.FirstOrDefault(t => t.Id == task.Id);
            if (done != null) CompletedTasks.Remove(done);

            ReorderTasksInPlace();
            await _dataService.SaveTasksAsync(Tasks);
            RecomputeDashboard();
        }


        // ===== Finalizar / Reabrir (no borra) =====
        private async Task ToggleDoneAsync(TaskModel task)
        {
            if (task == null) return;

            // Cambiar estado en la colección principal (por Id para asegurar)
            var inMain = Tasks.FirstOrDefault(t => t.Id == task.Id);
            if (inMain != null)
                inMain.IsCompleted = !inMain.IsCompleted;
            else
                task.IsCompleted = !task.IsCompleted; // fallback

            // Reflejar visualmente de inmediato en listas derivadas
            var up = UpcomingTasks.FirstOrDefault(t => t.Id == task.Id);
            var done = CompletedTasks.FirstOrDefault(t => t.Id == task.Id);

            if ((inMain ?? task).IsCompleted)
            {
                // Pasó a completada → salir de Upcoming y entrar a Completed
                if (up != null) UpcomingTasks.Remove(up);
                if (done == null)
                    CompletedTasks.Insert(0, inMain ?? task);
            }
            else
            {
                // Reabierta → salir de Completed y (si aplica) volver a Upcoming
                if (done != null) CompletedTasks.Remove(done);

                var src = inMain ?? task;
                if (src.DueDate.Date >= DateTime.Today) // solo “próximas”
                {
                    if (!UpcomingTasks.Any(t => t.Id == src.Id))
                        UpcomingTasks.Add(src);
                }
            }

            ReorderTasksInPlace();
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

        // ===== Reordenar y Dashboard =====
        private void ReorderTasksInPlace()
        {
            var ordered = Tasks
                .OrderBy(t => t.IsCompleted)     // pendientes primero
                .ThenBy(t => t.DueDate)          // por fecha
                .ThenBy(t => t.Title)            // desempate
                .ToList();

            Tasks.Clear();
            foreach (var t in ordered) Tasks.Add(t);
        }

        private void RecomputeDashboard()
        {
            var all = Tasks?.ToList() ?? new List<TaskModel>();
            var today = DateTime.Now.Date;

            PendingCount = all.Count(t => !t.IsCompleted);
            DueTodayCount = all.Count(t => !t.IsCompleted && t.DueDate.Date == today);
            CompletedCount = all.Count(t => t.IsCompleted);

            NextTask = all.Where(t => !t.IsCompleted)
                          .OrderBy(t => t.DueDate)
                          .FirstOrDefault();

            // Próximas (pendientes)
            UpcomingTasks.Clear();
            foreach (var t in all.Where(t => !t.IsCompleted)
                                 .OrderBy(t => t.DueDate)
                                 .Take(10))
                UpcomingTasks.Add(t);

            // Finalizadas (últimas 10)
            CompletedTasks.Clear();
            foreach (var t in all.Where(t => t.IsCompleted)
                                 .OrderByDescending(t => t.DueDate)
                                 .Take(10))
                CompletedTasks.Add(t);

            OnPropertyChanged(nameof(PendingCount));
            OnPropertyChanged(nameof(DueTodayCount));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(NextTask));
        }

        // ===== Suscripciones para recalcular en caliente =====
        private void HookTasksCollectionChanged()
        {
            if (Tasks == null) return;

            Tasks.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (TaskModel item in e.NewItems)
                        item.PropertyChanged += OnTaskItemPropertyChanged;

                if (e.OldItems != null)
                    foreach (TaskModel item in e.OldItems)
                        item.PropertyChanged -= OnTaskItemPropertyChanged;

                RecomputeDashboard();
            };

            foreach (var item in Tasks)
                item.PropertyChanged += OnTaskItemPropertyChanged;
        }

        private void OnTaskItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Cualquier cambio en una tarea (IsCompleted, Title, DueDate, etc.)
            RecomputeDashboard();
        }

        // ===== INotifyPropertyChanged =====
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
