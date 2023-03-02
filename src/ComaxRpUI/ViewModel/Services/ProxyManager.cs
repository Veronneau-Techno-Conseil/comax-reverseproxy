using ComaxRpUI.Models;
using ComaxRpUI.ViewModel.Services.Interfaces;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using CommunAxiom.DotnetSdk.Helpers;
using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace ComaxRpUI.ViewModel.Services
{
    public class ProxyManager : IProxyManager
    {
        private readonly HttpClient _httpClient;

        public ProxyManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool, JObject?)> Delete(Guid guid)
        {
            var res = await _httpClient.DeleteAsync($"/api/state/{guid}");
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
                return (true, null);

            var resTxt = await res.Content.ReadAsStringAsync();
            var (content, reason) = resTxt.Try(t => JObject.Parse(t));
            return (false, reason);
        }

        public async Task<(bool, JObject?, RpEntryVm?)> Get(Guid guid)
        {
            var res = await _httpClient.GetAsync($"/api/state/{guid}");
            if (res.IsSuccessStatusCode)
                return (true, null, await res.Content.ReadFromJsonAsync<RpEntryVm>());

            var resTxt = await res.Content.ReadAsStringAsync();
            var (content, reason) = resTxt.Try(t => JObject.Parse(t));
            
            return (false, reason, null);
        }

        public async Task<IList<RpEntryVm>?> GetEntries(int page, int itemsPerPage)
        {
            var res = await _httpClient.GetAsync($"/api/state/list?page={page}&itemsPerPage={itemsPerPage}");
            return await res.Content.ReadFromJsonAsync<IList<RpEntryVm>>();
        }

        public async Task<(bool, JObject?)> Upsert(Guid id, RpEntryVm rpEntryVm)
        {
            rpEntryVm.Id = id;
            var res = await _httpClient.PostAsJsonAsync($"/api/state/{id}", rpEntryVm);
            if (res.IsSuccessStatusCode)
                return (true, null);
            var resTxt = await res.Content.ReadAsStringAsync();
            var (content, reason) = resTxt.Try(t => JObject.Parse(t));
            return (false, reason);
        }
    }
}
