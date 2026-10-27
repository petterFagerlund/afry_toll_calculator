using TollCalculator.Application.Registrations;
using TollCalculator.Infrastructure.Registrations;
using TollCalculator.WebApi.Host.Auth.Registrations;
using TollCalculator.WebApi.Host.Registrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddVersionedApi();
builder.Services.AddProblemDetailsErrorHandling();
builder.Services.AddHealthChecks();

builder.Services.AddJwtOptions(builder.Configuration);
builder.Services.AddBearerAuthentication(builder.Environment);
builder.Services.AddTokenRateLimiting();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddMappers();
builder.Services.AddValidators();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateDatabaseAsync(CancellationToken.None);
}
else
{
    app.UseHsts();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
