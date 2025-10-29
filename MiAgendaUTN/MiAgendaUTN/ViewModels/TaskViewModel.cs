using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MiAgendaUTN.Models;

namespace MiAgendaUTN.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TaskModel> Tasks { get; set; } = new();

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

        public ICommand AddTaskCommand { get; }

        public TaskViewModel()
        {
            AddTaskCommand = new Command(AddTask);
        }

        private void AddTask()
        {
            if (!string.IsNullOrWhiteSpace(Title))
            {
                Tasks.Add(new TaskModel
                {
                    Title = Title ?? "",
                    Description = Description ?? "",
                    DueDate = DueDate,
                    IsCompleted = false
                });

                Title = string.Empty;
                Description = string.Empty;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
