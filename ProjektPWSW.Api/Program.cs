using Microsoft.EntityFrameworkCore;
using ProjektPWSW.Api.Data;
using ProjektPWSW.Api.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=finanse.db"));
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<SummaryService>();
builder.Services.AddScoped<SavingGoalService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ReceiptOcrService>();

var app = builder.Build();

app.Urls.Add("http://localhost:5000");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "API działa. Wejdź na /swagger");

app.MapControllers();

app.Run();
