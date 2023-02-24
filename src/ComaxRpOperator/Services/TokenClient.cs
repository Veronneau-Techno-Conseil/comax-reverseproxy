using ComaxRpOperator.Models;
using IdentityModel.Client;
using static System.Collections.Specialized.BitVector32;
using System.Security.Claims;

namespace ComaxRpOperator.Services
{
    public class TokenClient
    {
        public const string SCOPES_OFFLINE = "openid offline_access";
        const string WELL_KNOWN = ".well-known/openid-configuration";
        private readonly HttpClient _httpClient;
        private readonly OIDCSettings _settings;
        private DiscoveryDocumentResponse? _tokenMetadata = null;
        int metadataRetries = 5;


        public DiscoveryDocumentResponse TokenMetadata
        {
            get
            {
                if (_tokenMetadata == null)
                {
                    int trials = 0;
                    while (trials < metadataRetries && (_tokenMetadata == null))
                    {
                        string url = _settings.Authority.TrimEnd('/') + '/';
                        _tokenMetadata = _httpClient.GetDiscoveryDocumentAsync(url).GetAwaiter().GetResult();
                        if (_tokenMetadata.IsError)
                            _tokenMetadata = null;
                        trials++;
                    }

                }
                return _tokenMetadata;
            }
        }

        private bool _configured;
        public TokenClient(IConfiguration configuration)
        {
            this._settings = new OIDCSettings();
            configuration.Bind("OIDC", this._settings);
            this._httpClient = new HttpClient();
        }

        public TokenClient(OIDCSettings settings)
        {
            this._settings = settings;
            this._httpClient = new HttpClient();
        }

        public async Task Configure()
        {
            if (!_configured)
            {
                _configured = true;
            }
        }

        public async Task<(bool, TokenData?)> RefreshToken(string refreshToken)
        {
            await this.Configure();

            var res = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = TokenMetadata.TokenEndpoint,
                RefreshToken = refreshToken,
                ClientId = _settings.ClientId,
                ClientSecret = _settings.Secret
            });

            if (res.HttpResponse.IsSuccessStatusCode)
            {
                return (true, new TokenData { access_token = res.AccessToken, expires_in = res.ExpiresIn, refresh_token = res.RefreshToken, token_type = res.TokenType });
            }
            else if (res.HttpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, null);
            }
            else
            {
                throw new Exception($"Unexpected result calling token endpoint: {res.HttpStatusCode}=> {res.HttpErrorReason}, {await res.HttpResponse.Content.ReadAsStringAsync()}");
            }

        }

        public async Task<(bool, TokenData?)> AuthenticateClient(string clientId, string secret, string scope)
        {
            await this.Configure();

            var res = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = TokenMetadata.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = secret,
                Scope = scope
            });
            if (res.HttpResponse.IsSuccessStatusCode)
            {
                return (true, new TokenData { access_token = res.AccessToken, expires_in = res.ExpiresIn, refresh_token = res.RefreshToken, token_type = res.TokenType });
            }
            else if (res.HttpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, null);
            }
            else
            {
                throw new Exception($"Unexpected result calling token endpoint: {res.HttpStatusCode}=> {res.HttpErrorReason}, {await res.HttpResponse.Content.ReadAsStringAsync()}");
            }
        }

        public async Task<(bool, TokenData?)> Impersonate(string token)
        {
            await this.Configure();

            _httpClient.DefaultRequestHeaders.Add("x-act-as", token);
            var res = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = TokenMetadata.TokenEndpoint,
                ClientId = this._settings.ClientId,
                ClientSecret = this._settings.Secret,
                Scope = this._settings.Scopes
            });
            if (res.HttpResponse.IsSuccessStatusCode)
            {
                return (true, new TokenData { access_token = res.AccessToken, expires_in = res.ExpiresIn, refresh_token = res.RefreshToken, token_type = res.TokenType });
            }
            else if (res.HttpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, null);
            }
            else
            {
                throw new Exception($"Unexpected result calling token endpoint: {res.HttpStatusCode}=> {res.HttpErrorReason}, {await res.HttpResponse.Content.ReadAsStringAsync()}");
            }
        }

        public Task<(bool, TokenData?)> AuthenticatePassword(string username, string password)
        {
            return this.AuthenticatePassword(this._settings.ClientId, this._settings.Secret, this._settings.Scopes, username, password);
        }

        public async Task<(bool, TokenData?)> AuthenticatePassword(string clientId, string secret, string scope, string username, string password)
        {
            await this.Configure();

            var res = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = TokenMetadata.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = secret,
                Scope = scope,
                UserName = username,
                Password = password
            });
            if (res.HttpResponse.IsSuccessStatusCode)
            {
                return (true, new TokenData { access_token = res.AccessToken, expires_in = res.ExpiresIn, refresh_token = res.RefreshToken, token_type = res.TokenType });
            }
            else if (res.HttpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, null);
            }
            else
            {
                throw new Exception($"Unexpected result calling token endpoint: {res.HttpStatusCode}=> {res.HttpErrorReason}, {await res.HttpResponse.Content.ReadAsStringAsync()}");
            }
        }

        public async Task<(bool, ClaimsPrincipal?)> RequestIntrospection(string clientId, string secret, string token)
        {
            await this.Configure();

            var res = await _httpClient.IntrospectTokenAsync(new TokenIntrospectionRequest
            {
                Address = TokenMetadata.IntrospectionEndpoint,
                ClientId = clientId,
                ClientSecret = secret,
                Token = token,
                AuthorizationHeaderStyle = BasicAuthenticationHeaderStyle.Rfc2617
            });

            if (res.HttpResponse.IsSuccessStatusCode)
            {
                return (true, new ClaimsPrincipal(new ClaimsIdentity(res.Claims)));
            }
            else if (res.HttpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, null);
            }
            else
            {
                throw new Exception($"Unexpected result calling token endpoint: {res.HttpStatusCode}=> {res.HttpErrorReason}, {await res.HttpResponse.Content.ReadAsStringAsync()}");
            }
        }
    }
}
