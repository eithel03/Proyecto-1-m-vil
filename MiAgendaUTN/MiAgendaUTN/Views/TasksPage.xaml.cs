using MiAgendaUTN.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MiAgendaUTN.Views;

public partial class TasksPage : ContentPage
{
    private readonly TaskViewModel _vm;

    public TasksPage()
    {
        InitializeComponent();

        var sp = Application.Current?.Handler?.MauiContext?.Services
                 ?? throw new InvalidOperationException("No DI services available");

        _vm = sp.GetRequiredService<TaskViewModel>();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
