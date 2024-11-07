using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuración del pipeline de solicitud HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configura index.html en /frontend como página de inicio
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "frontend/index.html" }
});
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Configuración de rutas de controladores (si los necesitas)
app.MapControllers();

app.Run();