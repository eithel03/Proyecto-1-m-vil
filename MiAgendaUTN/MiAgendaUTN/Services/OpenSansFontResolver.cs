using System.Reflection;
using PdfSharpCore.Fonts;

namespace MiAgendaUTN.Services
{
    /// <summary>
    /// FontResolver para PdfSharpCore que sirve las TTF embebidas de OpenSans.
    /// Asegúrate de tener los recursos embebidos con estos LogicalName:
    /// - MiAgendaUTN.Assets.Fonts.OpenSans-Regular.ttf
    /// - MiAgendaUTN.Assets.Fonts.OpenSans-Semibold.ttf
    /// </summary>
    public class OpenSansFontResolver : IFontResolver
    {
        // 1) Nombre de fuente por defecto que usará PdfSharpCore
        public string DefaultFontName => "OpenSans";

        // 2) Mapea familia + estilo → un “faceName” interno
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("OpenSans", StringComparison.OrdinalIgnoreCase))
            {
                // No manejamos cursiva real aquí; si quisieras, agrega OpenSans-Italic.ttf
                return isBold
                    ? new FontResolverInfo("OpenSans#Semibold")
                    : new FontResolverInfo("OpenSans#Regular");
            }

            // Fallback a OpenSans regular
            return new FontResolverInfo("OpenSans#Regular");
        }

        // 3) Devuelve los bytes de la fuente para el faceName anterior
        public byte[]? GetFont(string faceName) => faceName switch
        {
            "OpenSans#Regular" => LoadResource("MiAgendaUTN.Assets.Fonts.OpenSans-Regular.ttf"),
            "OpenSans#Semibold" => LoadResource("MiAgendaUTN.Assets.Fonts.OpenSans-Semibold.ttf"),
            _ => LoadResource("MiAgendaUTN.Assets.Fonts.OpenSans-Regular.ttf"),
        };

        private static byte[] LoadResource(string resourceName)
        {
            var asm = typeof(OpenSansFontResolver).Assembly;
            using var s = asm.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"No se encontró el recurso embebido: {resourceName}");
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
