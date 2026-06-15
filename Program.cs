using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using ProposalEvaluationSystem.Components;
using ProposalEvaluationSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));

builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddSingleton<IDocumentTextExtractor, DocumentTextExtractor>();
builder.Services.AddSingleton<IPromptTemplateService, FilePromptTemplateService>();
builder.Services.AddSingleton<IPromptBuilder, PromptBuilder>();
builder.Services.AddSingleton<MockEvaluationService>();
builder.Services.AddHttpClient<OpenAiEvaluationService>((serviceProvider, httpClient) =>
{
    var openAiOptions = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
    httpClient.Timeout = TimeSpan.FromSeconds(openAiOptions.TimeoutSeconds);
});
builder.Services.AddSingleton<IEvaluationRunner, EvaluationRunner>();
builder.Services.AddSingleton<IEvaluationService, MockEvaluationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
