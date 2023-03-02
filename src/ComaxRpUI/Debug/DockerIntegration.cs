#if DEBUG
using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DockerIntegration
{
    public class Client
    {
        public async Task<bool> InstallContainer(string containerName, string image, string tag, Dictionary<string, string> portMappings, List<string> environmentVariables, bool recreate = false)
        {
            DockerClient client = new DockerClientConfiguration(
                new Uri("npipe://./pipe/docker_engine"))
                 .CreateClient();

            IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
                    new ContainersListParameters()
                    {
                        All = true
                    });

            var container = containers.Where(x => x.Names.Any(x => x.Contains(containerName))).SingleOrDefault();

            if (container == null)
            {

                var ports = new Dictionary<string, IList<PortBinding>>();
                var pms = portMappings?.Select(x => new KeyValuePair<string, IList<PortBinding>>(x.Key, new List<PortBinding> { new PortBinding { HostPort = x.Value } }));
                foreach (var p in pms)
                {
                    ports.Add(p.Key, p.Value);
                }

                await client.Images.CreateImageAsync(new ImagesCreateParameters()
                {
                    FromImage = image,
                    Tag = tag
                }, new AuthConfig(), new Progress<JSONMessage>(m => Console.WriteLine(m)));

                var res = await client.Containers.CreateContainerAsync(new CreateContainerParameters()
                {
                    Name = containerName,
                    Image = image + ":" + tag,
                    Env = environmentVariables.ToList(),
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>(pms)
                    }
                });

                Console.WriteLine(res.ToString());

                containers = await client.Containers.ListContainersAsync(
                   new ContainersListParameters()
                   {
                       All = true
                   });

                container = containers.Where(x => x.Names.Any(x => x.Contains(containerName))).SingleOrDefault();
            }

            await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters { });

            return true;
        }

        public async Task<bool> StopContainer(string containerName, string image, string tag, List<string> portMappings, List<string> environmentVariables, bool recreate = false)
        {
            DockerClient client = new DockerClientConfiguration(
                new Uri("npipe://./pipe/docker_engine"))
                 .CreateClient();

            IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(
                    new ContainersListParameters()
                    {
                        All = true
                    });

            var container = containers.Where(x => x.Names.Any(x => x.Contains(containerName))).SingleOrDefault();

            if (container == null)
                return false;

            await client.Containers.StopContainerAsync(container.ID, new ContainerStopParameters());

            return true;
        }
    }
}
#endif