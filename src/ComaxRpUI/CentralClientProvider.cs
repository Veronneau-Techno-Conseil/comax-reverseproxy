using CentralClient;
using CommunAxiom.DotnetSdk.Helpers.OIDC;
using Microsoft.Extensions.Options;

namespace ComaxRpUI
{
    public class CentralClientProvider : CommunAxiom.DotnetSdk.Helpers.OIDC.AuthClient<CentralApi>
    {
        private readonly IConfiguration _configuration;
        public CentralClientProvider(IConfiguration configuration, IOptionsMonitor<OIDCSettings> optionsMonitor): base(configuration, optionsMonitor)
        {
            _configuration= configuration;
        }

        protected override CentralApi CreateClient(HttpClient httpClient)
        {
            return new CentralApi(_configuration["CentralUrl"], httpClient);
        }
    }
}
