using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class ReportViewModel
    {
        public class Report1
        {
            string _FromCust = "";
            string _ToCust = "";
            public string UserId { get; set; }

            public string FromCust
            {
                get
                {
                    return _FromCust;
                }
                set
                {
                    _FromCust = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToCust
            {
                get
                {
                    return _ToCust;
                }
                set
                {
                    _ToCust = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string CutOffDate { get; set; }
        }

        public class Report2
        {
            string _FromSupp = "";
            string _ToSupp = "";
            public string UserId { get; set; }

            public string FromSupp
            {
                get
                {
                    return _FromSupp;
                }
                set
                {
                    _FromSupp = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToSupp
            {
                get
                {
                    return _ToSupp;
                }
                set
                {
                    _ToSupp = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string CutOffDate { get; set; }
        }

        public class Report3
        {
            string _FromCust = "";
            string _ToCust = "";
            public string UserId { get; set; }

            public string FromCust
            {
                get
                {
                    return _FromCust;
                }
                set
                {
                    _FromCust = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToCust
            {
                get
                {
                    return _ToCust;
                }
                set
                {
                    _ToCust = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string FromMonth { get; set; }
            public string CutOffDate { get; set; }
        }

        public class Report4
        {
            string _FromSupp = "";
            string _ToSupp = "";
            public string UserId { get; set; }

            public string FromSupp
            {
                get
                {
                    return _FromSupp;
                }
                set
                {
                    _FromSupp = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToSupp
            {
                get
                {
                    return _ToSupp;
                }
                set
                {
                    _ToSupp = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string FromMonth { get; set; }
            public string CutOffDate { get; set; }
        }

        public class Report5
        {
            string _FromSupp = "";
            string _ToSupp = "";
            string _FromBrg = "";
            string _ToBrg = "";
            //add by nurul 11/1/2019
            string _Order = "";
            //end add

            public string UserId { get; set; }

            public string FromSupp
            {
                get
                {
                    return _FromSupp;
                }
                set
                {
                    _FromSupp = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToSupp
            {
                get
                {
                    return _ToSupp;
                }
                set
                {
                    _ToSupp = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string FromBrg
            {
                get
                {
                    return _FromBrg;
                }
                set
                {
                    _FromBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToBrg
            {
                get
                {
                    return _ToBrg;
                }
                set
                {
                    _ToBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            //add by nurul 11/1/2019
            public string Order
            {
                get
                {
                    return _Order;
                }
                set
                {
                    _Order = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            //end add

            public string DrTanggal { get; set; }
            public string SdTanggal { get; set; }
        }

        public class Report6
        {
            string _FromCust = "";
            string _ToCust = "";
            string _FromBrg = "";
            string _ToBrg = "";
            //add by nurul 11/1/2019
            string _Order = "";
            //string _FromBuyer = "";
            //string _ToBuyer = "";
            //end add

            public string UserId { get; set; }

            public string FromCust
            {
                get
                {
                    return _FromCust;
                }
                set
                {
                    _FromCust = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToCust
            {
                get
                {
                    return _ToCust;
                }
                set
                {
                    _ToCust = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string FromBrg
            {
                get
                {
                    return _FromBrg;
                }
                set
                {
                    _FromBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToBrg
            {
                get
                {
                    return _ToBrg;
                }
                set
                {
                    _ToBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            //add by nurul 11/1/2019
            public string Order
            {
                get
                {
                    return _Order;
                }
                set
                {
                    _Order = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            //public string FromBuyer
            //{
            //    get
            //    {
            //        return _FromBuyer;
            //    }
            //    set
            //    {
            //        _FromBuyer = string.IsNullOrEmpty(value) ? "" : value;
            //    }
            //}

            //public string ToBuyer
            //{
            //    get
            //    {
            //        return _ToBuyer;
            //    }
            //    set
            //    {
            //        _ToBuyer = string.IsNullOrEmpty(value) ? "" : value;
            //    }
            //}
            //end add

            public string DrTanggal { get; set; }
            public string SdTanggal { get; set; }
        }

        public class Report7
        {
            string _Gudang = "";
            string _FromBrg = "";
            string _ToBrg = "";
            public string UserId { get; set; }

            public string Gudang
            {
                get
                {
                    return _Gudang;
                }
                set
                {
                    _Gudang = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string FromBrg
            {
                get
                {
                    return _FromBrg;
                }
                set
                {
                    _FromBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToBrg
            {
                get
                {
                    return _ToBrg;
                }
                set
                {
                    _ToBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string FromMonth { get; set; }
            public string CutOffDate { get; set; }
        }

        public class Report8
        {
            string _Gudang = "";
            string _FromBrg = "";
            string _ToBrg = "";

            public string UserId { get; set; }

            public string Gudang
            {
                get
                {
                    return _Gudang;
                }
                set
                {
                    _Gudang = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string FromBrg
            {
                get
                {
                    return _FromBrg;
                }
                set
                {
                    _FromBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToBrg
            {
                get
                {
                    return _ToBrg;
                }
                set
                {
                    _ToBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Tahun { get; set; }
            public string DrBulan { get; set; }
            public string SdBulan { get; set; }
        }

        public class Report9
        {
            string _KdLap = "";

            public string UserId { get; set; }

            public string KdLap
            {
                get
                {
                    return _KdLap;
                }
                set
                {
                    _KdLap = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Print { get; set; }
            public string Tahun { get; set; }
            public string Bulan { get; set; }
        }

        public class Report10
        {
            string _KdLap = "";

            public string UserId { get; set; }

            public string KdLap
            {
                get
                {
                    return _KdLap;
                }
                set
                {
                    _KdLap = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Print { get; set; }
            public string Tahun { get; set; }
            public string Bulan { get; set; }
        }

        public class Report11
        {
            string _DrRek = "";
            string _SdRek = "";

            public string UserId { get; set; }

            public string DrRek
            {
                get
                {
                    return _DrRek;
                }
                set
                {
                    _DrRek = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string SdRek
            {
                get
                {
                    return _SdRek;
                }
                set
                {
                    _SdRek = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Type { get; set; }
            public string Print { get; set; }
            public string Posting { get; set; }
            public string Tahun { get; set; }
            public string DrBulan { get; set; }
            public string SdBulan { get; set; }
        }

        public class Report12
        {
            string _DrRek = "";
            string _SdRek = "";

            public string UserId { get; set; }

            public string DrRek
            {
                get
                {
                    return _DrRek;
                }
                set
                {
                    _DrRek = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string SdRek
            {
                get
                {
                    return _SdRek;
                }
                set
                {
                    _SdRek = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Nol { get; set; }
            public string Type { get; set; }
            public string Posting { get; set; }
            public string Tahun { get; set; }
            public string DrBulan { get; set; }
            public string SdBulan { get; set; }
        }

        public class Report13
        {
            string _FromCust = "";
            string _ToCust = "";

            public string UserId { get; set; }

            public string FromCust
            {
                get
                {
                    return _FromCust;
                }
                set
                {
                    _FromCust = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToCust
            {
                get
                {
                    return _ToCust;
                }
                set
                {
                    _ToCust = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string DrTanggal { get; set; }
            public string SdTanggal { get; set; }

            //add by nurul 10/12/2020
            string _Status = "";
            public string Status
            {
                get
                {
                    return _Status;
                }
                set
                {
                    _Status = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            //end add by nurul 10/12/2020
        }

        public class Report14
        {
            string _FromSupp = "";
            string _ToSupp = "";

            public string UserId { get; set; }

            public string FromSupp
            {
                get
                {
                    return _FromSupp;
                }
                set
                {
                    _FromSupp = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToSupp
            {
                get
                {
                    return _ToSupp;
                }
                set
                {
                    _ToSupp = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string DrTanggal { get; set; }
            public string SdTanggal { get; set; }
        }

        public class Report15
        {
            string _Gudang = "";
            string _FromBrg = "";
            string _ToBrg = "";
            public string UserId { get; set; }

            public string Gudang
            {
                get
                {
                    return _Gudang;
                }
                set
                {
                    _Gudang = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string FromBrg
            {
                get
                {
                    return _FromBrg;
                }
                set
                {
                    _FromBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToBrg
            {
                get
                {
                    return _ToBrg;
                }
                set
                {
                    _ToBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string FromMonth { get; set; }
            public string CutOffDate { get; set; }
        }

        public class Report16
        {
            public string CutOffDate { get; set; }
            public string PilihStok { get; set; }
            string _Gudang1 = "";
            string _Gudang2 = "";
            string _Gudang3 = "";
            string _Gudang4 = "";
            string _Gudang5 = "";
            string _Gudang6 = "";
            string _Gudang7 = "";
            string _Gudang8 = "";
            string _Gudang9 = "";
            string _Gudang10 = "";
            string _FromBrg = "";
            string _ToBrg = "";
            public string UserId { get; set; }

            public string Gudang1
            {
                get
                {
                    return _Gudang1;
                }
                set
                {
                    _Gudang1 = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Gudang2
            {
                get
                {
                    return _Gudang2;
                }
                set
                {
                    _Gudang2 = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Gudang3
            {
                get
                {
                    return _Gudang3;
                }
                set
                {
                    _Gudang3 = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Gudang4
            {
                get
                {
                    return _Gudang4;
                }
                set
                {
                    _Gudang4 = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Gudang5
            {
                get
                {
                    return _Gudang5;
                }
                set
                {
                    _Gudang5 = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Gudang6
            {
                get
                {
                    return _Gudang6;
                }
                set
                {
                    _Gudang6 = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Gudang7
            {
                get
                {
                    return _Gudang7;
                }
                set
                {
                    _Gudang7 = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Gudang8
            {
                get
                {
                    return _Gudang8;
                }
                set
                {
                    _Gudang8 = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Gudang9
            {
                get
                {
                    return _Gudang9;
                }
                set
                {
                    _Gudang9 = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
            public string Gudang10
            {
                get
                {
                    return _Gudang10;
                }
                set
                {
                    _Gudang10 = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string FromBrg
            {
                get
                {
                    return _FromBrg;
                }
                set
                {
                    _FromBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }

            public string ToBrg
            {
                get
                {
                    return _ToBrg;
                }
                set
                {
                    _ToBrg = string.IsNullOrEmpty(value) ? "" : value;
                }
            }
        }
    }
}
