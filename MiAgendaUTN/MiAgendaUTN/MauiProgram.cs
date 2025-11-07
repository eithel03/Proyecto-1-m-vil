using MiAgendaUTN.Services;
using MiAgendaUTN.ViewModels;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Fonts;
using Microsoft.Extensions.DependencyInjection;

namespace MiAgendaUTN
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ¡Asignar sin leer el getter!
            GlobalFontSettings.FontResolver = new OpenSansFontResolver();
            builder.Services.AddSingleton<MiAgendaUTN.Services.TaskDataService>();
            builder.Services.AddSingleton<MiAgendaUTN.ViewModels.TaskViewModel>();


#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
