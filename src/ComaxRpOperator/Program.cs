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



static IHostBuilder CreateWebHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseKestrel(opts =>
            {
                if (webBuilder.GetSetting("Urls").StartsWith("https"))
                {
                    opts.ConfigureHttpsDefaults(def =>
                    {
                        var certPem = File.ReadAllText("cert.pem");
                        var eccPem = File.ReadAllText("key.pem");

                        var cert = X509Certificate2.CreateFromPem(certPem, eccPem);
                        def.ServerCertificate = new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
                    });
                }
            })
            .UseStartup<StartupWeb>();
        });


//var web = Task.Run(() =>
//{
//    CreateWebHostBuilder(args).Build().Run();
//});

CreateWebHostBuilder(args).Build().Run();

//await CreateHostBuilder(args)
//    .Build()
//    .RunOperatorAsync(args);

//await Task.WhenAll(web, operatorTask);