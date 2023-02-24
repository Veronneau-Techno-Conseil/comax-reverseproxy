using CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Entities;
using k8s;
using k8s.Models;
using System.Collections.ObjectModel;
using System.Resources;

namespace CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Builder
{
    public partial class DeploymentBuilder
    {
        public static IEnumerable<IKubernetesObject<V1ObjectMeta>> Build(ReverseProxy reverseProxy)
        {
            var labels = new Dictionary<string, string>()
            {
                { Constants.ControlledBy, Constants.OperatorName },
                { Constants.App, "reverseproxy" },
                { Constants.Kind, "service" },
                { Constants.Name, reverseProxy.Name() }
            };
            
            List<IKubernetesObject<V1ObjectMeta>> objects = new List<IKubernetesObject<V1ObjectMeta>>();

            objects.Add(new V1Deployment
            {
                Metadata = new V1ObjectMeta
                {
                    Name = reverseProxy.GetDeploymentName(),
                    NamespaceProperty = reverseProxy.Namespace(),
                    Labels = labels
                },
                Spec = CreateRPDeploymentSpec(reverseProxy, labels)
            });

            objects.Add(CreateRPService(reverseProxy, labels));
            return objects;
        }

        private static V1Service CreateRPService(ReverseProxy agentReferee, IDictionary<string, string> labels)
        {
            return new V1Service()
            {
                Metadata = new V1ObjectMeta
                {
                    Labels = labels,
                    Name = agentReferee.GetServiceName(),
                    NamespaceProperty = agentReferee.Namespace()
                },
                Spec = new V1ServiceSpec
                {
                    Ports = new V1ServicePort[]
                    {
                        new V1ServicePort
                        {
                            Name="HTTP",
                            Protocol= "TCP",
                            Port=80,
                            TargetPort=new IntstrIntOrString{ Value = "80" },
                        },
                        new V1ServicePort
                        {
                            Name="HTTPS",
                            Protocol= "TCP",
                            Port=443,
                            TargetPort=new IntstrIntOrString{ Value = "443" },
                        }
                    },
                    Selector = labels,
                    Type = "ClusterIP"
                }
            };
        }

        private static V1DeploymentSpec CreateRPDeploymentSpec(ReverseProxy agentReferee, IDictionary<string, string> labels)
        {
            foreach (var kvp in agentReferee.Spec.Labels)
                labels.TryAdd(kvp.Key, kvp.Value);

            var spec = new V1DeploymentSpec
            {
                Replicas = 1,
                Selector = new V1LabelSelector(null, labels),
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = labels,
                        Annotations = agentReferee.Spec.Annotations
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new Collection<V1Container>
                        {
                            GetRPContainerSpec(agentReferee)
                        }
                    }
                }
            };

            return spec;
        }

        private static V1Container GetRPContainerSpec(ReverseProxy resource)
        {
            var spec = resource.Spec;

            var envVariables = new List<V1EnvVar>();
            envVariables.AddRange(spec.EnvironmentVariables);



            var container = new V1Container
            {
                Name = "nginxrp",
                Image = "nginx:latest",
                Ports = new Collection<V1ContainerPort>
                {
                    new(80, name: "http"),
                    new(443, name: "https"),
                },
                Env = envVariables,
            };

            if (spec.Resources != null)
            {
                var reqs = new V1ResourceRequirements();
                reqs.Limits = spec.Resources?.Limits;
                reqs.Requests = spec.Resources?.Requests;
                container.Resources = reqs;
            }

            return container;
        }

        private static V1ConfigMap GetRPConfig(ReverseProxy resource)
        {
            var assembly = typeof(Builder.DeploymentBuilder).Assembly;
            Stream r = assembly.GetManifestResourceStream("ComaxRpOperator.Res.nginxconf.txt");
            StreamReader rdr = new StreamReader(r);

            V1ConfigMap v1ConfigMap = new V1ConfigMap(
                metadata: new V1ObjectMeta
                {
                    Name = resource.GetConfigName(),
                    NamespaceProperty = resource.Namespace()
                },
                data: new Dictionary<string, string>
                {
                    { "default.conf", rdr.ReadToEnd() }
                });

            return v1ConfigMap;
        }
    }
}
