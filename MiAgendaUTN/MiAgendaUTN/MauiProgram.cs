using Microsoft.Extensions.Logging;
using PdfSharpCore.Fonts;
using MiAgendaUTN.Services;

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

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
