using ComaxRpUI.Models;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace ComaxRpUI.ViewModel.Services.Interfaces
{
    public interface IProxyManager
    {
        Task<IList<RpEntryVm>?> GetEntries(int page, int itemsPerPage);
        Task<(bool, JObject?)> Upsert(Guid id, RpEntryVm rpEntryVm);
        Task<(bool, JObject?)> Delete(Guid guid);
        Task<(bool, JObject?, RpEntryVm?)> Get(Guid guid);
    }
}
