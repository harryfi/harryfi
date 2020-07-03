using Erasoft.Function;
using MasterOnline.ViewModels;
using MasterOnline.Services;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using MasterOnline.Models;
using Spire.Xls;
using System.Data.SqlClient;
using System.Data;
using MasterOnline.Utils;

namespace MasterOnline.Controllers
{
    public class TransferExcelController : Controller
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        string username;

        string dbPathEra = "";
        string DataSourcePath = "";
        public TransferExcelController()
        {
            MoDbContext = new MoDbContext("");
            username = "";
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DataSourcePath, sessionData.Account.DatabasePathErasoft);

                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
                dbPathEra = sessionData.Account.DatabasePathErasoft;
                DataSourcePath = sessionData.Account.DataSourcePath;
                username = sessionData.Account.Username;
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath, accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    dbPathEra = accFromUser.DatabasePathErasoft;
                    DataSourcePath = accFromUser.DataSourcePath;
                    username = accFromUser.Username;
                }
            }
            //if (username.Length > 20)
            //    username = username.Substring(0, 17) + "...";
        }

        public ActionResult SQLtoExcel(string cust)
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };

            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                    if (customer != null)
                    {
                        var mp = MoDbContext.Marketplaces.Where(m => m.IdMarket.ToString() == customer.NAMA).FirstOrDefault();
                        if (mp != null)
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(mp.NamaMarket);

                            worksheet.Cells[1, 1].Value = "Marketplace :";
                            worksheet.Cells[1, 2].Value = customer.CUST;
                            worksheet.Cells[1, 3].Value = customer.PERSO;
                            #region keterangan
                            worksheet.Cells[2, 1].Value = "Perhatian :";
                            worksheet.Cells[3, 1].Value = "FIELD-FIELD YANG PERLU DIISI";
                            worksheet.Cells[3, 2].Value = "KETERANGAN";
                            worksheet.Cells[4, 1].Value = "KODE_BRG_MO";
                            worksheet.Cells[4, 2].Value = "tidak boleh lebih dari 20 karakter";
                            worksheet.Cells[5, 1].Value = "KODE_BRG_INDUK_MO";
                            worksheet.Cells[5, 2].Value = "tidak boleh lebih dari 11 karakter. Diisi khusus untuk tipe barang variasi";
                            worksheet.Cells[6, 1].Value = "KODE_KATEGORI_MO";
                            worksheet.Cells[6, 2].Value = "diisi dengan kode dari sheet Master Kategori dan Merk";
                            worksheet.Cells[7, 1].Value = "KODE_MEREK_MO";
                            worksheet.Cells[7, 2].Value = "diisi dengan kode dari sheet Master Kategori dan Merk";

                            worksheet.Cells[2, 3].Value = "CONTOH KODE BARANG";
                            worksheet.Cells[3, 3].Value = "03.POL";
                            worksheet.Cells[3, 4].Value = "= Kaos Berkerah Polo ( Kode barang Induk MO )";
                            worksheet.Cells[4, 3].Value = "03.POL.01.S";
                            worksheet.Cells[4, 4].Value = "= Kaos Berkerah Polo Merah Ukuran S";
                            worksheet.Cells[5, 3].Value = "03.POL.01.M";
                            worksheet.Cells[5, 4].Value = "= Kaos Berkerah Polo Merah Ukuran M";
                            worksheet.Cells[6, 3].Value = "03.POL.02.S";
                            worksheet.Cells[6, 4].Value = "= Kaos Berkerah Polo Biru Ukuran S";
                            worksheet.Cells[7, 3].Value = "03.POL.02.M";
                            worksheet.Cells[7, 4].Value = "= Kaos Berkerah Polo Biru Ukuran M";

                            worksheet.Cells[9, 1].Value = "CONTOH PENGISIAN";
                            worksheet.Cells[9, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[9, 1].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                            worksheet.Cells[11, 3].Value = "03.POL";
                            worksheet.Cells[11, 1].Value = "Kaos Berkerah Polo";
                            worksheet.Cells[12, 3].Value = "03.POL.01.S";
                            worksheet.Cells[12, 1].Value = "Kaos Berkerah Polo Merah Ukuran S";
                            worksheet.Cells[13, 3].Value = "03.POL.01.M";
                            worksheet.Cells[13, 1].Value = "Kaos Berkerah Polo Merah Ukuran M";
                            worksheet.Cells[14, 3].Value = "03.POL.02.S";
                            worksheet.Cells[14, 1].Value = "Kaos Berkerah Polo Biru Ukuran S";
                            worksheet.Cells[15, 3].Value = "03.POL.02.M";
                            worksheet.Cells[15, 1].Value = "Kaos Berkerah Polo Biru Ukuran M";
                            worksheet.Cells[11, 4].Value = "(dikosongkan)";
                            worksheet.Cells[12, 4].Value = "03.POL";
                            worksheet.Cells[13, 4].Value = "03.POL";
                            worksheet.Cells[14, 4].Value = "03.POL";
                            worksheet.Cells[15, 4].Value = "03.POL";
                            worksheet.Cells[11, 5].Value = "Atasan";
                            worksheet.Cells[12, 5].Value = "Atasan";
                            worksheet.Cells[13, 5].Value = "Atasan";
                            worksheet.Cells[14, 5].Value = "Atasan";
                            worksheet.Cells[15, 5].Value = "Atasan";
                            worksheet.Cells[11, 6].Value = "Polo";
                            worksheet.Cells[12, 6].Value = "Polo";
                            worksheet.Cells[13, 6].Value = "Polo";
                            worksheet.Cells[14, 6].Value = "Polo";
                            worksheet.Cells[15, 6].Value = "Polo";
                            worksheet.Cells[11, 7].Value = "100000";
                            worksheet.Cells[12, 7].Value = "100000";
                            worksheet.Cells[13, 7].Value = "100000";
                            worksheet.Cells[14, 7].Value = "100000";
                            worksheet.Cells[15, 7].Value = "100000";
                            worksheet.Cells[11, 8].Value = "100000";
                            worksheet.Cells[12, 8].Value = "100000";
                            worksheet.Cells[13, 8].Value = "100000";
                            worksheet.Cells[14, 8].Value = "100000";
                            worksheet.Cells[15, 8].Value = "100000";
                            worksheet.Cells[11, 9].Value = "100";
                            worksheet.Cells[12, 9].Value = "100";
                            worksheet.Cells[13, 9].Value = "100";
                            worksheet.Cells[14, 9].Value = "100";
                            worksheet.Cells[15, 9].Value = "100";
                            worksheet.Cells[11, 11].Value = "a1234";
                            worksheet.Cells[12, 11].Value = "b3143";
                            worksheet.Cells[13, 11].Value = "b1233";
                            worksheet.Cells[14, 11].Value = "b4123";
                            worksheet.Cells[15, 11].Value = "b1231";

                            #endregion
                            worksheet.Cells[10, 1].Value = "NAMA1";
                            worksheet.Cells[10, 2].Value = "NAMA2";
                            worksheet.Cells[10, 3].Value = "KODE_BRG_MO";
                            worksheet.Cells[10, 4].Value = "KODE_BRG_INDUK_MO (diisi khusus untuk barang variasi)";
                            worksheet.Cells[10, 5].Value = "KODE_KATEGORI_MO";
                            worksheet.Cells[10, 6].Value = "KODE_MEREK_MO";
                            worksheet.Cells[10, 7].Value = "HJUAL";
                            worksheet.Cells[10, 8].Value = "HJUAL_MARKETPLACE";
                            worksheet.Cells[10, 9].Value = "BERAT";
                            worksheet.Cells[10, 10].Value = "IMAGE";
                            worksheet.Cells[10, 11].Value = "KODE_BRG_MARKETPLACE";

                            string sSQL = "SELECT replace(replace(BRG_MP, char(10), ''), char(13), '') AS BRG_MP, ";
                            sSQL += "replace(replace(NAMA, char(10), ''), char(13), '') NAMA, ";
                            sSQL += "replace(replace(NAMA2, char(10), ''), char(13), '') NAMA2, ";
                            sSQL += "replace(replace(NAMA3, char(10), ''), char(13), '') NAMA3, HJUAL, HJUAL_MP, KODE_BRG_INDUK, BERAT, IMAGE, ";
                            sSQL += "replace(replace(DESKRIPSI, char(10), ''), char(13), '') DESKRIPSI, ";
                            sSQL += "replace(replace(CATEGORY_NAME, char(10), ''), char(13), '') CATEGORY_NAME, ";
                            //change 10 Juli 2019, ambil seller sku dari temp
                            //sSQL += "'' SELLER_SKU,'' AS MEREK, '' AS CATEGORY";
                            sSQL += " SELLER_SKU,'' AS MEREK, '' AS CATEGORY";
                            //end change 10 Juli 2019, ambil seller sku dari temp
                            sSQL += " FROM TEMP_BRG_MP where cust = '" + customer.CUST + "' order by nama, nama2, [type] desc, recnum";
                            var dsBarang = EDB.GetDataSet("CString", "STF02", sSQL);

                            for (int i = 0; i < dsBarang.Tables[0].Rows.Count; i++)
                            {
                                worksheet.Cells[19 + i, 1].Value = dsBarang.Tables[0].Rows[i]["NAMA"].ToString();
                                worksheet.Cells[19 + i, 2].Value = dsBarang.Tables[0].Rows[i]["NAMA2"].ToString();
                                worksheet.Cells[19 + i, 3].Value = dsBarang.Tables[0].Rows[i]["SELLER_SKU"].ToString();
                                worksheet.Cells[19 + i, 4].Value = dsBarang.Tables[0].Rows[i]["KODE_BRG_INDUK"].ToString();
                                //worksheet.Cells[10 + i, 5].Value = "KODE_KATEGORI_MO";
                                //worksheet.Cells[10 + i, 6].Value = "KODE_MEREK_MO";
                                worksheet.Cells[19 + i, 7].Value = dsBarang.Tables[0].Rows[i]["HJUAL"].ToString();
                                worksheet.Cells[19 + i, 8].Value = dsBarang.Tables[0].Rows[i]["HJUAL_MP"].ToString();
                                worksheet.Cells[19 + i, 9].Value = dsBarang.Tables[0].Rows[i]["BERAT"].ToString();
                                worksheet.Cells[19 + i, 10].Value = dsBarang.Tables[0].Rows[i]["IMAGE"].ToString();
                                worksheet.Cells[19 + i, 11].Value = dsBarang.Tables[0].Rows[i]["BRG_MP"].ToString();
                            }

                            #region formatting
                            using (var range = worksheet.Cells[2, 3, 7, 4])
                            {
                                //range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                                //range.Style.Font.Color.SetColor(Color.White);
                            }

                            using (var range = worksheet.Cells[3, 1, 7, 2])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }

                            using (var range = worksheet.Cells[10, 1, 10, 11])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                            }

                            using (var range = worksheet.Cells[11, 1, 15, 11])
                            {
                                //range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                                //range.Style.Font.Color.SetColor(Color.White);
                            }

                            using (var range = worksheet.Cells[18, 1, 18, 11])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }
                            #endregion

                            ExcelRange rg0 = worksheet.Cells[18, 1, worksheet.Dimension.End.Row, 11];
                            string tableName0 = "TableBarang";
                            ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);
                            table0.Columns[0].Name = "NAMA1";
                            table0.Columns[1].Name = "NAMA2";
                            table0.Columns[2].Name = "KODE_BRG_MO";
                            table0.Columns[3].Name = "KODE_BRG_INDUK_MO (diisi khusus untuk barang variasi)";
                            table0.Columns[4].Name = "KODE_KATEGORI_MO";
                            table0.Columns[5].Name = "KODE_MEREK_MO";
                            table0.Columns[6].Name = "HJUAL";
                            table0.Columns[7].Name = "HJUAL_MARKETPLACE";
                            table0.Columns[8].Name = "BERAT";
                            table0.Columns[9].Name = "IMAGE";
                            table0.Columns[10].Name = "KODE_BRG_MARKETPLACE";
                            table0.ShowHeader = true;
                            table0.ShowFilter = true;
                            table0.ShowRowStripes = false;

                            worksheet.Cells.AutoFitColumns(0);
                            //XcelUtils.OutputDir = new DirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}MasterOnline");
                            //var xlFile = XcelUtils.GetFileInfo("coba123.xlsx");
                            // save our new workbook in the output directory and we are done!
                            //package.SaveAs(xlFile);
                            var sheet2 = worksheet.Workbook.Worksheets.Add("master_Kategori_dan_Merek");

                            sheet2.Cells[2, 1].Value = "KATEGORI";
                            sheet2.Cells[2, 6].Value = "MEREK";

                            //sheet2.Cells[3, 1].Value = "KODE";
                            //sheet2.Cells[3, 2].Value = "KET";
                            //sheet2.Cells[3, 6].Value = "KODE";
                            //sheet2.Cells[3, 7].Value = "KET";

                            //for (int index = 1; index <= brokerBranchs.Count; index++)
                            //{
                            //    ddList.Cells[index, 1].Value = brokerBranchs[index - 1].Title;
                            //}

                            var kategori = ErasoftDbContext.STF02E.Where(m => m.LEVEL == "1").ToList();
                            if (kategori.Count > 0)
                            {
                                for (int j = 0; j < kategori.Count; j++)
                                {
                                    sheet2.Cells[4 + j, 1].Value = kategori[j].KODE;
                                    sheet2.Cells[4 + j, 2].Value = kategori[j].KET;
                                }
                            }

                            if (dsBarang.Tables[0].Rows.Count > 0)
                            {
                                var validation = worksheet.DataValidations.AddListValidation(worksheet.Cells[19, 5, worksheet.Dimension.End.Row, 5].Address);
                                validation.ShowErrorMessage = true;
                                validation.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                                validation.ErrorTitle = "An invalid value was entered";
                                validation.Formula.ExcelFormula = string.Format("=master_Kategori_dan_Merek!${0}${1}:${2}${3}", "A", 4, "A", 3 + kategori.Count);
                            }

                            var merk = ErasoftDbContext.STF02E.Where(m => m.LEVEL == "2").ToList();
                            if (merk.Count > 0)
                            {
                                for (int k = 0; k < merk.Count; k++)
                                {
                                    sheet2.Cells[4 + k, 6].Value = merk[k].KODE;
                                    sheet2.Cells[4 + k, 7].Value = merk[k].KET;
                                }
                            }
                            //var a = new OfficeOpenXml.ExcelTableAddress[3, 1, 3, 2];
                            if (dsBarang.Tables[0].Rows.Count > 0)
                            {
                                var validation2 = worksheet.DataValidations.AddListValidation(worksheet.Cells[19, 6, worksheet.Dimension.End.Row, 6].Address);
                                validation2.ShowErrorMessage = true;
                                validation2.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                                validation2.ErrorTitle = "An invalid value was entered";
                                validation2.Formula.ExcelFormula = string.Format("=master_Kategori_dan_Merek!${0}${1}:${2}${3}", "F", 4, "F", 3 + merk.Count);
                            }

                            using (var range = sheet2.Cells[3, 1, 3, 2])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.Orange);

                                //ExcelTable table = sheet2.Tables.Add(range, "tblKategory");

                                //table.Columns[0].Name = "KODE";
                                //table.Columns[1].Name = "KET";
                                //table.ShowHeader = true;
                                //table.ShowFilter = true;
                            }

                            ExcelRange rg = sheet2.Cells[3, 1, worksheet.Dimension.End.Row, 2];
                            string tableName = "TableKategory";
                            ExcelTable table = sheet2.Tables.Add(rg, tableName);
                            table.Columns[0].Name = "KODE";
                            table.Columns[1].Name = "KET";
                            table.ShowHeader = true;
                            table.ShowFilter = true;
                            table.ShowRowStripes = false;

                            ExcelRange rg2 = sheet2.Cells[3, 6, worksheet.Dimension.End.Row, 7];
                            string tableName2 = "TableMerk";
                            ExcelTable table2 = sheet2.Tables.Add(rg2, tableName2);
                            table2.Columns[0].Name = "KODE";
                            table2.Columns[1].Name = "KET";
                            table2.ShowHeader = true;
                            table2.ShowFilter = true;
                            table2.ShowRowStripes = false;

                            using (var range = sheet2.Cells[3, 6, 3, 7])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                            }

                            //sheet2.Cells.AutoFitColumns(0);

                            //return File(package.GetAsByteArray(), System.Net.Mime.MediaTypeNames.Application.Octet, username + "_" + mp.NamaMarket + "(" + customer.PERSO + ")" + ".xlsx");
                            ret.byteExcel = package.GetAsByteArray();
                            ret.namaFile = username + "_" + mp.NamaMarket + "(" + customer.PERSO + ")" + ".xlsx";

                        }
                    }
                    else
                    {
                        ret.Errors.Add("Customer tidak ditemukan.");
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            //return Json(ret, JsonRequestBehavior.AllowGet);
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var result = new ContentResult
            {
                Content = serializer.Serialize(ret),
                ContentType = "application/json"
            };
            return result;
        }

        public ActionResult UploadXcel()
        {
            //var file = Request.Files[0];
            //List<string> excelData = new List<string>();
            //var listCust = new List<string>();
            BindUploadExcel ret = new BindUploadExcel();
            ret.Errors = new List<string>();
            ret.cust = new List<string>();
            ret.namaCust = new List<string>();
            ret.lastRow = new List<int>();
            try
            {
                var mp = MoDbContext.Marketplaces.ToList();
                for (int file_index = 0; file_index < Request.Files.Count; file_index++)
                {
                    var file = Request.Files[file_index];
                    if (file != null && file.ContentLength > 0)
                    {
                        byte[] data;
                        ret.lastRow.Add(0);
                        using (Stream inputStream = file.InputStream)
                        {
                            MemoryStream memoryStream = inputStream as MemoryStream;
                            if (memoryStream == null)
                            {
                                memoryStream = new MemoryStream();
                                inputStream.CopyTo(memoryStream);
                            }
                            data = memoryStream.ToArray();
                        }

                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            using (ExcelPackage excelPackage = new ExcelPackage(stream))
                            {
                                using (ErasoftContext eraDB = new ErasoftContext(DataSourcePath, dbPathEra))
                                {
                                    eraDB.Database.CommandTimeout = 180;
                                    //loop all worksheets
                                    var worksheet = excelPackage.Workbook.Worksheets[1];
                                    //foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                                    //{
                                    string cust = worksheet.Cells[1, 2].Value == null ? "" : worksheet.Cells[1, 2].Value.ToString();
                                    if (!string.IsNullOrEmpty(cust))
                                    {
                                        var customer = eraDB.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                                        if (customer != null)
                                        {
                                            var resetFlag = EDB.ExecuteSQL("CString", System.Data.CommandType.Text, "update temp_brg_mp set avalue_36 = '' where cust = '" + cust + "' and avalue_36 like 'auto%'");
                                            string namaMP = mp.Where(m => m.IdMarket.ToString() == customer.NAMA).SingleOrDefault().NamaMarket;
                                            ret.cust.Add(cust);
                                            ret.namaCust.Add(namaMP + "(" + customer.PERSO + ")");

                                            var listTemp = eraDB.TEMP_BRG_MP.Where(m => m.CUST == cust).ToList();
                                            if (listTemp.Count > 0)
                                            {
                                                //loop all rows
                                                for (int i = 19; i <= worksheet.Dimension.End.Row; i++)
                                                {
                                                    var kd_brg_mp = worksheet.Cells[i, 11].Value == null ? "" : worksheet.Cells[i, 11].Value.ToString();
                                                    if (!string.IsNullOrEmpty(kd_brg_mp))
                                                    {
                                                        ////loop all columns in a row
                                                        //for (int j = 1; j <= worksheet.Dimension.End.Column; j++)
                                                        //{
                                                        //    //add the cell data to the List
                                                        //    if (worksheet.Cells[i, j].Value != null)
                                                        //    {
                                                        //        excelData.Add(worksheet.Cells[i, j].Value.ToString());
                                                        //    }
                                                        //}
                                                        var current_brg = listTemp.Where(m => m.BRG_MP == kd_brg_mp).SingleOrDefault();
                                                        if (current_brg != null)
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 3].Value))) //user tidak isi kode barang mo, tidak perlu update  
                                                            {
                                                                if (!string.IsNullOrEmpty(current_brg.KODE_BRG_INDUK) && string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 4].Value)))
                                                                {
                                                                    // barang varian tapi tidak diisi kode brg induk di excel
                                                                    //break;
                                                                }
                                                                else
                                                                {
                                                                    //if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 1].Value)))
                                                                    //    current_brg.NAMA = worksheet.Cells[i, 1].Value.ToString();
                                                                    //current_brg.NAMA2 = worksheet.Cells[i, 2].Value == null ? "" : worksheet.Cells[i, 2].Value.ToString();
                                                                    current_brg.SELLER_SKU = worksheet.Cells[i, 3].Value == null ? "" : worksheet.Cells[i, 3].Value.ToString();
                                                                    current_brg.KODE_BRG_INDUK = worksheet.Cells[i, 4].Value == null ? "" : worksheet.Cells[i, 4].Value.ToString();
                                                                    //change 14 juni 2019, kode kategori mo disimpan di avalue_40, kode kategory mp tetap di category_code
                                                                    //current_brg.CATEGORY_CODE = worksheet.Cells[i, 5].Value == null ? "" : worksheet.Cells[i, 5].Value.ToString();
                                                                    current_brg.AVALUE_40 = worksheet.Cells[i, 5].Value == null ? "" : worksheet.Cells[i, 5].Value.ToString();
                                                                    //end change 14 juni 2019, kode kategori mo disimpan di avalue_40, kode kategory mp tetap di category_code
                                                                    current_brg.MEREK = worksheet.Cells[i, 6].Value == null ? "" : worksheet.Cells[i, 6].Value.ToString();
                                                                    //current_brg.HJUAL_MP = Convert.ToDouble(worksheet.Cells[i, 7].Value == null ? "0" : worksheet.Cells[i, 7].Value.ToString());
                                                                    current_brg.HJUAL = Convert.ToDouble(worksheet.Cells[i, 7].Value == null ? "0" : worksheet.Cells[i, 7].Value.ToString());
                                                                    //current_brg.BERAT = Convert.ToDouble(worksheet.Cells[i, 9].Value == null ? "0" : worksheet.Cells[i, 9].Value.ToString());
                                                                    //current_brg.IMAGE = worksheet.Cells[i, 10].Value == null ? "" : worksheet.Cells[i, 10].Value.ToString();
                                                                    current_brg.AVALUE_36 = "Auto Process";// barang yg akan di transfer ke master hasil upload excel saja
                                                                    try
                                                                    {
                                                                        eraDB.SaveChanges();
                                                                    }
                                                                    catch (Exception ex)
                                                                    {

                                                                    }
                                                                }

                                                            }
                                                        }
                                                        else
                                                        {
                                                            ret.Errors.Add(namaMP + "(" + customer.PERSO + ") : Kode Barang MP (" + kd_brg_mp + ") tidak ditemukan");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ret.Errors.Add(namaMP + "(" + customer.PERSO + ") : Kode barang marketplace tidak ditemukan lagi di baris " + i);
                                                        ret.lastRow[file_index] = i;
                                                        break;
                                                    }
                                                }
                                                if (ret.lastRow[file_index] == 0)
                                                    ret.lastRow[file_index] = worksheet.Dimension.End.Row;
                                            }
                                            else
                                            {
                                                //var mp = MoDbContext.Marketplaces.Where(m => m.IdMarket.ToString() == customer.NAMA).FirstOrDefault();
                                                //ret.Errors.Add("Data barang untuk akun " + mp.NamaMarket + "(" + customer.PERSO + ") tidak ditemukan");
                                                ret.Errors.Add(namaMP + "(" + customer.PERSO + ") : Data Barang untuk akun ini tidak ditemukan");
                                            }
                                        }
                                        else
                                        {
                                            //customer not found
                                            ret.Errors.Add("File " + file.FileName + ": Akun marketplace tidak ditemukan");
                                        }
                                    }
                                    else
                                    {
                                        //cust empty
                                        ret.Errors.Add("File " + file.FileName + ": Kode akun marketplace tidak ditemukan");
                                    }
                                    //}
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public ActionResult HargaJualtoExcel()
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };
            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Harga Jual Barang");

                    string sSQL = "SELECT S.BRG, ";
                    sSQL += "replace(replace(S.NAMA, char(10), ''), char(13), '') + ISNULL(replace(replace(S.NAMA2, char(10), ''), char(13), ''), '') AS NAMA, ";
                    sSQL += "M.NAMAMARKET + '(' + replace(replace(A.PERSO, char(10), ''), char(13), '') + ')' AS AKUN,H.HJUAL, M.IDMARKET, ISNULL(STF10.HPOKOK, 0) AS HPOKOK ";
                    sSQL += "FROM STF02 S INNER JOIN STF02H H ON S.BRG = H.BRG INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM ";
                    sSQL += "INNER JOIN MO..MARKETPLACE M ON A.NAMA = M.IDMARKET LEFT JOIN STF10 ON S.BRG = STF10.BRG WHERE TYPE = '3' ORDER BY NAMA,M.IDMARKET";
                    var dsBarang = EDB.GetDataSet("CString", "STF02", sSQL);

                    if (dsBarang.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsBarang.Tables[0].Rows.Count; i++)
                        {
                            worksheet.Cells[2 + i, 1].Value = dsBarang.Tables[0].Rows[i]["BRG"].ToString();
                            worksheet.Cells[2 + i, 2].Value = dsBarang.Tables[0].Rows[i]["NAMA"].ToString();
                            worksheet.Cells[2 + i, 3].Value = dsBarang.Tables[0].Rows[i]["AKUN"].ToString();
                            worksheet.Cells[2 + i, 4].Value = dsBarang.Tables[0].Rows[i]["HJUAL"].ToString();
                            worksheet.Cells[2 + i, 5].Value = dsBarang.Tables[0].Rows[i]["HPOKOK"].ToString();
                        }
                        ExcelRange rg0 = worksheet.Cells[1, 1, worksheet.Dimension.End.Row, 5];
                        string tableName0 = "TableBarang";
                        ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);
                        table0.Columns[0].Name = "KODE BARANG";
                        table0.Columns[1].Name = "NAMA BARANG";
                        table0.Columns[2].Name = "AKUN MARKETPLACE";
                        table0.Columns[3].Name = "HARGA JUAL";
                        table0.Columns[4].Name = "HARGA JUAL TERAKHIR";
                        table0.ShowHeader = true;
                        table0.ShowFilter = true;
                        table0.ShowRowStripes = false;
                        worksheet.Cells.AutoFitColumns(0);

                        //return File(package.GetAsByteArray(), System.Net.Mime.MediaTypeNames.Application.Octet, username + "_hargaJual" + ".xlsx");
                        ret.byteExcel = package.GetAsByteArray();
                        ret.namaFile = username + "_hargaJual" + ".xlsx";
                    }
                    else
                    {
                        ret.Errors.Add("Tidak ada data harga jual barang.");
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            //return Json(ret, JsonRequestBehavior.AllowGet);
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var result = new ContentResult
            {
                Content = serializer.Serialize(ret),
                ContentType = "application/json"
            };
            return result;
        }

        public FileResult DownloadFileExcel(/*byte[] file, string fileName*//*BindDownloadExcel data*/ string data)
        {
            return File(/*data.byteExcel*/ new byte[1], /*System.Net.Mime.MediaTypeNames.Application.Octet,*/ /*data.namaFile +*/ ".xlsx");
        }

        #region saldo awal inp.
        public ActionResult ListBarangtoExcel(/*string kd_gudang*/)
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };
            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                    //change by calvin 16 september 2019
                    //string sSQL = "select brg, nama + isnull(nama2, '') as nama from stf02 where type = 3 order by nama,nama2";
                    string sSQL = "select brg, nama + ' ' + isnull(nama2, '') as nama from stf02 where type = 3 order by nama,nama2";
                    //end change by calvin 16 september 2019

                    var dsBarang = EDB.GetDataSet("CString", "STF02", sSQL);

                    if (dsBarang.Tables[0].Rows.Count > 0)
                    {
                        worksheet.Cells["A1"].Value = "Kode Gudang";
                        worksheet.Cells[2, 2].Value = "Isi kode gudang sesuai dengan master gudang pada sheet2";
                        worksheet.Cells[2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                        //var gudang = ErasoftDbContext.STF18.Where(m => m.Kode_Gudang == kd_gudang).FirstOrDefault();
                        var gudang = ErasoftDbContext.STF18.ToList();

                        if (gudang.Count > 0)
                        {
                            worksheet.Cells[2, 1].Value = gudang[0].Kode_Gudang;

                            for (int i = 0; i < dsBarang.Tables[0].Rows.Count; i++)
                            {
                                worksheet.Cells[5 + i, 1].Value = dsBarang.Tables[0].Rows[i]["BRG"].ToString();
                                worksheet.Cells[5 + i, 2].Value = dsBarang.Tables[0].Rows[i]["NAMA"].ToString();
                            }
                            ExcelRange rg0 = worksheet.Cells[4, 1, worksheet.Dimension.End.Row, 4];
                            string tableName0 = "TableBarang";
                            ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);
                            table0.Columns[0].Name = "KODE BARANG";
                            table0.Columns[1].Name = "NAMA BARANG";
                            table0.Columns[2].Name = "QTY";
                            table0.Columns[3].Name = "HARGA MODAL";
                            //table0.Columns[4].Name = "NAMA BARANG";
                            #region formatting
                            using (var range = worksheet.Cells[1, 1, 2, 1])
                            {
                                //range.Style.Font.Bold = true;
                                //range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                //range.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                                //range.Style.Font.Color.SetColor(Color.White);
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }

                            using (var range = worksheet.Cells[4, 1, 4, 4])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }

                            #endregion
                            table0.ShowHeader = true;
                            table0.ShowFilter = true;
                            table0.ShowRowStripes = false;
                            worksheet.Cells.AutoFitColumns(0);

                            var sheet2 = worksheet.Workbook.Worksheets.Add("sheet2");

                            sheet2.Cells[2, 1].Value = "Kode Gudang";
                            sheet2.Cells[2, 2].Value = "Nama Gudang";

                            //var kategori = ErasoftDbContext.STF02E.Where(m => m.LEVEL == "1").ToList();
                            //if (kategori.Count > 0)
                            //{
                            for (int j = 0; j < gudang.Count; j++)
                            {
                                sheet2.Cells[3 + j, 1].Value = gudang[j].Kode_Gudang;
                                sheet2.Cells[3 + j, 2].Value = gudang[j].Nama_Gudang;
                            }
                            //}

                            using (var range = sheet2.Cells[2, 1, 2, 2])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }
                            sheet2.Cells.AutoFitColumns(0);

                            if (dsBarang.Tables[0].Rows.Count > 0)
                            {
                                //var validation = worksheet.DataValidations.AddListValidation(worksheet.Cells[19, 5, worksheet.Dimension.End.Row, 5].Address);
                                var validation = worksheet.DataValidations.AddListValidation("A2");

                                validation.ShowErrorMessage = true;
                                validation.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                                validation.ErrorTitle = "An invalid value was entered";
                                validation.Formula.ExcelFormula = string.Format("=sheet2!${0}${1}:${2}${3}", "A", 3, "A", 2 + gudang.Count);
                            }

                            //return File(package.GetAsByteArray(), System.Net.Mime.MediaTypeNames.Application.Octet, username + "_saldoawalstok" + ".xlsx");
                            ret.byteExcel = package.GetAsByteArray();
                            ret.namaFile = username + "_saldoawalstok" + ".xlsx";
                        }
                        else
                        {
                            ret.Errors.Add("Kode gudang tidak ditemukan.");
                        }

                    }
                    else
                    {
                        ret.Errors.Add("Tidak ada data barang.");
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            //return Json(ret, JsonRequestBehavior.AllowGet);
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var result = new ContentResult
            {
                Content = serializer.Serialize(ret),
                ContentType = "application/json"
            };
            return result;
        }
        #endregion

        public class dataByte
        {
            public byte[] data { get; set; }
        }

        #region Upload Excel Saldo Awal Stock
        public async Task<ActionResult> UploadXcelSaldoAwal(string nobuk, int countAll, string percentDanprogress, string statusLoopSuccess)
        {
            //var file = Request.Files[0];
            //List<string> excelData = new List<string>();
            //var listCust = new List<string>();
            BindUploadExcel ret = new BindUploadExcel();
            ret.Errors = new List<string>();
            ret.namaGudang = new List<string>();
            ret.lastRow = new List<int>();
            ret.nextFile = false;
            byte[] dataByte = null;
            //bool statusLoop = false;
            //bool statusComplete = false;

            string[] status = statusLoopSuccess.Split(';');
            string[] prog = percentDanprogress.Split(';');
            if (countAll > 0)
            {
                ret.countAll = countAll;
            }

            try
            {
                ret.statusLoop = Convert.ToBoolean(status[0]);
                ret.statusSuccess = Convert.ToBoolean(status[1]);

                if (ret.byteData == null && ret.statusLoop == false)
                {
                    if (Request.Files[0].ContentType.Contains("application/vnd.ms-excel"))
                    {
                        //using (Stream inputStream = Request.Files[0].InputStream)
                        //{
                        //    Workbook workbook = new Workbook();
                        //    MemoryStream memory = inputStream as MemoryStream;
                        //    if (memory == null)
                        //    {
                        //        memory = new MemoryStream();
                        //        inputStream.CopyTo(memory);
                        //        workbook.LoadFromStream(memory);
                        //        //MemoryStream memoryStream1 = new MemoryStream();
                        //        workbook.SaveToStream(memory, FileFormat.Version97to2003);
                        //        dataByte = memory.ToArray();
                        //    }
                        //}
                        ret.Errors.Add("Mohon maaf format file .xls saat ini belum mendukung untuk proses Upload Excel Saldo Awal. silahkan untuk mengganti format menjadi .xlsx");
                        ret.statusSuccess = false;
                        return Json(ret, JsonRequestBehavior.AllowGet);
                    }
                    else if (Request.Files[0].ContentType.Contains("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                    {
                        dataByte = UploadFileServices.UploadFile(Request.Files[0]);
                    }

                    ret.byteData = dataByte;
                }
                else
                {
                    ret.byteData = null;
                    ret.nobuk = nobuk;
                }

                for (int file_index = 0; file_index < Request.Files.Count; file_index++)
                {
                    //    byte[] data;
                    if (ret.statusLoop == false)
                    {
                        ret.lastRow.Add(0);
                    }

                    if (ret.statusLoop == true)
                    {
                        var file = Request.Files[file_index];
                        if (file != null && file.ContentLength > 0)
                        {
                            string[] cekFormat = file.FileName.Split('.');
                            if (cekFormat.Last().ToLower().ToString() == "xlsx")
                            {
                                using (Stream inputStream = file.InputStream)
                                {
                                    MemoryStream memoryStream = inputStream as MemoryStream;
                                    if (memoryStream == null)
                                    {
                                        memoryStream = new MemoryStream();
                                        inputStream.CopyTo(memoryStream);
                                    }
                                    ret.byteData = memoryStream.ToArray();
                                }
                            }
                            else if (cekFormat.Last().ToLower().ToString() == "xls")
                            {
                                using (Stream inputStream = file.InputStream)
                                {
                                    Workbook workbook = new Workbook();
                                    workbook.LoadFromStream(inputStream);
                                    MemoryStream memoryStream = new MemoryStream();
                                    workbook.SaveToStream(memoryStream, FileFormat.Version2013);
                                    ret.byteData = memoryStream.ToArray();
                                }
                            }
                            else
                            {
                                ret.Errors.Add("Format file tidak mendukung. Mohon untuk tidak mengubah format file excel hasil download program.");
                                ret.statusSuccess = false;
                                return Json(ret, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }


                    using (MemoryStream stream = new MemoryStream(ret.byteData))
                    {
                        //using (ExcelPackage excelPackage = new ExcelPackage(stream))
                        using (ExcelPackage excelPackage = new ExcelPackage(stream))

                        //FileInfo existingFile = new FileInfo("C:\\Users\\Agashi\\source\\repos\\MODev\\MasterOnline\\Content\\Uploaded\\Setiawan_qty_hargamodal.xlsx");
                        //using (ExcelPackage excelPackage = new ExcelPackage(existingFile))
                        {
                            using (ErasoftContext eraDB = new ErasoftContext(DataSourcePath, dbPathEra))
                            {
                                using (System.Data.Entity.DbContextTransaction transaction = eraDB.Database.BeginTransaction())
                                {
                                    eraDB.Database.CommandTimeout = 1800;
                                    //loop all worksheets
                                    var worksheet = excelPackage.Workbook.Worksheets[1];
                                    string gd = worksheet.Cells[2, 1].Value == null ? "" : worksheet.Cells[2, 1].Value.ToString();
                                    if (!string.IsNullOrEmpty(gd))
                                    {
                                        var gudang = eraDB.STF18.Where(m => m.Kode_Gudang == gd).Select(m => m.Nama_Gudang).FirstOrDefault();
                                        if (gudang != null)
                                        {
                                            if (ret.statusLoop == false)
                                            {
                                                ret.namaGudang.Add(gudang);
                                            }

                                            var listTemp = new List<string>();
                                            if (ret.countAll <= 0)
                                            {
                                                listTemp = eraDB.STF02.Where(m => m.TYPE == "3").Select(p => p.BRG).ToList();
                                                if (listTemp.Count() <= 0)
                                                {
                                                    transaction.Rollback();
                                                    ret.Errors.Add("Data Barang tidak ditemukan");
                                                    ret.statusSuccess = false;
                                                    return Json(ret, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            #region create induk
                                            if (ret.statusLoop == false)
                                            {
                                                eraDB.Database.ExecuteSqlCommand("DELETE FROM TEMP_SALDOAWAL");

                                                var stt01a = new STT01A
                                                {
                                                    Satuan = "",
                                                    Ket = "",
                                                    ST_Posting = "",
                                                    MK = "M",
                                                    JTran = "M",
                                                    Ref = "",
                                                    WORK_CENTER = "",
                                                    KLINE = "",
                                                    KODE_ANGKUTAN = "",
                                                    JENIS_MOBIL = "",
                                                    NO_POLISI = "",
                                                    NAMA_SOPIR = "",
                                                    No_PP = "",
                                                    CATATAN_1 = "",
                                                    CATATAN_2 = "",
                                                    CATATAN_3 = "",
                                                    CATATAN_4 = "",
                                                    CATATAN_5 = "",
                                                    CATATAN_6 = "",
                                                    CATATAN_7 = "",
                                                    CATATAN_8 = "",
                                                    CATATAN_9 = "",
                                                    CATATAN_10 = "",
                                                    NOBUK_POQC = "",
                                                    Supp = "",
                                                    NAMA_SUPP = "",
                                                    NO_PL = "",
                                                    NO_FAKTUR = "",
                                                    STATUS_LOADING = "0",
                                                    Tgl = DateTime.Now,
                                                    UserName = "UPLOAD_EXCEL_SA",
                                                    TglInput = DateTime.Now,
                                                    VALUTA = "IDR",
                                                    TUKAR = 1,
                                                    JRef = "6",
                                                    KOLI = 0,
                                                    VOLUME = 0,
                                                    BERAT = 0,
                                                    NILAI_ANGKUTAN = 0,
                                                    JLH_KARYAWAN = 0,
                                                    Kurs = 1,
                                                    ST_Cetak = "1",
                                                    Jenis_Form = 1,
                                                    Retur_Penuh = false,
                                                    Terima_Penuh = false,
                                                    TERIMA_PENUH_PO_QC = false,
                                                    JAM = 1
                                                };


                                                var lastBukti = new ManageController().GenerateAutoNumber(ErasoftDbContext, "ST", "STT01A", "Nobuk");
                                                //var lastBukti = ManageController().GenerateAutoNumber(ErasoftDbContext, "ST", "STT01A", "Nobuk");
                                                var noStok = "ST" + DateTime.UtcNow.AddHours(7).Year.ToString().Substring(2, 2) + Convert.ToString(Convert.ToInt32(lastBukti) + 1).PadLeft(6, '0');
                                                //end change by nurul 23/12/2019, perbaikan no bukti


                                                stt01a.Nobuk = noStok;
                                                ret.nobuk = noStok;

                                                object[] sParams1 = {
                                                    new SqlParameter("@NOBUK", noStok)
                                                };

                                                try
                                                {
                                                    eraDB.STT01A.Add(stt01a);
                                                    eraDB.SaveChanges();
                                                }
                                                catch (Exception ex)
                                                {
                                                    var tempSI = eraDB.STT01A.Where(a => a.Nobuk == stt01a.Nobuk).Single();
                                                    if (tempSI != null)
                                                    {
                                                        if (tempSI.Nobuk == noStok)
                                                        {
                                                            var lastBuktiNew = Convert.ToInt32(lastBukti);
                                                            lastBuktiNew++;
                                                            noStok = "ST" + DateTime.UtcNow.AddHours(7).Year.ToString().Substring(2, 2) + Convert.ToString(Convert.ToInt32(lastBuktiNew) + 1).PadLeft(6, '0');
                                                            stt01a.Nobuk = noStok;
                                                            ret.nobuk = noStok;
                                                            eraDB.STT01A.Add(stt01a);
                                                            eraDB.SaveChanges();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var errMsg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                                        ret.Errors.Add(errMsg);
                                                        transaction.Rollback();
                                                        eraDB.Database.ExecuteSqlCommand("DELETE FROM STT01A WHERE NOBUK = @NOBUK ", sParams1);
                                                        eraDB.Database.ExecuteSqlCommand("DELETE FROM STT01B WHERE NOBUK = @NOBUK ", sParams1);
                                                        //return Json(ret, JsonRequestBehavior.AllowGet);
                                                    }
                                                }
                                                //end change by nurul 23/12/2019, perbaikan no bukti

                                            }
                                            #endregion

                                            //ret.countAll = worksheet.Dimension.End.Row;

                                            object[] sParams = {
                                                    new SqlParameter("@NOBUK", ret.nobuk)
                                                };

                                            if (Convert.ToInt32(prog[1]) == 0)
                                            {
                                                prog[1] = "0";
                                            }
                                            var prosesinsertAwal = false;
                                            var checkTemp = eraDB.TEMP_SALDOAWAL.ToList();
                                            if (checkTemp.Count() <= 0)
                                            {
                                                for (int i = 5; i <= worksheet.Dimension.End.Row; i++)
                                                {
                                                    if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 1].Value)))
                                                    {
                                                        var current_brg = listTemp.Where(m => m == Convert.ToString(worksheet.Cells[i, 1].Value)).SingleOrDefault();
                                                        if (current_brg != null)
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 3].Value)))
                                                            {
                                                                if (Convert.ToInt32(worksheet.Cells[i, 3].Value) >= 0)
                                                                {
                                                                    TEMP_SALDOAWAL newrecord = new TEMP_SALDOAWAL()
                                                                    {
                                                                        BRG = Convert.ToString(worksheet.Cells[i, 1].Value),
                                                                        QTY = Convert.ToInt32(worksheet.Cells[i, 3].Value)
                                                                    };
                                                                    if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 4].Value)))
                                                                    {
                                                                        if (Convert.ToInt32(worksheet.Cells[i, 4].Value) >= 0)
                                                                        {
                                                                            newrecord.HARGA_SATUAN = Convert.ToDouble(worksheet.Cells[i, 4].Value);
                                                                        }
                                                                        else
                                                                        {
                                                                            transaction.Rollback();
                                                                            eraDB.Database.ExecuteSqlCommand("DELETE FROM STT01A WHERE NOBUK = @NOBUK ", sParams);
                                                                            eraDB.Database.ExecuteSqlCommand("DELETE FROM STT01B WHERE NOBUK = @NOBUK ", sParams);
                                                                            ret.Errors.Add("Ada kesalahan dalam Harga Modal, Harga Modal harus angka tidak boleh karakter huruf atau lainnya. Mohon untuk mencoba lagi proses Upload Excel Saldo Awal.");
                                                                            ret.statusSuccess = false;
                                                                            ret.lastRow[file_index] = i;
                                                                            i = worksheet.Dimension.End.Row;
                                                                            return Json(ret, JsonRequestBehavior.AllowGet);
                                                                        }
                                                                    }

                                                                    eraDB.TEMP_SALDOAWAL.Add(newrecord);
                                                                    eraDB.SaveChanges();
                                                                    ret.countAll = ret.countAll + 1;
                                                                    prosesinsertAwal = true;
                                                                }
                                                                else
                                                                {
                                                                    transaction.Rollback();
                                                                    eraDB.Database.ExecuteSqlCommand("DELETE FROM STT01A WHERE NOBUK = @NOBUK ", sParams);
                                                                    eraDB.Database.ExecuteSqlCommand("DELETE FROM STT01B WHERE NOBUK = @NOBUK ", sParams);
                                                                    ret.Errors.Add("Ada kesalahan dalam Quantity, Quantity harus angka tidak boleh karakter huruf atau lainnya. Mohon untuk mencoba lagi proses Upload Excel Saldo Awal.");
                                                                    ret.statusSuccess = false;
                                                                    ret.lastRow[file_index] = i;
                                                                    i = worksheet.Dimension.End.Row;
                                                                    return Json(ret, JsonRequestBehavior.AllowGet);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            transaction.Rollback();
                                                            eraDB.Database.ExecuteSqlCommand("DELETE FROM STT01A WHERE NOBUK = @NOBUK ", sParams);
                                                            eraDB.Database.ExecuteSqlCommand("DELETE FROM STT01B WHERE NOBUK = @NOBUK ", sParams);
                                                            ret.Errors.Add("Kode Barang (" + Convert.ToString(worksheet.Cells[i, 1].Value) + ") tidak ditemukan. Mohon untuk mencoba lagi proses Upload Excel Saldo Awal.");
                                                            ret.statusSuccess = false;
                                                            ret.lastRow[file_index] = i;
                                                            i = worksheet.Dimension.End.Row;
                                                            return Json(ret, JsonRequestBehavior.AllowGet);
                                                        }
                                                    }
                                                }
                                            }


                                            if (prosesinsertAwal == true)
                                            {
                                                ret.progress = -1;
                                                ret.statusLoop = true;
                                                ret.statusSuccess = false;
                                                transaction.Commit();
                                                return Json(ret, JsonRequestBehavior.AllowGet);
                                            }
                                            else
                                            {
                                                if (ret.countAll == 0)
                                                {
                                                    transaction.Rollback();
                                                    ret.Errors.Add("Mohon untuk mengisi kolom Quantity dan Harga Modal (jika diperlukan) untuk proses Upload Excel Saldo Awal.");
                                                    ret.statusSuccess = false;
                                                    return Json(ret, JsonRequestBehavior.AllowGet);
                                                }
                                            }

                                            if (checkTemp.Count() > 0)
                                            {
                                                for (int j = Convert.ToInt32(prog[1]); j <= checkTemp.Count() - 1; j++)
                                                {
                                                    try
                                                    {
                                                        ret.statusLoop = true;
                                                        ret.progress = j;
                                                        ret.percent = (j * 100) / (ret.countAll - 1);
                                                        var bagiProses = (ret.countAll - 1) * (Convert.ToDecimal(30) / Convert.ToDecimal(100));
                                                        bagiProses = Decimal.Round(bagiProses);

                                                        var stt01b = new STT01B
                                                        {
                                                            Dr_Gd = "",
                                                            WO = "",
                                                            Rak = "",
                                                            JTran = "M",
                                                            KLINK = "",
                                                            NO_WO = "",
                                                            KET = "",
                                                            BRG_ORIGINAL = "",
                                                            QTY3 = 0,
                                                            BUKTI_DS = "",
                                                            BUKTI_REFF = "",
                                                            UserName = "UPLOAD_EXCEL_SA",
                                                            Jenis_Form = 1,
                                                            Qty_Retur = 0,
                                                            Qty_Berat = 0,
                                                            TOTAL_LOT = 0,
                                                            TOTAL_QTY = 0,
                                                            QTY_TERIMA_PO_QC = 1,
                                                            TRANS_NO_URUT = 0,
                                                            STN_N = 0,
                                                            BIAYA_PER_QTY = 0,
                                                            QTY_CLAIM = 0,
                                                            NO_URUT_PO = 0,
                                                            NO_URUT_SJ = 0,
                                                            TglInput = DateTime.Now,
                                                            //Nobuk = stt01a.Nobuk,
                                                            Nobuk = ret.nobuk,
                                                            Satuan = "2",
                                                        };
                                                        stt01b.Kobar = checkTemp[j].BRG;
                                                        stt01b.Ke_Gd = gd;
                                                        if (!string.IsNullOrEmpty(Convert.ToString(checkTemp[j].HARGA_SATUAN)))
                                                        {
                                                            stt01b.Harsat = checkTemp[j].HARGA_SATUAN;
                                                        }
                                                        else
                                                        {
                                                            stt01b.Harsat = 0;
                                                        }
                                                        stt01b.Qty = Convert.ToInt32(checkTemp[j].QTY);
                                                        stt01b.Harga = stt01b.Harsat * stt01b.Qty;
                                                        eraDB.STT01B.Add(stt01b);
                                                        eraDB.SaveChanges();

                                                        //if (ret.percent == 10 || ret.percent == 20 ||
                                                        //ret.percent == 30 || ret.percent == 40 ||
                                                        //ret.percent == 50 || ret.percent == 60 ||
                                                        //ret.percent == 70 || ret.percent == 80 ||
                                                        //ret.percent == 90 || ret.percent == 100 || ret.percent >= 100)
                                                        //{



                                                        if (ret.percent >= 100 || ret.progress == ret.countAll - 1)
                                                        {
                                                            transaction.Commit();
                                                            // update stock all barang;
                                                            var doUpdateStock = new ManageController().MarketplaceLogRetryStock();
                                                            ret.statusSuccess = true;
                                                            eraDB.Database.ExecuteSqlCommand("DELETE FROM TEMP_SALDOAWAL");
                                                            return Json(ret, JsonRequestBehavior.AllowGet);
                                                        }

                                                        if (ret.countAll > 25)
                                                        {
                                                            if (ret.progress == (bagiProses * 1) || ret.progress == (bagiProses * 2) ||
                                                                                                                       ret.progress == (bagiProses * 3))
                                                            {
                                                                ret.statusSuccess = false;
                                                                transaction.Commit();
                                                                return Json(ret, JsonRequestBehavior.AllowGet);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ret.statusSuccess = false;
                                                            transaction.Commit();
                                                            return Json(ret, JsonRequestBehavior.AllowGet);
                                                        }

                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        transaction.Rollback();
                                                        ret.Errors.Add(ex.Message.ToString());
                                                        ret.statusSuccess = false;
                                                        eraDB.Database.ExecuteSqlCommand("DELETE FROM STT01A WHERE NOBUK = @NOBUK ", sParams);
                                                        eraDB.Database.ExecuteSqlCommand("DELETE FROM STT01B WHERE NOBUK = @NOBUK ", sParams);
                                                        return Json(ret, JsonRequestBehavior.AllowGet);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            transaction.Rollback();
                                            ret.Errors.Add("Kode gudang tidak ditemukan");
                                        }
                                    }
                                    else
                                    {
                                        transaction.Rollback();
                                        ret.Errors.Add("Kode gudang tidak ditemukan");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            return Json(ret, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Upload Excel Pesanan
        public async Task<ActionResult> UploadXcelPesanan(string nobuk, int countAll, string percentDanprogress, string statusLoopSuccess)
        {
            BindUploadExcel ret = new BindUploadExcel();
            ret.Errors = new List<string>();
            ret.namaGudang = new List<string>();
            ret.lastRow = new List<int>();
            ret.nextFile = false;
            byte[] dataByte = null;
            //bool statusLoop = false;
            //bool statusComplete = false;

            string[] status = statusLoopSuccess.Split(';');
            string[] prog = percentDanprogress.Split(';');
            if (countAll > 0)
            {
                ret.countAll = countAll;
            }

            try
            {
                ret.statusLoop = Convert.ToBoolean(status[0]);
                ret.statusSuccess = Convert.ToBoolean(status[1]);

                if (ret.byteData == null && ret.statusLoop == false)
                {
                    if (Request.Files[0].ContentType.Contains("application/vnd.ms-excel"))
                    {
                        //using (Stream inputStream = Request.Files[0].InputStream)
                        //{
                        //    Workbook workbook = new Workbook();
                        //    MemoryStream memory = inputStream as MemoryStream;
                        //    if (memory == null)
                        //    {
                        //        memory = new MemoryStream();
                        //        inputStream.CopyTo(memory);
                        //        workbook.LoadFromStream(memory);
                        //        //MemoryStream memoryStream1 = new MemoryStream();
                        //        workbook.SaveToStream(memory, FileFormat.Version97to2003);
                        //        dataByte = memory.ToArray();
                        //    }
                        //}
                        ret.Errors.Add("Mohon maaf format file .xls saat ini belum mendukung untuk proses Upload Excel Saldo Awal. silahkan untuk mengganti format menjadi .xlsx");
                        ret.statusSuccess = false;
                        return Json(ret, JsonRequestBehavior.AllowGet);
                    }
                    else if (Request.Files[0].ContentType.Contains("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                    {
                        dataByte = UploadFileServices.UploadFile(Request.Files[0]);
                    }

                    ret.byteData = dataByte;
                }
                else
                {
                    ret.byteData = null;
                    ret.nobuk = nobuk;
                }

                for (int file_index = 0; file_index < Request.Files.Count; file_index++)
                {
                    //    byte[] data;
                    if (ret.statusLoop == false)
                    {
                        ret.lastRow.Add(0);
                    }

                    if (ret.statusLoop == true)
                    {
                        var file = Request.Files[file_index];
                        if (file != null && file.ContentLength > 0)
                        {
                            string[] cekFormat = file.FileName.Split('.');
                            if (cekFormat.Last().ToLower().ToString() == "xlsx")
                            {
                                using (Stream inputStream = file.InputStream)
                                {
                                    MemoryStream memoryStream = inputStream as MemoryStream;
                                    if (memoryStream == null)
                                    {
                                        memoryStream = new MemoryStream();
                                        inputStream.CopyTo(memoryStream);
                                    }
                                    ret.byteData = memoryStream.ToArray();
                                }
                            }
                            else if (cekFormat.Last().ToLower().ToString() == "xls")
                            {
                                using (Stream inputStream = file.InputStream)
                                {
                                    Workbook workbook = new Workbook();
                                    workbook.LoadFromStream(inputStream);
                                    MemoryStream memoryStream = new MemoryStream();
                                    workbook.SaveToStream(memoryStream, FileFormat.Version2013);
                                    ret.byteData = memoryStream.ToArray();
                                }
                            }
                            else
                            {
                                ret.Errors.Add("Format file tidak mendukung. Mohon untuk tidak mengubah format file excel hasil download program.");
                                ret.statusSuccess = false;
                                return Json(ret, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }


                    using (MemoryStream stream = new MemoryStream(ret.byteData))
                    {
                        //using (ExcelPackage excelPackage = new ExcelPackage(stream))
                        using (ExcelPackage excelPackage = new ExcelPackage(stream))
                        {
                            //FileInfo existingFile = new FileInfo("C:\\Users\\Agashi\\source\\repos\\MODev\\MasterOnline\\Content\\Uploaded\\Setiawan_qty_hargamodal.xlsx");
                            //using (ExcelPackage excelPackage = new ExcelPackage(existingFile))

                            using (ErasoftContext eraDB = new ErasoftContext(DataSourcePath, dbPathEra))
                            {
                                using (System.Data.Entity.DbContextTransaction transaction = eraDB.Database.BeginTransaction())
                                {
                                    eraDB.Database.CommandTimeout = 1800;
                                    //loop all worksheets
                                    var worksheet = excelPackage.Workbook.Worksheets[1];

                                    ret.countAll = worksheet.Dimension.End.Row;
                                    if (Convert.ToInt32(prog[1]) == 0)
                                    {
                                        prog[1] = "0";
                                    }

                                    var noBuktiSO = "";

                                    eraDB.Database.ExecuteSqlCommand("DELETE FROM TEMP_UPLOADPESANAN");
                                    List<TEMP_UPLOADPESANAN> batchinsertItem = new List<TEMP_UPLOADPESANAN>();

                                    batchinsertItem = new List<TEMP_UPLOADPESANAN>();

                                    // start looping
                                    for (int i = Convert.ToInt32(prog[0]); i <= worksheet.Dimension.End.Row; i++)
                                    {
                                        ret.statusLoop = true;
                                        ret.progress = i;
                                        ret.percent = (i * 100) / (ret.countAll - 1);
                                        var bagiProses = (ret.countAll - 1) * (Convert.ToDecimal(30) / Convert.ToDecimal(100));
                                        bagiProses = Decimal.Round(bagiProses);

                                        //get ALL DATA
                                        string no_referensi = worksheet.Cells[5, 2].Value == null ? "" : worksheet.Cells[5, 2].Value.ToString();
                                        string tgl_pesanan = worksheet.Cells[5, 3].Value == null ? "" : worksheet.Cells[5, 3].Value.ToString();
                                        string marketplace = worksheet.Cells[5, 4].Value == null ? "" : worksheet.Cells[5, 4].Value.ToString();
                                        string nama_pembeli = worksheet.Cells[5, 6].Value == null ? "" : worksheet.Cells[5, 6].Value.ToString();
                                        string alamat_kirim = worksheet.Cells[5, 7].Value == null ? "" : worksheet.Cells[5, 7].Value.ToString();
                                        string kode_kurir = worksheet.Cells[5, 8].Value == null ? "" : worksheet.Cells[5, 8].Value.ToString();
                                        string top = worksheet.Cells[5, 9].Value == null ? "1" : worksheet.Cells[5, 9].Value.ToString();
                                        string tgl_jatuh_tempo = worksheet.Cells[5, 10].Value == null ? "" : worksheet.Cells[5, 10].Value.ToString();
                                        string keterangan = worksheet.Cells[5, 11].Value == null ? "" : worksheet.Cells[5, 11].Value.ToString();
                                        string bruto = worksheet.Cells[5, 12].Value == null ? "0" : worksheet.Cells[5, 12].Value.ToString();
                                        string diskon = worksheet.Cells[5, 13].Value == null ? "0" : worksheet.Cells[5, 13].Value.ToString();
                                        string ppn = worksheet.Cells[5, 14].Value == null ? "0" : worksheet.Cells[5, 14].Value.ToString();
                                        string nilai_ppn = worksheet.Cells[5, 15].Value == null ? "0" : worksheet.Cells[5, 15].Value.ToString();
                                        string ongkir = worksheet.Cells[5, 16].Value == null ? "0" : worksheet.Cells[5, 16].Value.ToString();
                                        string netto = worksheet.Cells[5, 17].Value == null ? "0" : worksheet.Cells[5, 17].Value.ToString();
                                        string status_pesanan = worksheet.Cells[5, 18].Value == null ? "" : worksheet.Cells[5, 18].Value.ToString();
                                        string kode_brg = worksheet.Cells[5, 19].Value == null ? "" : worksheet.Cells[5, 19].Value.ToString();
                                        string nama_brg = worksheet.Cells[5, 20].Value == null ? "" : worksheet.Cells[5, 20].Value.ToString();
                                        string qty = worksheet.Cells[5, 21].Value == null ? "0" : worksheet.Cells[5, 21].Value.ToString();
                                        string harga_satuan = worksheet.Cells[5, 22].Value == null ? "0" : worksheet.Cells[5, 22].Value.ToString();
                                        string disc1 = worksheet.Cells[5, 23].Value == null ? "0" : worksheet.Cells[5, 23].Value.ToString();
                                        string ndisc1 = worksheet.Cells[5, 24].Value == null ? "0" : worksheet.Cells[5, 24].Value.ToString();
                                        string disc2 = worksheet.Cells[5, 25].Value == null ? "0" : worksheet.Cells[5, 25].Value.ToString();
                                        string ndisc2 = worksheet.Cells[5, 26].Value == null ? "0" : worksheet.Cells[5, 26].Value.ToString();
                                        string total = worksheet.Cells[5, 27].Value == null ? "0" : worksheet.Cells[5, 27].Value.ToString();

                                        string[] no_cust = marketplace.Split(';');
                                        string[] kurir = kode_kurir.Split(';');

                                        if (!string.IsNullOrEmpty(no_referensi))
                                        {
                                            if (!string.IsNullOrEmpty(marketplace))
                                            {
                                                if (!string.IsNullOrEmpty(kode_kurir))
                                                {
                                                    if (!string.IsNullOrEmpty(kode_brg))
                                                    {
                                                        bruto = bruto.Replace(",", "").Replace(".", "");
                                                        diskon = diskon.Replace(",", "").Replace(".", "");
                                                        ppn = ppn.Replace(",", "").Replace(".", "");
                                                        nilai_ppn = nilai_ppn.Replace(",", "").Replace(".", "");
                                                        ongkir = ongkir.Replace(",", "").Replace(".", "");
                                                        netto = netto.Replace(",", "").Replace(".", "");
                                                        qty = qty.Replace(",", "").Replace(".", "");
                                                        harga_satuan = harga_satuan.Replace(",", "").Replace(".", "");
                                                        disc1 = disc1.Replace(",", "").Replace(".", "");
                                                        ndisc1 = ndisc1.Replace(",", "").Replace(".", "");
                                                        disc2 = disc2.Replace(",", "").Replace(".", "");
                                                        ndisc2 = ndisc2.Replace(",", "").Replace(".", "");
                                                        total = total.Replace(",", "").Replace(".", "");

                                                        TEMP_UPLOADPESANAN newrecordToTemp = new TEMP_UPLOADPESANAN()
                                                        {
                                                            NO_REFERENSI = no_referensi,
                                                            TGL_PESANAN = DateTime.Now.AddHours(7),
                                                            MARKETPLACE = no_cust[0].ToString(),
                                                            NAMA_PEMBELI = nama_pembeli,
                                                            ALAMAT_KIRIM = alamat_kirim,
                                                            KODE_KURIR = kurir[0],
                                                            TOP = Convert.ToInt32(top),
                                                            TGL_JATUH_TEMPO = DateTime.Now.AddHours(7).AddDays(1),
                                                            KETERANGAN = keterangan,
                                                            BRUTO = Convert.ToInt32(bruto),
                                                            DISKON = Convert.ToInt32(diskon),
                                                            PPN = Convert.ToInt32(ppn),
                                                            NILAI_PPN = Convert.ToInt32(nilai_ppn),
                                                            ONGKIR = Convert.ToInt32(ongkir),
                                                            NETTO = Convert.ToInt32(netto),
                                                            STATUS_PESANAN = status_pesanan,
                                                            KODE_BRG = kode_brg,
                                                            NAMA_BRG = nama_brg,
                                                            QTY = Convert.ToInt32(qty),
                                                            HARGA_SATUAN = Convert.ToInt32(harga_satuan),
                                                            DISC1 = Convert.ToInt32(disc1),
                                                            NDISC1 = Convert.ToInt32(ndisc1),
                                                            DISC2 = Convert.ToInt32(disc2),
                                                            NDISC2 = Convert.ToInt32(ndisc2),
                                                            TOTAL = Convert.ToInt32(total)
                                                        };

                                                        batchinsertItem.Add(newrecordToTemp);
                                                        //ret.countAll = ret.countAll + 1;
                                                    }
                                                    else
                                                    {
                                                        //log error masukan log tidak ada barang di DB
                                                    }
                                                }
                                                else
                                                {
                                                    //log error masukan log tidak ada kode kurir 
                                                }
                                            }
                                            else
                                            {
                                                //log error masukan log tidak ada marketplace
                                            }
                                        }
                                        else
                                        {
                                            //log error masukan log tidak ada no referensi
                                        }
                                        
                                    } // end looping

                                    eraDB.TEMP_UPLOADPESANAN.AddRange(batchinsertItem);
                                    eraDB.SaveChanges();
                                    transaction.Commit();
                                }
                            }
                        }
                    }

                    using (ErasoftContext eraDBagain = new ErasoftContext(DataSourcePath, dbPathEra))
                    {
                        using (System.Data.Entity.DbContextTransaction transc = eraDBagain.Database.BeginTransaction())
                        {
                            var NoReffTempUploadPesanan = eraDBagain.TEMP_UPLOADPESANAN.Select(p => p.NO_REFERENSI).ToList();

                            if (NoReffTempUploadPesanan != null)
                            {
                                var noReffDistinct = NoReffTempUploadPesanan.Distinct();

                                var noBuktiSO = "";

                                var dataMasterSTF02 = eraDBagain.STF02.Select(p => new { p.BRG, p.NAMA, p.NAMA2, p.NAMA3 }).ToList();
                                var dataMasterSTF02H = eraDBagain.STF02H.Select(p => new { p.BRG, p.BRG_MP, p.IDMARKET } ).ToList();
                                var dataMasterKurir = MoDbContext.Ekspedisi.ToList();
                                var dataMasterARF01 = eraDBagain.ARF01.ToList();

                                List<SOT01A> batchinsertHeader = new List<SOT01A>();
                                List<SOT01B> batchinsertItemDetail = new List<SOT01B>();

                                batchinsertHeader = new List<SOT01A>();
                                batchinsertItemDetail = new List<SOT01B>();

                                foreach (var itemNoReff in noReffDistinct) // LOOPING UNTUK SEMUA PESANAN
                                {
                                    string connID = Guid.NewGuid().ToString();

                                    if (itemNoReff != null)
                                    {
                                        var dataTempUploadPesananPerReff = eraDBagain.TEMP_UPLOADPESANAN.Where(p => p.NO_REFERENSI == itemNoReff).ToList();

                                        //check no referensi lagi untuk pesanan dengan 1 no ref/ 1 nobuk dan barang lebih dari satu
                                        var checkDuplicateSO = ErasoftDbContext.SOT01A.Where(p => p.NO_REFERENSI == itemNoReff).FirstOrDefault();
                                        if (checkDuplicateSO == null)
                                        {
                                            //if (!string.IsNullOrWhiteSpace(noBuktiSO))
                                            //{
                                            //    transc.Commit();
                                            //}
                                            var lastBukti = new ManageController().GenerateAutoNumber(ErasoftDbContext, "SO", "SOT01A", "NO_BUKTI");
                                            var noOrder = "SO" + DateTime.UtcNow.AddHours(7).Year.ToString().Substring(2, 2) + Convert.ToString(Convert.ToInt32(lastBukti) + 1).PadLeft(6, '0');
                                            noBuktiSO = noOrder;

                                            var alamat_kirim = "";
                                            var no_custMarketplace = "";
                                            var kodeKurir = "";
                                            var namaKurir = "";
                                            var kodePemesan = "";
                                            var namaPemesan = "";
                                            var namaPerso = "";
                                            var diskon = 0;
                                            var bruto = 0;
                                            var netto = 0;
                                            var ndisc1 = 0;
                                            var ndisc2 = 0;
                                            var nilai_ppn = 0;
                                            var total = 0;
                                            var ongkir = 0;

                                            bool statusDetailInsert = true;

                                            foreach (var item in dataTempUploadPesananPerReff) // LOOPING DI DALAM SATU PESANAN
                                            {
                                                if (statusDetailInsert)
                                                {
                                                    if (item != null)
                                                    {
                                                        //var checkBarang = ErasoftDbContext.STF02.Where(p => p.BRG == item.KODE_BRG).Select(p => p.BRG).FirstOrDefault();
                                                        var checkBarang = dataMasterSTF02.Where(p => p.BRG == item.KODE_BRG).FirstOrDefault();
                                                        if (checkBarang != null)
                                                        {
                                                            //var dataKurir = MoDbContext.Ekspedisi.Where(p => p.RecNum == Convert.ToInt32(item.KODE_KURIR)).FirstOrDefault();
                                                            var dataKurir = dataMasterKurir.Where(p => p.RecNum == Convert.ToInt32(item.KODE_KURIR)).FirstOrDefault();
                                                            if (dataKurir != null)
                                                            {
                                                                //var dataToko = ErasoftDbContext.ARF01.Where(p => p.CUST == item.MARKETPLACE).FirstOrDefault();
                                                                var dataToko = dataMasterARF01.Where(p => p.CUST == item.MARKETPLACE).FirstOrDefault();
                                                                if (dataToko != null)
                                                                {
                                                                    //var dataBarang = ErasoftDbContext.STF02H.Where(p => p.BRG == item.KODE_BRG && p.IDMARKET == dataToko.RecNum).FirstOrDefault();
                                                                    var dataBarang = dataMasterSTF02H.Where(p => p.BRG == item.KODE_BRG && p.IDMARKET == dataToko.RecNum).FirstOrDefault();
                                                                    if (dataBarang != null)
                                                                    {
                                                                        string[] brgMPOrderItemID = dataBarang.BRG_MP.Split(';');

                                                                        var kodePembeli = "";
                                                                        var dataPembeli = ErasoftDbContext.ARF01C.Where(p => p.NAMA == item.NAMA_PEMBELI && p.AL == item.ALAMAT_KIRIM).FirstOrDefault();
                                                                        //var dataPembeli = dataMasterARF01C.Where(p => p.NAMA == item.NAMA_PEMBELI && p.AL == item.ALAMAT_KIRIM).FirstOrDefault();
                                                                        string nama = item.NAMA_PEMBELI.Length > 30 ? item.NAMA_PEMBELI.Substring(0, 30) : item.NAMA_PEMBELI.ToString();

                                                                        if (dataPembeli == null)
                                                                        {
                                                                            var connIdARF01C = Guid.NewGuid().ToString();
                                                                            string insertPembeli = "INSERT INTO ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                                                            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                                                            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                                                            var kabKot = "3174";
                                                                            var prov = "31";


                                                                            insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
                                                                                ((nama ?? "").Replace("'", "`")),
                                                                                ((item.ALAMAT_KIRIM ?? "").Replace("'", "`")),
                                                                                 (("").Replace("'", "`")),
                                                                                (dataToko.PERSO.Replace(',', '.')),
                                                                                ((item.ALAMAT_KIRIM ?? "").Replace("'", "`")),
                                                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                                (username),
                                                                                (("-").Replace("'", "`")),
                                                                                kabKot,
                                                                                prov,
                                                                                connIdARF01C
                                                                                );
                                                                            insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                                                                            EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);
                                                                            kodePembeli = ErasoftDbContext.ARF01C.Where(p => p.NAMA == nama).Select(p => p.BUYER_CODE).FirstOrDefault();
                                                                            //kodePembeli = dataMasterARF01C.Where(p => p.NAMA == nama).Select(p => p.BUYER_CODE).FirstOrDefault();
                                                                        }
                                                                        else
                                                                        {
                                                                            kodePembeli = dataPembeli.BUYER_CODE;
                                                                        }

                                                                        //initialize for HEADER
                                                                        alamat_kirim = item.ALAMAT_KIRIM;
                                                                        bruto = Convert.ToInt32(item.BRUTO);
                                                                        no_custMarketplace = item.MARKETPLACE;
                                                                        diskon = Convert.ToInt32(item.DISKON);
                                                                        kodeKurir = dataKurir.RecNum.Value.ToString();
                                                                        namaKurir = dataKurir.NamaEkspedisi;
                                                                        namaPemesan = item.NAMA_PEMBELI;
                                                                        namaPerso = dataToko.PERSO;
                                                                        netto = Convert.ToInt32(item.NETTO);
                                                                        ndisc1 = Convert.ToInt32(item.NDISC1);
                                                                        ndisc2 = Convert.ToInt32(item.NDISC2);
                                                                        nilai_ppn = Convert.ToInt32(item.NILAI_PPN);
                                                                        total = Convert.ToInt32(item.TOTAL);
                                                                        ongkir = Convert.ToInt32(item.ONGKIR);
                                                                        kodePemesan = kodePembeli;

                                                                        var sot01b = new SOT01B
                                                                        {
                                                                            NO_BUKTI = noBuktiSO,
                                                                            BRG = dataBarang.BRG,
                                                                            BRG_CUST = "",
                                                                            SATUAN = "2",
                                                                            H_SATUAN = Convert.ToInt32(item.HARGA_SATUAN),
                                                                            QTY = Convert.ToInt32(item.QTY),
                                                                            DISCOUNT = Convert.ToInt32(item.DISKON),
                                                                            NILAI_DISC = Convert.ToInt32(item.NDISC1) + Convert.ToInt32(item.NDISC2),
                                                                            HARGA = Convert.ToInt32(item.HARGA_SATUAN),
                                                                            WRITE_KONFIG = false,
                                                                            QTY_KIRIM = 0,
                                                                            QTY_RETUR = 0,
                                                                            USER_NAME = "Upload Excel",
                                                                            TGL_INPUT = DateTime.Now.AddHours(7),
                                                                            TGL_KIRIM = null,
                                                                            LOKASI = "001",
                                                                            DISCOUNT_2 = 0,
                                                                            DISCOUNT_3 = 0,
                                                                            DISCOUNT_4 = 0,
                                                                            DISCOUNT_5 = 0,
                                                                            NILAI_DISC_1 = Convert.ToInt32(item.NDISC1),
                                                                            NILAI_DISC_2 = Convert.ToInt32(item.NDISC2),
                                                                            NILAI_DISC_3 = 0,
                                                                            NILAI_DISC_4 = 0,
                                                                            NILAI_DISC_5 = 0,
                                                                            CATATAN = "ORDER NO : " + item.NO_REFERENSI + "_;_" + checkBarang.NAMA + " " + checkBarang.NAMA2 + " " + checkBarang.NAMA3 + "_;_" + dataBarang.BRG_MP,
                                                                            TRANS_NO_URUT = 0,
                                                                            SATUAN_N = 0,
                                                                            QTY_N = Convert.ToInt32(item.QTY),
                                                                            NTITIPAN = 0,
                                                                            DISC_TITIPAN = 0,
                                                                            TOTAL = 0,
                                                                            PPN = Convert.ToInt32(item.NILAI_PPN),
                                                                            NETTO = Convert.ToInt32(item.NETTO),
                                                                            ORDER_ITEM_ID = brgMPOrderItemID[0].ToString(),
                                                                            STATUS_BRG = null,
                                                                            KET_DETAIL = item.KETERANGAN
                                                                        };

                                                                        try
                                                                        {
                                                                            batchinsertItemDetail.Add(sot01b);
                                                                            statusDetailInsert = true;
                                                                            //eraDB.SOT01B.Add(sot01b);
                                                                            //eraDB.SaveChanges();
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            statusDetailInsert = false;
                                                                            // error log terjadi error pada insert detail pesanan
                                                                        }

                                                                        if (ret.percent >= 100 || ret.progress == ret.countAll - 1)
                                                                        {
                                                                            //transaction.Commit();
                                                                            ret.statusSuccess = true;
                                                                            return Json(ret, JsonRequestBehavior.AllowGet);
                                                                        }

                                                                        //Functions.SendProgress("Process in progress...", i, worksheet.Dimension.End.Row);

                                                                    }
                                                                    else
                                                                    {
                                                                        statusDetailInsert = false;
                                                                        // log error masukan ke log tidak ada databarang marketplace di STF02H
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    statusDetailInsert = false;
                                                                    // log error masukan log tidak ada data toko
                                                                }
                                                            }
                                                            else
                                                            {
                                                                statusDetailInsert = false;
                                                                // log error masukan log tidak ada data kurir
                                                            }
                                                        }
                                                        else
                                                        {
                                                            statusDetailInsert = false;
                                                            //log error masukan log tidak ada barang di DB
                                                        }
                                                    }
                                                    else
                                                    {
                                                        statusDetailInsert = false;
                                                    }
                                                }
                                                else
                                                {
                                                    batchinsertItemDetail.Clear();
                                                }
                                            }

                                            if (statusDetailInsert)
                                            {
                                                var checkDuplicateHeader = ErasoftDbContext.SOT01A.Where(p => p.NO_REFERENSI == itemNoReff).FirstOrDefault();
                                                if (checkDuplicateHeader == null)
                                                {
                                                    var sot01a = new SOT01A
                                                    {
                                                        AL = null,
                                                        AL1 = null,
                                                        AL2 = null,
                                                        AL3 = null,
                                                        ALAMAT_KIRIM = alamat_kirim,
                                                        AL_CUST = "",
                                                        BRUTO = Convert.ToInt32(bruto),
                                                        CUST = no_custMarketplace,
                                                        CUST_QQ = "",
                                                        DISCOUNT = Convert.ToInt32(diskon),
                                                        Date_Approve = null,
                                                        EXPEDISI = kodeKurir,
                                                        HARGA_FRANCO = "0",
                                                        INDENT = false,
                                                        JAMKIRIM = null,
                                                        KET = null,
                                                        KIRIM_PENUH = false,
                                                        KODE_ALAMAT = "",
                                                        KODE_POS = null,
                                                        KODE_SALES = "",
                                                        KODE_WIL = "",
                                                        KOMISI = 0,
                                                        KOTA = null,
                                                        NAMAPEMESAN = namaPemesan,
                                                        NAMAPENGIRIM = null,
                                                        NAMA_CUST = namaPerso,
                                                        NETTO = Convert.ToInt32(netto),
                                                        NILAI_DISC = Convert.ToInt32(ndisc1) + Convert.ToInt32(ndisc2),
                                                        NILAI_PPN = Convert.ToInt32(nilai_ppn),
                                                        NILAI_TUKAR = 1,
                                                        NO_BUKTI = noBuktiSO,
                                                        NO_PENAWARAN = "",
                                                        NO_PO_CUST = "",
                                                        NO_REFERENSI = itemNoReff,
                                                        N_KOMISI = 0,
                                                        N_KOMISI1 = 0,
                                                        N_UCAPAN = "",
                                                        ONGKOS_KIRIM = Convert.ToInt32(ongkir),
                                                        PEMESAN = kodePemesan,
                                                        PENGIRIM = null,
                                                        PPN = Convert.ToInt32(nilai_ppn),
                                                        PRINT_COUNT = 0,
                                                        PROPINSI = null,
                                                        RETUR_PENUH = false,
                                                        RecNum = null,
                                                        SHIPMENT = namaKurir,
                                                        SOT01D = null,
                                                        STATUS = "0",
                                                        STATUS_TRANSAKSI = "01",
                                                        SUPP = "0",
                                                        Status_Approve = "",
                                                        TERM = 10,
                                                        TGL = DateTime.Now.AddHours(7),
                                                        TGL_INPUT = DateTime.Now.AddHours(7),
                                                        TGL_JTH_TEMPO = DateTime.Now.AddHours(7).AddDays(1),
                                                        TGL_KIRIM = null,
                                                        TIPE_KIRIM = 0,
                                                        TOTAL_SEMUA = Convert.ToInt32(total),
                                                        TOTAL_TITIPAN = 0,
                                                        TRACKING_SHIPMENT = null,
                                                        UCAPAN = "",
                                                        USER_NAME = "Upload Excel",
                                                        U_MUKA = 0,
                                                        VLT = "",
                                                        ZONA = "",
                                                        status_kirim = "0",
                                                        status_print = "0"
                                                    };

                                                    try
                                                    {
                                                        batchinsertHeader.Add(sot01a);

                                                        eraDBagain.SOT01A.AddRange(batchinsertHeader);
                                                        eraDBagain.SOT01B.AddRange(batchinsertItemDetail);
                                                        eraDBagain.SaveChanges();
                                                        batchinsertHeader.Clear();
                                                        batchinsertItemDetail.Clear();

                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        // error log terjadi error pada insert header pesanan
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            return Json(ret, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //add by Tri 28 okt 2019, tuning upload excel sinkronisasi barang
        public ActionResult UploadXcelwithPage(int page)
        {
            //var file = Request.Files[0];
            //List<string> excelData = new List<string>();
            //var listCust = new List<string>();
            BindUploadExcel ret = new BindUploadExcel();
            ret.Errors = new List<string>();
            ret.cust = new List<string>();
            ret.namaCust = new List<string>();
            ret.lastRow = new List<int>();
            ret.nextFile = false;
            try
            {
                if (Request.Files.Count > 0)
                {
                    var mp = MoDbContext.Marketplaces.ToList();
                    for (int file_index = 0; file_index < Request.Files.Count; file_index++)
                    {
                        var file = Request.Files[file_index];
                        if (file != null && file.ContentLength > 0)
                        {
                            byte[] data;
                            ret.lastRow.Add(0);
                            using (Stream inputStream = file.InputStream)
                            {
                                MemoryStream memoryStream = inputStream as MemoryStream;
                                if (memoryStream == null)
                                {
                                    memoryStream = new MemoryStream();
                                    inputStream.CopyTo(memoryStream);
                                }
                                data = memoryStream.ToArray();
                            }

                            using (MemoryStream stream = new MemoryStream(data))
                            {
                                using (ExcelPackage excelPackage = new ExcelPackage(stream))
                                {
                                    using (ErasoftContext eraDB = new ErasoftContext(DataSourcePath, dbPathEra))
                                    {
                                        eraDB.Database.CommandTimeout = 180;
                                        //loop all worksheets
                                        var worksheet = excelPackage.Workbook.Worksheets[1];
                                        //foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                                        //{
                                        string cust = worksheet.Cells[1, 2].Value == null ? "" : worksheet.Cells[1, 2].Value.ToString();
                                        if (!string.IsNullOrEmpty(cust))
                                        {
                                            var customer = eraDB.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                                            if (customer != null)
                                            {
                                                if (page == 0)
                                                {
                                                    var resetFlag = EDB.ExecuteSQL("CString", System.Data.CommandType.Text, "update temp_brg_mp set avalue_36 = '' where cust = '" + cust + "' and avalue_36 like 'auto%'");
                                                }
                                                string namaMP = mp.Where(m => m.IdMarket.ToString() == customer.NAMA).SingleOrDefault().NamaMarket;
                                                ret.cust.Add(cust);
                                                ret.namaCust.Add(namaMP + "(" + customer.PERSO + ")");
                                                int dataPerPage = 300;
                                                int maxData = 19 + (page * dataPerPage) + dataPerPage;
                                                if (19 + (page * dataPerPage) + dataPerPage >= worksheet.Dimension.End.Row)
                                                {
                                                    ret.nextFile = true;
                                                    maxData = worksheet.Dimension.End.Row;
                                                }
                                                var listCurrentBrg = new List<string>();
                                                for (int j = 19 + (page * dataPerPage); j <= maxData; j++)
                                                {
                                                    var brg_mp = worksheet.Cells[j, 11].Value == null ? "" : worksheet.Cells[j, 11].Value.ToString();
                                                    if (!string.IsNullOrEmpty(brg_mp))
                                                    {
                                                        //if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[j, 3].Value)))
                                                        //{
                                                        listCurrentBrg.Add(brg_mp);
                                                        //}
                                                    }
                                                }
                                                //var listTemp = eraDB.TEMP_BRG_MP.Where(m => m.CUST == cust).ToList();
                                                var listTemp = eraDB.TEMP_BRG_MP.Where(m => m.CUST == cust && listCurrentBrg.Contains(m.BRG_MP)).ToList();
                                                if (listTemp.Count > 0)
                                                {
                                                    //loop all rows
                                                    //for (int i = 19; i <= worksheet.Dimension.End.Row; i++)
                                                    for (int i = 19 + (page * dataPerPage); i <= maxData; i++)
                                                    {
                                                        var kd_brg_mp = worksheet.Cells[i, 11].Value == null ? "" : worksheet.Cells[i, 11].Value.ToString();
                                                        if (!string.IsNullOrEmpty(kd_brg_mp))
                                                        {
                                                            ////loop all columns in a row
                                                            //for (int j = 1; j <= worksheet.Dimension.End.Column; j++)
                                                            //{
                                                            //    //add the cell data to the List
                                                            //    if (worksheet.Cells[i, j].Value != null)
                                                            //    {
                                                            //        excelData.Add(worksheet.Cells[i, j].Value.ToString());
                                                            //    }
                                                            //}
                                                            var current_brg = listTemp.Where(m => m.BRG_MP == kd_brg_mp).SingleOrDefault();
                                                            if (current_brg != null)
                                                            {
                                                                if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 3].Value))) //user tidak isi kode barang mo, tidak perlu update  
                                                                {
                                                                    if (!string.IsNullOrEmpty(current_brg.KODE_BRG_INDUK) && string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 4].Value)))
                                                                    {
                                                                        // barang varian tapi tidak diisi kode brg induk di excel
                                                                        //break;
                                                                    }
                                                                    else
                                                                    {
                                                                        //if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 1].Value)))
                                                                        //    current_brg.NAMA = worksheet.Cells[i, 1].Value.ToString();
                                                                        //current_brg.NAMA2 = worksheet.Cells[i, 2].Value == null ? "" : worksheet.Cells[i, 2].Value.ToString();
                                                                        current_brg.SELLER_SKU = worksheet.Cells[i, 3].Value == null ? "" : worksheet.Cells[i, 3].Value.ToString();
                                                                        current_brg.KODE_BRG_INDUK = worksheet.Cells[i, 4].Value == null ? "" : worksheet.Cells[i, 4].Value.ToString();
                                                                        //change 14 juni 2019, kode kategori mo disimpan di avalue_40, kode kategory mp tetap di category_code
                                                                        //current_brg.CATEGORY_CODE = worksheet.Cells[i, 5].Value == null ? "" : worksheet.Cells[i, 5].Value.ToString();
                                                                        current_brg.AVALUE_40 = worksheet.Cells[i, 5].Value == null ? "" : worksheet.Cells[i, 5].Value.ToString();
                                                                        //end change 14 juni 2019, kode kategori mo disimpan di avalue_40, kode kategory mp tetap di category_code
                                                                        current_brg.MEREK = worksheet.Cells[i, 6].Value == null ? "" : worksheet.Cells[i, 6].Value.ToString();
                                                                        //current_brg.HJUAL_MP = Convert.ToDouble(worksheet.Cells[i, 7].Value == null ? "0" : worksheet.Cells[i, 7].Value.ToString());
                                                                        current_brg.HJUAL = Convert.ToDouble(worksheet.Cells[i, 7].Value == null ? "0" : worksheet.Cells[i, 7].Value.ToString());
                                                                        //current_brg.BERAT = Convert.ToDouble(worksheet.Cells[i, 9].Value == null ? "0" : worksheet.Cells[i, 9].Value.ToString());
                                                                        //current_brg.IMAGE = worksheet.Cells[i, 10].Value == null ? "" : worksheet.Cells[i, 10].Value.ToString();
                                                                        current_brg.AVALUE_36 = "Auto Process";// barang yg akan di transfer ke master hasil upload excel saja
                                                                        try
                                                                        {
                                                                            eraDB.SaveChanges();
                                                                        }
                                                                        catch (Exception ex)
                                                                        {

                                                                        }
                                                                    }

                                                                }
                                                            }
                                                            else
                                                            {
                                                                ret.Errors.Add(namaMP + "(" + customer.PERSO + ") : Kode Barang MP (" + kd_brg_mp + ") tidak ditemukan");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ret.nextFile = true;
                                                            ret.Errors.Add(namaMP + "(" + customer.PERSO + ") : Kode barang marketplace tidak ditemukan lagi di baris " + i);
                                                            ret.lastRow[file_index] = i;
                                                            break;
                                                        }
                                                    }
                                                    if (ret.lastRow[file_index] == 0)
                                                        ret.lastRow[file_index] = worksheet.Dimension.End.Row;
                                                }
                                                else
                                                {
                                                    //var mp = MoDbContext.Marketplaces.Where(m => m.IdMarket.ToString() == customer.NAMA).FirstOrDefault();
                                                    //ret.Errors.Add("Data barang untuk akun " + mp.NamaMarket + "(" + customer.PERSO + ") tidak ditemukan");
                                                    //remark by Tri 21 Nov 2019
                                                    //ret.Errors.Add(namaMP + "(" + customer.PERSO + ") : Data Barang untuk akun ini tidak ditemukan");
                                                    //ret.nextFile = true;
                                                    //end remark by Tri 21 Nov 2019
                                                }
                                            }
                                            else
                                            {
                                                //customer not found
                                                ret.Errors.Add("File " + file.FileName + ": Akun marketplace tidak ditemukan");
                                                ret.nextFile = true;
                                            }
                                        }
                                        else
                                        {
                                            //cust empty
                                            ret.Errors.Add("File " + file.FileName + ": Kode akun marketplace tidak ditemukan");
                                            ret.nextFile = true;
                                        }
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    ret.nextFile = true;
                }
            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
                ret.nextFile = true;
            }

            return Json(ret, JsonRequestBehavior.AllowGet);
        }
        //end by Tri add 28 okt 2019, tuning upload excel sinkronisasi barang

        //add by Indra 01 apr 2020, download pesanan
        public ActionResult ListPesanantoExcel(string orid, string drtgl, string sdtgl)
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };

            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("PESANAN");

                    string dt1 = DateTime.ParseExact(drtgl, "dd'/'MM'/'yyyy", CultureInfo.InvariantCulture).ToString("yyyy'-'MM'-'dd 00:00:00.000");
                    string dt2 = DateTime.ParseExact(sdtgl, "dd'/'MM'/'yyyy", CultureInfo.InvariantCulture).ToString("yyyy'-'MM'-'dd 23:59:59.999");

                    var pesanan = "";
                    switch (orid)
                    {
                        case "ALL": pesanan = "SEMUA PESANAN"; break;
                        case "0": pesanan = "BELUM BAYAR"; break;
                        case "01": pesanan = "SUDAH BAYAR"; break;
                        case "02": pesanan = "PACKING"; break;
                        case "03": pesanan = "FAKTUR"; break;
                        case "04": pesanan = "SELESAI"; break;
                        case "11": pesanan = "BATAL"; break;
                    }

                    string sSQL = "SELECT A.NO_BUKTI AS NO_PESANAN, NO_REFERENSI, A.TGL, E.NAMAMARKET + '(' + D.PERSO + ')' MARKETPLACE, A.CUST AS KODE_PEMBELI, NAMAPEMESAN AS PEMBELI, " +
                        "ALAMAT_KIRIM, A.TERM AS [TOP],  SHIPMENT AS KURIR, TGL_JTH_TEMPO AS TGL_JATUH_TEMPO, A.KET AS KETERANGAN, A.BRUTO, A.DISCOUNT AS DISC, A.PPN, A.NILAI_PPN, A.ONGKOS_KIRIM, A.NETTO, " +
                        "A.STATUS_TRANSAKSI AS STATUS_PESANAN, B.BRG AS KODE_BRG, ISNULL(C.NAMA,'') + ' ' + ISNULL(C.NAMA2, '') AS NAMA_BARANG, QTY, " +
                        "H_SATUAN AS HARGA_SATUAN, B.DISCOUNT AS DISC1, B.NILAI_DISC_1 AS NDISC1, B.DISCOUNT_2 AS DISC2, B.NILAI_DISC_2 AS NDISC2, HARGA AS TOTAL " +
                        "FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI " +
                        "LEFT JOIN STF02 C ON B.BRG = C.BRG " +
                        "INNER JOIN ARF01 D ON A.CUST = D.CUST " +
                        "INNER JOIN MO..MARKETPLACE E ON D.NAMA = E.IDMARKET " +
                        "WHERE A.TGL BETWEEN '" + dt1 + "' AND '" + dt2 + "'";

                    if (orid != "ALL")
                    {
                        sSQL += "AND A.STATUS_TRANSAKSI = '" + orid + "'";
                    }

                    sSQL += "ORDER BY A.TGL DESC, A.NO_BUKTI DESC";

                    var lsPesanan = EDB.GetDataSet("CString", "SOT01A", sSQL);

                    if (lsPesanan.Tables[0].Rows.Count > 0)
                    {

                        worksheet.Cells["A1"].Value = "Pesanan : " + pesanan;
                        worksheet.Cells["A2"].Value = "Dari Tanggal : " + drtgl + " Sampai Tanggal : " + sdtgl;

                        for (int i = 0; i < lsPesanan.Tables[0].Rows.Count; i++)
                        {
                            worksheet.Cells[5 + i, 1].Value = lsPesanan.Tables[0].Rows[i]["NO_PESANAN"];
                            worksheet.Cells[5 + i, 2].Value = lsPesanan.Tables[0].Rows[i]["NO_REFERENSI"];
                            worksheet.Cells[5 + i, 3].Value = Convert.ToDateTime(lsPesanan.Tables[0].Rows[i]["TGL"]).ToString("yyyy-MM-dd HH:mm:ss");
                            worksheet.Cells[5 + i, 4].Value = lsPesanan.Tables[0].Rows[i]["MARKETPLACE"];
                            worksheet.Cells[5 + i, 5].Value = lsPesanan.Tables[0].Rows[i]["KODE_PEMBELI"];
                            worksheet.Cells[5 + i, 6].Value = lsPesanan.Tables[0].Rows[i]["PEMBELI"];
                            worksheet.Cells[5 + i, 7].Value = lsPesanan.Tables[0].Rows[i]["ALAMAT_KIRIM"];
                            worksheet.Cells[5 + i, 8].Value = lsPesanan.Tables[0].Rows[i]["KURIR"];
                            worksheet.Cells[5 + i, 9].Value = lsPesanan.Tables[0].Rows[i]["TOP"];
                            worksheet.Cells[5 + i, 10].Value = Convert.ToDateTime(lsPesanan.Tables[0].Rows[i]["TGL_JATUH_TEMPO"]).ToString("yyyy-MM-dd HH:mm:ss");
                            worksheet.Cells[5 + i, 11].Value = lsPesanan.Tables[0].Rows[i]["KETERANGAN"];
                            worksheet.Cells[5 + i, 12].Value = lsPesanan.Tables[0].Rows[i]["BRUTO"];
                            worksheet.Cells[5 + i, 13].Value = lsPesanan.Tables[0].Rows[i]["DISC"];
                            worksheet.Cells[5 + i, 14].Value = lsPesanan.Tables[0].Rows[i]["PPN"];
                            worksheet.Cells[5 + i, 15].Value = lsPesanan.Tables[0].Rows[i]["NILAI_PPN"];
                            worksheet.Cells[5 + i, 16].Value = lsPesanan.Tables[0].Rows[i]["ONGKOS_KIRIM"];
                            worksheet.Cells[5 + i, 17].Value = lsPesanan.Tables[0].Rows[i]["NETTO"];
                            var pesanan1 = "";
                            switch (lsPesanan.Tables[0].Rows[i]["STATUS_PESANAN"])
                            {
                                case "0": pesanan1 = "BELUM BAYAR"; break;
                                case "01": pesanan1 = "SUDAH BAYAR"; break;
                                case "02": pesanan1 = "PACKING"; break;
                                case "03": pesanan1 = "FAKTUR"; break;
                                case "04": pesanan1 = "SELESAI"; break;
                                case "11": pesanan1 = "BATAL"; break;
                            }
                            worksheet.Cells[5 + i, 18].Value = pesanan1;
                            worksheet.Cells[5 + i, 19].Value = lsPesanan.Tables[0].Rows[i]["KODE_BRG"];
                            worksheet.Cells[5 + i, 20].Value = lsPesanan.Tables[0].Rows[i]["NAMA_BARANG"];
                            worksheet.Cells[5 + i, 21].Value = lsPesanan.Tables[0].Rows[i]["QTY"];
                            worksheet.Cells[5 + i, 22].Value = lsPesanan.Tables[0].Rows[i]["HARGA_SATUAN"];
                            worksheet.Cells[5 + i, 23].Value = lsPesanan.Tables[0].Rows[i]["DISC1"];
                            worksheet.Cells[5 + i, 24].Value = lsPesanan.Tables[0].Rows[i]["NDISC1"];
                            worksheet.Cells[5 + i, 25].Value = lsPesanan.Tables[0].Rows[i]["DISC2"];
                            worksheet.Cells[5 + i, 26].Value = lsPesanan.Tables[0].Rows[i]["NDISC2"];
                            worksheet.Cells[5 + i, 27].Value = lsPesanan.Tables[0].Rows[i]["TOTAL"];
                        }

                        ExcelRange rg0 = worksheet.Cells[4, 1, worksheet.Dimension.End.Row, 27];
                        string tableName0 = "TablePesanan";
                        ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);

                        table0.Columns[0].Name = "NO PESANAN";
                        table0.Columns[1].Name = "NO REFERENSI";
                        table0.Columns[2].Name = "TGL";
                        table0.Columns[3].Name = "MARKETPLACE";
                        table0.Columns[4].Name = "KODE PEMBELI";
                        table0.Columns[5].Name = "PEMBELI";
                        table0.Columns[6].Name = "ALAMAT KIRIM";
                        table0.Columns[7].Name = "KURIR";
                        table0.Columns[8].Name = "TOP";
                        table0.Columns[9].Name = "TGL JATUH TEMPO";
                        table0.Columns[10].Name = "KETERANGAN";
                        table0.Columns[11].Name = "BRUTO";
                        table0.Columns[12].Name = "DISC";
                        table0.Columns[13].Name = "PPN";
                        table0.Columns[14].Name = "NILAI PPN";
                        table0.Columns[15].Name = "ONGKOS KIRIM";
                        table0.Columns[16].Name = "NETTO";
                        table0.Columns[17].Name = "STATUS PESANAN";
                        table0.Columns[18].Name = "KODE BRG";
                        table0.Columns[19].Name = "NAMA BARANG";
                        table0.Columns[20].Name = "QTY";
                        table0.Columns[21].Name = "HARGA SATUAN";
                        table0.Columns[22].Name = "DISC1";
                        table0.Columns[23].Name = "NDISC1";
                        table0.Columns[24].Name = "DISC2";
                        table0.Columns[25].Name = "NDISC2";
                        table0.Columns[26].Name = "TOTAL";

                        using (var range = worksheet.Cells[4, 1, 4, 27])
                        {
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }

                        table0.ShowHeader = true;
                        table0.ShowFilter = true;
                        table0.ShowRowStripes = false;
                        worksheet.Cells.AutoFitColumns(0);

                        ret.byteExcel = package.GetAsByteArray();
                        ret.namaFile = username + "_pesanan_" + pesanan + ".xlsx";
                    }
                    else
                    {
                        ret.Errors.Add("Tidak ada data pesanan");
                    }

                }

            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;

            var result = new ContentResult
            {
                Content = serializer.Serialize(ret),
                ContentType = "application/json"
            };

            return result;

        }
        //end by Indra 01 apr 2020, download pesanan

        //add by fauzi 20 apr 2020, download pesanan example template for upload pesanan all
        public ActionResult ExampleTemplatePesanantoExcel()
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };

            try
            {
                var dateNow = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                var dateTGLTempo = DateTime.UtcNow.AddDays(3).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("PESANAN");
                    // SHEET 1
                    worksheet.Cells["A1"].Value = "Pesanan : SUDAH BAYAR";

                    for (int i = 0; i < 3; i++)
                    {
                        worksheet.Cells[5 + i, 1].Value = "-"; //NO_PESANAN
                        worksheet.Cells[5 + i, 2].Value = "488922301;INV/20200427/XX/IV/530482181" + i; //NO_REFERENSI
                        worksheet.Cells[5 + i, 3].Value = dateNow; //TGL 
                        worksheet.Cells[5 + i, 4].Value = "-- Silahkan Pilih Marketplace --"; //MARKETPLACE
                        //worksheet.Cells[5 + i, 5].Value = "001028"; //KODE_PEMBELI
                        worksheet.Cells[5 + i, 5].Value = "Dani"; //PEMBELI
                        worksheet.Cells[5 + i, 6].Value = "Jl. Alaydrus No.37, RT.8/RW.2, Petojo Utara, Kecamatan Gambir, Kota Jakarta Pusat, Daerah Khusus Ibukota Jakarta 10130"; //ALAMAT_KIRIM
                        worksheet.Cells[5 + i, 7].Value = "-- Silahkan Pilih Kurir --"; //KURIR
                        worksheet.Cells[5 + i, 8].Value = "10"; //TOP
                        worksheet.Cells[5 + i, 9].Value = dateTGLTempo; //TGL_JATUH_TEMPO
                        worksheet.Cells[5 + i, 10].Value = ""; //KETERANGAN
                        worksheet.Cells[5 + i, 11].Value = 10000; //BRUTO
                        worksheet.Cells[5 + i, 12].Value = 0; //DISC
                        worksheet.Cells[5 + i, 13].Value = 0; //PPN
                        worksheet.Cells[5 + i, 14].Value = 0; //NILAI_PPN
                        worksheet.Cells[5 + i, 15].Value = 5000; //ONGKOS_KIRIM
                        worksheet.Cells[5 + i, 16].Value = 15000; //NETTO
                        worksheet.Cells[5 + i, 17].Value = "SUDAH BAYAR"; //STATUS_PESANAN
                        worksheet.Cells[5 + i, 18].Value = "INDHNS"; //KODE_BRG
                        worksheet.Cells[5 + i, 19].Value = "Indomie Goreng Hot & Spicy"; //NAMA_BARANG
                        worksheet.Cells[5 + i, 20].Value = 1; //QTY
                        worksheet.Cells[5 + i, 21].Value = 10000; //HARGA_SATUAN
                        worksheet.Cells[5 + i, 22].Value = 0; //DISC1
                        worksheet.Cells[5 + i, 23].Value = 0; //NDISC1
                        worksheet.Cells[5 + i, 24].Value = 0; //DISC2
                        worksheet.Cells[5 + i, 25].Value = 0; //NDISC2
                        worksheet.Cells[5 + i, 26].Value = 10000; //TOTAL
                    }

                    ExcelRange rg0 = worksheet.Cells[4, 1, worksheet.Dimension.End.Row, 27];
                    string tableName0 = "TablePesanan";
                    ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);

                    table0.Columns[0].Name = "NO PESANAN";
                    table0.Columns[1].Name = "NO REFERENSI";
                    table0.Columns[2].Name = "TGL";
                    table0.Columns[3].Name = "MARKETPLACE";
                    //table0.Columns[4].Name = "KODE PEMBELI";
                    table0.Columns[4].Name = "PEMBELI";
                    table0.Columns[5].Name = "ALAMAT KIRIM";
                    table0.Columns[6].Name = "KURIR";
                    table0.Columns[7].Name = "TOP";
                    table0.Columns[8].Name = "TGL JATUH TEMPO";
                    table0.Columns[9].Name = "KETERANGAN";
                    table0.Columns[10].Name = "BRUTO";
                    table0.Columns[11].Name = "DISC";
                    table0.Columns[12].Name = "PPN";
                    table0.Columns[13].Name = "NILAI PPN";
                    table0.Columns[14].Name = "ONGKOS KIRIM";
                    table0.Columns[15].Name = "NETTO";
                    table0.Columns[16].Name = "STATUS PESANAN";
                    table0.Columns[17].Name = "KODE BRG";
                    table0.Columns[18].Name = "NAMA BARANG";
                    table0.Columns[19].Name = "QTY";
                    table0.Columns[20].Name = "HARGA SATUAN";
                    table0.Columns[21].Name = "DISC1";
                    table0.Columns[22].Name = "NDISC1";
                    table0.Columns[23].Name = "DISC2";
                    table0.Columns[24].Name = "NDISC2";
                    table0.Columns[25].Name = "TOTAL";

                    using (var range = worksheet.Cells[4, 1, 4, 27])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    table0.ShowHeader = true;
                    table0.ShowFilter = true;
                    table0.ShowRowStripes = false;
                    worksheet.Cells.AutoFitColumns(0);
                    //END SHEET 1


                    // SHEET 2
                    var sheet2 = worksheet.Workbook.Worksheets.Add("master_marketplace_kurir");

                    sheet2.Cells[2, 1].Value = "MARKETPLACES";
                    sheet2.Cells[2, 6].Value = "EXPEDITIONS";

                    // MARKETPLACES
                    var sSQL = "SELECT ISNULL(A.CUST, '') AS KODE_MP, ISNULL(B.NAMAMARKET, '') + '(' + ISNULL(A.PERSO, '') + ')' AS MARKETPLACE " +
                        "FROM ARF01 A " +
                        "LEFT JOIN MO..MARKETPLACE B ON A.NAMA = B.IDMARKET";

                    var resultMarketplace = EDB.GetDataSet("CString", "ARF01", sSQL);
                    if (resultMarketplace.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < resultMarketplace.Tables[0].Rows.Count; j++)
                        {
                            sheet2.Cells[4 + j, 1].Value = resultMarketplace.Tables[0].Rows[j]["KODE_MP"];
                            sheet2.Cells[4 + j, 2].Value = resultMarketplace.Tables[0].Rows[j]["KODE_MP"] + ";" + resultMarketplace.Tables[0].Rows[j]["MARKETPLACE"];
                        }
                    }

                    var validation = worksheet.DataValidations.AddListValidation(worksheet.Cells[5, 4, worksheet.Dimension.End.Row, 4].Address);
                    validation.ShowErrorMessage = true;
                    validation.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                    validation.ErrorTitle = "An invalid value was entered";
                    validation.Formula.ExcelFormula = string.Format("=master_marketplace_kurir!${0}${1}:${2}${3}", "B", 4, "B", 3 + resultMarketplace.Tables[0].Rows.Count);

                    using (var range = sheet2.Cells[3, 1, 3, 2])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                    }

                    ExcelRange rg = sheet2.Cells[3, 1, worksheet.Dimension.End.Row, 2];
                    string tableName = "TableMarketplace";
                    ExcelTable table = sheet2.Tables.Add(rg, tableName);
                    table.Columns[0].Name = "KODE_MP";
                    table.Columns[1].Name = "MARKETPLACE";
                    table.ShowHeader = true;
                    table.ShowFilter = true;
                    table.ShowRowStripes = false;
                    // END MARKETPLACES

                    // EXPEDITIONS
                    var dataKurir = MoDbContext.Ekspedisi.ToList();
                    if (dataKurir != null)
                    {
                        var j = 0;
                        foreach(var itemKurir in dataKurir)
                        {
                            sheet2.Cells[4 + j, 6].Value = itemKurir.RecNum;
                            sheet2.Cells[4 + j, 7].Value = itemKurir.NamaEkspedisi;
                            j += 1;
                        }
                    }

                    var validation2 = worksheet.DataValidations.AddListValidation(worksheet.Cells[5, 7, worksheet.Dimension.End.Row, 7].Address);
                    validation2.ShowErrorMessage = true;
                    validation2.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                    validation2.ErrorTitle = "An invalid value was entered";
                    validation2.Formula.ExcelFormula = string.Format("=master_marketplace_kurir!${0}${1}:${2}${3}", "G", 4, "G", 3 + dataKurir.Count());

                    using (var range = sheet2.Cells[3, 6, 3, 7])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                    }

                    ExcelRange rg2 = sheet2.Cells[3, 6, worksheet.Dimension.End.Row, 7];
                    string tableName2 = "TableExpeditions";
                    ExcelTable table2 = sheet2.Tables.Add(rg2, tableName2);
                    table2.Columns[0].Name = "KODE KURIR";
                    table2.Columns[1].Name = "NAMA KURIR";
                    table2.ShowHeader = true;
                    table2.ShowFilter = true;
                    table2.ShowRowStripes = false;
                    //END EXPEDITIONS

                    ret.byteExcel = package.GetAsByteArray();
                    ret.namaFile = username + "_template_upload_pesanan.xlsx";

                }

            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;

            var result = new ContentResult
            {
                Content = serializer.Serialize(ret),
                ContentType = "application/json"
            };

            return result;

        }
        //end add by fauzi

        //add by Indra 03 apr 2020, download faktur
        public ActionResult ListFakturtoExcel(string drtgl, string sdtgl)
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };

            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("FAKTUR");

                    string dt1 = DateTime.ParseExact(drtgl, "dd'/'MM'/'yyyy", CultureInfo.InvariantCulture).ToString("yyyy'-'MM'-'dd 00:00:00.000");
                    string dt2 = DateTime.ParseExact(sdtgl, "dd'/'MM'/'yyyy", CultureInfo.InvariantCulture).ToString("yyyy'-'MM'-'dd 23:59:59.999");

                    string sSQL = "SELECT ISNULL(A.NO_BUKTI,'') AS NO_FAKTUR, A.TGL AS TGL_FAKTUR, A.STATUS AS STATUS_FAKTUR, " +
                        "ISNULL(D.NO_BUKTI,'') AS NO_PESANAN, ISNULL(A.NO_REF, '') AS NO_REFERENSI, ISNULL(D.TGL, '') AS TGL_PESANAN, " +
                        "C.NAMAMARKET + '(' + B.PERSO + ')' MARKETPLACE, ISNULL(A.CUST, '') AS KODE_PEMBELI, ISNULL(A.NAMAPEMESAN, '') AS PEMBELI, " +
                        "ISNULL(A.AL, '') AS ALAMAT_KIRIM, ISNULL(A.TERM, '') AS [TOP], ISNULL(A.NAMAPENGIRIM, '') AS KURIR, ISNULL(A.TGL_JT_TEMPO, '') AS TGL_JATUH_TEMPO, " +
                        "ISNULL(D.KET, '') AS KETERANGAN, ISNULL(A.BRUTO, '') AS BRUTO, ISNULL(A.DISCOUNT,'') AS DISC, ISNULL(A.PPN, '') AS PPN, ISNULL(A.NILAI_PPN, '') AS NILAI_PPN, " +
                        "ISNULL(D.ONGKOS_KIRIM, '') AS ONGKOS_KIRIM, ISNULL(A.NETTO, '') AS NETTO, ISNULL(D.STATUS_TRANSAKSI, '') AS STATUS_PESANAN, " +
                        "ISNULL(G.BRG, '') AS KODE_BRG, ISNULL(H.NAMA,'') + ' ' + ISNULL(H.NAMA2, '') AS NAMA_BARANG, ISNULL(QTY, '') AS QTY, " +
                        "ISNULL(H_SATUAN, '') AS HARGA_SATUAN, ISNULL(G.DISCOUNT, '') AS DISC1, ISNULL(G.NILAI_DISC_1, '') AS NDISC1, " +
                        "ISNULL(G.DISCOUNT_2, '') AS DISC2, ISNULL(G.NILAI_DISC_2, '') AS NDISC2, ISNULL(HARGA, '') AS TOTAL " +
                        "FROM SIT01A A LEFT JOIN ARF01 B ON A.CUST = B.CUST " +
                        "LEFT JOIN MO.dbo.MARKETPLACE C ON B.NAMA = C.IdMarket " +
                        "LEFT JOIN SOT01A D ON A.NO_SO = D.NO_BUKTI " +
                        "LEFT JOIN SIT01B G ON A.NO_BUKTI = G.NO_BUKTI " +
                        "LEFT JOIN STF02 H ON G.BRG = H.BRG " +
                        "LEFT JOIN (SELECT DISTINCT NO_BUKTI FROM SIT01A A INNER JOIN ART03B B ON A.NO_BUKTI = B.NFAKTUR)E ON A.NO_BUKTI = E.NO_BUKTI " +
                        "LEFT JOIN (select ret.jenis_form,ret.no_bukti as bukti_ret,ret.no_ref as no_si,fkt.no_bukti as bukti_faktur from sit01a ret inner join sit01a fkt on fkt.no_bukti=ret.no_ref where ret.jenis_form='3') F ON A.NO_BUKTI=F.BUKTI_FAKTUR " +
                        "WHERE A.TGL BETWEEN '" + dt1 + "' AND '" + dt2 + "' " +
                        "AND A.JENIS_FORM = '2' " +
                        "ORDER BY A.TGL DESC, A.NO_BUKTI DESC";

                    var lsFaktur = EDB.GetDataSet("CString", "SIT01A", sSQL);

                    if (lsFaktur.Tables[0].Rows.Count > 0)
                    {

                        worksheet.Cells["A1"].Value = "FAKTUR PENJUALAN";
                        worksheet.Cells["A2"].Value = "Dari Tanggal : " + drtgl + " Sampai Tanggal : " + sdtgl;

                        for (int i = 0; i < lsFaktur.Tables[0].Rows.Count; i++)
                        {
                            worksheet.Cells[5 + i, 1].Value = lsFaktur.Tables[0].Rows[i]["NO_FAKTUR"];
                            worksheet.Cells[5 + i, 2].Value = Convert.ToDateTime(lsFaktur.Tables[0].Rows[i]["TGL_FAKTUR"]).ToString("yyyy-MM-dd HH:mm:ss");
                            var status1 = "";
                            switch (lsFaktur.Tables[0].Rows[i]["STATUS_FAKTUR"])
                            {
                                case "1": status1 = "SELESAI"; break;
                                case "2": status1 = "BATAL"; break;
                            }
                            worksheet.Cells[5 + i, 3].Value = status1;
                            worksheet.Cells[5 + i, 4].Value = lsFaktur.Tables[0].Rows[i]["NO_PESANAN"];
                            worksheet.Cells[5 + i, 5].Value = lsFaktur.Tables[0].Rows[i]["NO_REFERENSI"];
                            worksheet.Cells[5 + i, 6].Value = Convert.ToDateTime(lsFaktur.Tables[0].Rows[i]["TGL_PESANAN"]).ToString("yyyy-MM-dd HH:mm:ss");
                            worksheet.Cells[5 + i, 7].Value = lsFaktur.Tables[0].Rows[i]["MARKETPLACE"];
                            worksheet.Cells[5 + i, 8].Value = lsFaktur.Tables[0].Rows[i]["KODE_PEMBELI"];
                            worksheet.Cells[5 + i, 9].Value = lsFaktur.Tables[0].Rows[i]["PEMBELI"];
                            worksheet.Cells[5 + i, 10].Value = lsFaktur.Tables[0].Rows[i]["ALAMAT_KIRIM"];
                            worksheet.Cells[5 + i, 11].Value = lsFaktur.Tables[0].Rows[i]["KURIR"];
                            worksheet.Cells[5 + i, 12].Value = lsFaktur.Tables[0].Rows[i]["TOP"];
                            worksheet.Cells[5 + i, 13].Value = Convert.ToDateTime(lsFaktur.Tables[0].Rows[i]["TGL_JATUH_TEMPO"]).ToString("yyyy-MM-dd HH:mm:ss");
                            worksheet.Cells[5 + i, 14].Value = lsFaktur.Tables[0].Rows[i]["KETERANGAN"];
                            worksheet.Cells[5 + i, 15].Value = lsFaktur.Tables[0].Rows[i]["BRUTO"];
                            worksheet.Cells[5 + i, 16].Value = lsFaktur.Tables[0].Rows[i]["DISC"];
                            worksheet.Cells[5 + i, 17].Value = lsFaktur.Tables[0].Rows[i]["PPN"];
                            worksheet.Cells[5 + i, 18].Value = lsFaktur.Tables[0].Rows[i]["NILAI_PPN"];
                            worksheet.Cells[5 + i, 19].Value = lsFaktur.Tables[0].Rows[i]["ONGKOS_KIRIM"];
                            worksheet.Cells[5 + i, 20].Value = lsFaktur.Tables[0].Rows[i]["NETTO"];
                            var pesanan1 = "";
                            switch (lsFaktur.Tables[0].Rows[i]["STATUS_PESANAN"])
                            {
                                case "0": pesanan1 = "BELUM BAYAR"; break;
                                case "01": pesanan1 = "SUDAH BAYAR"; break;
                                case "02": pesanan1 = "PACKING"; break;
                                case "03": pesanan1 = "FAKTUR"; break;
                                case "04": pesanan1 = "SELESAI"; break;
                                case "11": pesanan1 = "BATAL"; break;
                            }
                            worksheet.Cells[5 + i, 21].Value = pesanan1;
                            worksheet.Cells[5 + i, 22].Value = lsFaktur.Tables[0].Rows[i]["KODE_BRG"];
                            worksheet.Cells[5 + i, 23].Value = lsFaktur.Tables[0].Rows[i]["NAMA_BARANG"];
                            worksheet.Cells[5 + i, 24].Value = lsFaktur.Tables[0].Rows[i]["QTY"];
                            worksheet.Cells[5 + i, 25].Value = lsFaktur.Tables[0].Rows[i]["HARGA_SATUAN"];
                            worksheet.Cells[5 + i, 26].Value = lsFaktur.Tables[0].Rows[i]["DISC1"];
                            worksheet.Cells[5 + i, 27].Value = lsFaktur.Tables[0].Rows[i]["NDISC1"];
                            worksheet.Cells[5 + i, 28].Value = lsFaktur.Tables[0].Rows[i]["DISC2"];
                            worksheet.Cells[5 + i, 29].Value = lsFaktur.Tables[0].Rows[i]["NDISC2"];
                            worksheet.Cells[5 + i, 30].Value = lsFaktur.Tables[0].Rows[i]["TOTAL"];
                        }

                        ExcelRange rg0 = worksheet.Cells[4, 1, worksheet.Dimension.End.Row, 30];
                        string tableName0 = "TableFaktur";
                        ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);

                        table0.Columns[0].Name = "NO FAKTUR";
                        table0.Columns[1].Name = "TGL FAKTUR";
                        table0.Columns[2].Name = "STATUS FAKTUR";
                        table0.Columns[3].Name = "NO PESANAN";
                        table0.Columns[4].Name = "NO REFERENSI";
                        table0.Columns[5].Name = "TGL PESANAN";
                        table0.Columns[6].Name = "MARKETPLACE";
                        table0.Columns[7].Name = "KODE PEMBELI";
                        table0.Columns[8].Name = "PEMBELI";
                        table0.Columns[9].Name = "ALAMAT KIRIM";
                        table0.Columns[10].Name = "KURIR";
                        table0.Columns[11].Name = "TOP";
                        table0.Columns[12].Name = "TGL JATUH TEMPO";
                        table0.Columns[13].Name = "KETERANGAN";
                        table0.Columns[14].Name = "BRUTO";
                        table0.Columns[15].Name = "DISC";
                        table0.Columns[16].Name = "PPN";
                        table0.Columns[17].Name = "NILAI PPN";
                        table0.Columns[18].Name = "ONGKOS KIRIM";
                        table0.Columns[19].Name = "NETTO";
                        table0.Columns[20].Name = "STATUS PESANAN";
                        table0.Columns[21].Name = "KODE BRG";
                        table0.Columns[22].Name = "NAMA BARANG";
                        table0.Columns[23].Name = "QTY";
                        table0.Columns[24].Name = "HARGA SATUAN";
                        table0.Columns[25].Name = "DISC1";
                        table0.Columns[26].Name = "NDISC1";
                        table0.Columns[27].Name = "DISC2";
                        table0.Columns[28].Name = "NDISC2";
                        table0.Columns[29].Name = "TOTAL";

                        using (var range = worksheet.Cells[4, 1, 4, 30])
                        {
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }

                        table0.ShowHeader = true;
                        table0.ShowFilter = true;
                        table0.ShowRowStripes = false;
                        worksheet.Cells.AutoFitColumns(0);

                        ret.byteExcel = package.GetAsByteArray();
                        ret.namaFile = username + "_faktur" + ".xlsx";
                    }
                    else
                    {
                        ret.Errors.Add("Tidak ada data faktur");
                    }

                }

            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;

            var result = new ContentResult
            {
                Content = serializer.Serialize(ret),
                ContentType = "application/json"
            };

            return result;

        }
        //end by Indra 03 apr 2020, download faktur

        //add by Indra 16 apr 2020, download stokopname
        public ActionResult ListStokOpnametoExcel()
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };
            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Barang");

                    string sSQL = "select brg, nama + ' ' + isnull(nama2, '') as nama from stf02 where type = 3 order by nama,nama2";

                    var dsBarang = EDB.GetDataSet("CString", "STF02", sSQL);

                    if (dsBarang.Tables[0].Rows.Count > 0)
                    {
                        worksheet.Cells["A1"].Value = "Kode Gudang";
                        worksheet.Cells[2, 2].Value = "Isi kode gudang sesuai dengan master gudang pada sheet2";
                        worksheet.Cells[2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);

                        var gudang = ErasoftDbContext.STF18.ToList();

                        if (gudang.Count > 0)
                        {
                            worksheet.Cells[2, 1].Value = gudang[0].Kode_Gudang;

                            for (int i = 0; i < dsBarang.Tables[0].Rows.Count; i++)
                            {
                                worksheet.Cells[5 + i, 1].Value = dsBarang.Tables[0].Rows[i]["BRG"].ToString();
                                worksheet.Cells[5 + i, 2].Value = dsBarang.Tables[0].Rows[i]["NAMA"].ToString();
                            }
                            ExcelRange rg0 = worksheet.Cells[4, 1, worksheet.Dimension.End.Row, 3];
                            string tableName0 = "TableBarang";
                            ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);
                            table0.Columns[0].Name = "KODE BARANG";
                            table0.Columns[1].Name = "NAMA BARANG";
                            table0.Columns[2].Name = "QTY";

                            #region formatting
                            using (var range = worksheet.Cells[1, 1, 2, 1])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }

                            using (var range = worksheet.Cells[4, 1, 4, 3])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }

                            #endregion
                            table0.ShowHeader = true;
                            table0.ShowFilter = true;
                            table0.ShowRowStripes = false;
                            worksheet.Cells.AutoFitColumns(0);

                            var sheet2 = worksheet.Workbook.Worksheets.Add("Gudang");

                            sheet2.Cells[2, 1].Value = "Kode Gudang";
                            sheet2.Cells[2, 2].Value = "Nama Gudang";

                            for (int j = 0; j < gudang.Count; j++)
                            {
                                sheet2.Cells[3 + j, 1].Value = gudang[j].Kode_Gudang;
                                sheet2.Cells[3 + j, 2].Value = gudang[j].Nama_Gudang;
                            }

                            using (var range = sheet2.Cells[2, 1, 2, 2])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }
                            sheet2.Cells.AutoFitColumns(0);

                            if (dsBarang.Tables[0].Rows.Count > 0)
                            {
                                var validation = worksheet.DataValidations.AddListValidation("A2");

                                validation.ShowErrorMessage = true;
                                validation.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                                validation.ErrorTitle = "An invalid value was entered";
                                validation.Formula.ExcelFormula = string.Format("=sheet2!${0}${1}:${2}${3}", "A", 3, "A", 2 + gudang.Count);
                            }

                            ret.byteExcel = package.GetAsByteArray();
                            ret.namaFile = username + "_stokopname" + ".xlsx";
                        }
                        else
                        {
                            ret.Errors.Add("Kode gudang tidak ditemukan.");
                        }

                    }
                    else
                    {
                        ret.Errors.Add("Tidak ada data barang.");
                    }
                }
            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var result = new ContentResult
            {
                Content = serializer.Serialize(ret),
                ContentType = "application/json"
            };
            return result;
        }
        //end by Indra 16 apr 2020, download stokopname

        //add by Indra 16 apr 2020, upload stokopname
        public async Task<ActionResult> UploadXcelStokOpname(string nobuk, int countAll, string percentDanprogress, string statusLoopSuccess)
        {
            BindUploadExcel ret = new BindUploadExcel();
            ret.Errors = new List<string>();
            ret.namaGudang = new List<string>();
            ret.lastRow = new List<int>();
            ret.nextFile = false;
            byte[] dataByte = null;

            string[] status = statusLoopSuccess.Split(';');
            string[] prog = percentDanprogress.Split(';');

            List<STT04A> newSTT04A = new List<STT04A>();
            List<STT04B> newSTT04B = new List<STT04B>();

            try
            {

                ret.statusLoop = Convert.ToBoolean(status[0]);
                ret.statusSuccess = Convert.ToBoolean(status[1]);

                if (ret.byteData == null && ret.statusLoop == false)
                {
                    dataByte = UploadFileServices.UploadFile(Request.Files[0]);
                    ret.byteData = dataByte;
                }
                else
                {
                    ret.byteData = null;
                    ret.nobuk = nobuk;
                }

                for (int file_index = 0; file_index < Request.Files.Count; file_index++)
                {
                    if (ret.statusLoop == false)
                    {
                        ret.lastRow.Add(0);
                    }

                    if (ret.statusLoop == true)
                    {
                        var file = Request.Files[file_index];
                        if (file != null && file.ContentLength > 0)
                        {
                            using (Stream inputStream = file.InputStream)
                            {
                                MemoryStream memoryStream = inputStream as MemoryStream;
                                if (memoryStream == null)
                                {
                                    memoryStream = new MemoryStream();
                                    inputStream.CopyTo(memoryStream);
                                }
                                ret.byteData = memoryStream.ToArray();
                            }
                        }
                    }

                    using (MemoryStream stream = new MemoryStream(ret.byteData))
                    {
                        using (ExcelPackage excelPackage = new ExcelPackage(stream))
                        {
                            using (ErasoftContext eraDB = new ErasoftContext(DataSourcePath, dbPathEra))
                            {
                                using (System.Data.Entity.DbContextTransaction transaction = ErasoftDbContext.Database.BeginTransaction())
                                {
                                    eraDB.Database.CommandTimeout = 180;
                                    var worksheet = excelPackage.Workbook.Worksheets[1];
                                    string gd = worksheet.Cells[2, 1].Value == null ? "" : worksheet.Cells[2, 1].Value.ToString();

                                    if (!string.IsNullOrEmpty(gd))
                                    {
                                        var gudang = eraDB.STF18.Where(m => m.Kode_Gudang == gd).FirstOrDefault();
                                        if (gudang != null)
                                        {
                                            if (ret.statusLoop == false)
                                            {
                                                ret.namaGudang.Add(gudang.Nama_Gudang);
                                            }

                                            var listTemp = eraDB.STF02.Where(m => m.TYPE == "3").ToList();
                                            if (listTemp.Count > 0)
                                            {
                                                if (ret.statusLoop == false)
                                                {

                                                    //create header
                                                    var stt04a = new STT04A
                                                    {
                                                        GUD = gd,
                                                        NAMA_GUDANG = gudang.Nama_Gudang,
                                                        USERNAME = "UPLOAD_EXCEL_SOP",
                                                        TGL = DateTime.Today,
                                                        POSTING = "0",
                                                    };

                                                    var lastBuktiOP = new ManageController().GenerateAutoNumber(ErasoftDbContext, "OP", "STT04A", "NOBUK");
                                                    var noStokOP = "OP" + DateTime.UtcNow.AddHours(7).Year.ToString().Substring(2, 2) + Convert.ToString(Convert.ToInt32(lastBuktiOP) + 1).PadLeft(6, '0');

                                                    stt04a.NOBUK = noStokOP;
                                                    ret.nobuk = noStokOP;

                                                    try
                                                    {
                                                        eraDB.STT04A.Add(stt04a);
                                                        eraDB.SaveChanges();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        var tempSI = eraDB.STT04A.Where(a => a.NOBUK == stt04a.NOBUK).Single();
                                                        if (tempSI != null)
                                                        {
                                                            if (tempSI.NOBUK == noStokOP)
                                                            {
                                                                var lastBuktiOPNew = Convert.ToInt32(lastBuktiOP);
                                                                lastBuktiOPNew++;
                                                                noStokOP = "OP" + DateTime.UtcNow.AddHours(7).Year.ToString().Substring(2, 2) + Convert.ToString(Convert.ToInt32(lastBuktiOPNew) + 1).PadLeft(6, '0');
                                                                stt04a.NOBUK = noStokOP;
                                                                ret.nobuk = noStokOP;
                                                                eraDB.STT04A.Add(stt04a);
                                                                eraDB.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                var errMsg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                                                ret.Errors.Add(errMsg);
                                                            }
                                                        }
                                                    }
                                                }

                                                ret.countAll = worksheet.Dimension.End.Row;
                                                if (Convert.ToInt32(prog[1]) == 0)
                                                {
                                                    prog[1] = "0";
                                                }


                                                for (int i = Convert.ToInt32(prog[1]); i <= worksheet.Dimension.End.Row; i++)
                                                {
                                                    ret.statusLoop = true;
                                                    ret.progress = i;
                                                    ret.percent = (i * 100) / ret.countAll;

                                                    var kd_brg = worksheet.Cells[i, 1].Value == null ? "" : worksheet.Cells[i, 1].Value.ToString();
                                                    if (!string.IsNullOrEmpty(kd_brg))
                                                    {
                                                        var current_brg = listTemp.Where(m => m.BRG == kd_brg).SingleOrDefault();
                                                        if (current_brg != null)
                                                        {
                                                            if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 3].Value)))
                                                            {
                                                                //stok 0 juga bisa masuk
                                                                if (Convert.ToInt32(worksheet.Cells[i, 3].Value) >= 0)
                                                                {
                                                                    STT04B stt04b = new STT04B
                                                                    {
                                                                        Gud = gd,
                                                                        Brg = current_brg.BRG,
                                                                        Qty = Convert.ToInt32(worksheet.Cells[i, 3].Value),
                                                                        Tgl = DateTime.Today,
                                                                        HPokok = 0,
                                                                        BK = "",
                                                                        Stn = "",
                                                                        WO = "",
                                                                        Nama_Barang = current_brg.NAMA,
                                                                        Qty_Berat = 0,
                                                                        QTY_KECIL = 0,
                                                                        QTY_BESAR = 0,
                                                                        QTY_3 = 0,
                                                                        QTY_4 = 0,
                                                                        LKS = "",
                                                                        USERNAME = "UPLOAD_EXCEL_SOP",
                                                                        NOBUK = ret.nobuk,
                                                                    };
                                                                    newSTT04B.Add(stt04b);
                                                                    eraDB.STT04B.AddRange(newSTT04B);
                                                                    eraDB.SaveChanges();
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ret.Errors.Add("Kode Barang (" + kd_brg + ") tidak ditemukan");
                                                            ret.statusSuccess = true;
                                                            ret.lastRow[file_index] = i;
                                                            i = worksheet.Dimension.End.Row;
                                                        }
                                                    }


                                                    if (ret.percent >= 10 || ret.percent >= 20 ||
                                                        ret.percent >= 30 || ret.percent >= 40 ||
                                                        ret.percent >= 50 || ret.percent >= 60 ||
                                                        ret.percent >= 70 || ret.percent >= 80 ||
                                                        ret.percent >= 90 || ret.percent >= 100)
                                                    {
                                                        ret.statusSuccess = false;
                                                        if (ret.percent >= 100)
                                                        {
                                                            try
                                                            {
                                                                //eraDB.SaveChanges();
                                                                transaction.Commit();
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                transaction.Rollback();
                                                                ret.Errors.Add(ex.InnerException == null ? ex.Message : "Data tidak berhasil diproses, " + ex.InnerException.Message);
                                                            }
                                                            ret.statusSuccess = true;
                                                        }
                                                        return Json(ret, JsonRequestBehavior.AllowGet);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ret.Errors.Add("Kode gudang tidak ditemukan");
                                        }
                                    }
                                    else
                                    {
                                        ret.Errors.Add("Kode gudang tidak ditemukan");
                                    }
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ret.Errors.Add(ex.InnerException == null ? ex.Message : "Data tidak berhasil diproses, " + ex.InnerException.Message);
            }

            return Json(ret, JsonRequestBehavior.AllowGet);

        }
        //end by Indra 16 apr 2020, upload stokopname

    }


    //add by fauzi uploadStockSaldoAwal
    public class BindUploadExcelStockSaldoAwal
    {
        public string error { get; set; }
        public int countAllRow { get; set; }
        public int nilaiPercent { get; set; }
        public int nilaiProgressLooping { get; set; }
        public bool status { get; set; }
    }

    public class BindUploadExcel
    {
        public List<string> Errors { get; set; }
        public List<int> lastRow { get; set; }
        public bool success { get; set; }
        public List<string> cust { get; set; }
        public List<string> namaCust { get; set; }
        public List<string> namaGudang { get; set; }
        public bool nextFile { get; set; }
        public byte[] byteData { get; set; }
        public bool statusLoop { get; set; }
        public bool statusSuccess { get; set; }
        public int progress { get; set; }
        public int percent { get; set; }
        public int countAll { get; set; }
        public string nobuk { get; set; }

        //add by nurul 6/4/2020\
        public double TBAYAR { get; set; }
        public double TPOT { get; set; }
        public double? TLEBIHBAYAR { get; set; }
        //end add by nurul 6/4/2020
    }

    public class BindDownloadExcel
    {
        public List<string> Errors { get; set; }
        public byte[] byteExcel { get; set; }
        public string namaFile { get; set; }
    }
    
}