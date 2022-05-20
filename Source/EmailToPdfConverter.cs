using Microsoft.Exchange.WebServices.Data;
using PdfSharpCore.Pdf;
using PdfSharpCore;
using VetCV.HtmlRendererCore.PdfSharpCore;
using System.Text.RegularExpressions;

namespace MailExporter
{
    internal class EmailToPdfConverter
    {


        // Constructor.
        internal EmailToPdfConverter(Config config, Logger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        // Config of the application.
        Config config;

        // The Logger
        Logger logger;

        // Funktion to convert an email to pdf.
        internal async System.Threading.Tasks.Task ConvertEmailToPdf(object? sender, NewMailEventArgs e)
        {
            // log message
            logger.Log($"Converting {e.NewMail.Id} to pdf.");

            var email = await EmailMessage.Bind(e.NewMail.Service, e.NewMail.Id);

            // Check if Directory exists.
            if (!System.IO.Directory.Exists(config.savePath))
            {
                // Create directory.
                System.IO.Directory.CreateDirectory(config.savePath);
            }
            
            // Check if the email is a html email.
            if (email.Body.BodyType == BodyType.HTML)
            {
                // Generate the pdf and save it.
                var pdf = PdfGenerator.GeneratePdf(email.Body, PdfSharpCore.PageSize.A4);
                using (var fileStream = new System.IO.FileStream(System.IO.Path.Combine(config.savePath, $"{email.Id}.pdf"), System.IO.FileMode.Create))
                {
                    pdf.Save(fileStream);
                }
            }
        }
    }
}