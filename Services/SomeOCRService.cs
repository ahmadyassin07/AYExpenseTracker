using System.Threading.Tasks;

namespace AYExpenseTracker.Services
{
    public static class SomeOCRService
    {
        // Placeholder for now — implement real OCR later
        public static async Task<string> ExtractTextAsync(string base64Image)
        {
            await Task.Delay(500); // simulate API delay
            return "Sample extracted text (implement OCR later)";
        }
    }
}