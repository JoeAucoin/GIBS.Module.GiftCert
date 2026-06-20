using GIBS.Module.GiftCert.Models;
using GIBS.Module.GiftCert.Repository;
using GIBS.Module.GiftCert.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MimeKit;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Environment = System.Environment;
using ParagraphAlignment = MigraDoc.DocumentObjectModel.ParagraphAlignment;
using LineSpacingRule = MigraDoc.DocumentObjectModel.LineSpacingRule;
using Colors = MigraDoc.DocumentObjectModel.Colors;
using Unit = MigraDoc.DocumentObjectModel.Unit;

namespace GIBS.Module.GiftCert.Services
{
    public class ServerGiftCertService : IGiftCertService
    {
        private readonly IGiftCertRepository _GiftCertRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;
        private readonly ISqlRepository _sql;
        private readonly ISettingRepository _settings;
        private readonly IWebHostEnvironment _environment;

        static ServerGiftCertService()
        {
            // Initialize PdfSharp with custom font resolver
            try
            {
                GlobalFontSettings.FontResolver = new PdfFontResolver();
            }
            catch (Exception ex)
            {
                // If font resolver setup fails, log but don't crash startup
                System.Diagnostics.Debug.WriteLine($"Failed to initialize PDF font resolver: {ex.Message}");
            }
        }

        public ServerGiftCertService(IGiftCertRepository GiftCertRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor, ISqlRepository sql, ISettingRepository settings, IWebHostEnvironment environment)
        {
            _GiftCertRepository = GiftCertRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
            _sql = sql;
            _settings = settings;
            _environment = environment;
        }

        // ORIGINAL METHOD COMMENTED OUT - no longer used since PayPal methods are disabled
        /*
        private (string PayPalPayee, string ClientId, string ClientSecret, string Environment) GetPayPalCredentials(int moduleId)
        {
            var settings = _settings.GetSettings("Module", moduleId).ToDictionary(s => s.SettingName, s => s.SettingValue);
            var isSandbox = bool.Parse(settings.GetValueOrDefault("PayPalSandboxMode", "true"));

            if (isSandbox)
            {
                return (
                    settings.GetValueOrDefault("PayPalSandboxPayee"),
                    settings.GetValueOrDefault("PayPalSandboxClientId"),
                    settings.GetValueOrDefault("PayPalSandboxClientSecret"),
                    "Sandbox"
                );
            }
            else
            {
                return (
                    settings.GetValueOrDefault("PayPalPayee"),
                    settings.GetValueOrDefault("OAuthClientId"),
                    settings.GetValueOrDefault("OAuthClientSecret"),
                    "Production"
                );
            }
        }
        */

        public Task<List<Models.GiftCert>> GetGiftCertsAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_GiftCertRepository.GetGiftCerts(ModuleId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Get Attempt {ModuleId}", ModuleId);
                return null;
            }
        }

        public Task<Models.GiftCert> GetGiftCertAsync(int GiftCertId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_GiftCertRepository.GetGiftCert(GiftCertId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Get Attempt {GiftCertId} {ModuleId}", GiftCertId, ModuleId);
                return null;
            }
        }

