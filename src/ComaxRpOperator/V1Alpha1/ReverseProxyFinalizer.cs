using CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Builder;
using CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Entities;
using k8s.Models;
using KubeOps.KubernetesClient;
using KubeOps.Operator.Finalizer;

namespace CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1
{
    public class ReverseProxyFinalizer : IResourceFinalizer<ReverseProxy>
    {
        private readonly ILogger<ReverseProxyFinalizer> _logger;
        private readonly IKubernetesClient _client;

        public ReverseProxyFinalizer(ILogger<ReverseProxyFinalizer> logger, IKubernetesClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task FinalizeAsync(ReverseProxy entity)
        {
            await _client.DeleteObject<V1Service>(_logger, entity.Namespace(), entity.GetServiceName());

            await _client.DeleteObject<V1Deployment>(_logger, entity.Namespace(), entity.GetDeploymentName());

            _logger.LogInformation(
                "{Name} in namespace {Namespace} deleted",
                entity.Name(),
                entity.Namespace()
            );

        }
    }
}
