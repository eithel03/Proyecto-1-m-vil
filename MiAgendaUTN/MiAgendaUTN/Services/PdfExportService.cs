using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using MiAgendaUTN.Models;

namespace MiAgendaUTN.Services
{
    public class PdfExportService
    {
        public async Task ExportToPdfAsync(IEnumerable<TaskModel> tasks)
        {
            try
            {
                var doc = new PdfDocument();
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                var h1 = new XFont("OpenSans", 16, XFontStyle.Bold);
                var f1 = new XFont("OpenSans", 12, XFontStyle.Regular);
                var f2 = new XFont("OpenSans", 10, XFontStyle.Italic);

                double y = 40;
                gfx.DrawString("📅 Mi Agenda UTN — Tareas", h1, XBrushes.Black,
                    new XRect(0, 0, page.Width, 30), XStringFormats.TopCenter);

                foreach (var t in tasks)
                {
                    gfx.DrawString($"• {t.Title} ({t.DueDate:dd/MM/yyyy}) {(t.IsCompleted ? "[✓]" : "")}",
                        f1, XBrushes.Black, 40, y);
                    y += 18;

                    if (!string.IsNullOrWhiteSpace(t.Description))
                    {
                        gfx.DrawString($"   {t.Description}", f2, XBrushes.Gray, 40, y);
                        y += 16;
                    }

                    if (y > page.Height - 50)
                    {
                        page = doc.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        y = 40;
                    }
                }

                var file = Path.Combine(FileSystem.CacheDirectory, "MiAgendaUTN_Tareas.pdf");
                doc.Save(file);

                await Share.RequestAsync(new ShareFileRequest("Tareas en PDF", new ShareFile(file)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al exportar PDF: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo generar el PDF.", "OK");
            }
        }
    }
}
