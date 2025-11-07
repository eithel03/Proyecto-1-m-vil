using MiAgendaUTN.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MiAgendaUTN.Views;

public partial class HomePage : ContentPage
{
    private readonly TaskViewModel _vm;

    public HomePage()
    {
        InitializeComponent();

        // ✅ Obtén el ServiceProvider desde el MauiContext
        var sp = Application.Current?.Handler?.MauiContext?.Services
                 ?? throw new InvalidOperationException("No DI services available");

        _vm = sp.GetRequiredService<TaskViewModel>();
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync(); // idempotente
    }
}