        public Task<Models.GiftCert> AddGiftCertAsync(Models.GiftCert GiftCert)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, GiftCert.ModuleId, PermissionNames.View))
            {
                GiftCert = _GiftCertRepository.AddGiftCert(GiftCert);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "GiftCert Added {GiftCert}", GiftCert);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Add Attempt {GiftCert}", GiftCert);
                GiftCert = null;
            }
            return Task.FromResult(GiftCert);
        }

        public Task<Models.GiftCert> UpdateGiftCertAsync(Models.GiftCert GiftCert)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, GiftCert.ModuleId, PermissionNames.View))
            {
                GiftCert = _GiftCertRepository.UpdateGiftCert(GiftCert);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "GiftCert Updated {GiftCert}", GiftCert);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Update Attempt {GiftCert}", GiftCert);
                GiftCert = null;
            }
            return Task.FromResult(GiftCert);
        }

        public Task DeleteGiftCertAsync(int GiftCertId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                _GiftCertRepository.DeleteGiftCert(GiftCertId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "GiftCert Deleted {GiftCertId}", GiftCertId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized GiftCert Delete Attempt {GiftCertId} {ModuleId}", GiftCertId, ModuleId);
            }
            return Task.CompletedTask;
        }

        public async Task<PayPalOrderResponseDto> CreatePayPalOrderAsync(Models.GiftCert giftCert)
        {
            _logger.Log(LogLevel.Warning, this, LogFunction.Create, 
                "PayPal order creation requested but PayPalServerSDK dependencies not properly loaded. " +
                "Ensure PayPalServerSDK package is installed in the Oqtane application.");

            await Task.Delay(0);
            return null;
        }

        // NOTE: Original PayPal order creation code commented out to prevent assembly loading issues at startup

        public async Task<string> CapturePayPalOrderAsync(string orderId, int moduleId)
        {
            _logger.Log(LogLevel.Warning, this, LogFunction.Create, 
                "PayPal order capture requested but PayPalServerSDK dependencies not properly loaded. " +
                "Ensure PayPalServerSDK package is installed in the Oqtane application.");

            await Task.Delay(0);
            return null;
        }

        // ORIGINAL METHOD COMMENTED OUT - uncomment when PayPalServerSDK is properly available
        /*
        public async Task<string> CapturePayPalOrderAsyncOriginal(string orderId, int moduleId)
        {
            // Original PayPal capture code was here - intentionally omitted to prevent assembly loading
        }
        */

        public async Task SendHtmlEmailAsync(string recipientName, string recipientEmail, string bccName, string bccEmail, string replyToName, string replyToEmail, string subject, string htmlMessage)
        {
            // Retrieve Site Settings
            //await SettingService.GetSiteSettingsAsync(PageState.Site.SiteId);
            //  var settings = _settings.GetSettings(,_alias.SiteId).ToList();
            var settings = _settings.GetSettings(EntityNames.Site, _alias.SiteId, EntityNames.Host, -1).ToList();

            string GetSetting(string key, string defaultValue) =>
                settings.FirstOrDefault(s => s.SettingName == key)?.SettingValue ?? defaultValue;

            string smtpHost = GetSetting("SMTPHost", "");
            int smtpPort = int.Parse(GetSetting("SMTPPort", "587"));
            string smtpUserName = GetSetting("SMTPUsername", "");
            string smtpPassword = GetSetting("SMTPPassword", "");
            string smtpSender = GetSetting("SMTPSender", smtpUserName);
            string smtpSSL = GetSetting("SMTPSSL", "false"); // Oqtane often has this setting

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Webmaster", smtpSender));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.ReplyTo.Add(new MailboxAddress(replyToName, replyToEmail));

            if (!string.IsNullOrEmpty(bccEmail))
            {
                message.Bcc.Add(new MailboxAddress(bccName, bccEmail));
            }

            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage,
                TextBody = "Please view this email in a client that supports HTML."
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.CheckCertificateRevocation = false;

            // Connect
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.Auto);

            // Authenticate
            if (!string.IsNullOrEmpty(smtpUserName) && !string.IsNullOrEmpty(smtpPassword))
            {
                await client.AuthenticateAsync(smtpUserName, smtpPassword);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task<byte[]> GenerateCertificatePdfAsync(int giftCertId, int moduleId)
        {
            try
            {
                // Retrieve the gift certificate
                var giftCert = _GiftCertRepository.GetGiftCert(giftCertId);
                if (giftCert == null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "Gift certificate not found: {GiftCertId}", giftCertId);
                    return null;
                }

                // Check authorization
                if (!_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized PDF Generation Attempt {GiftCertId}", giftCertId);
                    return null;
                }

                // Retrieve module settings for certificate template
                var settings = _settings.GetSettings("Module", moduleId).ToDictionary(s => s.SettingName, s => s.SettingValue);

                string certBannerText = settings.GetValueOrDefault("CertBannerText", "Gift Certificate");
                string certReturnAddress = settings.GetValueOrDefault("CertReturnAddress", "");
                string certFooterText = settings.GetValueOrDefault("CertFooterText", "");
                string certNotes = settings.GetValueOrDefault("CertNotes", "");
                string certLogo = settings.GetValueOrDefault("CertLogo", "");
                string certWatermark = settings.GetValueOrDefault("CertWatermark", "");
                string mailToAddressField = settings.GetValueOrDefault("MailToAddressField", $"{giftCert.ToName}\r\n{giftCert.MailToAddress}\r\n{giftCert.MailToCity}, {giftCert.MailToState} {giftCert.MailToZip}");

                string certAmountNumber = "$" + giftCert.CertAmount.ToString("F2");
                int certAmountInt = (int)giftCert.CertAmount;
                string certAmountWords = NumberToWords(certAmountInt) + " dollars";

                // Create the MigraDoc document
                var document = new Document();

                // Page Setup
                Unit width, height;
                PageSetup.GetPageSize(PageFormat.Letter, out width, out height);

                Section section = document.AddSection();
                section.PageSetup.PageHeight = height;
                section.PageSetup.PageWidth = width;
                section.PageSetup.LeftMargin = 20;
                section.PageSetup.RightMargin = 10;
                section.PageSetup.TopMargin = 20;

                // Logo
                if (!string.IsNullOrEmpty(certLogo))
                {
                    // Resolve Path: Assuming certLogo is relative to Site Root, e.g. "Images/logo.png"
                    string contentRoot = Path.Combine(_environment.WebRootPath, "Content", "Tenants", _alias.TenantId.ToString(), "Sites", _alias.SiteId.ToString());
                    string logoPath = Path.Combine(contentRoot, certLogo.TrimStart('/', '\\'));

                    if (System.IO.File.Exists(logoPath))
                    {
                        var image = section.Headers.Primary.AddImage(logoPath);
                        image.LockAspectRatio = true;
                        image.RelativeVertical = MigraDoc.DocumentObjectModel.Shapes.RelativeVertical.Line;
                        image.RelativeHorizontal = MigraDoc.DocumentObjectModel.Shapes.RelativeHorizontal.Margin;
                        image.Top = MigraDoc.DocumentObjectModel.Shapes.ShapePosition.Top;
                        image.Left = MigraDoc.DocumentObjectModel.Shapes.ShapePosition.Right;
                        image.WrapFormat.Style = MigraDoc.DocumentObjectModel.Shapes.WrapStyle.Through;
                    }
                }

                // Header Table (Return Address)
                var headerTable = new Table();
                headerTable.Borders.Width = 0;
                headerTable.LeftPadding = 10;
                headerTable.RightPadding = 10;

                Unit columnWidth = ((section.PageSetup.PageWidth - section.PageSetup.LeftMargin - section.PageSetup.RightMargin) / 2);
                headerTable.AddColumn(columnWidth);

                var row1 = headerTable.AddRow();
                row1.Cells[0].Elements.AddParagraph(certReturnAddress);
                row1.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                section.Add(headerTable);

                // Spacing
                var paragraph = section.AddParagraph();
                paragraph.Format.LineSpacingRule = LineSpacingRule.Exactly;
                paragraph.Format.LineSpacing = Unit.FromMillimeter(30.0);

                // Mail To Table
                var tableMailTo = new Table();
                tableMailTo.Borders.Width = 0;
                tableMailTo.LeftPadding = 20;
                tableMailTo.RightPadding = 10;

                tableMailTo.AddColumn(section.PageSetup.PageWidth - section.PageSetup.LeftMargin - section.PageSetup.RightMargin);

                var rowMailTo = tableMailTo.AddRow();
                var cellMailTo = rowMailTo.Cells[0];
                cellMailTo.Format.Font.Color = Colors.Black;
                cellMailTo.Format.Alignment = ParagraphAlignment.Left;
                cellMailTo.Format.Font.Name = "Times New Roman";
                cellMailTo.Format.Font.Size = 20;

                cellMailTo.AddParagraph(mailToAddressField);
                section.Add(tableMailTo);

                // Spacer
                var pSpacer = section.AddParagraph();
                pSpacer.Format.LineSpacingRule = LineSpacingRule.Exactly;
                pSpacer.Format.LineSpacing = Unit.FromMillimeter(20.0);

                // Cert Table
                var tableCert = new Table();
                tableCert.Borders.Width = 0;
                tableCert.LeftPadding = 10;
                tableCert.RightPadding = 10;
                tableCert.Borders.Right.Visible = true;

                var colCert = tableCert.AddColumn(section.PageSetup.PageWidth - section.PageSetup.LeftMargin - section.PageSetup.RightMargin);
                colCert.Format.Alignment = ParagraphAlignment.Center;

                // Banner
                var rowCert1 = tableCert.AddRow();
                var cellCert1 = rowCert1.Cells[0];
                cellCert1.Format.Font.Color = Colors.Black;
                cellCert1.Format.Font.Name = "Times New Roman";
                cellCert1.Format.Font.Size = 30;
                cellCert1.AddParagraph(certBannerText);

                // Amount Words
                var rowCert2 = tableCert.AddRow();
                var cellCert2 = rowCert2.Cells[0];
                cellCert2.Format.Font.Color = Colors.Black;
                cellCert2.Format.Alignment = ParagraphAlignment.Center;
                cellCert2.Format.Font.Name = "Times New Roman";
                cellCert2.Format.Font.Size = 30;
                cellCert2.AddParagraph(certAmountWords);

                // Details
                var rowCert3 = tableCert.AddRow();
                var cellCert3 = rowCert3.Cells[0];
                cellCert3.Format.Font.Color = Colors.Black;
                cellCert3.Format.Alignment = ParagraphAlignment.Left;
                cellCert3.Format.Font.Name = "Times New Roman";
                cellCert3.Format.Font.Size = 20;
                cellCert3.Format.SpaceBefore = Unit.FromMillimeter(10.0);
                cellCert3.Format.LeftIndent = Unit.FromMillimeter(15.0);

                string details = $"Date: {giftCert.CreatedOn.ToShortDateString()}{System.Environment.NewLine}" +
                                 $"Presented to: {giftCert.ToName}{System.Environment.NewLine}" +
                                 $"From: {giftCert.FromName}{System.Environment.NewLine}" +
                                 $"Amount: {certAmountNumber}";
                cellCert3.AddParagraph(details);

                // Signature
                var rowCert4 = tableCert.AddRow();
                var cellCert4 = rowCert4.Cells[0];
                cellCert4.Format.Font.Color = Colors.Black;
                cellCert4.Format.Alignment = ParagraphAlignment.Left;
                cellCert4.Format.Font.Name = "Arial";
                cellCert4.Format.Font.Size = 12;
                cellCert4.Format.SpaceBefore = Unit.FromMillimeter(10.0);
                cellCert4.Format.LeftIndent = Unit.FromMillimeter(15.0);
                cellCert4.AddParagraph("Authorized Signature: __________________________________________________");

                // Cert Number
                var rowCert5 = tableCert.AddRow();
                var cellCert5 = rowCert5.Cells[0];
                cellCert5.Format.Font.Color = Colors.Black;
                cellCert5.Format.Alignment = ParagraphAlignment.Center;
                cellCert5.Format.Font.Name = "Arial";
                cellCert5.Format.Font.Size = 10;
                cellCert5.Format.SpaceBefore = Unit.FromMillimeter(3.0);
                cellCert5.AddParagraph($"Certificate Number 00{giftCert.GiftCertId}");

                document.LastSection.Add(tableCert);

                // Notes
                if (!string.IsNullOrWhiteSpace(certNotes))
                {
                    var pNotes = section.AddParagraph();
                    pNotes.Format.LineSpacingRule = LineSpacingRule.Exactly;
                    pNotes.Format.LineSpacing = Unit.FromMillimeter(8.0);
                    pNotes.Format.SpaceBefore = Unit.FromMillimeter(10.0);
                    pNotes.AddText($"NOTES:{System.Environment.NewLine}{certNotes}");
                }

                // Footer
                var footerText = new Paragraph();
                footerText.Format.Alignment = ParagraphAlignment.Center;
                footerText.AddText(certFooterText);
                section.Footers.Primary.Add(footerText);

                // Render
                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer();
                pdfRenderer.Document = document;
                pdfRenderer.RenderDocument();

                byte[] pdfBytes;
                using (var stream = new MemoryStream())
                {
                    pdfRenderer.PdfDocument.Save(stream, false);
                    pdfBytes = stream.ToArray();
                }

                // Watermark
                if (!string.IsNullOrWhiteSpace(certWatermark))
                {
                    using (var streamIn = new MemoryStream(pdfBytes))
                    using (var pdfIn = PdfReader.Open(streamIn, PdfDocumentOpenMode.Import))
                    using (var pdfOut = new PdfDocument())
                    {
                        for (int i = 0; i < pdfIn.PageCount; i++)
                        {
                            var pg = pdfIn.Pages[i];
                            pg = pdfOut.AddPage(pg);

                            using (var gfx = XGraphics.FromPdfPage(pg, XGraphicsPdfPageOptions.Prepend))
                            {
                                var fontWM = new XFont("Verdana", 62);
                                var size = gfx.MeasureString(certWatermark, fontWM);

                                gfx.TranslateTransform(pg.Width.Point / 2, pg.Height.Point / 2);
                                gfx.RotateTransform(-Math.Atan(pg.Height.Point / pg.Width.Point) * 180 / Math.PI);
                                gfx.TranslateTransform(-pg.Width.Point / 2, -pg.Height.Point / 2);

                                var format = new XStringFormat();
                                format.Alignment = XStringAlignment.Near;
                                format.LineAlignment = XLineAlignment.Near;

                                var brush = new XSolidBrush(XColor.FromArgb(32, 0, 0, 255));

                                gfx.DrawString(certWatermark, fontWM, brush,
                                    new XPoint((pg.Width.Point - size.Width) / 2, (pg.Height.Point - size.Height) / 2),
                                    format);
                            }
                        }

                        using (var streamOut = new MemoryStream())
                        {
                            pdfOut.Save(streamOut);
                            return streamOut.ToArray();
                        }
                    }
                }

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, ex, 
                    "Error generating PDF: {Error}", ex.Message);
                return null;
            }
        }

        private static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        public byte[] GetFont(string faceName)
        {
            // Attempt to load from standard Windows Fonts directory
            var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), faceName);

            if (System.IO.File.Exists(fontPath))
            {
                return (System.IO.File.ReadAllBytes(fontPath));
            }

            // If not found, you might want to log this or return null (which causes an exception)
            // For Linux/Docker, you would check /usr/share/fonts or a local 'Fonts' folder here.
            return null;
        }
    }
}