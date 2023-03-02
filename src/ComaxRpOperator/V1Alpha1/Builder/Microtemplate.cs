using CommunAxiom.Commons.Client.Hosting.Operator.V1Alpha1.Entities;
using Microsoft.AspNetCore.Routing.Template;
using RazorLight;
using System.Dynamic;
using System.IO;
using System.Reflection;

namespace ComaxRpOperator.V1Alpha1.Builder
{
    public class Microtemplate
    {
        public static dynamic BuildObject(IConfiguration configuration, ReverseProxy entity)
        {
            var ex = (dynamic)new ExpandoObject();
            ex.proxy_dns = configuration["DSN"];
            ex.server_name = entity.GetDeploymentName().TrimEnd('/');
            ex.upstream = entity.Spec.ForwardAddress.TrimEnd('/');
            return ex;
        }

        public static string Processor(string ressource, object values)
        {
            var engine = new RazorLightEngineBuilder()
                       .UseEmbeddedResourcesProject(typeof(Program))
            .UseMemoryCachingProvider()
            .Build();

            var view = engine.CompileRenderAsync(ressource, values);
            return view.ConfigureAwait(true).GetAwaiter().GetResult();
        }
    }
}
