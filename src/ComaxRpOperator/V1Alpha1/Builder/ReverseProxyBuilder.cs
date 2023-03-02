using ComaxRpOperator.V1Alpha1.Builder;
using CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Entities;
using CommunAxiom.DotnetSdk.Helpers.Certificates;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Collections.ObjectModel;
using System.Resources;

namespace CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Builder
{
    public partial class DeploymentBuilder
    {
        public static IEnumerable<IKubernetesObject<V1ObjectMeta>> Build(ReverseProxy reverseProxy, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            var labels = new Dictionary<string, string>()
            {
                { Constants.ControlledBy, Constants.OperatorName },
                { Constants.App, "reverseproxy" },
                { Constants.Kind, "service" },
                { Constants.Name, reverseProxy.Name() }
            };
            
            List<IKubernetesObject<V1ObjectMeta>> objects = new List<IKubernetesObject<V1ObjectMeta>>();

            objects.Add(GetDeplCert(reverseProxy, configuration, serviceProvider));

            objects.Add(GetRPConfig(reverseProxy, configuration));

            objects.Add(new V1Deployment
            {
                Metadata = new V1ObjectMeta
                {
                    Name = reverseProxy.GetDeploymentName(),
                    NamespaceProperty = reverseProxy.Namespace(),
                    Labels = labels
                },
                Spec = CreateRPDeploymentSpec(reverseProxy, labels, configuration)
            });

            objects.Add(CreateRPService(reverseProxy, labels));

            objects.Add(CreateRPIngress(reverseProxy, labels, configuration));

            return objects;
        }

        private static IKubernetesObject<V1ObjectMeta> GetDeplCert(ReverseProxy reverseProxy, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            var certProvider = serviceProvider.GetService<ICertGenerator>();
            var cert = certProvider.Generate();
            return new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = reverseProxy.GetNginxSecretName(),
                    NamespaceProperty = reverseProxy.Namespace()
                },
                Type = "kubernetes.io/tls",
                Data = new Dictionary<string, byte[]>
                {
                    {"tls.crt", System.Text.Encoding.UTF8.GetBytes(cert.Certificate) },
                    {"tls.key", System.Text.Encoding.UTF8.GetBytes(cert.Key) }
                }
            };
        }

        private static IKubernetesObject<V1ObjectMeta> CreateRPIngress(ReverseProxy reverseProxy, Dictionary<string, string> labels, IConfiguration configuration)
        {
            return new V1Ingress(metadata: new V1ObjectMeta
            {
                Name = reverseProxy.GetIngressName(),
                NamespaceProperty = reverseProxy.Namespace(),
                Labels = labels,
                Annotations = new Dictionary<string, string>() {
                    {
                        "cert-manager.io/cluster-issuer", reverseProxy.Spec.IngressCertManager
                    },
                    {
                        "acme.cert-manager.io/http01-edit-in-place", "true"
                    }
                }
            },
            spec: new V1IngressSpec
            {
                IngressClassName = configuration["IngressClassName"],
                Tls = new List<V1IngressTLS> {
                    new V1IngressTLS
                    {
                        Hosts = new List<string>
                        {
                            reverseProxy.Spec.IngressHost
                        },
                        SecretName = reverseProxy.Spec.IngressCertSecret
                    }
                },
                Rules = new List<V1IngressRule>
                {
                    new V1IngressRule
                    {
                        Host= reverseProxy.Spec.IngressHost,
                        Http= new V1HTTPIngressRuleValue
                        {
                            Paths = new List<V1HTTPIngressPath>
                            {
                                new V1HTTPIngressPath
                                {
                                    Path = "/",
                                    PathType = "Prefix",
                                    Backend = new V1IngressBackend
                                    {
                                        Service = new V1IngressServiceBackend
                                        {
                                            Name = reverseProxy.GetServiceName(),
                                            Port = new V1ServiceBackendPort(number: 80)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        private static V1Service CreateRPService(ReverseProxy rp, IDictionary<string, string> labels)
        {
            return new V1Service()
            {
                Metadata = new V1ObjectMeta
                {
                    Labels = labels,
                    Name = rp.GetServiceName(),
                    NamespaceProperty = rp.Namespace()
                },
                Spec = new V1ServiceSpec
                {
                    Ports = new V1ServicePort[]
                    {
                        new V1ServicePort
                        {
                            Name="http",
                            Protocol= "TCP",
                            Port=80,
                            TargetPort=new IntstrIntOrString{ Value = "80" },
                        },
                        new V1ServicePort
                        {
                            Name="https",
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

        private static V1DeploymentSpec CreateRPDeploymentSpec(ReverseProxy rp, IDictionary<string, string> labels, IConfiguration configuration)
        {
            foreach (var kvp in rp.Spec.Labels)
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
                        Annotations = rp.Spec.Annotations
                    },
                    Spec = new V1PodSpec
                    {
                        Volumes = new Collection<V1Volume>
                        {
                            new V1Volume ("configs-volume", configMap: new V1ConfigMapVolumeSource(name: rp.GetConfigName())),
                            new V1Volume ("certs", secret: new V1SecretVolumeSource(secretName: rp.GetNginxSecretName()))
                        },
                        Containers = new Collection<V1Container>
                        {
                            GetRPContainerSpec(rp, configuration)
                        }
                    }
                }
            };

            return spec;
        }

        private static V1Container GetRPContainerSpec(ReverseProxy resource, IConfiguration configuration)
        {
            var spec = resource.Spec;

            var envVariables = new List<V1EnvVar>();
            envVariables.AddRange(spec.EnvironmentVariables);

            envVariables.Add(new("PROXY_SSL_CERT", "/etc/ssl/private/priv.pem"));
            envVariables.Add(new("PROXY_SSL_CERT_KEY", "/etc/ssl/private/priv.key"));
            envVariables.Add(new("server_name", resource.Spec.ForwardAddress));
            envVariables.Add(new("proxy_dns", configuration["DSN"]));

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
                VolumeMounts= new Collection<V1VolumeMount>
                {
                    new V1VolumeMount("/etc/nginx/conf.d/", "configs-volume"),
                    new V1VolumeMount("/etc/ssl/private/priv.pem", "certs", subPath: "tls.crt"),
                    new V1VolumeMount("/etc/ssl/private/priv.key", "certs", subPath: "tls.key"),
                }
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

        private static V1ConfigMap GetRPConfig(ReverseProxy resource, IConfiguration configuration)
        {
            string res = Microtemplate.Processor("ComaxRpOperator.Res.nginxconf.txt", Microtemplate.BuildObject(configuration, resource));
         
            V1ConfigMap v1ConfigMap = new V1ConfigMap(
                metadata: new V1ObjectMeta
                {
                    Name = resource.GetConfigName(),
                    NamespaceProperty = resource.Namespace()
                },
                data: new Dictionary<string, string>
                {
                    { "default.conf", res }
                });
            
            return v1ConfigMap;
        }
    }
}
