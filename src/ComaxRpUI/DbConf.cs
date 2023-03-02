using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ComaxRpUI
{
    public class DbConf
    {
        public bool MemoryDb { get; set; }
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public bool ShouldDrop { get; set; }
        public bool ShouldMigrate { get; set; }
        public string Port { get; set; }
    }
}
