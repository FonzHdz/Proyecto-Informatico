using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using ProyectoInformatico.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using ProyectoInformatico.DTOs;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IMongoClient>(s =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDB");

    var settings = MongoClientSettings.FromConnectionString(connectionString);
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);

    return new MongoClient(settings);
});

builder.Services.AddSingleton(sp =>
    new BlobStorageService(
        builder.Configuration["AzureStorage:ConnectionString"]
    ));


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/acceso-denegado";
        options.AccessDeniedPath = "/acceso-denegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorPolicy", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("PacientePolicy", policy => policy.RequireRole("Paciente"));
});

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<IOptions<EmailSettings>>().Value
);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddScoped<EspecialistaService>();
builder.Services.AddScoped<PacienteService>();
builder.Services.AddScoped<CitaService>();
builder.Services.AddScoped<DiagnosticoService>();
builder.Services.AddScoped<VideoEcografiaService>();
builder.Services.AddSingleton<ImagenRadiologicaService>();
builder.Services.AddScoped<AdminService>();

var app = builder.Build();

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
app.UseAuthentication();
app.UseAuthorization();

var rutaArchivosTemporales = Path.Combine(Directory.GetCurrentDirectory(), "ArchivosTemporales");

if (!Directory.Exists(rutaArchivosTemporales))
{
    Directory.CreateDirectory(rutaArchivosTemporales);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(rutaArchivosTemporales),
    RequestPath = "/ArchivosTemporales"
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "acceso-denegado",
    pattern: "acceso-denegado",
    defaults: new { controller = "Home", action = "AccesoDenegado" });

app.MapControllerRoute(
    name: "contacto",
    pattern: "contacto",
    defaults: new { controller = "Home", action = "Contacto" });

app.MapControllerRoute(
    name: "acceso-doctor",
    pattern: "acceso-doctor",
    defaults: new { controller = "Especialista", action = "AccesoDoctor" });

app.MapControllerRoute(
    name: "acceso-paciente",
    pattern: "acceso-paciente",
    defaults: new { controller = "Paciente", action = "AccesoPaciente" });

app.MapControllerRoute(
    name: "registro-paciente",
    pattern: "registro-paciente",
    defaults: new { controller = "Paciente", action = "RegistroPaciente" });

app.MapControllerRoute(
    name: "login-admin",
    pattern: "login-admin",
    defaults: new { controller = "Admin", action = "AccesoAdmin" });

app.Run();