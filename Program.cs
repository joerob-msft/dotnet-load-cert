using AzureCertInventory.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

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

// Add API documentation services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Azure Certificate Inventory API", 
        Version = "v1",
        Description = "API for managing and retrieving certificate information from Azure App Service and local certificate stores."
    });
    
    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}
else
{
    // Enable Swagger in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure Certificate Inventory API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Simple approach: don't use any complex routing, just basic controllers
app.MapControllers();

app.Run();