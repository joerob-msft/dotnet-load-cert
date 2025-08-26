using AzureCertInventory.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Temporarily comment out controllers to test
// builder.Services.AddControllers();

// Register certificate services based on environment
var isAppService = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

if (isAppService)
{
    // Use App Service optimized service in Azure
    builder.Services.AddSingleton<CertificateService>();
    builder.Services.AddSingleton<ICertificateService>(provider =>
    {
        var logger = provider.GetRequiredService<ILogger<AppServiceCertificateService>>();
        var baseService = provider.GetRequiredService<CertificateService>();
        return new AppServiceCertificateService(logger, baseService);
    });
}
else
{
    // Use basic service for local development
    builder.Services.AddSingleton<ICertificateService, CertificateService>();
}

// Add API documentation services - temporarily commented out
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new() { 
//         Title = "Azure Certificate Inventory API", 
//         Version = "v1",
//         Description = "API for managing and retrieving certificate information from Azure App Service and local certificate stores."
//     });
    
//     // Include XML comments for better documentation
//     var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
//     var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//     if (File.Exists(xmlPath))
//     {
//         c.IncludeXmlComments(xmlPath);
//     }
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}
// else
// {
//     // Enable Swagger in development
//     app.UseSwagger();
//     app.UseSwaggerUI(c =>
//     {
//         c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure Certificate Inventory API v1");
//         c.RoutePrefix = "swagger";
//     });
// }

app.UseHttpsRedirection();
app.UseStaticFiles();

// DON'T USE app.UseRouting() - this might be claiming all routes
// app.UseRouting();

app.UseAuthorization();

// Minimal routing - only handle very specific paths
app.MapGet("/certinventory", async context =>
{
    var serviceProvider = context.RequestServices;
    var certificateService = serviceProvider.GetRequiredService<ICertificateService>();
    
    var certificates = certificateService.GetPrivateCertificates();
    var hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME") ?? "Unknown";
    
    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync($@"
<!DOCTYPE html>
<html>
<head>
    <title>Certificate Inventory</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .cert {{ border: 1px solid #ccc; margin: 10px 0; padding: 10px; }}
    </style>
</head>
<body>
    <h1>Certificate Inventory</h1>
    <p>Hostname: {hostname}</p>
    <h2>Certificates:</h2>
    {string.Join("", certificates.Select(c => $@"
    <div class='cert'>
        <strong>{c.Name}</strong><br>
        Subject: {c.Subject}<br>
        Expires: {c.ValidUntil}<br>
        Status: {c.Status}
    </div>"))}
</body>
</html>");
});

app.MapGet("/", async context =>
{
    context.Response.Redirect("/certinventory");
});

// Temporarily comment out controllers to test if they're causing the conflict
// app.MapControllers();

app.Run();