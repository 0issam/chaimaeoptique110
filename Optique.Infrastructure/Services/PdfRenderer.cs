using Microsoft.Extensions.Options;
using Optique.Domain.Entities;
using Optique.Infrastructure.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Text; // en haut du fichier si pas déjà
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using DColor = System.Drawing.Color;
using DImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Optique.Infrastructure.Services;

public interface IPdfRenderer
{
    byte[] RenderFacture(Facture f, Client client, Medecin? medecin, Ordonnance? ord);
}


public sealed class PdfRenderer : IPdfRenderer
{
    private readonly BrandingOptions _brand;
    public PdfRenderer(IOptions<BrandingOptions> brand) => _brand = brand.Value;

    // Couleurs
    private static string Blue => Colors.Blue.Medium;
    private static string BlueDark => Colors.Blue.Darken2;
    private static string Grey => Colors.Grey.Darken4;

    // Helpers
    private static IContainer CellHeader(IContainer c)
        => c.BorderBottom(1).BorderColor(Blue).PaddingBottom(4).DefaultTextStyle(x => x.Bold());

    private static string Upper(string? s)
    => CultureInfo.GetCultureInfo("fr-FR").TextInfo.ToUpper(string.IsNullOrWhiteSpace(s) ? " " : s);

    // Crée un PNG transparent de la taille d'une page A4 (~200 DPI), avec un texte
    // placé au coin bas-gauche puis ROTATION pour remonter en diagonale vers le coin haut-droit.
    private static byte[] BuildDiagonalWatermarkPngCentered(
    string text,
    string hexColor = "#2504f999",
    int pxWidth = 1654, int pxHeight = 2339,   // A4 ~200 dpi
    float? angleDegrees = null,                // null => angle exact de la diagonale
    float alpha = 0.22f,                       // 0..1
    double coverage = 0.85                     // part de la diagonale à couvrir (85%)
)
{
    using var bmp = new Bitmap(pxWidth, pxHeight, PixelFormat.Format32bppArgb);
    bmp.SetResolution(200, 200);

    using var g = Graphics.FromImage(bmp);
    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
    g.Clear(DColor.Transparent);

    // angle: bas-gauche -> haut-droit
    float angle = angleDegrees ?? (float)(-Math.Atan2(pxHeight, pxWidth) * 180.0 / Math.PI);

    // couleur + alpha
    var baseColor = System.Drawing.ColorTranslator.FromHtml(hexColor);
    int a = Math.Clamp((int)(alpha * 255f), 0, 255);
    var fill = DColor.FromArgb(a, baseColor.R, baseColor.G, baseColor.B);
    using var brush = new SolidBrush(fill);

    // largeur cible = % de la diagonale
    double diag = Math.Sqrt(pxWidth * (double)pxWidth + pxHeight * (double)pxHeight);
    double targetWidth = diag * coverage;

    // calcule la taille de police pour atteindre targetWidth
    float fontPx = 140f;
    SizeF size;

    var sf = StringFormat.GenericTypographic; // mesures plus précises
    sf.FormatFlags |= StringFormatFlags.NoClip;

    using (var tmp = new Font("Arial", fontPx, FontStyle.Bold, GraphicsUnit.Pixel))
        size = g.MeasureString(text, tmp, new PointF(0, 0), sf);

    while (size.Width < targetWidth && fontPx < 320f)
    {
        fontPx += 8f;
        using var f = new Font("Arial", fontPx, FontStyle.Bold, GraphicsUnit.Pixel);
        size = g.MeasureString(text, f, new PointF(0, 0), sf);
    }

    using var font = new Font("Arial", fontPx, FontStyle.Bold, GraphicsUnit.Pixel);

    // centre de la page -> rotation -> on dessine centré
    g.TranslateTransform(pxWidth / 2f, pxHeight / 2f);
    g.RotateTransform(angle);

    // dessiner avec centrage (on soustrait la moitié largeur/hauteur)
    g.DrawString(text, font, brush, new PointF(-size.Width / 2f, -size.Height / 2f), sf);

    g.ResetTransform();

    using var ms = new MemoryStream();
    bmp.Save(ms, DImageFormat.Png);
    return ms.ToArray();
}





