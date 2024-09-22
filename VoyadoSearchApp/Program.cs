using VoyadoSearchApp.Logic.Interfaces;
using VoyadoSearchApp.Logic.Services;
using VoyadoSearchApp_Integrations.Interfaces;
using VoyadoSearchApp_Integrations.Services;

var builder = WebApplication.CreateBuilder(args);

var googleBaseAddress = builder.Configuration["GoogleSearch:BaseAddress"];

var bingBaseAddress = builder.Configuration["BingSearch:BaseAddress"];
var bingApiKey = builder.Configuration["BingSearch:ApiKey"];

if (googleBaseAddress != null)
{
    builder.Services.AddHttpClient<GoogleService>(client =>
    {
        client.BaseAddress = new Uri(googleBaseAddress); 
    });
}

if (bingBaseAddress != null)
{
    builder.Services.AddHttpClient<BingService>(client =>
    {
        client.BaseAddress = new Uri(bingBaseAddress);
        client.DefaultRequestHeaders.Add("ApiKey", bingApiKey);
    });
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISearchService, GoogleService>();
builder.Services.AddScoped<ISearchServiceFactory, SearchServiceFactory>(); 
builder.Services.AddScoped<ISearchAggregatorService, SearchAggregatorService>(); 


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowFrontend");

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILogger<Program>>();

app.Run();
