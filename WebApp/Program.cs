using BLL.CosmosOdyssey;
using BLL;
using BLL.Identity;
using DAL.EF;
using Serilog;
using Serilog.Settings.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration, new ConfigurationReaderOptions { SectionName = "Logging:Serilog" })
    .CreateLogger();
builder.Logging.AddSerilog();
builder.Services.AddSingleton(Log.Logger);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDbPersistenceEf(builder.Configuration);

builder.Services.AddControllersWithViews();
builder.Services.AddMvc();
builder.AddCustomIdentity();

builder.Services.AddBll();
builder.Services.AddCosmosOdyssey();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.SeedIdentity();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Route}/{action=Index}");
app.MapRazorPages();

app.Run();