using CentralClient;
using ComaxRpUI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace ComaxRpUI.Workers
{
    public class SyncFromCentral : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly CentralClientProvider _centralClientProvider;
        private readonly IConfiguration _configuration;
        public SyncFromCentral(IServiceProvider svcProvider, ILogger<SyncFromCentral> logger, CentralClientProvider centralClientProvider, IConfiguration configuration) 
        {
            _logger= logger;
            _serviceProvider = svcProvider;
            _centralClientProvider= centralClientProvider;
            _configuration= configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                bool.TryParse(_configuration["CentralSyncActive"], out bool active);
                int.TryParse(_configuration["CentralSyncInterval"], out int interval);
                var fwdAddr = _configuration["ForwardAddressTemplate"];
                var ingrAddr = _configuration["IngressTemplate"];
                var certMan = _configuration["IngressCertManager"];


                if (!active)
                {
                    await Task.Delay((interval > 0 ? interval : 30) * 1000);
                    continue;
                }
                
                try
                {
                    await _centralClientProvider.WithClient(async c =>
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var svc = scope.ServiceProvider;
                        using var ctxt = svc.GetService<RpDbContext>();
                        var apps = await c.HostedAppsAsync("Commons");
                        var set = ctxt.Set<RpEntry>();
                        List<string> hashes= new List<string>();
                        // Add or update
                        foreach (var app in apps)
                        {
                            var cfg = await c.AppConfigAsync(app.AppId);
                            var configs = cfg.ToDictionary(x => x.Key, x => x);
                            var hash = configs["APP_HASH"];
                            var entry = await set.FirstOrDefaultAsync(x => x.Name == hash.Value);
                            hashes.Add(hash.Value);

                            if(entry == null)
                            {
                                entry = new RpEntry()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = hash.Value,
                                    CreatedDate = DateTime.UtcNow,
                                    ModifiedDate = DateTime.UtcNow,
                                    ForwardAddress = fwdAddr.Replace("[HASH]", hash.Value),
                                    IngressHost = ingrAddr.Replace("[HASH]", hash.Value),
                                    IngressCertSecret = $"{hash.Value}-tls",
                                    IngressCertManager = certMan,
                                    Managed= true,
                                    UseHttps = true,
                                    Active = true
                                };
                                set.Add(entry);
                            }
                            else
                            {
                                entry.ModifiedDate = DateTime.UtcNow;
                                entry.ForwardAddress = fwdAddr.Replace("[HASH]", hash.Value);
                                entry.IngressHost = ingrAddr.Replace("[HASH]", hash.Value);
                                entry.IngressCertSecret = $"{hash.Value}-tls";
                                entry.IngressCertManager = certMan;
                                entry.UseHttps = true;
                                entry.Managed = true;
                            }
                            await ctxt.SaveChangesAsync();
                        }

                        // Deactivate old entries
                        var arr = hashes.ToArray();
                        var toRemove = await set.Where(x => x.Managed && !arr.Contains(x.Name)).ToListAsync();
                        set.RemoveRange(toRemove);
                        await ctxt.SaveChangesAsync();
                    });
                }
                catch(Exception ex) 
                {
                    _logger.LogError("Error syncing from central", ex);
                }

                await Task.Delay((interval > 0 ? interval : 30) * 1000);
            }
        }
    }
}
