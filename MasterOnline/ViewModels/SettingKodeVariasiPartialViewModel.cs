using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class SettingKodeVariasiPartialViewModel
    {
        public Dictionary<string, string> MapKodeVariasiTemp { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> MapNamaVariasiTemp { get; set; } = new Dictionary<string, string>();
    }
}