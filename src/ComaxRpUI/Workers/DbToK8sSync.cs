using ComaxRpUI.Models;
using CommunAxiom.Commons.Client.Hosting.Operator;
using CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Entities;
using KubeOps.KubernetesClient;

namespace ComaxRpUI.Workers
{
    public class DbToK8sSync : BackgroundService
    {
        private readonly ILogger<DbToK8sSync> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        public DbToK8sSync(IConfiguration configuration, ILogger<DbToK8sSync> logger, IServiceProvider serviceProvider) 
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var svcs = scope.ServiceProvider;
                    var dbcontext = svcs.GetService<RpDbContext>();
                    var kclient = svcs.GetService<IKubernetesClient>();
                    var ns = _configuration["TgtNamespace"];

                    var entries = dbcontext.Set<Models.RpEntry>();

                    // Create or update new resources
                    foreach(var item in entries)
                    {
                        var rp = await kclient.Get<ReverseProxy>(item.Name, ns);
                        ReverseProxy reverseProxy = new ReverseProxy()
                        {
                            Metadata = new k8s.Models.V1ObjectMeta
                            {
                                Name = item.Name,
                                NamespaceProperty = ns,
                                Labels = new Dictionary<string, string>
                                {
                                    { "communaxiom.org/src", "worker" }
                                }
                            },
                            Spec = new ReverseProxySpec
                            {
                                ForwardAddress = item.ForwardAddress,
                                IngressCertManager = item.IngressCertManager,
                                IngressCertSecret = item.IngressCertSecret,
                                IngressHost = item.IngressHost,
                                UseHttps = item.UseHttps
                            }
                        };

                        if (rp == null)
                        {
                            await kclient.Create<ReverseProxy>(reverseProxy);
                        }
                        else
                        {
                            rp.Spec.Assign(reverseProxy.Spec);
                            await kclient.UpdateObject(rp);
                        }
                    }

                    var existing = await kclient.List<ReverseProxy>(ns, "communaxiom.org/src=worker");
                    foreach(var e in existing)
                    {
                        // Entry no longuer exists in database, remove from k8s
                        if(!entries.Any(x=>x.Name == e.Metadata.Name))
                        {
                            await kclient.DeleteObject<ReverseProxy>(_logger, ns, e.Metadata.Name);
                        }
                    }

                }
                catch (Exception e) 
                {
                    _logger.LogError("Something went wrong syncing", e);
                }
                await Task.Delay(30000);
            }
        }
    }
}
