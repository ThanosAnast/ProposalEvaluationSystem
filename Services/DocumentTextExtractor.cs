using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using ProposalEvaluationSystem.Models;
using UglyToad.PdfPig;

namespace ProposalEvaluationSystem.Services;

public class DocumentTextExtractor : IDocumentTextExtractor
{
    private const long MaxFileSize = 20 * 1024 * 1024;
    private const int MinimumUsefulPdfCharacters = 100;
    private const string ScannedPdfWarning = "This PDF may be scanned or image-based. Please paste the text manually.";

    public async Task<ProcessedDocument> ExtractAsync(IBrowserFile file, DocumentType documentType, CancellationToken cancellationToken = default)
    {
        var result = new ProcessedDocument
        {
            FileName = file.Name,
            DocumentType = documentType
        };

        var extension = Path.GetExtension(file.Name).ToLowerInvariant();

        try
        {
            await using var stream = file.OpenReadStream(MaxFileSize, cancellationToken);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            var fileBytes = memoryStream.ToArray();

            result.ExtractedText = extension switch
            {
                ".txt" or ".md" => ExtractPlainText(fileBytes),
                ".pdf" => ExtractPdfText(fileBytes),
                _ => string.Empty
            };

            if (extension is not ".txt" and not ".md" and not ".pdf")
            {
                result.Warnings.Add("Unsupported file type. Use .txt, .md, or selectable-text .pdf files.");
                result.ExtractionSucceeded = false;
                return result;
            }

            var trimmedLength = result.ExtractedText.Trim().Length;
            result.ExtractionSucceeded = trimmedLength > 0;

            if (trimmedLength == 0)
            {
                result.Warnings.Add("No readable text was extracted from this file.");
            }

            if (extension == ".pdf" && trimmedLength < MinimumUsefulPdfCharacters)
            {
                result.Warnings.Add(ScannedPdfWarning);
                result.ExtractionSucceeded = trimmedLength >= 25;
            }
        }
        catch (Exception ex)
        {
            result.ExtractionSucceeded = false;
            result.Warnings.Add($"Extraction failed: {ex.Message}");

            if (extension == ".pdf")
            {
                result.Warnings.Add(ScannedPdfWarning);
            }
        }

        return result;
    }

    private static string ExtractPlainText(byte[] fileBytes)
    {
        var preamble = Encoding.UTF8.GetPreamble();
        var offset = fileBytes.AsSpan().StartsWith(preamble) ? preamble.Length : 0;
        return Encoding.UTF8.GetString(fileBytes, offset, fileBytes.Length - offset);
    }

    private static string ExtractPdfText(byte[] fileBytes)
    {
        var builder = new StringBuilder();

        using var stream = new MemoryStream(fileBytes);
        using var document = PdfDocument.Open(stream);

        foreach (var page in document.GetPages())
        {
            builder.AppendLine(page.Text);
            builder.AppendLine();
        }

        return builder.ToString().Trim();
    }
}
