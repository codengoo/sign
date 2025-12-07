using Signer.Middlewares;
using Signer.Services;
using Signer.Services.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ISignService, SignService>();
builder.Services.AddScoped<IFileUpload, FileUpload>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
