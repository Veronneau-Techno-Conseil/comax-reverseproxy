using CommunAxiom.Commons.Client.Hosting.Operator;
using KubeOps.Operator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
           webBuilder.UseStartup<Startup>();
        });

//var web = Task.Run(() =>
//{
//    CreateWebHostBuilder(args).Build().Run();
//});

//CreateWebHostBuilder(args).Build().Run();

await CreateHostBuilder(args)
    .Build()
    .RunOperatorAsync(args);

//await Task.WhenAll(web, operatorTask);