var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot"; // The Angular SPA build output will be copied here.
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSpaStaticFiles();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapReverseProxy();
});
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "wwwroot";
});

app.Run();