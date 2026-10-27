using TollCalculator.Infrastructure.Registrations;
using TollCalculator.WebApi.Host.Registrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMappers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateDatabaseAsync(CancellationToken.None);
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
