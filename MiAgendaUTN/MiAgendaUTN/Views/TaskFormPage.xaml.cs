using MiAgendaUTN.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MiAgendaUTN.Views;

public partial class TaskFormPage : ContentPage
{
    private readonly TaskViewModel _vm;

    public TaskFormPage()
    {
        InitializeComponent();

        // Usa el mismo VM que Home/Tasks
        var sp = Application.Current?.Handler?.MauiContext?.Services
                 ?? throw new InvalidOperationException("No DI services available");
        _vm = sp.GetRequiredService<TaskViewModel>();
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // No llames LoadAsync aquí: ya lo maneja Home/Tasks
        // El VM ya trae la colección viva y actualizada
    }
}
