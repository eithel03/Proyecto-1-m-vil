using System.ComponentModel;
using System.Runtime.CompilerServices;
using MiAgendaUTN.Models;

namespace MiAgendaUTN.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private SettingsModel _settings;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsViewModel()
        {
            _settings = new SettingsModel();
        }

        public bool IsDarkMode
        {
            get => _settings.IsDarkMode;
            set
            {
                if (_settings.IsDarkMode != value)
                {
                    _settings.IsDarkMode = value;
                    OnPropertyChanged();
                    ApplyTheme();
                }
            }
        }

        private void ApplyTheme()
        {
            Application.Current!.UserAppTheme = IsDarkMode
                ? AppTheme.Dark
                : AppTheme.Light;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
