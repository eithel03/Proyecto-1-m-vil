
using MiAgendaUTN.ViewModels;
namespace MiAgendaUTN.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        BindingContext = new TaskViewModel();
    }
}
