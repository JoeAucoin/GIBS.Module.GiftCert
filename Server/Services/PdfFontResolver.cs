using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.IO;

namespace GIBS.Module.GiftCert.Services
{
    /// <summary>
    /// Custom font resolver for PdfSharp/MigraDoc that resolves fonts from Windows system directory or local font files
    /// </summary>
    public class PdfFontResolver : IFontResolver
    {
        private readonly string _fontDirectory;
        private readonly Dictionary<string, string> _fontMap;

        public PdfFontResolver(string fontDirectory = null)
        {
            // Use the provided font directory or default to Windows Fonts folder
            if (string.IsNullOrEmpty(fontDirectory))
            {
                _fontDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
            }
            else
            {
                _fontDirectory = fontDirectory;
            }

            _fontMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Map common font names to their actual file names
                { "Arial", "arial.ttf" },
                { "Arial Black", "ariblk.ttf" },
                { "Arial Bold", "arialbd.ttf" },
                { "Arial Bold Italic", "arialbi.ttf" },
                { "Arial Italic", "ariali.ttf" },
                { "Courier New", "cour.ttf" },
                { "Courier New Bold", "courbd.ttf" },
                { "Courier New Bold Italic", "courbi.ttf" },
                { "Courier New Italic", "couri.ttf" },
                { "Times New Roman", "times.ttf" },
                { "Times New Roman Bold", "timesbd.ttf" },
                { "Times New Roman Bold Italic", "timesbi.ttf" },
                { "Times New Roman Italic", "timesi.ttf" },
                { "Verdana", "verdana.ttf" },
                { "Verdana Bold", "verdanab.ttf" },
                { "Verdana Italic", "verdanai.ttf" },
                { "Verdana Bold Italic", "verdanaz.ttf" },
            };
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Try to find the font in our map
            string fontKey = familyName;
            if (isBold && isItalic)
            {
                fontKey = $"{familyName} Bold Italic";
            }
            else if (isBold)
            {
                fontKey = $"{familyName} Bold";
            }
            else if (isItalic)
            {
                fontKey = $"{familyName} Italic";
            }

            if (_fontMap.TryGetValue(fontKey, out string fontFileName))
            {
                string fontPath = Path.Combine(_fontDirectory, fontFileName);
                if (File.Exists(fontPath))
                {
                    return new FontResolverInfo(fontFileName);
                }
            }

            // If exact match not found, try just the family name
            if (_fontMap.TryGetValue(familyName, out fontFileName))
            {
                string fontPath = Path.Combine(_fontDirectory, fontFileName);
                if (File.Exists(fontPath))
                {
                    return new FontResolverInfo(fontFileName);
                }
            }

            // Fallback to a default font if nothing is found
            return new FontResolverInfo("arial.ttf");
        }

        public byte[] GetFont(string faceName)
        {
            try
            {
                string fontPath = Path.Combine(_fontDirectory, faceName);
                if (File.Exists(fontPath))
                {
                    return File.ReadAllBytes(fontPath);
                }

                // If not found in the default directory, try some alternative locations
                string[] searchPaths = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts", faceName),
                    Path.Combine(AppContext.BaseDirectory, "fonts", faceName),
                    Path.Combine(AppContext.BaseDirectory, "Fonts", faceName),
                };

                foreach (var path in searchPaths)
                {
                    if (File.Exists(path))
                    {
                        return File.ReadAllBytes(path);
                    }
                }

                throw new FileNotFoundException($"Font file '{faceName}' not found in any known location.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot load font '{faceName}': {ex.Message}", ex);
            }
        }
    }
}
