using Microsoft.Extensions.Options;

using ComaxRpUI.Profiles;
using ComaxRpUI.Models.Configurations;
using ComaxRpUI.ViewModel;
using ComaxRpUI.ViewModel.Services.Interfaces;
using ComaxRpUI.ViewModel.Services;
using CommunAxiom.DotnetSdk.Helpers.JsonLocalizer;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Blazored.Toast;
using Blazorise;
using Blazorise.Bulma;
using Blazorise.Icons.FontAwesome;
using k8s.Models;
using KubeOps.KubernetesClient;
using ComaxRpUI.Workers;
using ComaxRpUI;
using CommunAxiom.DotnetSdk.Helpers.OIDC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.Configure<ComaxRpUI.DbConf>(x => builder.Configuration.GetSection("DbConfig").Bind(x));
builder.Services.AddDbContext<ComaxRpUI.Models.RpDbContext>();
builder.Services.Configure<OIDCSettings>(x => builder.Configuration.GetSection("OIDC").Bind(x));
builder.Services.AddAutoMapper(typeof(RpEntryProfile).Assembly);

var cl = new KubernetesClient();
builder.Services.AddSingleton<IKubernetesClient>(cl);

builder.Services.AddHostedService<DbToK8sSync>();
builder.Services.AddHostedService<SyncFromCentral>();

builder.Services.AddTransient<CentralClientProvider>();

#region Blazor section


var applicationSettingsSection = builder.Configuration;
builder.Services.Configure<ApplicationSettings>(options => { applicationSettingsSection.Bind(options); });
var cfg = applicationSettingsSection.Get<ApplicationSettings>();
// transactional named http clients
var clientConfigurator = void (IServiceProvider serviceProvider, HttpClient client) =>
{
    client.BaseAddress = new Uri(cfg.BaseAddress);
};

// configuring http clients
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(cfg.BaseAddress) });

builder.Services.AddHttpClient<IProxyManager, ProxyManager>("ProxyManagerClient", clientConfigurator);


builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

CultureInfo[] supportedCultures = new[] {
                new CultureInfo("fr"),
                new CultureInfo("en")
            };

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider()
                };

});

builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();

builder.Services.AddBlazoredToast();
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBulmaProviders()
    .AddFontAwesomeIcons();

#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

#if DEBUG
Console.WriteLine("Launching mariadb in debug");
var dbConf = app.Services.GetService<IOptionsMonitor<ComaxRpUI.DbConf>>();
var curconf = dbConf.CurrentValue;
try
{
    DockerIntegration.Client client = new DockerIntegration.Client();
    await client.InstallContainer("rpmariadb", "mariadb", "10.10",
        new Dictionary<string, string> { { "3306/tcp", curconf.Port } },
        new List<string> { $"MYSQL_ROOT_PASSWORD={curconf.Password}" });
}
catch (Exception ex)
{
    throw;
}

#endif

Console.WriteLine($"Migrate db...");
app.Services.MigrateDb();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Run();
