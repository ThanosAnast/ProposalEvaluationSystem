using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using ProposalEvaluationSystem.Components;
using ProposalEvaluationSystem.Models;
using ProposalEvaluationSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));

builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<ExperimentsOptions>(builder.Configuration.GetSection("Experiments"));

builder.Services.AddSingleton<IDocumentTextExtractor, DocumentTextExtractor>();
builder.Services.AddSingleton<IPromptTemplateService, FilePromptTemplateService>();
builder.Services.AddSingleton<IPromptBuilder, PromptBuilder>();
builder.Services.AddSingleton<IEvaluationProfileService, EvaluationProfileService>();
builder.Services.AddSingleton<IScoreCalculator, ScoreCalculator>();
builder.Services.AddSingleton<IEvaluationResultProcessor, EvaluationResultProcessor>();
builder.Services.AddSingleton<IComparisonCalculator, ComparisonCalculator>();
builder.Services.AddSingleton<IExperimentRepository, FileExperimentRepository>();
builder.Services.AddSingleton<IExperimentRunFactory, ExperimentRunFactory>();
builder.Services.AddSingleton<IExperimentCsvExporter, ExperimentCsvExporter>();
builder.Services.AddSingleton<IExperimentJsonExporter, ExperimentJsonExporter>();
builder.Services.AddScoped<EvaluationWorkflowState>();

builder.Services.AddHttpClient<OpenAiEvaluationService>((serviceProvider, httpClient) =>
{
    var openAiOptions = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
    httpClient.Timeout = TimeSpan.FromSeconds(openAiOptions.TimeoutSeconds);
});
builder.Services.AddScoped<IEvaluationService>(
    serviceProvider => serviceProvider.GetRequiredService<OpenAiEvaluationService>());
builder.Services.AddScoped<IEvaluationRunner, EvaluationRunner>();

builder.Services.AddHttpClient<OpenAiEsrComparisonService>((serviceProvider, httpClient) =>
{
    var openAiOptions = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
    httpClient.Timeout = TimeSpan.FromSeconds(openAiOptions.TimeoutSeconds);
});
builder.Services.AddScoped<IEsrComparisonService>(
    serviceProvider => serviceProvider.GetRequiredService<OpenAiEsrComparisonService>());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();
app.MapStaticAssets();

app.MapGet("/api/experiments/experiment-runs.csv", async (
    IExperimentRepository repository,
    IExperimentCsvExporter exporter,
    CancellationToken cancellationToken) =>
{
    var runs = await repository.GetAllAsync(cancellationToken);
    var csv = exporter.ExportExperimentRuns(runs);
    return Results.File(
        Encoding.UTF8.GetBytes(csv),
        "text/csv; charset=utf-8",
        "experiment-runs.csv");
});

app.MapGet("/api/experiments/criterion-comparisons.csv", async (
    IExperimentRepository repository,
    IExperimentCsvExporter exporter,
    CancellationToken cancellationToken) =>
{
    var runs = await repository.GetAllAsync(cancellationToken);
    var csv = exporter.ExportCriterionComparisons(runs);
    return Results.File(
        Encoding.UTF8.GetBytes(csv),
        "text/csv; charset=utf-8",
        "criterion-comparisons.csv");
});

app.MapGet("/api/experiments/export.csv", () =>
    Results.Redirect("/api/experiments/experiment-runs.csv"));

app.MapGet("/api/experiments/{datasetId}/{runId}/download", async Task<IResult> (
    string datasetId,
    string runId,
    IExperimentRepository repository,
    IExperimentJsonExporter exporter,
    CancellationToken cancellationToken) =>
{
    ExperimentRun? run;
    try
    {
        run = await repository.GetAsync(datasetId, runId, cancellationToken);
    }
    catch (ArgumentException)
    {
        return Results.BadRequest();
    }

    if (run is null)
    {
        return Results.NotFound();
    }

    var json = exporter.Export(run);
    var fileName = $"{run.Metadata.DatasetId}_{run.Metadata.RunId}.json";
    return Results.File(
        Encoding.UTF8.GetBytes(json),
        "application/json; charset=utf-8",
        fileName);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
