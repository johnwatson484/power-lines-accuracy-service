using Microsoft.EntityFrameworkCore;
using PowerLinesAccuracyService.Accuracy;
using PowerLinesAccuracyService.Analysis;
using PowerLinesAccuracyService.Data;
using PowerLinesAccuracyService.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MessageOptions>(builder.Configuration.GetSection(key: "Message"));
builder.Services.Configure<AnalysisOptions>(builder.Configuration.GetSection(key: "AnalysisUrl"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PowerLinesAccuracyService"), options =>
        options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null))
    );

builder.Services.AddSingleton<IAnalysisApi, AnalysisApi>();
builder.Services.AddScoped<IAccuracyApi, AccuracyApi>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<MessageService>();
builder.Services.AddHostedService<AnalysisService>();
builder.Services.AddHostedService<AccuracyService>();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

ApplyMigrations(app.Services);

await app.RunAsync();

void ApplyMigrations(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
