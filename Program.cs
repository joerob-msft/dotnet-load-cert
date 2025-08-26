using AzureCertInventory.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSingleton<ICertificateService, CertificateService>();

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

// Add middleware to exclude certain paths from being handled by controllers
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLowerInvariant();
    
    // Skip controller routing for site extension paths
    if (path != null && (path.StartsWith("/ai/") || path.StartsWith("/.well-known/")))
    {
        await next();
        return;
    }
    
    await next();
});

// Map with lower order to ensure site extension routes take precedence
app.MapRazorPages();

// Map controllers with higher order (processed after other routes)
var controllerGroup = app.MapGroup("api/cert-inventory")
    .WithTags("Certificate Inventory");

controllerGroup.MapControllers();

app.Run();