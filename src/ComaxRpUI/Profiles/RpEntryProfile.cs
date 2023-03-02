using ComaxRpUI.Models;

namespace ComaxRpUI.Profiles
{
    public class RpEntryProfile: AutoMapper.Profile
    {
        public RpEntryProfile() 
        { 
            this.CreateMap<RpEntry, RpEntry>();
            this.CreateMap<RpEntry, RpEntryVm>();
            this.CreateMap<RpEntryVm, RpEntry>();
            this.CreateMap<RpEntryVm, RpEntryVm>();
        }
    }
}
