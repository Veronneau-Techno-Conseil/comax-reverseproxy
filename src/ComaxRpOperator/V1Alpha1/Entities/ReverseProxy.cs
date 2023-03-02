using k8s.Models;
using KubeOps.Operator.Entities;
using System.Text.Json.Serialization;

namespace CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Entities
{
    [KubernetesEntity(
        ApiVersion = "v1alpha1",
        Group = "communaxiom.org",
        Kind = "ReverseProxy",
        PluralName = "reverseproxies")]
    public class ReverseProxy : CustomKubernetesEntity<ReverseProxySpec, ReverseProxyState>, IAssignableSpec<ReverseProxySpec>
    {
        public ReverseProxy()
        {
            this.ApiVersion = "communaxiom.org/v1alpha1";
            this.Kind = "ReverseProxy";
        }

        public void Assign(IAssignableSpec<ReverseProxySpec> other)
        {
            this.Spec.Assign(other.Spec);
        }

        public void Assign(IAssignableSpec other)
        {
            var ao = (ReverseProxy)other;
            this.Spec.Assign(ao.Spec);
        }
    }

    public static class ReverseProxyExtensions
    {
        public static string GetDeploymentName(this ReverseProxy agentReferee)
        {
            return $"{agentReferee.Name()}-depl";
        }
        public static string GetServiceName(this ReverseProxy agentReferee)
        {
            return $"{agentReferee.GetDeploymentName()}-ep";
        }
        
        public static string GetNginxSecretName(this ReverseProxy agentReferee)
        {
            return $"{agentReferee.GetDeploymentName()}-crts";
        }

        public static string GetIngrSecretName(this ReverseProxy agentReferee)
        {
            return $"{agentReferee.GetDeploymentName()}-tls";
        }

        public static string GetConfigName(this ReverseProxy agentReferee)
        {
            return $"{agentReferee.GetDeploymentName()}-cfg";
        }

        public static string GetServiceAccountName(this ReverseProxy agentReferee)
        {
            return $"{agentReferee.GetDeploymentName()}-sa";
        }

        public static string GetIngressName(this ReverseProxy agentReferee)
        {
            return $"{agentReferee.GetDeploymentName()}-ingr";
        }
    }

    public class ReverseProxyState : IComaxState
    {

        [JsonPropertyName("currentState")]
        public string CurrentState { get; set; } = Status.Unknown.ToString();

        [JsonPropertyName("stateTsMs")]
        public long StateTsMs { get; set; } = 0;

        [JsonPropertyName("stateTs")]
        public long StateTs { get; set; } = 0;
    }

    public class ReverseProxySpec : IAssignable<ReverseProxySpec>
    {
        public void Assign(ReverseProxySpec other)
        {
            this.Annotations = other.Annotations;
            this.EnvironmentVariables = other.EnvironmentVariables;
            
            this.UseHttps = other.UseHttps;
            this.Labels = other.Labels;
            this.Resources = other.Resources;
        }


        [JsonPropertyName("ingressHost")]
        public string IngressHost { get; set; }

        [JsonPropertyName("ingressCertManager")]
        public string IngressCertManager { get; set; }

        [JsonPropertyName("ingressCertSecret")]
        public string IngressCertSecret { get; set; }

        [JsonPropertyName("useHttps")]
        public bool UseHttps { get; set; }



        [JsonPropertyName("forwardAddress")]
        public string ForwardAddress { get; set; }

        /// <summary>
        /// Optional resource limits and requests.
        /// Defaults to limit.cpu: 500m, limit.memory: 512M, requests.cpu: 200m, requests.memory: 256M
        /// </summary>
        [JsonPropertyName("resources")]
        public V1ResourceRequirements? Resources { get; set; } = new();

        /// <summary>
        /// Additional environment variables to add to the primary container.
        /// </summary>
        [JsonPropertyName("environmentVariables")]
        public IList<V1EnvVar> EnvironmentVariables { get; set; } = new List<V1EnvVar>();

        /// <summary>
        /// Optional extra annotations to add to deployment.
        /// </summary>
        [JsonPropertyName("annotations")]
        public IDictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Optional extra labels to add to deployment.
        /// </summary>
        [JsonPropertyName("labels")]
        public IDictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
    }
}