    private static void LabeledLine(ColumnDescriptor col, string label, string? value = null)
    {
        col.Item().Row(r =>
        {
            // Colonne du label
            r.AutoItem().Text(label).FontColor(BlueDark);

            // Colonne de la valeur : on ajoute un padding à gauche pour créer l'espace
            r.RelativeItem()
             .PaddingLeft(8) // 👈 espace visible entre "M." / "DR." et le nom
             .BorderBottom(1).BorderColor(Blue).PaddingBottom(2)
             .Text(string.IsNullOrWhiteSpace(value) ? " " : value);
        });
    }


    private static string V(decimal? v) => v.HasValue ? v.Value.ToString("0.00") : "____";

    public byte[] RenderFacture(Facture f, Client c, Medecin? m, Ordonnance? o)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                
            page.Background().Element(c =>
            {
                var wm = BuildDiagonalWatermarkPngCentered(
                    Upper("CHAIMAE OPTIQUE"),
                    "#C8D9FF",
                    1654, 2339,   // A4 ~200 dpi
                    null,         // angle auto = diagonale
                    0.22f,        // transparence
                    0.85          // 85% de la diagonale => pas de coupe
                );
                c.Image(wm, ImageScaling.FitArea);
            });


                page.Margin(28);


                // ENTÊTE
                page.Header().Column(h =>
                {
                    h.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(_brand.CompanyName).Bold().FontSize(18).FontColor(BlueDark);
                            col.Item().Text(_brand.Activity).FontColor(Grey);
                        });
                        row.ConstantItem(260).Column(col =>
                        {
                            col.Item().AlignRight().Text($"{_brand.City}, Le : {f.DateFacture:dd/MM/yyyy}").FontColor(BlueDark);
                            col.Item().AlignRight().Text("FACTURE").Bold().FontColor(BlueDark).FontSize(14);
                            col.Item().AlignRight().Text("N° "+f.Numero).FontColor(Colors.Yellow.Medium).FontSize(16);
                        });
                    });
                    h.Item().LineHorizontal(1).LineColor(Blue);
                });

                // CONTENU
                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    // Bloc M. / Dr.
                    col.Item().Border(1).BorderColor(Blue).Padding(8).Row(r =>
                     {
                         r.RelativeItem().Column(c1 =>
                         {
                             // Sans civilité : seulement Nom + Prénom
                             var nomClient = $"{(c.Nom ?? "").Trim()} {(c.Prenom ?? "").Trim()}".Trim();
                             LabeledLine(c1, "M.", nomClient);
                         });

                         r.RelativeItem().Column(c2 =>
                         {
                             var dr = m is null ? "" : $"{(m.Nom ?? "").Trim()} {(m.Prenom ?? "").Trim()}".Trim();
                             LabeledLine(c2, "DR.", dr);
                         });
                     });


                    // VISION LOIN / PRÈS
                    col.Item().Row(r =>
                    {
                        // LOIN
                        r.RelativeItem()
                         .Border(1).BorderColor(Blue).Padding(8)
                         .Column(v =>
                         {
                             v.Item().Text("VISION LOIN").Bold().FontColor(BlueDark).FontSize(11);
                             v.Spacing(6);

                             v.Item().Row(rr =>
                             {
                                 rr.AutoItem().Text("OD :").FontColor(BlueDark);
                                 rr.RelativeItem().Row(tr =>
                                 {
                                     tr.Spacing(8);
                                     tr.AutoItem().Text("Cyl").FontColor(Grey);
                                     tr.AutoItem().Text(V(o?.Loin_OD_Cyl));
                                     tr.AutoItem().Text("Sph").FontColor(Grey);
                                     tr.AutoItem().Text(V(o?.Loin_OD_Sph));
                                 });
                             });

                             v.Item().Row(rr =>
                             {
                                 rr.AutoItem().Text("OG :").FontColor(BlueDark);
                                 rr.RelativeItem().Row(tr =>
                                 {
                                     tr.Spacing(8);
                                     tr.AutoItem().Text("Cyl").FontColor(Grey);
                                     tr.AutoItem().Text(V(o?.Loin_OG_Cyl));
                                     tr.AutoItem().Text("Sph").FontColor(Grey);
                                     tr.AutoItem().Text(V(o?.Loin_OG_Sph));
                                 });
                             });
                         });

                        r.ConstantItem(12);

                        // PRÈS
                        r.RelativeItem()
                         .Border(1).BorderColor(Blue).Padding(8)
                         .Column(v =>
                         {
                             v.Item().Text("VISION PRÈS").Bold().FontColor(BlueDark).FontSize(11);
                             v.Spacing(6);

                             v.Item().Row(rr =>
                             {
                                 rr.AutoItem().Text("OD :").FontColor(BlueDark);
                                 rr.RelativeItem().Row(tr =>
                                 {
                                     tr.Spacing(8);
                                     tr.AutoItem().Text("Cyl").FontColor(Grey);
                                     tr.AutoItem().Text(V(o?.Pres_OD_Cyl));
                                     tr.AutoItem().Text("Sph").FontColor(Grey);
                                     tr.AutoItem().Text(V(o?.Pres_OD_Sph));
                                 });
                             });

                             v.Item().Row(rr =>
                             {
                                 rr.AutoItem().Text("OG :").FontColor(BlueDark);
                                 rr.RelativeItem().Row(tr =>
                                 {
                                     tr.Spacing(8);
                                     tr.AutoItem().Text("Cyl").FontColor(Grey);
                                     tr.AutoItem().Text(V(o?.Pres_OG_Cyl));
                                     tr.AutoItem().Text("Sph").FontColor(Grey);
                                     tr.AutoItem().Text(V(o?.Pres_OG_Sph));
                                 });
                             });
                         });
                    });

                    // DÉSIGNATION
                    col.Item().Column(cc =>
                    {
                        cc.Item().Text("DÉSIGNATION").Bold().FontColor(BlueDark);
                        cc.Item().Border(1).BorderColor(Blue).Padding(8).Table(t =>
                        {
                            t.ColumnsDefinition(cdef =>
                            {
                                cdef.RelativeColumn(5); // Désignation
                                cdef.RelativeColumn(2); // Prix
                            });
                            t.Header(h =>
                            {
                                h.Cell().Element(CellHeader).Text("Désignation");
                                h.Cell().Element(CellHeader).AlignRight().Text("Prix");
                            });

                            foreach (var l in f.Lignes)
                            {
                                t.Cell().Text(l.Designation);
                                t.Cell().AlignRight().Text($"{l.Total:0.00}");
                            }

                            // Lignes vides pour style “papier”
                            var remaining = Math.Max(0, 4 - f.Lignes.Count);
                            for (int i = 0; i < remaining; i++)
                            {
                                t.Cell().Text(" ");
                                t.Cell().Text(" ");
                            }
                        });
                    });

                    // Cachet / Total
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Border(1).BorderColor(Blue).Padding(10)
                          .Text("Cachet - Signature").Italic().FontColor(Grey);

                        r.ConstantItem(12);

                        r.RelativeItem().Border(1).BorderColor(Blue).Padding(10).Column(rc =>
                        {
                            rc.Item().Row(rr =>
                            {
                                rr.RelativeItem().Text("Total :").Bold().FontColor(BlueDark);
                                rr.ConstantItem(10);
                                rr.RelativeItem().AlignRight().Text($"{f.TotalTTC:0.00}");
                            });
                        });
                    });

                    // Montant en lettres
                    col.Item().Text("MONTANT (en lettres)").Bold().FontColor(BlueDark);
                    col.Item().Border(1).BorderColor(Blue).Height(60).Padding(8)
                    .Text(Upper(f.MontantLettres));


                    col.Item().LineHorizontal(1).LineColor(Blue);
                });

                // PIED
                // PIED
                page.Footer()
                    .PaddingTop(6)
                    .Column(foot =>
                {
                    foot.Spacing(4);

                    // 1) Ligne centrée : ICE | Patente | RC | IF
                    foot.Item()
                        .AlignCenter()
                        .Text($"{_brand.ICE}   -   {_brand.Patente}   -   {_brand.RC}   -   {_brand.IF}")
                        .FontSize(9).FontColor(Grey);

                    // 2) Séparateur
                    foot.Item().LineHorizontal(1).LineColor(Blue);

                    // 3) Le reste des infos, en bas, centré
                    foot.Item()
                        .AlignCenter()
                        .Text($"{_brand.Address}")
                        .FontSize(9).FontColor(Grey);

                    foot.Item()
                        .AlignCenter()
                        .Text($"{_brand.Phone}  |  {_brand.Email}  |  {_brand.Website}")
                        .FontSize(9).FontColor(Grey);
                });

            });
        }).GeneratePdf();
    }

}
