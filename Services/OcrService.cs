using AYExpenseTracker.Models;
using Microsoft.JSInterop;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace AYExpenseTracker.Services
{
    public class OcrWord
    {
        public string Text { get; set; } = "";
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    public class OcrService
    {
        private readonly IJSRuntime _js;

        public OcrService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<List<OcrWord>> ExtractWordsFromImageAsync(string base64Image)
        {
            if (string.IsNullOrWhiteSpace(base64Image))
                return new List<OcrWord>();

            try
            {
                var words = await _js.InvokeAsync<List<OcrWord>>("ocr.recognize", base64Image);
                Console.WriteLine($"--- OCR Words Count: {words.Count} ---");
                return words ?? new List<OcrWord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("OCR JS interop failed: " + ex);
                return new List<OcrWord>();
            }
        }

        public Expense ParseExpenseFromWords(List<OcrWord> words, string rawText)
        {
            var expense = new Expense
            {
                Title = "Receipt",
                Date = DateTime.Now,
                Description = rawText
            };

            if (words == null || words.Count == 0) return expense;

            expense.Amount = ParseAmount(words);
            expense.Date = ParseDate(rawText) ?? expense.Date;
            expense.Title = ParseVendorName(rawText) ?? expense.Title;

            return expense;
        }

        private decimal ParseAmount(List<OcrWord> words)
        {
            // Look for numbers near keywords like "Total", "Amount", "KWD"
            var amountRegex = new Regex(@"(\d{1,3}(?:[.,]?\d{3})*(?:[.,]\d{2,3}))");
            var totalRegex = new Regex(@"\b(total|amount|sum|grand|net|إجمالي|المجموع|صافي)\b", RegexOptions.IgnoreCase);

            decimal bestAmount = 0;
            
            // 1. Look for words that look like amounts and are near "Total" keywords
            foreach (var word in words)
            {
                var text = ConvertArabicNumbers(word.Text).Replace("KWD", "").Replace("KD", "").Trim();
                var match = amountRegex.Match(text);
                if (match.Success)
                {
                    if (decimal.TryParse(match.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                    {
                        // Favor amounts found after a "Total" keyword in the sequence
                        // Or just take the largest one found in the bottom half
                        if (val > bestAmount) bestAmount = val;
                    }
                }
            }

            // Fallback: search raw text for specific patterns
            if (bestAmount == 0)
            {
                var rawText = string.Join(" ", words.Select(w => w.Text)).ToLower();
                var matches = amountRegex.Matches(rawText);
                foreach (Match m in matches)
                {
                    if (decimal.TryParse(m.Value.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                    {
                        if (v > bestAmount) bestAmount = v;
                    }
                }
            }

            return bestAmount;
        }

        private string ConvertArabicNumbers(string input)
        {
            var map = new Dictionary<char, char>
            {
                ['٠'] = '0', ['١'] = '1', ['٢'] = '2', ['٣'] = '3', ['٤'] = '4',
                ['٥'] = '5', ['٦'] = '6', ['٧'] = '7', ['٨'] = '8', ['٩'] = '9'
            };
            return new string(input.Select(c => map.ContainsKey(c) ? map[c] : c).ToArray());
        }

        private DateTime? ParseDate(string text)
        {
            var datePatterns = new[] {
                @"\b(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})\b",
                @"\b(\d{4}[/-]\d{1,2}[/-]\d{1,2})\b",
                @"\b(\d{1,2}\s+(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\s+\d{2,4})\b"
            };

            foreach (var pattern in datePatterns)
            {
                foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    if (DateTime.TryParse(ConvertArabicNumbers(match.Value), CultureInfo.InvariantCulture, out var date))
                        if (date.Year > 2000 && date.Year <= DateTime.Now.Year + 1)
                            return date;
                }
            }
            return null;
        }

        private string ParseVendorName(string text)
        {
            var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 2)
                            .ToArray();

            var negativeKeywords = @"\b(total|tax|date|amount|sum|cash|change|inv|receipt|ticket|tel|phone|st|ave|road|kwd|kd|إجمالي|ضريبة|نقدا|فاتورة|هاتف)\b";

            // The vendor name is almost always in the first 3 non-empty, non-numeric lines
            int count = 0;
            foreach (var line in lines)
            {
                // Skip lines that are mostly numbers or contain negative keywords
                if (Regex.IsMatch(line, @"\d{3,}") || Regex.IsMatch(line, negativeKeywords, RegexOptions.IgnoreCase))
                    continue;

                // Return the first line that looks like a name
                if (Regex.IsMatch(line, @"[a-zA-Z\u0600-\u06FF]{3,}"))
                {
                    return line;
                }

                if (++count > 5) break; 
            }

            return "Receipt";
        }
    }
}