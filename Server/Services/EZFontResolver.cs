using Microsoft.AspNetCore.Hosting;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Oqtane.Infrastructure;
using Environment = System.Environment;

namespace GIBS.Module.GiftCert.Services
{
    public class EZFontResolver : IFontResolver
    {
        private readonly string _fontPath;

        public EZFontResolver(IWebHostEnvironment environment)
        {
            _fontPath = Path.Combine(environment.WebRootPath, "_content", "GIBS.Module.GiftCert", "fonts");
        }

        public EZFontResolver()
        {
            // Fallback for static context where IWebHostEnvironment is not available.
            // Assumes the application is running from the content root and wwwroot is present.
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _fontPath = Path.Combine(webRoot, "_content", "GIBS.Module.GiftCert", "fonts");
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string name = familyName.ToLower().Trim();

            // Map the font family and style to a specific file key
            switch (name)
            {
                case "times new roman":
                    if (isBold && isItalic) return new FontResolverInfo("TimesNewRoman#bi");
                    if (isBold) return new FontResolverInfo("TimesNewRoman#b");
                    if (isItalic) return new FontResolverInfo("TimesNewRoman#i");
                    return new FontResolverInfo("TimesNewRoman#");

                case "arial":
                    if (isBold && isItalic) return new FontResolverInfo("Arial#bi");
                    if (isBold) return new FontResolverInfo("Arial#b");
                    if (isItalic) return new FontResolverInfo("Arial#i");
                    return new FontResolverInfo("Arial#");

                case "courier new":
                    if (isBold && isItalic) return new FontResolverInfo("CourierNew#bi");
                    if (isBold) return new FontResolverInfo("CourierNew#b");
                    if (isItalic) return new FontResolverInfo("CourierNew#i");
                    return new FontResolverInfo("CourierNew#");

                // Added Verdana support for Watermark
                case "verdana":
                    if (isBold && isItalic) return new FontResolverInfo("Verdana#bi");
                    if (isBold) return new FontResolverInfo("Verdana#b");
                    if (isItalic) return new FontResolverInfo("Verdana#i");
                    return new FontResolverInfo("Verdana#");
            }

            // Return null to use the default system fallback (may fail in Azure/Linux)
            return null;
        }

        public byte[] GetFont(string faceName)
        {
            // Map the key from ResolveTypeface to the actual filename on disk
            string fileName = faceName switch
            {
                "TimesNewRoman#" => "TIMES.TTF",
                "TimesNewRoman#b" => "TIMESBD.TTF",
                "TimesNewRoman#i" => "TIMESI.TTF",
                "TimesNewRoman#bi" => "TIMESBI.TTF",

                "Arial#" => "ARIAL.TTF",
                "Arial#b" => "ARIALBD.TTF",
                "Arial#i" => "ARIALI.TTF",
                "Arial#bi" => "ARIALBI.TTF",

                "CourierNew#" => "COUR.TTF",
                "CourierNew#b" => "COURBD.TTF",
                "CourierNew#i" => "COURI.TTF",
                "CourierNew#bi" => "COURBI.TTF",

                "Verdana#" => "VERDANA.TTF",
                "Verdana#b" => "VERDANAB.TTF",
                "Verdana#i" => "VERDANAI.TTF",
                "Verdana#bi" => "VERDANAZ.TTF",

                _ => null
            };

            if (fileName == null || string.IsNullOrEmpty(_fontPath)) return null;

            string fullPath = Path.Combine(_fontPath, fileName);
            return File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;
        }
    }
}