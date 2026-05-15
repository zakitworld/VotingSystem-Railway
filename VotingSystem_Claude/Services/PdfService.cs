using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using VotingSystem_Claude.Services.Interfaces;
using System.IO;

namespace VotingSystem_Claude.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateVotingReceipt(string voterName, string electionTitle, string studentId, string classInfo)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A6);
                document.SetMargins(20, 20, 20, 20);

                // Header
                var header = new Paragraph("VOTING RECEIPT")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetBold()
                    .SetFontColor(ColorConstants.DARK_GRAY);
                document.Add(header);

                document.Add(new iText.Layout.Element.LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine()));


                // Details
                document.Add(new Paragraph("Election:").SetBold().SetFontSize(10));
                document.Add(new Paragraph(electionTitle).SetFontSize(10).SetMarginBottom(10));

                document.Add(new Paragraph("Voter Name:").SetBold().SetFontSize(10));
                document.Add(new Paragraph(voterName).SetFontSize(10).SetMarginBottom(10));

                document.Add(new Paragraph("Student ID:").SetBold().SetFontSize(10));
                document.Add(new Paragraph(studentId).SetFontSize(10).SetMarginBottom(10));

                document.Add(new Paragraph("Class:").SetBold().SetFontSize(10));
                document.Add(new Paragraph(classInfo).SetFontSize(10).SetMarginBottom(10));

                document.Add(new Paragraph("Date & Time:").SetBold().SetFontSize(10));
                document.Add(new Paragraph(DateTime.Now.ToString("f")).SetFontSize(10).SetMarginBottom(20));

                // Transaction Hash (Mock for now)
                var hash = Guid.NewGuid().ToString("N").ToUpper();
                document.Add(new Paragraph("Transaction ID:").SetBold().SetFontSize(8));
                document.Add(new Paragraph(hash).SetFontSize(8).SetFontColor(ColorConstants.GRAY));

                // Footer
                document.Add(new Paragraph("\nThank you for voting!")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(8)
                    .SetItalic()
                    .SetFontColor(ColorConstants.GRAY));

                document.Close();
                return stream.ToArray();
            }
        }
    }
}
