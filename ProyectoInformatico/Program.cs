using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using ProyectoInformatico.Services;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IMongoClient>(s =>
{
    // Obtener la cadena de conexión de MongoDB desde la configuración
    var connectionString = builder.Configuration.GetConnectionString("MongoDB");

    var settings = MongoClientSettings.FromConnectionString(connectionString);
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);

    return new MongoClient(settings);
});
builder.Services.AddScoped<DoctorService>();

var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Configuración de rutas de nivel superior
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "contacto",
    pattern: "contacto",
    defaults: new { controller = "Home", action = "Contacto" });

app.MapControllerRoute(
    name: "acceso-doctor",
    pattern: "acceso-doctor",
    defaults: new { controller = "Doctor", action = "AccesoDoctor" });

app.MapControllerRoute(
    name: "registro-doctor",
    pattern: "registro-doctor",
    defaults: new { controller = "Doctor", action = "RegistroDoctor" });

app.Run();