using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DairyManagementSystem.Helpers
{
    public class PdfSection
    {
        public string Title { get; set; } = string.Empty;
        public string[] Headers { get; set; } = Array.Empty<string>();
        public List<string[]> Rows { get; set; } = new();
    }

    // Stateless, no DI dependencies — a pure rendering utility, not a
    // service. Every report's controller builds one or more PdfSection
    // objects from its already-fetched ReportService data and hands them
    // here; this class knows nothing about dairy-domain concepts, only how
    // to lay out a title + sections of tables on a page.
    public static class PdfReportGenerator
    {
        public static byte[] Generate(string reportTitle, string subtitle, IEnumerable<PdfSection> sections, string[]? summaryLines = null)
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Text(reportTitle).FontSize(16).Bold();
                        col.Item().Text(subtitle).FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        foreach (var section in sections)
                        {
                            if (!string.IsNullOrEmpty(section.Title))
                            {
                                col.Item().PaddingTop(10).PaddingBottom(4).Text(section.Title).FontSize(12).Bold();
                            }

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    foreach (var _ in section.Headers)
                                    {
                                        columns.RelativeColumn();
                                    }
                                });

                                table.Header(header =>
                                {
                                    foreach (var h in section.Headers)
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(h).Bold();
                                    }
                                });

                                foreach (var row in section.Rows)
                                {
                                    foreach (var cell in row)
                                    {
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(cell);
                                    }
                                }
                            });

                            if (section.Rows.Count == 0)
                            {
                                col.Item().PaddingTop(4).Text("No data for this period.").Italic().FontColor(Colors.Grey.Medium);
                            }
                        }

                        if (summaryLines is { Length: > 0 })
                        {
                            col.Item().PaddingTop(12).Column(sc =>
                            {
                                foreach (var line in summaryLines)
                                {
                                    sc.Item().Text(line).Bold();
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span(DateTime.Now.ToString("dd-MMM-yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span(" — Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
