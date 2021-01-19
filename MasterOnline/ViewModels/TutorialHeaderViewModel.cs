using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class TutorialHeaderViewModel
    {
        public Tutorial_Header Tutorial_Header { get; set; }
        public IList<Tutorial_Header> ListTutorialHeader { get; set; } = new List<Tutorial_Header>();
    }
}