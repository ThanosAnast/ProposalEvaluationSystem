using ProposalEvaluationSystem.Models;

namespace ProposalEvaluationSystem.Services;

public class FilePromptTemplateService(IWebHostEnvironment environment) : IPromptTemplateService
{
    private readonly string templateRoot = Path.Combine(environment.ContentRootPath, "PromptTemplates");

    public async Task<IReadOnlyList<PromptTemplateInfo>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        EnsureTemplateDirectory();

        return await Task.Run(() =>
            Directory.GetFiles(templateRoot, "*.md")
                .Select(CreateInfo)
                .OrderBy(template => template.FileName)
                .ToList()
                .AsReadOnly(), cancellationToken);
    }

    public async Task<string> GetTemplateContentAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var path = GetTemplatePath(fileName);
        return File.Exists(path)
            ? await File.ReadAllTextAsync(path, cancellationToken)
            : string.Empty;
    }

    public async Task SaveTemplateAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        var path = GetTemplatePath(fileName);
        await File.WriteAllTextAsync(path, content, cancellationToken);
    }

    public async Task<PromptTemplateInfo> CreateTemplateAsync(string fileName, string content, CancellationToken cancellationToken = default)
    {
        var path = GetTemplatePath(fileName);

        if (File.Exists(path))
        {
            throw new InvalidOperationException($"Template '{Path.GetFileName(path)}' already exists.");
        }

        await File.WriteAllTextAsync(path, content, cancellationToken);
        return CreateInfo(path);
    }

    public async Task<PromptTemplateInfo> DuplicateTemplateAsync(string sourceFileName, string? newFileName = null, CancellationToken cancellationToken = default)
    {
        var sourcePath = GetTemplatePath(sourceFileName);

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("The selected prompt template was not found.", sourceFileName);
        }

        var content = await File.ReadAllTextAsync(sourcePath, cancellationToken);
        var duplicateName = string.IsNullOrWhiteSpace(newFileName)
            ? GetDefaultDuplicateName(sourceFileName)
            : NormalizeFileName(newFileName);

        return await CreateTemplateAsync(duplicateName, content, cancellationToken);
    }

    private void EnsureTemplateDirectory()
    {
        Directory.CreateDirectory(templateRoot);
    }

    private string GetTemplatePath(string fileName)
    {
        EnsureTemplateDirectory();
        return Path.Combine(templateRoot, NormalizeFileName(fileName));
    }

    private string GetDefaultDuplicateName(string sourceFileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(NormalizeFileName(sourceFileName));

        for (var index = 1; index < 100; index++)
        {
            var candidate = $"{baseName}_copy{index}.md";
            if (!File.Exists(Path.Combine(templateRoot, candidate)))
            {
                return candidate;
            }
        }

        return $"{baseName}_copy.md";
    }

    private static string NormalizeFileName(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName.Trim());

        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("Template file name is required.", nameof(fileName));
        }

        if (safeFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Template file name contains invalid characters.", nameof(fileName));
        }

        return Path.GetExtension(safeFileName).Equals(".md", StringComparison.OrdinalIgnoreCase)
            ? safeFileName
            : $"{safeFileName}.md";
    }

    private static PromptTemplateInfo CreateInfo(string path)
    {
        var fileName = Path.GetFileName(path);

        return new PromptTemplateInfo
        {
            FileName = fileName,
            DisplayName = Path.GetFileNameWithoutExtension(fileName).Replace('_', ' '),
            LastModifiedUtc = File.GetLastWriteTimeUtc(path)
        };
    }
}
