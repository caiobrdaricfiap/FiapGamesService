using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FiapGamesService.API.IoC;
using FiapGamesService.Infrastructure;
using FiapGamesService.Infrastructure.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Config ElasticSettings
builder.Services.Configure<ElasticSettings>(builder.Configuration.GetSection("ElasticSettings"));
builder.Services.AddSingleton<IElasticSettings>(sp => sp.GetRequiredService<IOptions<ElasticSettings>>().Value);

// REGISTRA o client oficial do ES
builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var s = sp.GetRequiredService<IElasticSettings>();
    var settings = new ElasticsearchClientSettings(s.CloudId, new ApiKey(s.ApiKey))
        .DefaultIndex(string.IsNullOrWhiteSpace(s.Index) ? "games" : s.Index);
    return new ElasticsearchClient(settings);
});

builder.Services.AddSingleton<IElasticClient, ElasticClient>();

builder.Services.AddDependencyInjection();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
