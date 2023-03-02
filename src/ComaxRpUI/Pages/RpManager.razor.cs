using Blazorise.DataGrid;
using ComaxRpUI.Models;
using ComaxRpUI.ViewModel.Services;
using ComaxRpUI.ViewModel.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Blazorise.Bulma;
using Blazorise;

namespace ComaxRpUI.Pages
{
    public partial class RpManager : ComponentBase
    {
        [Inject]
        public IProxyManager ProxyManager { get; set; }

        [Inject]
        public ILogger<RpManager> Logger { get; set; }

        public DataGrid<RpEntryVm> Grid;

        public List<RpEntryVm> Items { get; set; }

        public RpEntryMessages Errors { get; set; } = new RpEntryMessages();

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        protected async Task OnReadData(DataGridReadDataEventArgs<RpEntryVm> e)
        {
            var p = e.Page;
            var ps = e.PageSize;
            var res = await ProxyManager.GetEntries(p, ps);
            Items = res.ToList();
            StateHasChanged();
        }

        protected async Task OnRowUpdated(CancellableRowChange<RpEntryVm, Dictionary<string, object>> e)
        {
            if (e.Item.Id == Guid.Empty)
                e.Item.Id = Guid.NewGuid();

            e.Item.ForwardAddress = (string)e.Values.GetValueOrDefault("ForwardAddress");
            e.Item.IngressCertManager = (string)e.Values.GetValueOrDefault("IngressCertManager");
            e.Item.IngressCertSecret = (string)e.Values.GetValueOrDefault("IngressCertSecret");
            e.Item.IngressHost = (string)e.Values.GetValueOrDefault("IngressHost");
            e.Item.Name = (string)e.Values.GetValueOrDefault("Name");
            e.Item.UseHttps = (bool)e.Values.GetValueOrDefault("UseHttps", false);
            e.Item.CreatedDate = (DateTime)e.Values.GetValueOrDefault("CreatedDate", DateTime.Now);
            e.Item.ModifiedDate = (DateTime)e.Values.GetValueOrDefault("ModifiedDate", DateTime.Now);

            var (success, reason) = await ProxyManager.Upsert(e.Item.Id, e.Item);

            if (reason != null)
            {
                this.Errors = reason.SelectToken("errors").ToObject<RpEntryMessages>();
            }

            if (!success)
                e.Cancel = true;
        }

        protected async Task OnRowRemoved(CancellableRowChange<RpEntryVm> e)
        {
            var (success, reason) = await ProxyManager.Delete(e.Item.Id);

            if (reason != null)
            {
                this.Errors = reason.SelectToken("errors").ToObject<RpEntryMessages>();
            }

            if (!success)
                e.Cancel = true;
        }
        protected void OnSelectedRowChanged(RpEntryVm model)
        {
            Logger.LogInformation($"selected row changed: {Newtonsoft.Json.JsonConvert.SerializeObject(model)}");
        }


    }
}

