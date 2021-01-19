using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class TutorialDetailViewModel
    {
        public Tutorial_Detail Tutorial_Detail { get; set; }
        public IList<Tutorial_Detail> ListTutorialDetail { get; set; } = new List<Tutorial_Detail>();
        public List<Tutorial_Header> ListTutorialHeader { get; set; } = new List<Tutorial_Header>();
        public List<SelectTutorialDetail> selectTutorialDetails { get; set; } //= new List<SelectTutorialDetail>();
        public List<SelectTutorialHeader> selectTutorialHeaders { get; set; }
    }

    public class SelectTutorialDetail
    {
        public int DetailId { get; set; }
        public int CategoryId { get; set; }
        public string Kategori { get; set; }
        public string Judul { get; set; }
        public string Konten { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SelectTutorialHeader
    {
        public int IdKategori { get; set; }
        public string NamaKategori { get; set; }
    }
}