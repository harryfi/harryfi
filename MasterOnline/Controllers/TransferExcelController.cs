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
using System.Text.RegularExpressions;
using Hangfire;
using Hangfire.Server;
using Hangfire.SqlServer;

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
        string dbSourceEra = "";
        public TransferExcelController()
        {
            MoDbContext = new MoDbContext("");
            username = "";
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                {
                    ErasoftDbContext = new ErasoftContext();
                }
                else
                {
#if (Debug_AWS)
                    dbSourceEra = sessionData.Account.DataSourcePathDebug;
#else
                    dbSourceEra = sessionData.Account.DataSourcePath;
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, sessionData.Account.DatabasePathErasoft);
                }

                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
                dbPathEra = sessionData.Account.DatabasePathErasoft;
                //DataSourcePath = sessionData.Account.DataSourcePath;
                DataSourcePath = dbSourceEra;
                username = sessionData.Account.Username;
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
#if (Debug_AWS)
                    dbSourceEra = accFromUser.DataSourcePathDebug;
#else
                    dbSourceEra = accFromUser.DataSourcePath;
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    dbPathEra = accFromUser.DatabasePathErasoft;
                    //DataSourcePath = accFromUser.DataSourcePath;
                    DataSourcePath = dbSourceEra;
                    username = accFromUser.Username;
                }
            }
            if (username.Length > 20)
                username = username.Substring(0, 20);
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
                                worksheet.Cells[19 + i, 9].Value = dsBarang.Tables[0].Rows[i]["HJUAL"].ToString();
                                worksheet.Cells[19 + i, 10].Value = dsBarang.Tables[0].Rows[i]["HJUAL_MP"].ToString();
                                worksheet.Cells[19 + i, 11].Value = dsBarang.Tables[0].Rows[i]["BERAT"].ToString();
                                worksheet.Cells[19 + i, 12].Value = dsBarang.Tables[0].Rows[i]["IMAGE"].ToString();
                                worksheet.Cells[19 + i, 13].Value = dsBarang.Tables[0].Rows[i]["BRG_MP"].ToString();
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

                            using (var range = worksheet.Cells[18, 1, 18, 13])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }
                            #endregion

                            ExcelRange rg0 = worksheet.Cells[18, 1, worksheet.Dimension.End.Row, 13];
                            string tableName0 = "TableBarang";
                            ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);
                            table0.Columns[0].Name = "NAMA1";
                            table0.Columns[1].Name = "NAMA2";
                            table0.Columns[2].Name = "KODE_BRG_MO";
                            table0.Columns[3].Name = "KODE_BRG_INDUK_MO (diisi khusus untuk barang variasi)";
                            table0.Columns[4].Name = "KODE_BARCODE";
                            table0.Columns[5].Name = "LOKASI_RAK";
                            table0.Columns[6].Name = "KODE_KATEGORI_MO";
                            table0.Columns[7].Name = "KODE_MEREK_MO";
                            table0.Columns[8].Name = "HJUAL";
                            table0.Columns[9].Name = "HJUAL_MARKETPLACE";
                            table0.Columns[10].Name = "BERAT";
                            table0.Columns[11].Name = "IMAGE";
                            table0.Columns[12].Name = "KODE_BRG_MARKETPLACE";
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
                                var validation = worksheet.DataValidations.AddListValidation(worksheet.Cells[19, 7, worksheet.Dimension.End.Row, 7].Address);
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
                                var validation2 = worksheet.DataValidations.AddListValidation(worksheet.Cells[19, 8, worksheet.Dimension.End.Row, 8].Address);
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
                    //sSQL += "replace(replace(S.NAMA, char(10), ''), char(13), '') + ISNULL(replace(replace(S.NAMA2, char(10), ''), char(13), ''), '') AS NAMA, ";
                    //sSQL += "M.NAMAMARKET + '(' + replace(replace(A.PERSO, char(10), ''), char(13), '') + ')' AS AKUN,H.HJUAL, M.IDMARKET, ISNULL(STF10.HPOKOK, 0) AS HPOKOK ";
                    //sSQL += "FROM STF02 S INNER JOIN STF02H H ON S.BRG = H.BRG INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM ";
                    //sSQL += "INNER JOIN MO..MARKETPLACE M ON A.NAMA = M.IDMARKET LEFT JOIN STF10 ON S.BRG = STF10.BRG WHERE TYPE = '3' ORDER BY NAMA,M.IDMARKET";
                    sSQL += "replace(replace(S.NAMA, char(10), ''), char(13), '') + ISNULL(replace(replace(S.NAMA2, char(10), ''), char(13), ''), '') AS NAMA, ";
                    sSQL += "M.NAMAMARKET + '(' + replace(replace(A.PERSO, char(10), ''), char(13), '') + ')' AS AKUN,H.HJUAL, M.IDMARKET, ";
                    sSQL += "ISNULL((SELECT TOP 1 ISNULL(E.HBELI,0) AS HBELI FROM PBT01A F LEFT JOIN PBT01B E ON F.INV = E.INV WHERE E.BRG = S.BRG ORDER BY F.TGL DESC, E.NO DESC), 0) AS HPOKOK ";
                    sSQL += "FROM STF02 S INNER JOIN STF02H H ON S.BRG = H.BRG INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM ";
                    sSQL += "INNER JOIN MO..MARKETPLACE M ON A.NAMA = M.IDMARKET WHERE TYPE = '3' ORDER BY NAMA,M.IDMARKET";
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
                        //table0.Columns[4].Name = "HARGA JUAL TERAKHIR";
                        table0.Columns[4].Name = "HARGA BELI TERAKHIR";

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

                    //change by nurul 9/11/2020, bundling
                    ////change by calvin 16 september 2019
                    ////string sSQL = "select brg, nama + isnull(nama2, '') as nama from stf02 where type = 3 order by nama,nama2";
                    //string sSQL = "select brg, nama + ' ' + isnull(nama2, '') as nama from stf02 where type = 3 order by nama,nama2";
                    ////end change by calvin 16 september 2019
                    string sSQL = "select a.brg, nama + ' ' + isnull(nama2, '') as nama from stf02 a (nolock) " +
                                  "left join (select distinct unit from stf03 (nolock)) b on a.brg=b.unit " +
                                  "where type = '3' and isnull(b.unit,'')='' order by nama,nama2";
                    //end change by nurul 9/11/2020, bundling

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
                                    eraDB.Database.CommandTimeout = 0;
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

                                            //add by nurul 9/11/2020,bundling 
                                            var listTempBundling = eraDB.STF03.Select(a => a.Unit).Distinct().ToList();
                                            //end add by nurul 9/11/2020, bundling

                                            #region create induk
                                            if (ret.statusLoop == false)
                                            {
                                                //eraDB.Database.ExecuteSqlCommand("DELETE FROM TEMP_SALDOAWAL");

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
                                                            if (!listTempBundling.Contains(Convert.ToString(worksheet.Cells[i, 1].Value))){
                                                                if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 3].Value)))
                                                                {
                                                                    var qtyTemp = worksheet.Cells[i, 3].Value.ToString().Replace("\r", "").Replace("\n", "").Replace(" ", "");
                                                                    double dqty = Convert.ToDouble(qtyTemp);

                                                                    if (dqty >= 0)
                                                                        //if (Convert.ToInt32(worksheet.Cells[i, 3].Value) >= 0)
                                                                    {
                                                                        TEMP_SALDOAWAL newrecord = new TEMP_SALDOAWAL()
                                                                        {
                                                                            BRG = Convert.ToString(worksheet.Cells[i, 1].Value),
                                                                            //QTY = Convert.ToInt32(worksheet.Cells[i, 3].Value)
                                                                            QTY = dqty
                                                                        };
                                                                        if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 4].Value)))
                                                                        {
                                                                            var modalTemp = worksheet.Cells[i, 4].Value.ToString().Replace("\r", "").Replace("\n", "").Replace(" ", "").Replace("&nbsp;", "").Replace("\r\n", "");
                                                                            var te = modalTemp.Replace("\r", "").Replace("\n", "").Replace("\r\r", "").Replace("&nbsp;", "").Replace("\r\n", "");
                                                                            double dmodal = Convert.ToDouble(modalTemp);
                                                                            if (dmodal >= 0)
                                                                            {
                                                                                newrecord.HARGA_SATUAN = dmodal;
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
                                                                ret.Errors.Add("Kode Barang (" + Convert.ToString(worksheet.Cells[i, 1].Value) + ") merupakan barang bundling. Mohon untuk mencoba lagi proses Upload Excel Saldo Awal.");
                                                                ret.statusSuccess = false;
                                                                ret.lastRow[file_index] = i;
                                                                i = worksheet.Dimension.End.Row;
                                                                return Json(ret, JsonRequestBehavior.AllowGet);
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
                                                    eraDB.Database.ExecuteSqlCommand("DELETE FROM TEMP_SALDOAWAL");
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
                                                        if (ret.progress == 0)
                                                        {
                                                            ret.percent = (j * 100) / (ret.countAll);
                                                        }
                                                        else
                                                        {
                                                            ret.percent = (j * 100) / (ret.countAll - 1);
                                                        }
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

            //try
            //{
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
                        using (ErasoftContext eraDB = new ErasoftContext(DataSourcePath, dbPathEra))
                        {
                            //using (System.Data.Entity.DbContextTransaction transaction = eraDB.Database.BeginTransaction())
                            //{
                            string connID = Guid.NewGuid().ToString();

                            //initialize log txt
                            #region Logging
                            string messageErrorLog = "";
                            string filename = "Log_Upload_Pesanan_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
                            var path = Path.Combine(Server.MapPath("~/Content/Uploaded/" + dbPathEra + "/"), filename);
                            #endregion

                            if (!System.IO.File.Exists(path))
                            {
                                System.IO.Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Content/Uploaded/" + dbPathEra + "/"), ""));
                                var asd = System.IO.File.Create(path);
                                asd.Close();
                            }
                            StreamWriter tw = new StreamWriter(path);

                            try
                            {
                                eraDB.Database.CommandTimeout = 1800;
                                //loop all worksheets
                                var worksheet = excelPackage.Workbook.Worksheets[1];

                                ret.countAll = Convert.ToInt32(worksheet.Dimension.End.Row - 5);

                                if (Convert.ToInt32(prog[1]) == 0)
                                {
                                    prog[1] = "0";
                                }

                                var noBuktiSO = "";

                                //change by nurul 17/9/2020, brg multi sku 
                                //var dataMasterSTF02 = eraDB.STF02.Select(p => new { p.BRG, p.NAMA, p.NAMA2, p.NAMA3 }).ToList();
                                var dataMasterSTF02 = eraDB.STF02.AsNoTracking().Select(p => new { p.BRG, p.NAMA, p.NAMA2, p.NAMA3, p.TYPE, p.KUBILASI, p.BRG_NON_OS }).ToList();
                                //end change by nurul 17/9/2020, brg multi sku 
                                var dataMasterSTF02H = eraDB.STF02H.AsNoTracking().Select(p => new { p.BRG, p.BRG_MP, p.IDMARKET }).ToList();
                                var dataMasterKurir = MoDbContext.Ekspedisi.AsNoTracking().ToList();
                                var dataMasterARF01 = eraDB.ARF01.AsNoTracking().ToList();

                                //eraDB.Database.ExecuteSqlCommand("DELETE FROM TEMP_UPLOADPESANAN");
                                //List<TEMP_UPLOADPESANAN> batchinsertItem = new List<TEMP_UPLOADPESANAN>();
                                string queryInsertLogError = "INSERT INTO API_LOG_MARKETPLACE (CUST, REQUEST_ID, REQUEST_ACTION, REQUEST_DATETIME, REQUEST_STATUS, REQUEST_RESULT, CUST_ATTRIBUTE_1, REQUEST_EXCEPTION) VALUES ";

                                //batchinsertItem = new List<TEMP_UPLOADPESANAN>();

                                string idRequest = Guid.NewGuid().ToString();

                                int iProcess = 0;
                                int success = 0;


                                // validasi jika file baru lagi dengan no referensi dan no cust sama tidak boleh proses
                                // add by fauzi 23/09/2020
                                #region validasi duplicate dari file berbeda
                                var dataNoReferensiNoCust = new List<string>();

                                for (int i = Convert.ToInt32(prog[0]); i <= worksheet.Dimension.End.Row; i++)
                                {
                                    string no_referensi = worksheet.Cells[i, 2].Value == null ? "" : worksheet.Cells[i, 2].Value.ToString();
                                    string marketplace = worksheet.Cells[i, 4].Value == null ? "" : worksheet.Cells[i, 4].Value.ToString();
                                    if (!string.IsNullOrEmpty(no_referensi))
                                    {
                                        if (!string.IsNullOrEmpty(marketplace))
                                        {
                                            string[] no_cust = marketplace.Split(';');
                                            var noCust = no_cust[0].ToString();
                                            dataNoReferensiNoCust.Add(no_referensi + ";" + noCust);
                                        }
                                    }
                                }

                                if (dataNoReferensiNoCust.Count() > 0)
                                {
                                    var dataFilterRef = dataNoReferensiNoCust.Distinct().ToList();
                                    if (dataFilterRef.Count() > 0)
                                    {
                                        foreach (var refCheck in dataFilterRef)
                                        {
                                            string[] splitRef = refCheck.Split(';');
                                            var resNoref = splitRef[0].ToString();
                                            var resNocust = splitRef[1].ToString();
                                            var checkDB = eraDB.SOT01A.AsNoTracking().Where(c => c.NO_REFERENSI == resNoref && c.CUST == resNocust).SingleOrDefault();
                                            if(checkDB != null)
                                            {
                                                messageErrorLog = "No Referensi pesanan " + resNoref + " dan Kode Customer toko " + resNocust + " sudah pernah dimasukan.Proses Upload Pesanan dibatalkan.";
                                                tw.WriteLine(messageErrorLog);
                                                tw.Close();
                                                tw.Dispose();

                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                if (cekLog == null)
                                                {
                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                (resNocust),
                                                (connID),
                                                ("Upload Excel Pesanan"),
                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                ("FAILED"),
                                                (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                (username),
                                                (filename));
                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                    // error log terjadi error pada insert header pesanan
                                                }
                                                ret.Errors.Add("No Referensi pesanan " + resNoref + " dan Kode Customer toko " + resNocust + " sudah pernah dimasukan. Proses Upload Pesanan dibatalkan.");
                                                return Json(ret, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                    }
                                }
                                #endregion
                                // end add by fauzi 23/09/2020

                                //add by nurul 20/1/2021, bundling 
                                var default_gudang = "";
                                var gudang_parsys = eraDB.SIFSYS.FirstOrDefault().GUDANG;
                                var cekgudang = eraDB.STF18.ToList();
                                if (cekgudang.Where(p => p.Kode_Gudang == gudang_parsys).Count() > 0)
                                {
                                    default_gudang = gudang_parsys;
                                }
                                else
                                {
                                    default_gudang = cekgudang.FirstOrDefault().Kode_Gudang;
                                }

                                var gd_Bundling = "";
                                var gudang_parsys_Bundling = eraDB.SIFSYS.FirstOrDefault().GUDANG;
                                var cekgudang_Bundling = eraDB.STF18.Where(a => a.KD_HARGA_JUAL != "1" && a.Kode_Gudang != "GB" && a.Nama_Gudang != "Gudang Bundling").ToList();
                                if (cekgudang_Bundling.Where(p => p.Kode_Gudang == gudang_parsys_Bundling && p.KD_HARGA_JUAL != "1").Count() > 0)
                                {
                                    gd_Bundling = gudang_parsys_Bundling;
                                }
                                else
                                {
                                    gd_Bundling = cekgudang_Bundling.FirstOrDefault().Kode_Gudang;
                                }
                                //add by nurul 20/1/2021, bundling

                                // start looping
                                for (int i = Convert.ToInt32(prog[0]); i <= worksheet.Dimension.End.Row; i++)
                                {


                                    ret.statusLoop = true;
                                    ret.progress = i;
                                    //ret.percent = (i * 100) / (ret.countAll - 1);
                                    //var bagiProses = (ret.countAll - 1) * (Convert.ToDecimal(30) / Convert.ToDecimal(100));
                                    //bagiProses = Decimal.Round(bagiProses);

                                    //get ALL DATA
                                    string no_referensi = worksheet.Cells[i, 2].Value == null ? "" : worksheet.Cells[i, 2].Value.ToString();
                                    string tgl_pesanan = worksheet.Cells[i, 3].Value == null ? "" : worksheet.Cells[i, 3].Value.ToString();
                                    string marketplace = worksheet.Cells[i, 4].Value == null ? "" : worksheet.Cells[i, 4].Value.ToString();
                                    string nama_pembeli = worksheet.Cells[i, 5].Value == null ? "" : worksheet.Cells[i, 5].Value.ToString();
                                    string alamat_kirim = worksheet.Cells[i, 6].Value == null ? "" : worksheet.Cells[i, 6].Value.ToString();
                                    string no_telpPembeli = worksheet.Cells[i, 7].Value == null ? "" : worksheet.Cells[i, 7].Value.ToString();
                                    string kode_kurir = worksheet.Cells[i, 8].Value == null ? "" : worksheet.Cells[i, 8].Value.ToString();
                                    string top = worksheet.Cells[i, 9].Value == null ? "1" : worksheet.Cells[i, 9].Value.ToString();
                                    //string tgl_jatuh_tempo = worksheet.Cells[i, 9].Value == null ? "" : worksheet.Cells[i, 9].Value.ToString(); DIHAPUS
                                    string keterangan = worksheet.Cells[i, 10].Value == null ? "" : worksheet.Cells[i, 10].Value.ToString();
                                    string bruto = worksheet.Cells[i, 11].Value == null ? "0" : worksheet.Cells[i, 11].Value.ToString();
                                    string diskon = worksheet.Cells[i, 12].Value == null ? "0" : worksheet.Cells[i, 12].Value.ToString();
                                    string ppn = worksheet.Cells[i, 13].Value == null ? "0" : worksheet.Cells[i, 13].Value.ToString();
                                    string nilai_ppn = worksheet.Cells[i, 14].Value == null ? "0" : worksheet.Cells[i, 14].Value.ToString();
                                    string ongkir = worksheet.Cells[i, 15].Value == null ? "0" : worksheet.Cells[i, 15].Value.ToString();
                                    string netto = worksheet.Cells[i, 16].Value == null ? "0" : worksheet.Cells[i, 16].Value.ToString();
                                    //string status_pesanan = worksheet.Cells[i, 17].Value == null ? "" : worksheet.Cells[i, 17].Value.ToString(); DIHAPUS
                                    string kode_brg = worksheet.Cells[i, 17].Value == null ? "" : worksheet.Cells[i, 17].Value.ToString();
                                    string nama_brg = worksheet.Cells[i, 18].Value == null ? "" : worksheet.Cells[i, 18].Value.ToString();
                                    string qty = worksheet.Cells[i, 19].Value == null ? "0" : worksheet.Cells[i, 19].Value.ToString();
                                    string harga_satuan = worksheet.Cells[i, 20].Value == null ? "0" : worksheet.Cells[i, 20].Value.ToString();
                                    //string disc1 = worksheet.Cells[i, 22].Value == null ? "0" : worksheet.Cells[i, 22].Value.ToString(); DIHAPUS
                                    string ndisc1 = worksheet.Cells[i, 21].Value == null ? "0" : worksheet.Cells[i, 21].Value.ToString();
                                    //string disc2 = worksheet.Cells[i, 24].Value == null ? "0" : worksheet.Cells[i, 24].Value.ToString(); DIHAPUS
                                    //string ndisc2 = worksheet.Cells[i, 25].Value == null ? "0" : worksheet.Cells[i, 25].Value.ToString(); DIHAPUS
                                    string total = worksheet.Cells[i, 22].Value == null ? "0" : worksheet.Cells[i, 22].Value.ToString();

                                    if (marketplace.Contains("Silahkan") && kode_kurir.Contains("Silahkan"))
                                    {
                                        marketplace = "";
                                        kode_kurir = "";
                                    }


                                    if (!string.IsNullOrEmpty(no_referensi) || !string.IsNullOrEmpty(kode_brg))
                                    {
                                        if (no_referensi.Length <= 70)
                                        {
                                            if (!string.IsNullOrEmpty(marketplace))
                                            {
                                                if (!string.IsNullOrEmpty(kode_kurir))
                                                {
                                                    string[] no_cust = marketplace.Split(';');
                                                    string[] kurir = kode_kurir.Split(';');

                                                    var noCust = no_cust[0].ToString();

                                                    if (!string.IsNullOrEmpty(no_telpPembeli))
                                                    {
                                                        if (!no_telpPembeli.Contains(".") || !top.Contains(".") || !bruto.Contains(".") || !diskon.Contains(".") || !ppn.Contains(".") || !nilai_ppn.Contains(".") || !ongkir.Contains(".") || !netto.Contains(".") || !qty.Contains(".") || !harga_satuan.Contains(".") || !ndisc1.Contains(".") || !total.Contains("."))
                                                        {
                                                            if (!no_telpPembeli.Contains(",") || !top.Contains(",") || !bruto.Contains(",") || !diskon.Contains(",") || !ppn.Contains(",") || !nilai_ppn.Contains(",") || !ongkir.Contains(",") || !netto.Contains(",") || !qty.Contains(",") || !harga_satuan.Contains(",") || !ndisc1.Contains(",") || !total.Contains(","))
                                                            {
                                                                if (!string.IsNullOrEmpty(kode_brg))
                                                                {
                                                                    if (!string.IsNullOrEmpty(harga_satuan) || !string.IsNullOrEmpty(qty))
                                                                    {
                                                                        if (kode_brg.Length <= 20)
                                                                        {
                                                                            //var checkBarang = ErasoftDbContext.STF02.Where(p => p.BRG == item.KODE_BRG).Select(p => p.BRG).FirstOrDefault();
                                                                            var checkBarang = dataMasterSTF02.Where(p => p.BRG == kode_brg).FirstOrDefault();
                                                                            if (checkBarang != null)
                                                                            {
                                                                                //var dataKurir = MoDbContext.Ekspedisi.Where(p => p.RecNum == Convert.ToInt32(item.KODE_KURIR)).FirstOrDefault();
                                                                                int checkKodeKurir = Convert.ToInt32(kurir[0]);
                                                                                var dataKurir = dataMasterKurir.Where(p => p.RecNum == checkKodeKurir).FirstOrDefault();
                                                                                if (dataKurir != null)
                                                                                {
                                                                                    var kodeCust = no_cust[0];
                                                                                    //var dataToko = ErasoftDbContext.ARF01.Where(p => p.CUST == item.MARKETPLACE).FirstOrDefault();
                                                                                    var dataToko = dataMasterARF01.Where(p => p.CUST == kodeCust).FirstOrDefault();
                                                                                    if (dataToko != null)
                                                                                    {
                                                                                        if (dataToko.STATUS_API == "0" || string.IsNullOrEmpty(dataToko.STATUS_API))
                                                                                        {

                                                                                            var KodeBRGMP = "";
                                                                                            //var dataBarang = ErasoftDbContext.STF02H.Where(p => p.BRG == item.KODE_BRG && p.IDMARKET == dataToko.RecNum).FirstOrDefault();
                                                                                            var dataBarang = dataMasterSTF02H.Where(p => p.BRG == kode_brg && p.IDMARKET == dataToko.RecNum).FirstOrDefault();
                                                                                            if (dataBarang != null)
                                                                                            {
                                                                                                KodeBRGMP = "";
                                                                                                //if (dataBarang.BRG_MP.Contains(';'))
                                                                                                //{
                                                                                                //    string[] brgMPOrderItemID = dataBarang.BRG_MP.Split(';');
                                                                                                //    KodeBRGMP = brgMPOrderItemID[0];
                                                                                                //}
                                                                                                //else
                                                                                                //{
                                                                                                //    KodeBRGMP = dataBarang.BRG_MP;
                                                                                                //}

                                                                                                var kodePembeli = "";
                                                                                                string address = "";
                                                                                                var dataPembeli = eraDB.ARF01C.Where(p => p.NAMA == nama_pembeli.Replace("'", "`") && p.TLP == no_telpPembeli.Replace(" ", "").Replace("'", "`").Replace("`", "").Replace("+", "").Replace("-", "")).FirstOrDefault();

                                                                                                var alamatAutoSplit1 = "";
                                                                                                var alamatAutoSplit2 = "";
                                                                                                var alamatAutoSplit3 = "";

                                                                                                if (alamat_kirim.Length >= 40)
                                                                                                {
                                                                                                    alamatAutoSplit1 = alamat_kirim.Substring(0, 40);
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    alamatAutoSplit1 = alamat_kirim.ToString();
                                                                                                }
                                                                                                //if (alamat_kirim.Length > 30 && alamat_kirim.Length <= 40)
                                                                                                //    alamatAutoSplit2 = alamat_kirim.Substring(30, alamat_kirim.Length - 30);
                                                                                                //if (alamat_kirim.Length > 60 && alamat_kirim.Length <= 70)
                                                                                                //    alamatAutoSplit3 = alamat_kirim.Substring(60, alamat_kirim.Length - 60);


                                                                                                //var alamatAutoSplit1 = alamat_kirim.Length > 40 ? alamat_kirim.Substring(0, 40) : alamat_kirim.ToString();
                                                                                                //var alamatAutoSplit2 = alamat_kirim.Length > 80 ? alamat_kirim.Substring(30, 40) : "";
                                                                                                //var alamatAutoSplit3 = alamat_kirim.Length > 120 ? alamat_kirim.Substring(80, 119) : "";

                                                                                                if (dataPembeli == null)
                                                                                                {
                                                                                                    var connIdARF01C = Guid.NewGuid().ToString();
                                                                                                    var kodePembeliLast = eraDB.ARF01C.AsNoTracking().Select(p => p.BUYER_CODE).ToList().LastOrDefault();
                                                                                                    kodePembeliLast = Convert.ToString(Convert.ToInt32(kodePembeliLast) + 1).PadLeft(10, '0');

                                                                                                    string insertPembeli = "INSERT INTO ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                                                                                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                                                                                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, BUYER_CODE) VALUES ";
                                                                                                    var kabKot = "3174";
                                                                                                    var prov = "31";

                                                                                                    nama_pembeli = nama_pembeli.Length > 30 ? nama_pembeli.Substring(0, 30) : nama_pembeli.ToString();
                                                                                                    address = alamatAutoSplit1;


                                                                                                    insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01', 1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
                                                                                                        ((nama_pembeli ?? "").Replace("'", "`")),
                                                                                                        (alamatAutoSplit1.Length > 30 ? alamatAutoSplit1.Substring(0, 30).Replace("'", "`") : alamatAutoSplit1.Replace("'", "`")),
                                                                                                         ((no_telpPembeli).Replace("'", "`").Replace("`", "").Replace("+", "").Replace(" ", "").Replace("-", "")),
                                                                                                        (dataToko.PERSO.Replace(',', '.')),
                                                                                                        (alamatAutoSplit1.Length > 30 ? alamatAutoSplit1.Substring(0, 30).Replace("'", "`") : alamatAutoSplit1.Replace("'", "`")),
                                                                                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                                                        (username),
                                                                                                        (("").Replace("'", "`")),
                                                                                                        kabKot,
                                                                                                        prov,
                                                                                                        kodePembeliLast
                                                                                                        );
                                                                                                    insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                                                                                                    EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);
                                                                                                    kodePembeli = eraDB.ARF01C.Where(p => p.NAMA == nama_pembeli.Replace("'", "`") && p.TLP == no_telpPembeli.Replace("-", "").Replace(" ", "").Replace("'", "`").Replace("`", "").Replace("+", "")).Select(p => p.BUYER_CODE).FirstOrDefault();
                                                                                                    //kodePembeli = dataMasterARF01C.Where(p => p.NAMA == nama).Select(p => p.BUYER_CODE).FirstOrDefault();
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    address = dataPembeli.AL.Length > 30 ? dataPembeli.AL.Substring(0, 29) : dataPembeli.AL;
                                                                                                    kodePembeli = dataPembeli.BUYER_CODE;
                                                                                                }

                                                                                                var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == dataToko.CUST).FirstOrDefault();
                                                                                                if (checkDuplicateHeader == null)
                                                                                                {
                                                                                                    var lastBukti = new ManageController().GenerateAutoNumber(eraDB, "SU", "SOT01A", "NO_BUKTI");
                                                                                                    var noOrder = "SU" + DateTime.UtcNow.AddHours(7).Year.ToString().Substring(2, 2) + Convert.ToString(Convert.ToInt32(lastBukti) + 1).PadLeft(6, '0');
                                                                                                    noBuktiSO = noOrder;

                                                                                                    var sot01a = new SOT01A
                                                                                                    {
                                                                                                        AL = alamatAutoSplit1,
                                                                                                        AL1 = alamatAutoSplit1,
                                                                                                        AL2 = alamatAutoSplit2,
                                                                                                        AL3 = alamatAutoSplit3,
                                                                                                        ALAMAT_KIRIM = alamat_kirim,
                                                                                                        AL_CUST = "",
                                                                                                        BRUTO = Convert.ToInt32(bruto),
                                                                                                        CUST = no_cust[0].ToString(),
                                                                                                        CUST_QQ = "",
                                                                                                        DISCOUNT = Convert.ToInt32(diskon),
                                                                                                        Date_Approve = null,
                                                                                                        EXPEDISI = kurir[0],
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
                                                                                                        NAMAPEMESAN = nama_pembeli.Replace("'", "`"),
                                                                                                        NAMAPENGIRIM = null,
                                                                                                        NAMA_CUST = dataToko.PERSO,
                                                                                                        NETTO = Convert.ToInt32(netto),
                                                                                                        NILAI_DISC = Convert.ToInt32(ndisc1),
                                                                                                        NILAI_PPN = Convert.ToInt32(nilai_ppn),
                                                                                                        NILAI_TUKAR = 1,
                                                                                                        NO_BUKTI = noBuktiSO,
                                                                                                        NO_PENAWARAN = "",
                                                                                                        NO_PO_CUST = "",
                                                                                                        NO_REFERENSI = no_referensi,
                                                                                                        N_KOMISI = 0,
                                                                                                        N_KOMISI1 = 0,
                                                                                                        N_UCAPAN = "",
                                                                                                        ONGKOS_KIRIM = Convert.ToInt32(ongkir),
                                                                                                        PEMESAN = kodePembeli,
                                                                                                        PENGIRIM = null,
                                                                                                        PPN = Convert.ToInt32(nilai_ppn),
                                                                                                        PRINT_COUNT = 0,
                                                                                                        PROPINSI = null,
                                                                                                        RETUR_PENUH = false,
                                                                                                        RecNum = null,
                                                                                                        SHIPMENT = dataKurir.NamaEkspedisi,
                                                                                                        SOT01D = null,
                                                                                                        STATUS = "0",
                                                                                                        STATUS_TRANSAKSI = "01",
                                                                                                        SUPP = "0",
                                                                                                        Status_Approve = "",
                                                                                                        TERM = Convert.ToInt32(top),
                                                                                                        TGL = DateTime.Now.AddHours(7),
                                                                                                        TGL_INPUT = DateTime.Now.AddHours(7),
                                                                                                        TGL_JTH_TEMPO = DateTime.Now.AddHours(7).AddDays(Convert.ToInt32(top)),
                                                                                                        TGL_KIRIM = null,
                                                                                                        TIPE_KIRIM = 0,
                                                                                                        TOTAL_SEMUA = Convert.ToInt32(bruto),
                                                                                                        TOTAL_TITIPAN = 0,
                                                                                                        TRACKING_SHIPMENT = null,
                                                                                                        UCAPAN = "",
                                                                                                        USER_NAME = "Upload Excel",
                                                                                                        U_MUKA = 0,
                                                                                                        VLT = "IDR",
                                                                                                        ZONA = "",
                                                                                                        status_kirim = "0",
                                                                                                        status_print = "0"
                                                                                                    };

                                                                                                    try
                                                                                                    {
                                                                                                        eraDB.SOT01A.Add(sot01a);
                                                                                                        //transaction.Commit();
                                                                                                    }
                                                                                                    catch (Exception ex)
                                                                                                    {
                                                                                                        messageErrorLog = "terjadi error pada insert header pesanan pada row " + i;
                                                                                                        tw.WriteLine(messageErrorLog);

                                                                                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                                        if (cekLog == null)
                                                                                                        {
                                                                                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                                        (no_cust[0]),
                                                                                                        (connID),
                                                                                                        ("Upload Excel Pesanan"),
                                                                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                                        ("FAILED"),
                                                                                                        (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                                                        (username),
                                                                                                        (filename));
                                                                                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                                            // error log terjadi error pada insert header pesanan
                                                                                                        }

                                                                                                        checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == dataToko.CUST).FirstOrDefault();
                                                                                                        if (checkDuplicateHeader != null)
                                                                                                        {
                                                                                                            //transaction.Rollback();
                                                                                                            //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");
                                                                                                            eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                                                            eraDB.SaveChanges();
                                                                                                            //EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01B WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");
                                                                                                            string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                                            EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                                            new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                                        }

                                                                                                    }
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    noBuktiSO = checkDuplicateHeader.NO_BUKTI;
                                                                                                }

                                                                                                if (string.IsNullOrEmpty(diskon) || string.IsNullOrEmpty(ndisc1) || string.IsNullOrEmpty(nilai_ppn))
                                                                                                {
                                                                                                    diskon = "0";
                                                                                                    ndisc1 = "0";
                                                                                                    nilai_ppn = "0";
                                                                                                    //netto = "0";
                                                                                                    //total = "0";
                                                                                                }


                                                                                                var listBrgToUpdateStock = new List<string>();
                                                                                                var sot01b = new SOT01B
                                                                                                {
                                                                                                    NO_BUKTI = noBuktiSO,
                                                                                                    BRG = dataBarang.BRG,
                                                                                                    BRG_CUST = "",
                                                                                                    SATUAN = "2",
                                                                                                    H_SATUAN = Convert.ToInt32(harga_satuan),
                                                                                                    QTY = Convert.ToInt32(qty),
                                                                                                    DISCOUNT = Convert.ToInt32(diskon),
                                                                                                    NILAI_DISC = Convert.ToInt32(ndisc1),
                                                                                                    HARGA = Convert.ToInt32(total),
                                                                                                    WRITE_KONFIG = false,
                                                                                                    QTY_KIRIM = 0,
                                                                                                    QTY_RETUR = 0,
                                                                                                    USER_NAME = "Upload Excel",
                                                                                                    TGL_INPUT = DateTime.Now.AddHours(7),
                                                                                                    TGL_KIRIM = null,
                                                                                                    //LOKASI = "001",
                                                                                                    LOKASI= default_gudang,
                                                                                                    DISCOUNT_2 = 0,
                                                                                                    DISCOUNT_3 = 0,
                                                                                                    DISCOUNT_4 = 0,
                                                                                                    DISCOUNT_5 = 0,
                                                                                                    NILAI_DISC_1 = Convert.ToInt32(ndisc1),
                                                                                                    NILAI_DISC_2 = 0,
                                                                                                    NILAI_DISC_3 = 0,
                                                                                                    NILAI_DISC_4 = 0,
                                                                                                    NILAI_DISC_5 = 0,
                                                                                                    CATATAN = "ORDER NO : " + no_referensi + "_;_" + checkBarang.NAMA + " " + checkBarang.NAMA2 + " " + checkBarang.NAMA3 + "_;_" + dataBarang.BRG,
                                                                                                    TRANS_NO_URUT = 0,
                                                                                                    SATUAN_N = 0,
                                                                                                    QTY_N = Convert.ToInt32(qty),
                                                                                                    NTITIPAN = 0,
                                                                                                    DISC_TITIPAN = 0,
                                                                                                    TOTAL = Convert.ToInt32(total),
                                                                                                    PPN = Convert.ToInt32(nilai_ppn),
                                                                                                    NETTO = Convert.ToInt32(netto),
                                                                                                    ORDER_ITEM_ID = KodeBRGMP,
                                                                                                    STATUS_BRG = null,
                                                                                                    KET_DETAIL = keterangan
                                                                                                };

                                                                                                //add by nurul 17/9/2020
                                                                                                if (checkBarang.TYPE == "6" && checkBarang.KUBILASI == 1 && !string.IsNullOrEmpty(checkBarang.BRG_NON_OS))
                                                                                                {
                                                                                                    sot01b.BRG = checkBarang.BRG_NON_OS;
                                                                                                    sot01b.BRG_MULTISKU = dataBarang.BRG;
                                                                                                }
                                                                                                //end add by nurul 17/9/2020

                                                                                                //add by nurul 22/1/2021, bundling 
                                                                                                List<SOT01B> listPesananDetailBundling = new List<SOT01B>();
                                                                                                List<SOT01G> newPesananBundling = new List<SOT01G>();
                                                                                                var cekAdaBrgBundling = eraDB.STF03.Where(a => a.Unit == sot01b.BRG).ToList();
                                                                                                if (cekAdaBrgBundling.Count() > 0)
                                                                                                {
                                                                                                    foreach (var detailPesananKomponen in cekAdaBrgBundling)
                                                                                                    {
                                                                                                        SOT01B newPesanandetailKomponen = new SOT01B() { };
                                                                                                        newPesanandetailKomponen = sot01b;
                                                                                                        newPesanandetailKomponen.BRG = detailPesananKomponen.Brg;
                                                                                                        newPesanandetailKomponen.QTY = Convert.ToDouble(detailPesananKomponen.Qty * sot01b.QTY);
                                                                                                        newPesanandetailKomponen.H_SATUAN = Convert.ToDouble(detailPesananKomponen.HARGA);
                                                                                                        newPesanandetailKomponen.HARGA = Convert.ToDouble(detailPesananKomponen.HARGA * detailPesananKomponen.Qty * sot01b.QTY);
                                                                                                        newPesanandetailKomponen.LOKASI = gd_Bundling;
                                                                                                        newPesanandetailKomponen.BRG_BUNDLING = sot01b.BRG;
                                                                                                        listPesananDetailBundling.Add(newPesanandetailKomponen);
                                                                                                    }
                                                                                                    var totalHargaBundling = cekAdaBrgBundling.Sum(p => (double?)(p.TOTALHARGA)) ?? 0;
                                                                                                    SOT01G newPesanandetailBundling = new SOT01G()
                                                                                                    {
                                                                                                        NO_BUKTI = noBuktiSO,
                                                                                                        BRG = sot01b.BRG,
                                                                                                        QTY = sot01b.QTY,
                                                                                                        HARGA = totalHargaBundling,
                                                                                                        TGL_EDIT = DateTime.UtcNow.AddHours(7),
                                                                                                        USERNAME = "Upload Excel",
                                                                                                    };
                                                                                                    newPesananBundling.Add(newPesanandetailBundling);
                                                                                                }
                                                                                                //end add by nurul 22/1/2021, bundling 

                                                                                                try
                                                                                                {
                                                                                                    //change by nurul 22/1/2021, bundling
                                                                                                    //eraDB.SOT01B.Add(sot01b);
                                                                                                    if(listPesananDetailBundling.Count() > 0)
                                                                                                    {
                                                                                                        listBrgToUpdateStock.AddRange(listPesananDetailBundling.Select(a => a.BRG).ToList());
                                                                                                        eraDB.SOT01B.AddRange(listPesananDetailBundling);
                                                                                                        eraDB.SOT01G.AddRange(newPesananBundling);
                                                                                                        //eraDB.SOT01A.Where(a => a.NO_BUKTI == noBuktiSO).FirstOrDefault().Status_Approve = "1";
                                                                                                        var sSQLUpdateHeader = "UPDATE A SET Status_Approve ='1', BRUTO = B.BRUTO, NILAI_PPN = (((B.BRUTO - A.NILAI_DISC) * A.PPN) / 100), NETTO = (B.BRUTO + A.ONGKOS_KIRIM + (((B.BRUTO - A.NILAI_DISC) * A.PPN) / 100) - A.NILAI_DISC) " +
                                                                                                                               "FROM SOT01A A(NOLOCK) INNER JOIN " +
                                                                                                                               "(SELECT NO_BUKTI, ISNULL(SUM(ISNULL(ISNULL(QTY,0) *ISNULL(H_SATUAN, 0),0) -ISNULL(ISNULL(NILAI_DISC_1, 0) + ISNULL(NILAI_DISC_2, 0), 0)),0) AS BRUTO FROM SOT01B(NOLOCK) WHERE NO_BUKTI = '" + noBuktiSO + "' GROUP BY NO_BUKTI)B " +
                                                                                                                               "ON A.NO_BUKTI = B.NO_BUKTI WHERE A.NO_BUKTI = '" + noBuktiSO + "'";
                                                                                                        EDB.ExecuteSQL("Constring", System.Data.CommandType.Text, sSQLUpdateHeader);
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        listBrgToUpdateStock.Add(sot01b.BRG);
                                                                                                        eraDB.SOT01B.Add(sot01b);
                                                                                                    }
                                                                                                    //end change by nurul 22/1/2021, bundling
                                                                                                    eraDB.SaveChanges();
                                                                                                    //transaction.Commit();

                                                                                                    //string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                                    //EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                                    //new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);

                                                                                                    string sSQLValues = "";
                                                                                                    var listBarangUpdateStock = listBrgToUpdateStock.Where(p => p != "NOT_FOUND").Distinct().ToList();
                                                                                                    foreach (var item in listBarangUpdateStock)
                                                                                                    //foreach (var item in ListBrgProcess)
                                                                                                    {
                                                                                                        sSQLValues = sSQLValues + "('" + item + "', '" + connID + "'),";
                                                                                                    }

                                                                                                    if (sSQLValues != "")
                                                                                                    {
                                                                                                        sSQLValues = sSQLValues.Substring(0, sSQLValues.Length - 1);
                                                                                                        EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + sSQLValues);
                                                                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                                    }
                                                                                                }
                                                                                                catch (Exception ex)
                                                                                                {
                                                                                                    //if (eraDB.SOT01B.Count() > 0)
                                                                                                    //{
                                                                                                    //    eraDB.SOT01B.Remove(sot01b);
                                                                                                    //}
                                                                                                    messageErrorLog = "terjadi error pada insert detail pesanan pada row " + i;
                                                                                                    tw.WriteLine(messageErrorLog);
                                                                                                    var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                                    if (cekLog == null)
                                                                                                    {
                                                                                                        string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                                        (dataToko.CUST),
                                                                                                        (connID),
                                                                                                        ("Upload Excel Pesanan"),
                                                                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                                        ("FAILED"),
                                                                                                        (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                                                        (username),
                                                                                                        (filename));
                                                                                                        var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                                        // error log terjadi error pada insert detail pesanan
                                                                                                    }
                                                                                                    checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == dataToko.CUST).FirstOrDefault();
                                                                                                    if (checkDuplicateHeader != null)
                                                                                                    {
                                                                                                        //transaction.Rollback();
                                                                                                        eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                                                        eraDB.SaveChanges();
                                                                                                        //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                                                        string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                                        EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                                    }
                                                                                                }

                                                                                                //if (ret.percent >= 100 || ret.progress == ret.countAll - 1)
                                                                                                //{
                                                                                                //    transaction.Commit();
                                                                                                //    ret.statusSuccess = true;
                                                                                                //    return Json(ret, JsonRequestBehavior.AllowGet);
                                                                                                //}
                                                                                                iProcess = iProcess + 1;
                                                                                                success = success + 1;
                                                                                                Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                //transaction.Rollback();
                                                                                                //transaction.Commit();

                                                                                                iProcess = iProcess + 1;
                                                                                                Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                                                                int IDMarket = Convert.ToInt32(dataToko.NAMA);
                                                                                                var dataMP = MoDbContext.Marketplaces.AsNoTracking().Where(p => p.IdMarket == IDMarket).SingleOrDefault();
                                                                                                messageErrorLog = "Kode Barang " + kode_brg + " saat ini tidak link di toko " + dataToko.PERSO + " (" + dataMP.NamaMarket.ToString() + ")";
                                                                                                tw.WriteLine(messageErrorLog);
                                                                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                                if (cekLog == null)
                                                                                                {
                                                                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                                    (dataToko.CUST),
                                                                                                    (connID),
                                                                                                    ("Upload Excel Pesanan"),
                                                                                                    (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                                    ("FAILED"),
                                                                                                    (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                                                    (username),
                                                                                                    (filename));
                                                                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                                    // log error masukan ke log tidak ada databarang marketplace di STF02H
                                                                                                }
                                                                                                var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == dataToko.CUST).FirstOrDefault();
                                                                                                if (checkDuplicateHeader != null)
                                                                                                {
                                                                                                    //transaction.Rollback();
                                                                                                    eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                                                    eraDB.SaveChanges();
                                                                                                    //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                                                    string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                                    EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                                    new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            //transaction.Rollback();
                                                                                            //transaction.Commit();

                                                                                            iProcess = iProcess + 1;
                                                                                            Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                                                            int IDMarket = Convert.ToInt32(dataToko.NAMA);
                                                                                            var dataMP = MoDbContext.Marketplaces.AsNoTracking().Where(p => p.IdMarket == IDMarket).SingleOrDefault();
                                                                                            messageErrorLog = "Toko " + dataToko.PERSO + " saat ini link ke marketplaces (" + dataMP.NamaMarket.ToString() + ").";
                                                                                            tw.WriteLine(messageErrorLog);
                                                                                            var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                            if (cekLog == null)
                                                                                            {
                                                                                                string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                                (dataToko.CUST),
                                                                                                (connID),
                                                                                                ("Upload Excel Pesanan"),
                                                                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                                ("FAILED"),
                                                                                                (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                                                (username),
                                                                                                (filename));
                                                                                                var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                                // log error masukan ke log tidak ada databarang marketplace di STF02H
                                                                                            }
                                                                                            var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == dataToko.CUST).FirstOrDefault();
                                                                                            if (checkDuplicateHeader != null)
                                                                                            {
                                                                                                //transaction.Rollback();
                                                                                                eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                                                eraDB.SaveChanges();
                                                                                                //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                                                string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                                EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                                new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //transaction.Rollback();
                                                                                        //transaction.Commit();
                                                                                        iProcess = iProcess + 1;
                                                                                        Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                                                        messageErrorLog = "Kode Customer Toko " + no_cust[0] + " tidak ditemukan.";
                                                                                        tw.WriteLine(messageErrorLog);
                                                                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                        if (cekLog == null)
                                                                                        {
                                                                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                            (dataToko.CUST),
                                                                                            (connID),
                                                                                            ("Upload Excel Pesanan"),
                                                                                            (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                            ("FAILED"),
                                                                                            (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                                            (username),
                                                                                            (filename));
                                                                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                            // log error masukan log tidak ada data toko
                                                                                        }
                                                                                        var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == dataToko.CUST).FirstOrDefault();
                                                                                        if (checkDuplicateHeader != null)
                                                                                        {
                                                                                            //transaction.Rollback();
                                                                                            eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                                            eraDB.SaveChanges();
                                                                                            //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                                            string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                            EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                            new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                        }
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    //transaction.Rollback();
                                                                                    //transaction.Commit();
                                                                                    iProcess = iProcess + 1;
                                                                                    Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                                                    messageErrorLog = "Kode Kurir " + kode_kurir[0] + " tidak ditemukan.";
                                                                                    tw.WriteLine(messageErrorLog);
                                                                                    var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                    if (cekLog == null)
                                                                                    {
                                                                                        string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                        (noCust),
                                                                                        (connID),
                                                                                        ("Upload Excel Pesanan"),
                                                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                        ("FAILED"),
                                                                                        (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                                        (username),
                                                                                        (filename));
                                                                                        var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                        // log error masukan log tidak ada data kurir
                                                                                    }
                                                                                    var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == noCust).FirstOrDefault();
                                                                                    if (checkDuplicateHeader != null)
                                                                                    {
                                                                                        //transaction.Rollback();
                                                                                        eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                                        eraDB.SaveChanges();
                                                                                        //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                                        string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                        EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                    }
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                //transaction.Rollback();
                                                                                //transaction.Commit();
                                                                                iProcess = iProcess + 1;
                                                                                Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                                                messageErrorLog = "Kode Barang " + kode_brg + " tidak ditemukan.";
                                                                                tw.WriteLine(messageErrorLog);
                                                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                if (cekLog == null)
                                                                                {
                                                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                     (noCust),
                                                                                     (connID),
                                                                                     ("Upload Excel Pesanan"),
                                                                                     (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                     ("FAILED"),
                                                                                     (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                                     (username),
                                                                                     (filename));
                                                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                    //log error masukan log tidak ada barang di DB
                                                                                }
                                                                                var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == noCust).FirstOrDefault();
                                                                                if (checkDuplicateHeader != null)
                                                                                {
                                                                                    //transaction.Rollback();
                                                                                    eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                                    eraDB.SaveChanges();
                                                                                    //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                                    string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                    EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                    new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                }
                                                                            }

                                                                        }
                                                                        else
                                                                        {
                                                                            //transaction.Rollback();
                                                                            //transaction.Commit();
                                                                            iProcess = iProcess + 1;
                                                                            Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                                            messageErrorLog = "kode barang lebih dari 20 karakter pada row " + i;
                                                                            tw.WriteLine(messageErrorLog);
                                                                            var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                            if (cekLog == null)
                                                                            {
                                                                                string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                (noCust),
                                                                                (idRequest),
                                                                                ("Upload Excel Pesanan"),
                                                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                ("FAILED"),
                                                                                (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                                (username),
                                                                                (filename));
                                                                                var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                //log error masukan log kode barang lebih dari 20 karakter
                                                                            }
                                                                            var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == noCust).FirstOrDefault();
                                                                            if (checkDuplicateHeader != null)
                                                                            {
                                                                                //transaction.Rollback();
                                                                                eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                                eraDB.SaveChanges();
                                                                                //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                                string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                            }
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        //transaction.Rollback();
                                                                        //transaction.Commit();
                                                                        iProcess = iProcess + 1;
                                                                        Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                                        var errorMessage = "";
                                                                        if (string.IsNullOrEmpty(qty))
                                                                        {
                                                                            errorMessage = "qty kosong pada row " + i;
                                                                        }
                                                                        else
                                                                        {
                                                                            errorMessage = "harga satuan kosong pada row " + i;
                                                                        }
                                                                        messageErrorLog = errorMessage;
                                                                        tw.WriteLine(messageErrorLog);
                                                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                        if (cekLog == null)
                                                                        {
                                                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                            (noCust),
                                                                            (idRequest),
                                                                            ("Upload Excel Pesanan"),
                                                                            (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                            ("FAILED"),
                                                                            (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                            (username),
                                                                            (filename));
                                                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                            //log error masukan log harga satuan kosong
                                                                        }
                                                                        var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == noCust).FirstOrDefault();
                                                                        if (checkDuplicateHeader != null)
                                                                        {
                                                                            //transaction.Rollback();
                                                                            eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                            eraDB.SaveChanges();
                                                                            //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                            string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                            EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                            new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    //transaction.Rollback();
                                                                    //transaction.Commit();
                                                                    iProcess = iProcess + 1;
                                                                    Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                                    messageErrorLog = "kode barang kosong pada row " + i;
                                                                    tw.WriteLine(messageErrorLog);
                                                                    var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                    if (cekLog == null)
                                                                    {
                                                                        string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                        (noCust),
                                                                        (idRequest),
                                                                        ("Upload Excel Pesanan"),
                                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                        ("FAILED"),
                                                                        (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                        (username),
                                                                        (filename));
                                                                        var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                        //log error masukan log kode barang kosong
                                                                    }
                                                                    var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == noCust).FirstOrDefault();
                                                                    if (checkDuplicateHeader != null)
                                                                    {
                                                                        //transaction.Rollback();
                                                                        eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                        eraDB.SaveChanges();
                                                                        //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                        string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                        EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //transaction.Rollback();
                                                                //transaction.Commit();
                                                                iProcess = iProcess + 1;
                                                                Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                                messageErrorLog = "terdapat karakter koma pada kolom pengisian angka di row " + i;
                                                                tw.WriteLine(messageErrorLog);
                                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                if (cekLog == null)
                                                                {
                                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                    (noCust),
                                                                    (idRequest),
                                                                    ("Upload Excel Pesanan"),
                                                                    (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                    ("FAILED"),
                                                                    (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                    (username),
                                                                    (filename));
                                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                    //log error masukan log ada koma
                                                                }
                                                                var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == noCust).FirstOrDefault();
                                                                if (checkDuplicateHeader != null)
                                                                {
                                                                    //transaction.Rollback();
                                                                    eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                    eraDB.SaveChanges();
                                                                    //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                    string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                    EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                    new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //transaction.Rollback();
                                                            //transaction.Commit();
                                                            iProcess = iProcess + 1;
                                                            Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                            messageErrorLog = "terdapat karakter titik pada kolom pengisian angka di row " + i;
                                                            tw.WriteLine(messageErrorLog);
                                                            var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                            if (cekLog == null)
                                                            {
                                                                string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                (noCust),
                                                                (idRequest),
                                                                ("Upload Excel Pesanan"),
                                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                ("FAILED"),
                                                                (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                                (username),
                                                                (filename));
                                                                var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                //log error masukan log ada titik
                                                            }
                                                            var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == noCust).FirstOrDefault();
                                                            if (checkDuplicateHeader != null)
                                                            {
                                                                //transaction.Rollback();
                                                                eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                                eraDB.SaveChanges();
                                                                //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                                string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //transaction.Rollback();
                                                        //transaction.Commit();
                                                        iProcess = iProcess + 1;
                                                        Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                        messageErrorLog = "no telepon kosong di row " + i;
                                                        tw.WriteLine(messageErrorLog);
                                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                        if (cekLog == null)
                                                        {
                                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                            (""),
                                                            (idRequest),
                                                            ("Upload Excel Pesanan"),
                                                            (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                            ("FAILED"),
                                                            (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                            (username),
                                                            (filename));
                                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                            //log error masukan log tidak ada no telepon
                                                        }
                                                        var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == noCust).FirstOrDefault();
                                                        if (checkDuplicateHeader != null)
                                                        {
                                                            //transaction.Rollback();
                                                            eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                            eraDB.SaveChanges();
                                                            //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                            string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                            EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                            new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //transaction.Rollback();
                                                    //transaction.Commit();
                                                    iProcess = iProcess + 1;
                                                    Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                    messageErrorLog = "kode kurir kosong pada row " + i;
                                                    tw.WriteLine(messageErrorLog);
                                                    string[] no_cust2 = marketplace.Split(';');
                                                    var noCust = no_cust2[0];
                                                    var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                    if (cekLog == null)
                                                    {
                                                        string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                        (noCust),
                                                        (idRequest),
                                                        ("Upload Excel Pesanan"),
                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                        ("FAILED"),
                                                        (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                        (username),
                                                        (filename));
                                                        var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                        //log error masukan log kode kurir kosong
                                                    }
                                                    var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == noCust).FirstOrDefault();
                                                    if (checkDuplicateHeader != null)
                                                    {
                                                        //transaction.Rollback();
                                                        eraDB.SOT01A.Remove(checkDuplicateHeader);
                                                        eraDB.SaveChanges();
                                                        //var result1 = EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");

                                                        string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                        EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                iProcess = iProcess + 1;
                                                Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                                messageErrorLog = "marketplace kosong pada row " + i;
                                                tw.WriteLine(messageErrorLog);
                                                string[] no_cust2 = marketplace.Split(';');
                                                var noCust = no_cust2[0];
                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                                if (cekLog == null)
                                                {
                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                    (noCust),
                                                    (idRequest),
                                                    ("Upload Excel Pesanan"),
                                                    (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                    ("FAILED"),
                                                    (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                    (username),
                                                    (filename));
                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                    //log error masukan log marketplace kosong
                                                }
                                                //var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == noCust).FirstOrDefault();
                                                //EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");
                                                //EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01B WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");
                                                //new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                            }
                                        }
                                        else
                                        {
                                            iProcess = iProcess + 1;
                                            Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                            messageErrorLog = "no referensi lebih dari 70 karakter pada row " + i;
                                            tw.WriteLine(messageErrorLog);
                                            var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                            if (cekLog == null)
                                            {
                                                string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                (""),
                                                (idRequest),
                                                ("Upload Excel Pesanan"),
                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                ("FAILED"),
                                                (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                                (username),
                                                (filename));
                                                var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                //log error masukan log noref lebih dari 70 karakter
                                            }
                                            //var checkDuplicateHeader = eraDB.SOT01A.Where(p => p.NO_REFERENSI == no_referensi && p.CUST == dataToko.CUST).FirstOrDefault();
                                            //EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01A WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");
                                            //EDB.ExecuteSQL("Constring", CommandType.Text, "DELETE FROM SOT01B WHERE NO_BUKTI ='" + checkDuplicateHeader.NO_BUKTI + "'");
                                            //new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                        }
                                    }
                                    else
                                    {
                                        iProcess = iProcess + 1;
                                        Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                        if (string.IsNullOrEmpty(kode_brg))
                                        {
                                            messageErrorLog = "kode barang kosong pada row " + i;
                                        }
                                        else
                                        {
                                            messageErrorLog = "no referensi kosong pada row " + i;
                                        }
                                        messageErrorLog = "no referensi kosong pada row " + i;
                                        tw.WriteLine(messageErrorLog);
                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Pesanan" && p.REQUEST_ID == connID).FirstOrDefault();
                                        if (cekLog == null)
                                        {
                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                            (""),
                                            (idRequest),
                                            ("Upload Excel Pesanan"),
                                            (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                            ("FAILED"),
                                            (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                            (username),
                                            (filename));
                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                            //log error masukan log no referensi kosong
                                        }
                                    }

                                } // end looping



                                //eraDB.TEMP_UPLOADPESANAN.AddRange(batchinsertItem);
                                //eraDB.SaveChanges();
                                //transaction.Commit();

                            }
                            catch (Exception ex)
                            {
                                 tw.WriteLine(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
                                //transaction.Rollback();
                                //new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
                            }

                            tw.Close();
                            //}
                        }
                    }
                }
            }

            //}
            //catch (Exception ex)
            //{
            //    ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            //}

            return Json(ret, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Upload Excel Pesanan
        public async Task<ActionResult> UploadXcelInvoicePembelian(string nobuk, int countAll, string percentDanprogress, string statusLoopSuccess)
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

            //try
            //{
            ret.statusLoop = Convert.ToBoolean(status[0]);
            ret.statusSuccess = Convert.ToBoolean(status[1]);

            if (ret.byteData == null && ret.statusLoop == false)
            {
                if (Request.Files[0].ContentType.Contains("application/vnd.ms-excel"))
                {
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
                        using (ErasoftContext eraDB = new ErasoftContext(DataSourcePath, dbPathEra))
                        {
                            //using (System.Data.Entity.DbContextTransaction transaction = eraDB.Database.BeginTransaction())
                            //{
                            string connID = Guid.NewGuid().ToString();

                            //initialize log txt
                            #region Logging
                            string messageErrorLog = "";
                            string filename = "Log_Upload_InvoicePembelian_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
                            var path = Path.Combine(Server.MapPath("~/Content/Uploaded/" + dbPathEra + "/"), filename);
                            #endregion

                            if (!System.IO.File.Exists(path))
                            {
                                System.IO.Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Content/Uploaded/" + dbPathEra + "/"), ""));
                                var asd = System.IO.File.Create(path);
                                asd.Close();
                            }
                            StreamWriter tw = new StreamWriter(path);

                            try
                            {
                                eraDB.Database.CommandTimeout = 1800;
                                //loop all worksheets
                                var worksheet = excelPackage.Workbook.Worksheets[1];

                                ret.countAll = Convert.ToInt32(worksheet.Dimension.End.Row - 9);

                                if (Convert.ToInt32(prog[1]) == 0)
                                {
                                    prog[1] = "0";
                                }

                                var noBuktiPB = "";

                                //change by nurul 17/9/2020, brg multi sku 
                                //var dataMasterSTF02 = eraDB.STF02.Select(p => new { p.BRG, p.NAMA, p.NAMA2, p.NAMA3 }).ToList();
                                var dataMasterSTF02 = eraDB.STF02.AsNoTracking().Select(p => new { p.BRG, p.NAMA, p.NAMA2, p.NAMA3, p.TYPE, p.KUBILASI, p.BRG_NON_OS }).ToList();
                                var dataMasterSupplier = eraDB.APF01.AsNoTracking().Select(p => new { p.SUPP, p.NAMA }).ToList();
                                //end change by nurul 17/9/2020, brg multi sku 
                                //var dataMasterSTF02H = eraDB.STF02H.AsNoTracking().Select(p => new { p.BRG, p.BRG_MP, p.IDMARKET }).ToList();
                                //var dataMasterKurir = MoDbContext.Ekspedisi.AsNoTracking().ToList();
                                //var dataMasterARF01 = eraDB.ARF01.AsNoTracking().ToList();

                                //eraDB.Database.ExecuteSqlCommand("DELETE FROM TEMP_UPLOADPESANAN");
                                //List<TEMP_UPLOADPESANAN> batchinsertItem = new List<TEMP_UPLOADPESANAN>();
                                string queryInsertLogError = "INSERT INTO API_LOG_MARKETPLACE (CUST, REQUEST_ID, REQUEST_ACTION, REQUEST_DATETIME, REQUEST_STATUS, REQUEST_RESULT, CUST_ATTRIBUTE_1, REQUEST_EXCEPTION) VALUES ";

                                //batchinsertItem = new List<TEMP_UPLOADPESANAN>();

                                string idRequest = Guid.NewGuid().ToString();

                                int iProcess = 0;
                                int success = 0;

                                #region PROSES_UPLOAD_EXCEL_PEMBELIAN_VERSI1
                                //// START PROSES UPLOAD EXCEL INVOICE PEMBELIAN VERSI 1 04/03/2021 by Fauzi
                                //// validasi jika file baru lagi dengan no referensi dan no cust sama tidak boleh proses
                                //// add by fauzi 23/09/2020
                                //#region validasi duplicate dari file berbeda
                                //var dataNoBuktiCodeSupplier = new List<string>();

                                //for (int i = Convert.ToInt32(prog[0]); i <= worksheet.Dimension.End.Row; i++)
                                //{
                                //    string tgl = worksheet.Cells[i, 2].Value == null ? "" : worksheet.Cells[i, 2].Value.ToString();
                                //    string kode_supplier = worksheet.Cells[i, 3].Value == null ? "" : worksheet.Cells[i, 3].Value.ToString();
                                //    string kode_barang = worksheet.Cells[i, 7].Value == null ? "" : worksheet.Cells[i, 7].Value.ToString();
                                //    if (!string.IsNullOrEmpty(tgl) && !string.IsNullOrEmpty(kode_supplier) && !string.IsNullOrEmpty(kode_barang))
                                //    {
                                //        tgl = Convert.ToDateTime(tgl).ToString("yyyy-MM-dd");
                                //        dataNoBuktiCodeSupplier.Add(tgl + ";" + kode_supplier);
                                //    }
                                //    else if (string.IsNullOrEmpty(tgl))
                                //    {
                                //        messageErrorLog = "Tanggal invoice pembelian kosong. Proses Upload dibatalkan.";
                                //        tw.WriteLine(messageErrorLog);
                                //    }
                                //    else if (string.IsNullOrEmpty(kode_supplier))
                                //    {
                                //        messageErrorLog = "Kode Supplier invoice pembelian kosong. Proses Upload dibatalkan.";
                                //        tw.WriteLine(messageErrorLog);
                                //    }
                                //    else if (string.IsNullOrEmpty(kode_barang))
                                //    {
                                //        messageErrorLog = "Kode barang invoice pembelian kosong. Proses Upload dibatalkan.";
                                //        tw.WriteLine(messageErrorLog);
                                //    }



                                //    if (!string.IsNullOrEmpty(messageErrorLog))
                                //    {
                                //        tw.Close();
                                //        tw.Dispose();

                                //        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //        if (cekLog == null)
                                //        {
                                //            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //        (""),
                                //        (connID),
                                //        ("Upload Excel Invoice Pembelian"),
                                //        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //        ("FAILED"),
                                //        (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //        (username),
                                //        (filename));
                                //            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //            // error log terjadi error pada insert header pesanan
                                //        }
                                //        ret.Errors.Add(messageErrorLog);
                                //        return Json(ret, JsonRequestBehavior.AllowGet);
                                //    }
                                //}

                                //double Tempbruto = 0;
                                //double Tempnetto = 0;
                                //double TempPPNPersen = 0;
                                //double TempPPN = 0;
                                //double TempOngkir = 0;
                                //double TempTotalHargaBarang = 0;

                                //if (dataNoBuktiCodeSupplier.Count() > 0)
                                //{
                                //    var dataFilterRef = dataNoBuktiCodeSupplier.Distinct().ToList();
                                //    if (dataFilterRef.Count() > 0)
                                //    {
                                //        foreach (var refCheck in dataFilterRef)
                                //        {
                                //            string[] splitRef = refCheck.Split(';');
                                //            DateTime resTgl = Convert.ToDateTime(splitRef[0].ToString());
                                //            var resCodeSupplier = splitRef[1].ToString();
                                //            //var resKodeBarang = splitRef[2].ToString();
                                //            var checkDB = eraDB.PBT01A.AsNoTracking().Where(c => c.TGL == resTgl && c.SUPP == resCodeSupplier).SingleOrDefault();
                                //            if (checkDB != null)
                                //            {
                                //                messageErrorLog = "Tanggal invoice pembelian " + resTgl + " dan Kode Supplier " + resCodeSupplier + " sudah pernah dimasukan. Proses Upload dibatalkan.";
                                //                tw.WriteLine(messageErrorLog);
                                //                tw.Close();
                                //                tw.Dispose();

                                //                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                if (cekLog == null)
                                //                {
                                //                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                (resCodeSupplier),
                                //                (connID),
                                //                ("Upload Excel Invoice Pembelian"),
                                //                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                ("FAILED"),
                                //                (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                (username),
                                //                (filename));
                                //                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                    // error log terjadi error pada insert header pesanan
                                //                }
                                //                ret.Errors.Add("Tanggal invoice pembelian " + resTgl + " dan Kode Supplier " + resCodeSupplier + " sudah pernah dimasukan. Proses Upload dibatalkan.");
                                //                return Json(ret, JsonRequestBehavior.AllowGet);
                                //            }
                                //            else
                                //            {
                                //                // start looping
                                //                for (int i = Convert.ToInt32(prog[0]); i <= worksheet.Dimension.End.Row; i++)
                                //                {
                                //                    ret.statusLoop = true;
                                //                    ret.progress = i;

                                //                    //get ALL DATA
                                //                    string no_bukti = worksheet.Cells[i, 1].Value == null ? "" : worksheet.Cells[i, 1].Value.ToString();
                                //                    string tgl = worksheet.Cells[i, 2].Value == null ? "" : worksheet.Cells[i, 2].Value.ToString();
                                //                    string kode_supplier = worksheet.Cells[i, 3].Value == null ? "" : worksheet.Cells[i, 3].Value.ToString();
                                //                    string top = worksheet.Cells[i, 4].Value == null ? "" : worksheet.Cells[i, 4].Value.ToString();
                                //                    string ppn = worksheet.Cells[i, 5].Value == null ? "0" : worksheet.Cells[i, 5].Value.ToString();
                                //                    //string nilai_ppn = worksheet.Cells[i, 6].Value == null ? "0" : worksheet.Cells[i, 6].Value.ToString();
                                //                    string ongkir = worksheet.Cells[i, 6].Value == null ? "0" : worksheet.Cells[i, 6].Value.ToString();
                                //                    string kode_brg = worksheet.Cells[i, 7].Value == null ? "" : worksheet.Cells[i, 7].Value.ToString();
                                //                    string nama_brg = worksheet.Cells[i, 8].Value == null ? "" : worksheet.Cells[i, 8].Value.ToString();
                                //                    string gudang = worksheet.Cells[i, 9].Value == null ? "" : worksheet.Cells[i, 9].Value.ToString();
                                //                    string qty = worksheet.Cells[i, 10].Value == null ? "0" : worksheet.Cells[i, 10].Value.ToString();
                                //                    string harga_satuan = worksheet.Cells[i, 11].Value == null ? "0" : worksheet.Cells[i, 11].Value.ToString();
                                //                    string total_nilaidisc = worksheet.Cells[i, 12].Value == null ? "0" : worksheet.Cells[i, 12].Value.ToString();
                                //                    string total = worksheet.Cells[i, 13].Value == null ? "0" : worksheet.Cells[i, 13].Value.ToString();


                                //                    if (kode_supplier.Contains("Silahkan"))
                                //                    {
                                //                        kode_supplier = "";
                                //                    }

                                //                    if (gudang.Contains("Silahkan"))
                                //                    {
                                //                        gudang = "";
                                //                    }

                                //                    if (!string.IsNullOrEmpty(tgl))
                                //                    {
                                //                        tgl = Convert.ToDateTime(tgl).ToString("yyyy-MM-dd");
                                //                        DateTime dttgl = Convert.ToDateTime(tgl);
                                //                        if (!string.IsNullOrEmpty(kode_brg))
                                //                        {
                                //                            if (!string.IsNullOrEmpty(kode_supplier))
                                //                            {
                                //                                if(resCodeSupplier == kode_supplier && resTgl == dttgl)
                                //                                {
                                //                                    var namaSupplier = dataMasterSupplier.Where(p => p.SUPP == kode_supplier).SingleOrDefault().NAMA;

                                //                                    if (!string.IsNullOrEmpty(gudang))
                                //                                    {
                                //                                        if (!top.Contains(".") || !ppn.Contains(".") || !ongkir.Contains(".") || !qty.Contains(".") || !harga_satuan.Contains(".") || !total_nilaidisc.Contains(".") || !total.Contains("."))
                                //                                        {
                                //                                            if (!top.Contains(",") || !ppn.Contains(",") || !ongkir.Contains(",") || !qty.Contains(",") || !harga_satuan.Contains(",") || !total_nilaidisc.Contains(",") || !total.Contains(","))
                                //                                            {
                                //                                                if (!string.IsNullOrEmpty(kode_brg))
                                //                                                {
                                //                                                    if (!string.IsNullOrEmpty(harga_satuan) || !string.IsNullOrEmpty(qty))
                                //                                                    {
                                //                                                        if (kode_brg.Length <= 20)
                                //                                                        {
                                //                                                            //var checkBarang = ErasoftDbContext.STF02.Where(p => p.BRG == item.KODE_BRG).Select(p => p.BRG).FirstOrDefault();
                                //                                                            var checkBarang = dataMasterSTF02.Where(p => p.BRG == kode_brg).FirstOrDefault();
                                //                                                            if (checkBarang != null)
                                //                                                            {
                                //                                                                if (string.IsNullOrEmpty(total_nilaidisc) || string.IsNullOrEmpty(total) || string.IsNullOrEmpty(harga_satuan) || string.IsNullOrEmpty(qty))
                                //                                                                {
                                //                                                                    total_nilaidisc = "0";
                                //                                                                    total = "0";
                                //                                                                    harga_satuan = "0";
                                //                                                                    qty = "0";
                                //                                                                    //total = "0";
                                //                                                                }
                                //                                                                if(Convert.ToInt32(total_nilaidisc) > 0)
                                //                                                                {
                                //                                                                    TempTotalHargaBarang = (Convert.ToInt32(harga_satuan) * Convert.ToInt32(qty) - Convert.ToInt32(total_nilaidisc));
                                //                                                                }
                                //                                                                else
                                //                                                                {
                                //                                                                    TempTotalHargaBarang = Convert.ToInt32(harga_satuan) * Convert.ToInt32(qty);
                                //                                                                }

                                //                                                                Tempbruto += TempTotalHargaBarang;

                                //                                                                if(TempPPNPersen == 0)
                                //                                                                {
                                //                                                                    TempPPNPersen = Convert.ToInt32(ppn);
                                //                                                                    //TempPPN = Convert.ToInt32(nilai_ppn);
                                //                                                                }

                                //                                                                if(TempOngkir == 0)
                                //                                                                {
                                //                                                                    TempOngkir = Convert.ToInt32(ongkir);
                                //                                                                }

                                //                                                                var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                //                                                                if (checkDuplicateHeader == null)
                                //                                                                {
                                //                                                                    var lastBukti = new ManageController().GenerateAutoNumber(eraDB, "PB", "PBT01A", "INV");
                                //                                                                    var nopb = "PB" + DateTime.UtcNow.AddHours(7).Year.ToString().Substring(2, 2) + Convert.ToString(Convert.ToInt32(lastBukti) + 1).PadLeft(6, '0');
                                //                                                                    noBuktiPB = nopb;

                                //                                                                    if (string.IsNullOrEmpty(top))
                                //                                                                    {
                                //                                                                        top = "0";
                                //                                                                    }

                                //                                                                    var pbt01a = new PBT01A
                                //                                                                    {
                                //                                                                        JENISFORM = "1",
                                //                                                                        INV = noBuktiPB,
                                //                                                                        TGL = Convert.ToDateTime(tgl),
                                //                                                                        JENIS = "1",
                                //                                                                        PO = "-",
                                //                                                                        STATUS = "1",
                                //                                                                        POSTING = "-",
                                //                                                                        SUPP = kode_supplier,
                                //                                                                        NAMA = namaSupplier,
                                //                                                                        VLT = "IDR",
                                //                                                                        TERM = Convert.ToInt16(top),
                                //                                                                        BIAYA_LAIN = Convert.ToInt32(ongkir),
                                //                                                                        PPN = Convert.ToInt32(ppn),
                                //                                                                        //NPPN = Convert.ToInt32(nilai_ppn),
                                //                                                                        NPPN = 0,
                                //                                                                        BRUTO = 0,
                                //                                                                        NETTO = 0,
                                //                                                                        USERNAME = username,
                                //                                                                        TGLINPUT = DateTime.Now.AddHours(7),
                                //                                                                        TGJT = DateTime.Now.AddHours(7).AddDays(Convert.ToInt32(top)),
                                //                                                                        KET = "-",
                                //                                                                        APP = "-",
                                //                                                                        REF = "-",
                                //                                                                        NO_INVOICE_SUPP = "-",
                                //                                                                    };

                                //                                                                    try
                                //                                                                    {
                                //                                                                        eraDB.PBT01A.Add(pbt01a);
                                //                                                                        //transaction.Commit();
                                //                                                                    }
                                //                                                                    catch (Exception ex)
                                //                                                                    {
                                //                                                                        messageErrorLog = "terjadi error pada insert header invoice pembelian pada row " + i;
                                //                                                                        tw.WriteLine(messageErrorLog);

                                //                                                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                                                                        if (cekLog == null)
                                //                                                                        {
                                //                                                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                                                        (kode_supplier),
                                //                                                                        (connID),
                                //                                                                        ("Upload Excel Invoice Pembelian"),
                                //                                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                                                        ("FAILED"),
                                //                                                                        (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                                                        (username),
                                //                                                                        (filename));
                                //                                                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                                                            // error log terjadi error pada insert header pesanan
                                //                                                                        }

                                //                                                                        checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                //                                                                        if (checkDuplicateHeader != null)
                                //                                                                        {
                                //                                                                            //transaction.Rollback();
                                //                                                                            eraDB.PBT01A.Remove(checkDuplicateHeader);
                                //                                                                            eraDB.SaveChanges();
                                //                                                                            string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                //                                                                            EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                //                                                                            new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                //                                                                        }

                                //                                                                    }
                                //                                                                }
                                //                                                                else
                                //                                                                {
                                //                                                                    noBuktiPB = checkDuplicateHeader.INV;
                                //                                                                    var checkDetailPB = eraDB.PBT01B.Where(p => p.INV == noBuktiPB).ToList();
                                //                                                                    if (checkDetailPB.Count() > 0)
                                //                                                                    {
                                //                                                                        EDB.ExecuteSQL("Constring", CommandType.Text, "UPDATE PBT01B SET THARGA = " + TempTotalHargaBarang + " WHERE TGL = '" + dttgl.ToString("yyyy-MM-dd") + "' AND SUPP = '" + kode_supplier + "' AND INV = '" + noBuktiPB + "'");
                                //                                                                    }
                                //                                                                }



                                //                                                                //var listBrgToUpdateStock = new List<string>();
                                //                                                                var pbt01b = new PBT01B
                                //                                                                {
                                //                                                                    JENISFORM = "1",
                                //                                                                    INV = noBuktiPB,
                                //                                                                    PO = "-",
                                //                                                                    BRG = kode_brg,
                                //                                                                    GD = gudang,
                                //                                                                    BK = "2",
                                //                                                                    QTY = Convert.ToInt32(qty),
                                //                                                                    HBELI = Convert.ToInt32(harga_satuan),
                                //                                                                    THARGA = (TempTotalHargaBarang),
                                //                                                                    USERNAME = username,
                                //                                                                    TGLINPUT = DateTime.Now.AddHours(7),
                                //                                                                    DISCOUNT_1 = 0,
                                //                                                                    NILAI_DISC_1 = Convert.ToInt32(total_nilaidisc),
                                //                                                                    NOBUK = "-",
                                //                                                                    AUTO_LOAD = "-",
                                //                                                                    KET = "-",
                                //                                                                    BRG_ORIGINAL = "-",
                                //                                                                    LKU = "-",
                                //                                                                };

                                //                                                                try
                                //                                                                {
                                //                                                                    eraDB.PBT01B.Add(pbt01b);
                                //                                                                    eraDB.SaveChanges();

                                //                                                                    string sSQLValues = "";
                                //                                                                    sSQLValues = sSQLValues + "('" + kode_brg + "', '" + connID + "')";
                                //                                                                    if (sSQLValues != "")
                                //                                                                    {
                                //                                                                        EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + sSQLValues);
                                //                                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                //                                                                    }
                                //                                                                }
                                //                                                                catch (Exception ex)
                                //                                                                {
                                //                                                                    messageErrorLog = "terjadi error pada insert detail invoice pembelian pada row " + i;
                                //                                                                    tw.WriteLine(messageErrorLog);
                                //                                                                    var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                                                                    if (cekLog == null)
                                //                                                                    {
                                //                                                                        string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                                                        (kode_supplier),
                                //                                                                        (connID),
                                //                                                                        ("Upload Excel Invoice Pembelian"),
                                //                                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                                                        ("FAILED"),
                                //                                                                        (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                                                        (username),
                                //                                                                        (filename));
                                //                                                                        var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                                                        // error log terjadi error pada insert detail pesanan
                                //                                                                    }
                                //                                                                    checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                //                                                                    if (checkDuplicateHeader != null)
                                //                                                                    {
                                //                                                                        //transaction.Rollback();
                                //                                                                        eraDB.PBT01A.Remove(checkDuplicateHeader);
                                //                                                                        eraDB.SaveChanges();

                                //                                                                        string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                //                                                                        EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                //                                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                //                                                                    }
                                //                                                                }

                                //                                                                //if (ret.percent >= 100 || ret.progress == ret.countAll - 1)
                                //                                                                //{
                                //                                                                //    transaction.Commit();
                                //                                                                //    ret.statusSuccess = true;
                                //                                                                //    return Json(ret, JsonRequestBehavior.AllowGet);
                                //                                                                //}
                                //                                                                iProcess = iProcess + 1;
                                //                                                                success = success + 1;
                                //                                                                Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));


                                //                                                            }
                                //                                                            else
                                //                                                            {
                                //                                                                //transaction.Rollback();
                                //                                                                //transaction.Commit();
                                //                                                                iProcess = iProcess + 1;
                                //                                                                Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                //                                                                messageErrorLog = "Kode Barang " + kode_brg + " tidak ditemukan.";
                                //                                                                tw.WriteLine(messageErrorLog);
                                //                                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                                                                if (cekLog == null)
                                //                                                                {
                                //                                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                                                     (kode_supplier),
                                //                                                                     (connID),
                                //                                                                     ("Upload Excel Invoice Pembelian"),
                                //                                                                     (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                                                     ("FAILED"),
                                //                                                                     (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                                                     (username),
                                //                                                                     (filename));
                                //                                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                                                    //log error masukan log tidak ada barang di DB
                                //                                                                }
                                //                                                                var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                //                                                                if (checkDuplicateHeader != null)
                                //                                                                {
                                //                                                                    //transaction.Rollback();
                                //                                                                    eraDB.PBT01A.Remove(checkDuplicateHeader);
                                //                                                                    eraDB.SaveChanges();

                                //                                                                    string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                //                                                                    EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                //                                                                    new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                //                                                                }
                                //                                                            }

                                //                                                        }
                                //                                                        else
                                //                                                        {
                                //                                                            //transaction.Rollback();
                                //                                                            //transaction.Commit();
                                //                                                            iProcess = iProcess + 1;
                                //                                                            Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                //                                                            messageErrorLog = "kode barang lebih dari 20 karakter pada row " + i;
                                //                                                            tw.WriteLine(messageErrorLog);
                                //                                                            var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                                                            if (cekLog == null)
                                //                                                            {
                                //                                                                string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                                                (kode_supplier),
                                //                                                                (idRequest),
                                //                                                                ("Upload Excel Invoice Pembelian"),
                                //                                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                                                ("FAILED"),
                                //                                                                (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                                                (username),
                                //                                                                (filename));
                                //                                                                var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                                                //log error masukan log kode barang lebih dari 20 karakter
                                //                                                            }
                                //                                                            var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                //                                                            if (checkDuplicateHeader != null)
                                //                                                            {
                                //                                                                //transaction.Rollback();
                                //                                                                eraDB.PBT01A.Remove(checkDuplicateHeader);
                                //                                                                eraDB.SaveChanges();

                                //                                                                string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                //                                                                EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                //                                                                new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                //                                                            }
                                //                                                        }
                                //                                                    }
                                //                                                    else
                                //                                                    {
                                //                                                        //transaction.Rollback();
                                //                                                        //transaction.Commit();
                                //                                                        iProcess = iProcess + 1;
                                //                                                        Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                //                                                        var errorMessage = "";
                                //                                                        if (string.IsNullOrEmpty(qty))
                                //                                                        {
                                //                                                            errorMessage = "qty kosong pada row " + i;
                                //                                                        }
                                //                                                        else
                                //                                                        {
                                //                                                            errorMessage = "harga satuan kosong pada row " + i;
                                //                                                        }
                                //                                                        messageErrorLog = errorMessage;
                                //                                                        tw.WriteLine(messageErrorLog);
                                //                                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                                                        if (cekLog == null)
                                //                                                        {
                                //                                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                                            (kode_supplier),
                                //                                                            (idRequest),
                                //                                                            ("Upload Excel Invoice Pembelian"),
                                //                                                            (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                                            ("FAILED"),
                                //                                                            (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                                            (username),
                                //                                                            (filename));
                                //                                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                                            //log error masukan log harga satuan kosong
                                //                                                        }
                                //                                                        var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                //                                                        if (checkDuplicateHeader != null)
                                //                                                        {
                                //                                                            //transaction.Rollback();
                                //                                                            eraDB.PBT01A.Remove(checkDuplicateHeader);
                                //                                                            eraDB.SaveChanges();

                                //                                                            string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                //                                                            EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                //                                                            new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                //                                                        }
                                //                                                    }
                                //                                                }
                                //                                                else
                                //                                                {
                                //                                                    //transaction.Rollback();
                                //                                                    //transaction.Commit();
                                //                                                    iProcess = iProcess + 1;
                                //                                                    Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                //                                                    messageErrorLog = "kode barang kosong pada row " + i;
                                //                                                    tw.WriteLine(messageErrorLog);
                                //                                                    var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                                                    if (cekLog == null)
                                //                                                    {
                                //                                                        string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                                        (kode_supplier),
                                //                                                        (idRequest),
                                //                                                        ("Upload Excel Invoice Pembelian"),
                                //                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                                        ("FAILED"),
                                //                                                        (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                                        (username),
                                //                                                        (filename));
                                //                                                        var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                                        //log error masukan log kode barang kosong
                                //                                                    }
                                //                                                    var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                //                                                    if (checkDuplicateHeader != null)
                                //                                                    {
                                //                                                        //transaction.Rollback();
                                //                                                        eraDB.PBT01A.Remove(checkDuplicateHeader);
                                //                                                        eraDB.SaveChanges();

                                //                                                        string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                //                                                        EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                //                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                //                                                    }
                                //                                                }
                                //                                            }
                                //                                            else
                                //                                            {
                                //                                                //transaction.Rollback();
                                //                                                //transaction.Commit();
                                //                                                iProcess = iProcess + 1;
                                //                                                Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                //                                                messageErrorLog = "terdapat karakter koma pada kolom pengisian angka di row " + i;
                                //                                                tw.WriteLine(messageErrorLog);
                                //                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                                                if (cekLog == null)
                                //                                                {
                                //                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                                    (kode_supplier),
                                //                                                    (idRequest),
                                //                                                    ("Upload Excel Invoice Pembelian"),
                                //                                                    (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                                    ("FAILED"),
                                //                                                    (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                                    (username),
                                //                                                    (filename));
                                //                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                                    //log error masukan log ada koma
                                //                                                }
                                //                                                var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                //                                                if (checkDuplicateHeader != null)
                                //                                                {
                                //                                                    //transaction.Rollback();
                                //                                                    eraDB.PBT01A.Remove(checkDuplicateHeader);
                                //                                                    eraDB.SaveChanges();

                                //                                                    string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                //                                                    EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                //                                                    new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                //                                                }
                                //                                            }
                                //                                        }
                                //                                        else
                                //                                        {
                                //                                            //transaction.Rollback();
                                //                                            //transaction.Commit();
                                //                                            iProcess = iProcess + 1;
                                //                                            Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                //                                            messageErrorLog = "terdapat karakter titik pada kolom pengisian angka di row " + i;
                                //                                            tw.WriteLine(messageErrorLog);
                                //                                            var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                                            if (cekLog == null)
                                //                                            {
                                //                                                string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                                (kode_supplier),
                                //                                                (idRequest),
                                //                                                ("Upload Excel Invoice Pembelian"),
                                //                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                                ("FAILED"),
                                //                                                (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                                (username),
                                //                                                (filename));
                                //                                                var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                                //log error masukan log ada titik
                                //                                            }
                                //                                            var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                //                                            if (checkDuplicateHeader != null)
                                //                                            {
                                //                                                //transaction.Rollback();
                                //                                                eraDB.PBT01A.Remove(checkDuplicateHeader);
                                //                                                eraDB.SaveChanges();

                                //                                                string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                //                                                EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                //                                                new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                //                                            }
                                //                                        }

                                //                                    }
                                //                                    else
                                //                                    {
                                //                                        //transaction.Rollback();
                                //                                        //transaction.Commit();
                                //                                        iProcess = iProcess + 1;
                                //                                        Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                //                                        messageErrorLog = "kode gudang kosong pada row " + i;
                                //                                        tw.WriteLine(messageErrorLog);
                                //                                        string[] codeSup = kode_supplier.Split(';');
                                //                                        var codeSupplier = codeSup[0];
                                //                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                                        if (cekLog == null)
                                //                                        {
                                //                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                            (kode_supplier),
                                //                                            (idRequest),
                                //                                            ("Upload Excel Invoice Pembelian"),
                                //                                            (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                            ("FAILED"),
                                //                                            (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                            (username),
                                //                                            (filename));
                                //                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                            //log error masukan log kode kurir kosong
                                //                                        }
                                //                                        var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.INV == no_bukti && p.SUPP == codeSupplier).FirstOrDefault();
                                //                                        if (checkDuplicateHeader != null)
                                //                                        {
                                //                                            //transaction.Rollback();
                                //                                            eraDB.PBT01A.Remove(checkDuplicateHeader);
                                //                                            eraDB.SaveChanges();

                                //                                            string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                //                                            EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                //                                            new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                //                                        }
                                //                                    }
                                //                                }
                                //                            }
                                //                            else
                                //                            {
                                //                                iProcess = iProcess + 1;
                                //                                Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                //                                messageErrorLog = "kode supplier kosong pada row " + i;
                                //                                tw.WriteLine(messageErrorLog);
                                //                                string[] no_cust2 = kode_supplier.Split(';');
                                //                                var noCust = no_cust2[0];
                                //                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                                if (cekLog == null)
                                //                                {
                                //                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                    (kode_supplier),
                                //                                    (idRequest),
                                //                                    ("Upload Excel Invoice Pembelian"),
                                //                                    (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                    ("FAILED"),
                                //                                    (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                    (username),
                                //                                    (filename));
                                //                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                    //log error masukan log marketplace kosong
                                //                                }
                                //                            }
                                //                        }
                                //                        else
                                //                        {
                                //                            iProcess = iProcess + 1;
                                //                            Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                //                            if (string.IsNullOrEmpty(kode_brg))
                                //                            {
                                //                                messageErrorLog = "kode barang kosong pada row " + i;
                                //                            }
                                //                            messageErrorLog = "kode barang kosong pada row " + i;
                                //                            tw.WriteLine(messageErrorLog);
                                //                            var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                            if (cekLog == null)
                                //                            {
                                //                                string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                                (kode_supplier),
                                //                                (idRequest),
                                //                                ("Upload Excel Invoice Pembelian"),
                                //                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                                ("FAILED"),
                                //                                (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                                (username),
                                //                                (filename));
                                //                                var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                                //log error masukan log no referensi kosong
                                //                            }
                                //                        }
                                //                    }
                                //                    else
                                //                    {
                                //                        iProcess = iProcess + 1;
                                //                        Functions.SendProgress("Process in progress...", iProcess, Convert.ToInt32(ret.countAll - 1));

                                //                        if (string.IsNullOrEmpty(kode_brg))
                                //                        {
                                //                            messageErrorLog = "tanggal kosong pada row " + i;
                                //                        }
                                //                        messageErrorLog = "tanggal kosong pada row " + i;
                                //                        tw.WriteLine(messageErrorLog);
                                //                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                //                        if (cekLog == null)
                                //                        {
                                //                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                //                            (kode_supplier),
                                //                            (idRequest),
                                //                            ("Upload Excel Invoice Pembelian"),
                                //                            (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                //                            ("FAILED"),
                                //                            (success + " / " + Convert.ToInt32(ret.countAll - 1)),
                                //                            (username),
                                //                            (filename));
                                //                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                //                            //log error masukan log no referensi kosong
                                //                        }
                                //                    }

                                //                } // end looping

                                //                TempPPN = Tempbruto * (TempPPNPersen / 100);
                                //                Tempnetto = Tempbruto + TempPPN + TempOngkir;
                                //                var checkheader = eraDB.PBT01A.Where(p => p.INV == noBuktiPB).ToList();
                                //                if (checkheader.Count() > 0)
                                //                {
                                //                    EDB.ExecuteSQL("Constring", CommandType.Text, "UPDATE PBT01A SET BRUTO = " + Tempbruto + " , NETTO = " + Tempnetto + " , PPN = " + TempPPNPersen + " , NPPN = " + TempPPN + " WHERE INV = '" + noBuktiPB + "'");
                                //                }

                                //            }

                                //            Tempbruto = 0;
                                //            Tempnetto = 0;
                                //            TempPPNPersen = 0;
                                //            TempPPN = 0;
                                //            TempOngkir = 0;
                                //            TempTotalHargaBarang = 0;
                                //        }
                                //    }
                                //}
                                //#endregion
                                //// end add by fauzi 23/09/2020

                                //// END PROSES UPLOAD EXCEL INVOICE PEMBELIAN VERSI 1 04/03/2021 by Fauzi
                                #endregion PROSES_UPLOAD_EXCEL_PEMBELIAN_VERSI1

                                #region PROSES_UPLOAD_EXCEL_PEMBELIAN_VERSI2
                                // START PROSES UPLOAD EXCEL INVOICE PEMBELIAN VERSI 1 04/03/2021 by Fauzi
                                // validasi jika file baru lagi dengan no referensi dan no cust sama tidak boleh proses
                                // add by fauzi 23/09/2020
                                #region validasi duplicate dari file berbeda
                                eraDB.Database.ExecuteSqlCommand("delete from TEMP_UPLOAD_EXCEL_INVOICE_PEMBELIAN");
                                var dataNoBuktiCodeSupplier = new List<string>();
                                List<TEMP_UPLOAD_EXCEL_INVOICE_PEMBELIAN> listTempUploadExcelInvoicePembelian = new List<TEMP_UPLOAD_EXCEL_INVOICE_PEMBELIAN>();

                                var vCountInTemp = 0;
                                var vCountAllRow = ret.countAll;
                                int iCountProcessInsertTemp = 0;
                                bool checklastRow = false;

                                for (int i = Convert.ToInt32(prog[0]); i <= worksheet.Dimension.End.Row; i++)
                                {

                                    string noref = worksheet.Cells[i, 1].Value == null ? "" : worksheet.Cells[i, 1].Value.ToString();
                                    string tgl = worksheet.Cells[i, 2].Value == null ? "" : worksheet.Cells[i, 2].Value.ToString();
                                    string kode_supplier = worksheet.Cells[i, 3].Value == null ? "" : worksheet.Cells[i, 3].Value.ToString();
                                    string kode_barang = worksheet.Cells[i, 7].Value == null ? "" : worksheet.Cells[i, 7].Value.ToString();
                                    string top = worksheet.Cells[i, 4].Value == null ? "0" : worksheet.Cells[i, 4].Value.ToString();
                                    string ppn = worksheet.Cells[i, 5].Value == null ? "0" : worksheet.Cells[i, 5].Value.ToString();
                                    string ongkir = worksheet.Cells[i, 6].Value == null ? "0" : worksheet.Cells[i, 6].Value.ToString();
                                    string gudang = worksheet.Cells[i, 9].Value == null ? "" : worksheet.Cells[i, 9].Value.ToString();
                                    string qty = worksheet.Cells[i, 10].Value == null ? "" : worksheet.Cells[i, 10].Value.ToString();
                                    string harga_satuan = worksheet.Cells[i, 11].Value == null ? "" : worksheet.Cells[i, 11].Value.ToString();
                                    string total_nilaidisc = worksheet.Cells[i, 12].Value == null ? "0" : worksheet.Cells[i, 12].Value.ToString();
                                    //string total = worksheet.Cells[i, 13].Value == null ? "" : worksheet.Cells[i, 13].Value.ToString();


                                    if (!string.IsNullOrEmpty(noref) && !string.IsNullOrEmpty(tgl) && !string.IsNullOrEmpty(kode_supplier) && !string.IsNullOrEmpty(kode_barang)
                                         && !string.IsNullOrEmpty(gudang) && !string.IsNullOrEmpty(qty) && !string.IsNullOrEmpty(harga_satuan)
                                          //&& !string.IsNullOrEmpty(total)
                                          )
                                    {
                                        iCountProcessInsertTemp += 1;
                                        vCountInTemp += 1;
                                        tgl = Convert.ToDateTime(tgl).ToString("yyyy-MM-dd");
                                        //dataNoBuktiCodeSupplier.Add(tgl + ";" + kode_supplier);
                                        dataNoBuktiCodeSupplier.Add(noref);
                                        TEMP_UPLOAD_EXCEL_INVOICE_PEMBELIAN newTempUploadExcelInvoicePembelian = new TEMP_UPLOAD_EXCEL_INVOICE_PEMBELIAN() { };
                                        newTempUploadExcelInvoicePembelian.NOBUK = noref;
                                        newTempUploadExcelInvoicePembelian.TGL = Convert.ToDateTime(tgl);
                                        newTempUploadExcelInvoicePembelian.KODE_SUPPLIER = kode_supplier;
                                        newTempUploadExcelInvoicePembelian.TOP = Convert.ToInt32(top);
                                        newTempUploadExcelInvoicePembelian.PPN = Convert.ToDouble(ppn);
                                        newTempUploadExcelInvoicePembelian.ONGKIR = Convert.ToDouble(ongkir); ;
                                        newTempUploadExcelInvoicePembelian.KODE_BRG = kode_barang;
                                        newTempUploadExcelInvoicePembelian.GUDANG = gudang;
                                        newTempUploadExcelInvoicePembelian.QTY = Convert.ToInt32(qty);
                                        newTempUploadExcelInvoicePembelian.HARGA_SATUAN = Convert.ToDouble(harga_satuan);
                                        newTempUploadExcelInvoicePembelian.TOTAL_NILAI_DISC = Convert.ToDouble(total_nilaidisc);
                                        //newTempUploadExcelInvoicePembelian.TOTAL = Convert.ToDouble(total);
                                        listTempUploadExcelInvoicePembelian.Add(newTempUploadExcelInvoicePembelian);
                                    }
                                    else if (string.IsNullOrEmpty(noref) && string.IsNullOrEmpty(tgl) && string.IsNullOrEmpty(kode_supplier) && string.IsNullOrEmpty(kode_barang)
                                         && string.IsNullOrEmpty(gudang) && string.IsNullOrEmpty(qty) && string.IsNullOrEmpty(harga_satuan)
                                          //&& string.IsNullOrEmpty(total)
                                          )
                                    {
                                        checklastRow = true;
                                    }

                                    if (string.IsNullOrEmpty(noref))
                                    {
                                        checklastRow = false;
                                        messageErrorLog = "No Bukti invoice pembelian kosong pada row " + i + ". Proses Upload dibatalkan.";
                                        tw.WriteLine(messageErrorLog);
                                    }

                                    if (string.IsNullOrEmpty(tgl))
                                    {
                                        checklastRow = false;
                                        messageErrorLog = "Tanggal invoice pembelian kosong pada row " + i + ". Proses Upload dibatalkan.";
                                        tw.WriteLine(messageErrorLog);
                                    }

                                    if (string.IsNullOrEmpty(kode_supplier))
                                    {
                                        checklastRow = false;
                                        messageErrorLog = "Terdapat kolom Kode Supplier kosong pada row " + i + ". Proses Upload dibatalkan.";
                                        tw.WriteLine(messageErrorLog);
                                    }

                                    if (string.IsNullOrEmpty(kode_barang))
                                    {
                                        checklastRow = false;
                                        messageErrorLog = "Terdapat kolom Kode barang kosong pada row " + i + ". Proses Upload dibatalkan.";
                                        tw.WriteLine(messageErrorLog);
                                    }

                                    //if (string.IsNullOrEmpty(top))
                                    //{
                                    //    checklastRow = false;
                                    //    messageErrorLog = "Terdapat kolom Term of payment invoice pembelian kosong pada row " + i + ". Proses Upload dibatalkan.";
                                    //    tw.WriteLine(messageErrorLog);
                                    //}

                                    if (string.IsNullOrEmpty(gudang))
                                    {
                                        checklastRow = false;
                                        messageErrorLog = "Terdapat kolom Gudang kosong pada row " + i + ". Proses Upload dibatalkan.";
                                        tw.WriteLine(messageErrorLog);
                                    }

                                    if (string.IsNullOrEmpty(harga_satuan))
                                    {
                                        checklastRow = false;
                                        messageErrorLog = "Terdapat kolom Harga satuan kosong pada row " + i + ". Proses Upload dibatalkan.";
                                        tw.WriteLine(messageErrorLog);
                                    }

                                    Functions.SendProgress("Processing upload to Temporary...", iCountProcessInsertTemp, Convert.ToInt32(ret.countAll));

                                    if (!string.IsNullOrEmpty(messageErrorLog) && checklastRow == false && i == worksheet.Dimension.End.Row)
                                    {
                                        dataNoBuktiCodeSupplier.Clear();
                                        listTempUploadExcelInvoicePembelian.Clear();
                                        eraDB.Database.ExecuteSqlCommand("delete from TEMP_UPLOAD_EXCEL_INVOICE_PEMBELIAN");
                                        tw.Close();
                                        //tw.Dispose();

                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                        if (cekLog == null)
                                        {
                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                        (""),
                                        (connID),
                                        ("Upload Excel Invoice Pembelian"),
                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                        ("FAILED"),
                                        (iCountProcessInsertTemp + " / " + Convert.ToInt32(ret.countAll)),
                                        (username),
                                        (filename));
                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                            // error log terjadi error pada insert header pesanan
                                        }
                                        ret.Errors.Add(messageErrorLog);
                                        return Json(ret, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        if (vCountInTemp == 10 && i <= worksheet.Dimension.End.Row || i == worksheet.Dimension.End.Row)
                                        {
                                            vCountInTemp = 0;
                                            eraDB.TEMP_UPLOAD_EXCEL_INVOICE_PEMBELIAN.AddRange(listTempUploadExcelInvoicePembelian);
                                            listTempUploadExcelInvoicePembelian.Clear();
                                            await eraDB.SaveChangesAsync();
                                        }
                                    }
                                }

                                double Tempbruto = 0;
                                double Tempnetto = 0;
                                double TempPPNPersen = 0;
                                double TempPPN = 0;
                                double TempOngkir = 0;
                                double TempTotalHargaBarang = 0;

                                var checkAlreadyTempInvoicePembelian = eraDB.TEMP_UPLOAD_EXCEL_INVOICE_PEMBELIAN.ToList();

                                if (dataNoBuktiCodeSupplier.Count() > 0 && checkAlreadyTempInvoicePembelian.Count() == iCountProcessInsertTemp)
                                {
                                    var dataFilterRef = dataNoBuktiCodeSupplier.Distinct().ToList();
                                    if (dataFilterRef.Count() > 0)
                                    {

                                        int iCountProcessInsertDB = 0;
                                        int iPercentase = 0;

                                        string sSQLValues = "";

                                        foreach (var refCheck in dataFilterRef)
                                        {
                                            //string[] splitRef = refCheck.Split(';');
                                            //DateTime resTgl = Convert.ToDateTime(splitRef[0].ToString());
                                            //var resCodeSupplier = splitRef[1].ToString();
                                            //var resKodeBarang = splitRef[2].ToString();
                                            //var checkDB = eraDB.PBT01A.AsNoTracking().Where(c => c.TGL == resTgl && c.SUPP == resCodeSupplier).SingleOrDefault();
                                            var checkDB = eraDB.PBT01A.AsNoTracking().Where(c => c.NO_INVOICE_SUPP == refCheck).SingleOrDefault();
                                            if (checkDB != null)
                                            {
                                                //messageErrorLog = "Tanggal invoice pembelian " + resTgl.ToString() + " dan Kode Supplier " + resCodeSupplier + " sudah pernah dimasukan. Proses Upload dibatalkan.";
                                                messageErrorLog = "No bukti invoice pembelian sudah pernah dimasukan. Proses Upload dibatalkan.";
                                                tw.WriteLine(messageErrorLog);
                                                tw.Close();
                                                //tw.Dispose();

                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                if (cekLog == null)
                                                {
                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                (refCheck),
                                                (connID),
                                                ("Upload Excel Invoice Pembelian"),
                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                ("FAILED"),
                                                (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                (username),
                                                (filename));
                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                    // error log terjadi error pada insert header pesanan
                                                }
                                                ret.Errors.Add("No bukti invoice pembelian sudah pernah dimasukan. Proses Upload dibatalkan.");
                                                return Json(ret, JsonRequestBehavior.AllowGet);
                                            }
                                            else
                                            {
                                                // start looping
                                                //for (int i = Convert.ToInt32(prog[0]); i <= checkAlreadyTempInvoicePembelian.Count(); i++)
                                                //{
                                                var cekPer10 = 0;
                                                var progressTemp = 0;
                                                var percentTemp = 0;
                                                bool statusSuccessTemp = false;
                                                bool statusLoopTemp = false;
                                                bool sudahSimpanTemp = false;
                                                var tempPercent = 0;
                                                var temp40 = 0;

                                                foreach (var itemTemp in checkAlreadyTempInvoicePembelian)
                                                {
                                                    ret.statusLoop = true;
                                                    //ret.progress = i;


                                                    #region var lama
                                                    //get ALL DATA
                                                    //string no_bukti = worksheet.Cells[i, 1].Value == null ? "" : worksheet.Cells[i, 1].Value.ToString();
                                                    //string tgl = worksheet.Cells[i, 2].Value == null ? "" : worksheet.Cells[i, 2].Value.ToString();
                                                    //string kode_supplier = worksheet.Cells[i, 3].Value == null ? "" : worksheet.Cells[i, 3].Value.ToString();
                                                    //string top = worksheet.Cells[i, 4].Value == null ? "" : worksheet.Cells[i, 4].Value.ToString();
                                                    //string ppn = worksheet.Cells[i, 5].Value == null ? "0" : worksheet.Cells[i, 5].Value.ToString();
                                                    ////string nilai_ppn = worksheet.Cells[i, 6].Value == null ? "0" : worksheet.Cells[i, 6].Value.ToString();
                                                    //string ongkir = worksheet.Cells[i, 6].Value == null ? "0" : worksheet.Cells[i, 6].Value.ToString();
                                                    //string kode_brg = worksheet.Cells[i, 7].Value == null ? "" : worksheet.Cells[i, 7].Value.ToString();
                                                    //string nama_brg = worksheet.Cells[i, 8].Value == null ? "" : worksheet.Cells[i, 8].Value.ToString();
                                                    //string gudang = worksheet.Cells[i, 9].Value == null ? "" : worksheet.Cells[i, 9].Value.ToString();
                                                    //string qty = worksheet.Cells[i, 10].Value == null ? "0" : worksheet.Cells[i, 10].Value.ToString();
                                                    //string harga_satuan = worksheet.Cells[i, 11].Value == null ? "0" : worksheet.Cells[i, 11].Value.ToString();
                                                    //string total_nilaidisc = worksheet.Cells[i, 12].Value == null ? "0" : worksheet.Cells[i, 12].Value.ToString();
                                                    //string total = worksheet.Cells[i, 13].Value == null ? "0" : worksheet.Cells[i, 13].Value.ToString();
                                                    #endregion

                                                    string noref = itemTemp.NOBUK;
                                                    //string noref = itemTemp.NOREF;
                                                    string tgl = Convert.ToDateTime(itemTemp.TGL).ToString("yyyy-MM-dd");
                                                    string kode_supplier = itemTemp.KODE_SUPPLIER;
                                                    string top = Convert.ToString(itemTemp.TOP);
                                                    string ppn = Convert.ToString(itemTemp.PPN);
                                                    //string nilai_ppn = worksheet.Cells[i, 6].Value == null ? "0" : worksheet.Cells[i, 6].Value.ToString();
                                                    string ongkir = Convert.ToString(itemTemp.ONGKIR);
                                                    string kode_brg = itemTemp.KODE_BRG;
                                                    //string nama_brg = itemTemp.NAMA;
                                                    string gudang = itemTemp.GUDANG;
                                                    string qty = Convert.ToString(itemTemp.QTY);
                                                    string harga_satuan = Convert.ToString(itemTemp.HARGA_SATUAN);
                                                    string total_nilaidisc = Convert.ToString(itemTemp.TOTAL_NILAI_DISC);
                                                    //string total = Convert.ToString(itemTemp.TOTAL);


                                                    if (kode_supplier.Contains("Silahkan"))
                                                    {
                                                        kode_supplier = "";
                                                    }

                                                    if (gudang.Contains("Silahkan"))
                                                    {
                                                        gudang = "";
                                                    }

                                                    if (!string.IsNullOrEmpty(tgl))
                                                    {
                                                        tgl = Convert.ToDateTime(tgl).ToString("yyyy-MM-dd");
                                                        DateTime dttgl = Convert.ToDateTime(tgl);
                                                        if (!string.IsNullOrEmpty(kode_brg))
                                                        {
                                                            if (!string.IsNullOrEmpty(kode_supplier))
                                                            {
                                                                //if (resCodeSupplier == kode_supplier && resTgl == dttgl)
                                                                if (refCheck == noref)
                                                                {
                                                                    iCountProcessInsertDB += 1;
                                                                    iPercentase = ((iCountProcessInsertDB * 100) / iCountProcessInsertTemp);

                                                                    var namaSupplier = dataMasterSupplier.Where(p => p.SUPP == kode_supplier).SingleOrDefault().NAMA;

                                                                    if (!string.IsNullOrEmpty(gudang))
                                                                    {
                                                                        if (!top.Contains(".") || !ongkir.Contains(".") || !qty.Contains(".") || !harga_satuan.Contains(".") || !total_nilaidisc.Contains("."))
                                                                        {
                                                                            if (!top.Contains(",") || !ongkir.Contains(",") || !qty.Contains(",") || !harga_satuan.Contains(",") || !total_nilaidisc.Contains(","))
                                                                            {
                                                                                if (!string.IsNullOrEmpty(kode_brg))
                                                                                {
                                                                                    if (!string.IsNullOrEmpty(harga_satuan) || !string.IsNullOrEmpty(qty))
                                                                                    {
                                                                                        if (kode_brg.Length <= 20)
                                                                                        {
                                                                                            //var checkBarang = ErasoftDbContext.STF02.Where(p => p.BRG == item.KODE_BRG).Select(p => p.BRG).FirstOrDefault();
                                                                                            var checkBarang = dataMasterSTF02.Where(p => p.BRG == kode_brg).FirstOrDefault();
                                                                                            if (checkBarang != null)
                                                                                            {
                                                                                                if (string.IsNullOrEmpty(total_nilaidisc) || string.IsNullOrEmpty(harga_satuan) || string.IsNullOrEmpty(qty))
                                                                                                {
                                                                                                    total_nilaidisc = "0";
                                                                                                    //total = "0";
                                                                                                    harga_satuan = "0";
                                                                                                    qty = "0";
                                                                                                    //total = "0";
                                                                                                }
                                                                                                if (Convert.ToInt32(total_nilaidisc) > 0)
                                                                                                {
                                                                                                    TempTotalHargaBarang = (Convert.ToInt32(harga_satuan) * Convert.ToInt32(qty) - Convert.ToInt32(total_nilaidisc));
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    TempTotalHargaBarang = Convert.ToInt32(harga_satuan) * Convert.ToInt32(qty);
                                                                                                }

                                                                                                Tempbruto += TempTotalHargaBarang;

                                                                                                if (TempPPNPersen == 0)
                                                                                                {
                                                                                                    TempPPNPersen = Convert.ToInt32(ppn);
                                                                                                    //TempPPN = Convert.ToInt32(nilai_ppn);
                                                                                                }

                                                                                                if (TempOngkir == 0)
                                                                                                {
                                                                                                    TempOngkir = Convert.ToInt32(ongkir);
                                                                                                }

                                                                                                //var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                                                                                var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.NO_INVOICE_SUPP == refCheck).FirstOrDefault();
                                                                                                if (checkDuplicateHeader == null)
                                                                                                {
                                                                                                    var lastBukti = new ManageController().GenerateAutoNumber(eraDB, "PB", "PBT01A", "INV");
                                                                                                    var nopb = "PB" + DateTime.UtcNow.AddHours(7).Year.ToString().Substring(2, 2) + Convert.ToString(Convert.ToInt32(lastBukti) + 1).PadLeft(6, '0');
                                                                                                    noBuktiPB = nopb;

                                                                                                    if (string.IsNullOrEmpty(top))
                                                                                                    {
                                                                                                        top = "0";
                                                                                                    }

                                                                                                    #region tabel_PBT01A
                                                                                                    var pbt01a = new PBT01A
                                                                                                    {
                                                                                                        JENISFORM = "1",
                                                                                                        INV = noBuktiPB,
                                                                                                        TGL = Convert.ToDateTime(tgl),
                                                                                                        JENIS = "1",
                                                                                                        PO = "-",
                                                                                                        STATUS = "1",
                                                                                                        POSTING = "-",
                                                                                                        SUPP = kode_supplier,
                                                                                                        NAMA = namaSupplier,
                                                                                                        VLT = "IDR",
                                                                                                        TERM = Convert.ToInt16(top),
                                                                                                        BIAYA_LAIN = Convert.ToInt32(ongkir),
                                                                                                        PPN = Convert.ToInt32(ppn),
                                                                                                        //NPPN = Convert.ToInt32(nilai_ppn),
                                                                                                        NPPN = 0,
                                                                                                        BRUTO = 0,
                                                                                                        NETTO = 0,
                                                                                                        USERNAME = username,
                                                                                                        TGLINPUT = DateTime.Now.AddHours(7),
                                                                                                        TGJT = DateTime.Now.AddHours(7).AddDays(Convert.ToInt32(top)),
                                                                                                        KET = "-",
                                                                                                        APP = "-",
                                                                                                        REF = "-",
                                                                                                        NO_INVOICE_SUPP = refCheck,
                                                                                                    };
                                                                                                    #endregion

                                                                                                    try
                                                                                                    {
                                                                                                        eraDB.PBT01A.Add(pbt01a);
                                                                                                        //transaction.Commit();
                                                                                                    }
                                                                                                    catch (Exception ex)
                                                                                                    {
                                                                                                        messageErrorLog = "terjadi error pada insert header invoice pembelian pada row " + iCountProcessInsertDB;
                                                                                                        tw.WriteLine(messageErrorLog);

                                                                                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                                        if (cekLog == null)
                                                                                                        {
                                                                                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                                        (kode_supplier),
                                                                                                        (connID),
                                                                                                        ("Upload Excel Invoice Pembelian"),
                                                                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                                        ("FAILED"),
                                                                                                        (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                                                        (username),
                                                                                                        (filename));
                                                                                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                                            // error log terjadi error pada insert header pesanan
                                                                                                        }

                                                                                                        //checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                                                                                        checkDuplicateHeader = eraDB.PBT01A.Where(p => p.NO_INVOICE_SUPP == refCheck).FirstOrDefault();
                                                                                                        if (checkDuplicateHeader != null)
                                                                                                        {
                                                                                                            //transaction.Rollback();
                                                                                                            eraDB.PBT01A.Remove(checkDuplicateHeader);
                                                                                                            await eraDB.SaveChangesAsync();
                                                                                                            string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                                            EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                                            new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                                        }

                                                                                                    }
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    noBuktiPB = checkDuplicateHeader.INV;
                                                                                                    var checkDetailPB = eraDB.PBT01B.Where(p => p.INV == noBuktiPB).ToList();
                                                                                                    if (checkDetailPB.Count() > 0)
                                                                                                    {
                                                                                                        EDB.ExecuteSQL("Constring", CommandType.Text, "UPDATE PBT01B SET THARGA = " + TempTotalHargaBarang + " WHERE TGL = '" + dttgl.ToString("yyyy-MM-dd") + "' AND SUPP = '" + kode_supplier + "' AND INV = '" + noBuktiPB + "'");
                                                                                                    }
                                                                                                }



                                                                                                //var listBrgToUpdateStock = new List<string>();
                                                                                                var pbt01b = new PBT01B
                                                                                                {
                                                                                                    JENISFORM = "1",
                                                                                                    INV = noBuktiPB,
                                                                                                    PO = "-",
                                                                                                    BRG = kode_brg,
                                                                                                    GD = gudang,
                                                                                                    BK = "2",
                                                                                                    QTY = Convert.ToInt32(qty),
                                                                                                    HBELI = Convert.ToInt32(harga_satuan),
                                                                                                    THARGA = (TempTotalHargaBarang),
                                                                                                    USERNAME = username,
                                                                                                    TGLINPUT = DateTime.Now.AddHours(7),
                                                                                                    DISCOUNT_1 = 0,
                                                                                                    NILAI_DISC_1 = Convert.ToInt32(total_nilaidisc),
                                                                                                    NOBUK = "-",
                                                                                                    AUTO_LOAD = "-",
                                                                                                    KET = "-",
                                                                                                    BRG_ORIGINAL = "-",
                                                                                                    LKU = "-",
                                                                                                };

                                                                                                try
                                                                                                {
                                                                                                    eraDB.PBT01B.Add(pbt01b);
                                                                                                    await eraDB.SaveChangesAsync();

                                                                                                    sSQLValues = sSQLValues + "('" + kode_brg + "', '" + connID + "'),";
                                                                                                    
                                                                                                }
                                                                                                catch (Exception ex)
                                                                                                {
                                                                                                    messageErrorLog = "terjadi error pada insert detail invoice pembelian pada row " + iCountProcessInsertDB;
                                                                                                    tw.WriteLine(messageErrorLog);
                                                                                                    var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                                    if (cekLog == null)
                                                                                                    {
                                                                                                        string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                                        (kode_supplier),
                                                                                                        (connID),
                                                                                                        ("Upload Excel Invoice Pembelian"),
                                                                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                                        ("FAILED"),
                                                                                                        (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                                                        (username),
                                                                                                        (filename));
                                                                                                        var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                                        // error log terjadi error pada insert detail pesanan
                                                                                                    }
                                                                                                    //checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                                                                                    checkDuplicateHeader = eraDB.PBT01A.Where(p => p.NO_INVOICE_SUPP == refCheck).FirstOrDefault();
                                                                                                    if (checkDuplicateHeader != null)
                                                                                                    {
                                                                                                        //transaction.Rollback();
                                                                                                        eraDB.PBT01A.Remove(checkDuplicateHeader);
                                                                                                        await eraDB.SaveChangesAsync();

                                                                                                        string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                                        EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                                    }
                                                                                                }

                                                                                                //if (ret.percent >= 100 || ret.progress == ret.countAll - 1)
                                                                                                //{
                                                                                                //    transaction.Commit();
                                                                                                //    ret.statusSuccess = true;
                                                                                                //    return Json(ret, JsonRequestBehavior.AllowGet);
                                                                                                //}
                                                                                                //iProcess = iProcess + 1;
                                                                                                success = success + 1;
                                                                                                if(iCountProcessInsertDB == iCountProcessInsertTemp)
                                                                                                {
                                                                                                    Functions.SendProgress("Process Upload Complete !", iCountProcessInsertDB, iCountProcessInsertTemp);
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);
                                                                                                }

                                                                                                if (cekPer10 > 1000)
                                                                                                {
                                                                                                    if ((progressTemp == temp40) || percentTemp == 100)
                                                                                                    {
                                                                                                        statusSuccessTemp = false;
                                                                                                        if (percentTemp > 99 && percentTemp <= 101)
                                                                                                        {
                                                                                                            statusSuccessTemp = true;
                                                                                                            statusLoopTemp = false;
                                                                                                            sudahSimpanTemp = true;
                                                                                                        }
                                                                                                        if (tempPercent != percentTemp)
                                                                                                        {
                                                                                                            if (statusSuccessTemp == false)
                                                                                                            {
                                                                                                                //return Json(ret, JsonRequestBehavior.AllowGet);
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                                else if (percentTemp == 20 ||
                                                                                                percentTemp == 40 ||
                                                                                                percentTemp == 60 ||
                                                                                                percentTemp == 80 ||
                                                                                                percentTemp == 100)
                                                                                                {
                                                                                                    statusSuccessTemp = false;
                                                                                                    if (percentTemp > 99 && percentTemp <= 101)
                                                                                                    {
                                                                                                        statusSuccessTemp = true;
                                                                                                        statusLoopTemp = false;
                                                                                                        sudahSimpanTemp = true;
                                                                                                    }
                                                                                                    if (tempPercent != percentTemp)
                                                                                                    {
                                                                                                        if (statusSuccessTemp == false)
                                                                                                        {
                                                                                                            //return Json(ret, JsonRequestBehavior.AllowGet);
                                                                                                        }
                                                                                                    }
                                                                                                }

                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                //transaction.Rollback();
                                                                                                //transaction.Commit();
                                                                                                iProcess = iProcess + 1;
                                                                                                Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);

                                                                                                messageErrorLog = "Kode Barang " + kode_brg + " tidak ditemukan.";
                                                                                                tw.WriteLine(messageErrorLog);
                                                                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                                if (cekLog == null)
                                                                                                {
                                                                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                                     (kode_supplier),
                                                                                                     (connID),
                                                                                                     ("Upload Excel Invoice Pembelian"),
                                                                                                     (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                                     ("FAILED"),
                                                                                                     (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                                                     (username),
                                                                                                     (filename));
                                                                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                                    //log error masukan log tidak ada barang di DB
                                                                                                }
                                                                                                //var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                                                                                var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.NO_INVOICE_SUPP == refCheck).FirstOrDefault();
                                                                                                if (checkDuplicateHeader != null)
                                                                                                {
                                                                                                    //transaction.Rollback();
                                                                                                    eraDB.PBT01A.Remove(checkDuplicateHeader);
                                                                                                    await eraDB.SaveChangesAsync();

                                                                                                    string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                                    EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                                    new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                                }
                                                                                            }

                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            //transaction.Rollback();
                                                                                            //transaction.Commit();
                                                                                            iProcess = iProcess + 1;
                                                                                            Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);

                                                                                            messageErrorLog = "kode barang lebih dari 20 karakter pada row " + iCountProcessInsertDB;
                                                                                            tw.WriteLine(messageErrorLog);
                                                                                            var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                            if (cekLog == null)
                                                                                            {
                                                                                                string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                                (kode_supplier),
                                                                                                (idRequest),
                                                                                                ("Upload Excel Invoice Pembelian"),
                                                                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                                ("FAILED"),
                                                                                                (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                                                (username),
                                                                                                (filename));
                                                                                                var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                                //log error masukan log kode barang lebih dari 20 karakter
                                                                                            }
                                                                                            //var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                                                                            var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.NO_INVOICE_SUPP == refCheck).FirstOrDefault();
                                                                                            if (checkDuplicateHeader != null)
                                                                                            {
                                                                                                //transaction.Rollback();
                                                                                                eraDB.PBT01A.Remove(checkDuplicateHeader);
                                                                                                await eraDB.SaveChangesAsync();

                                                                                                string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                                EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                                new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        //transaction.Rollback();
                                                                                        //transaction.Commit();
                                                                                        iProcess = iProcess + 1;
                                                                                        Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);

                                                                                        var errorMessage = "";
                                                                                        if (string.IsNullOrEmpty(qty))
                                                                                        {
                                                                                            errorMessage = "qty kosong pada row " + iCountProcessInsertDB;
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            errorMessage = "harga satuan kosong pada row " + iCountProcessInsertDB;
                                                                                        }
                                                                                        messageErrorLog = errorMessage;
                                                                                        tw.WriteLine(messageErrorLog);
                                                                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                        if (cekLog == null)
                                                                                        {
                                                                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                            (kode_supplier),
                                                                                            (idRequest),
                                                                                            ("Upload Excel Invoice Pembelian"),
                                                                                            (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                            ("FAILED"),
                                                                                            (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                                            (username),
                                                                                            (filename));
                                                                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                            //log error masukan log harga satuan kosong
                                                                                        }
                                                                                        //var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                                                                        var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.NO_INVOICE_SUPP == refCheck).FirstOrDefault();
                                                                                        if (checkDuplicateHeader != null)
                                                                                        {
                                                                                            //transaction.Rollback();
                                                                                            eraDB.PBT01A.Remove(checkDuplicateHeader);
                                                                                            await eraDB.SaveChangesAsync();

                                                                                            string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                            EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                            new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                        }
                                                                                    }
                                                                                }
                                                                                else
                                                                                {
                                                                                    //transaction.Rollback();
                                                                                    //transaction.Commit();
                                                                                    iProcess = iProcess + 1;
                                                                                    Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);

                                                                                    messageErrorLog = "kode barang kosong pada row " + iCountProcessInsertDB;
                                                                                    tw.WriteLine(messageErrorLog);
                                                                                    var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                    if (cekLog == null)
                                                                                    {
                                                                                        string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                        (kode_supplier),
                                                                                        (idRequest),
                                                                                        ("Upload Excel Invoice Pembelian"),
                                                                                        (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                        ("FAILED"),
                                                                                        (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                                        (username),
                                                                                        (filename));
                                                                                        var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                        //log error masukan log kode barang kosong
                                                                                    }
                                                                                    //var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                                                                    var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.NO_INVOICE_SUPP == refCheck).FirstOrDefault();
                                                                                    if (checkDuplicateHeader != null)
                                                                                    {
                                                                                        //transaction.Rollback();
                                                                                        eraDB.PBT01A.Remove(checkDuplicateHeader);
                                                                                        await eraDB.SaveChangesAsync();

                                                                                        string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                        EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                        new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                    }
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                //transaction.Rollback();
                                                                                //transaction.Commit();
                                                                                iProcess = iProcess + 1;
                                                                                Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);

                                                                                messageErrorLog = "terdapat karakter koma pada kolom pengisian angka di row " + iCountProcessInsertDB;
                                                                                tw.WriteLine(messageErrorLog);
                                                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                                if (cekLog == null)
                                                                                {
                                                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                    (kode_supplier),
                                                                                    (idRequest),
                                                                                    ("Upload Excel Invoice Pembelian"),
                                                                                    (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                    ("FAILED"),
                                                                                    (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                                    (username),
                                                                                    (filename));
                                                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                    //log error masukan log ada koma
                                                                                }
                                                                                //var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                                                                var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.NO_INVOICE_SUPP == refCheck).FirstOrDefault();
                                                                                if (checkDuplicateHeader != null)
                                                                                {
                                                                                    //transaction.Rollback();
                                                                                    eraDB.PBT01A.Remove(checkDuplicateHeader);
                                                                                    await eraDB.SaveChangesAsync();

                                                                                    string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                    EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                    new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            //transaction.Rollback();
                                                                            //transaction.Commit();
                                                                            iProcess = iProcess + 1;
                                                                            Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);

                                                                            messageErrorLog = "terdapat karakter titik pada kolom pengisian angka di row " + iCountProcessInsertDB;
                                                                            tw.WriteLine(messageErrorLog);
                                                                            var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                            if (cekLog == null)
                                                                            {
                                                                                string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                                (kode_supplier),
                                                                                (idRequest),
                                                                                ("Upload Excel Invoice Pembelian"),
                                                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                                ("FAILED"),
                                                                                (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                                (username),
                                                                                (filename));
                                                                                var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                                //log error masukan log ada titik
                                                                            }
                                                                            //var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.TGL == dttgl && p.SUPP == kode_supplier).FirstOrDefault();
                                                                            var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.NO_INVOICE_SUPP == refCheck).FirstOrDefault();
                                                                            if (checkDuplicateHeader != null)
                                                                            {
                                                                                //transaction.Rollback();
                                                                                eraDB.PBT01A.Remove(checkDuplicateHeader);
                                                                                await eraDB.SaveChangesAsync();

                                                                                string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                                EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                                new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                            }
                                                                        }

                                                                    }
                                                                    else
                                                                    {
                                                                        //transaction.Rollback();
                                                                        //transaction.Commit();
                                                                        iProcess = iProcess + 1;
                                                                        Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);

                                                                        messageErrorLog = "kode gudang kosong pada row " + iCountProcessInsertDB;
                                                                        tw.WriteLine(messageErrorLog);
                                                                        string[] codeSup = kode_supplier.Split(';');
                                                                        var codeSupplier = codeSup[0];
                                                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                        if (cekLog == null)
                                                                        {
                                                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                            (kode_supplier),
                                                                            (idRequest),
                                                                            ("Upload Excel Invoice Pembelian"),
                                                                            (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                            ("FAILED"),
                                                                            (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                            (username),
                                                                            (filename));
                                                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                            //log error masukan log kode kurir kosong
                                                                        }
                                                                        //var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.INV == no_bukti && p.SUPP == codeSupplier).FirstOrDefault();
                                                                        var checkDuplicateHeader = eraDB.PBT01A.Where(p => p.NO_INVOICE_SUPP == refCheck).FirstOrDefault();
                                                                        if (checkDuplicateHeader != null)
                                                                        {
                                                                            //transaction.Rollback();
                                                                            eraDB.PBT01A.Remove(checkDuplicateHeader);
                                                                            await eraDB.SaveChangesAsync();

                                                                            string listAddBrg = "('" + kode_brg + "', '" + connID + "')";
                                                                            EDB.ExecuteSQL("Constring", CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + listAddBrg);
                                                                            new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                iProcess = iProcess + 1;
                                                                Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);

                                                                messageErrorLog = "kode supplier kosong pada row " + iCountProcessInsertDB;
                                                                tw.WriteLine(messageErrorLog);
                                                                string[] no_cust2 = kode_supplier.Split(';');
                                                                var noCust = no_cust2[0];
                                                                var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                                if (cekLog == null)
                                                                {
                                                                    string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                    (kode_supplier),
                                                                    (idRequest),
                                                                    ("Upload Excel Invoice Pembelian"),
                                                                    (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                    ("FAILED"),
                                                                    (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                    (username),
                                                                    (filename));
                                                                    var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                    //log error masukan log marketplace kosong
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            iProcess = iProcess + 1;
                                                            Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);

                                                            if (string.IsNullOrEmpty(kode_brg))
                                                            {
                                                                messageErrorLog = "kode barang kosong pada row " + iCountProcessInsertDB;
                                                            }
                                                            messageErrorLog = "kode barang kosong pada row " + iCountProcessInsertDB;
                                                            tw.WriteLine(messageErrorLog);
                                                            var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                            if (cekLog == null)
                                                            {
                                                                string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                                (kode_supplier),
                                                                (idRequest),
                                                                ("Upload Excel Invoice Pembelian"),
                                                                (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                                ("FAILED"),
                                                                (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                                (username),
                                                                (filename));
                                                                var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                                //log error masukan log no referensi kosong
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //iProcess = iProcess + 1;
                                                        Functions.SendProgress("Process in progress...", iCountProcessInsertDB, iCountProcessInsertTemp);

                                                        if (string.IsNullOrEmpty(kode_brg))
                                                        {
                                                            messageErrorLog = "tanggal kosong pada row " + iCountProcessInsertDB;
                                                        }
                                                        messageErrorLog = "tanggal kosong pada row " + iCountProcessInsertDB;
                                                        tw.WriteLine(messageErrorLog);
                                                        var cekLog = eraDB.API_LOG_MARKETPLACE.AsNoTracking().Where(p => p.REQUEST_ACTION == "Upload Excel Invoice Pembelian" && p.REQUEST_ID == connID).FirstOrDefault();
                                                        if (cekLog == null)
                                                        {
                                                            string InsertLogError = string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                                                            (kode_supplier),
                                                            (idRequest),
                                                            ("Upload Excel Invoice Pembelian"),
                                                            (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                                            ("FAILED"),
                                                            (iCountProcessInsertDB + " / " + iCountProcessInsertTemp),
                                                            (username),
                                                            (filename));
                                                            var result = EDB.ExecuteSQL("Constring", CommandType.Text, queryInsertLogError + InsertLogError);
                                                            //log error masukan log no referensi kosong
                                                        }
                                                    }

                                                } // end looping

                                                TempPPN = Tempbruto * (TempPPNPersen / 100);
                                                Tempnetto = Tempbruto + TempPPN + TempOngkir;
                                                var checkheader = eraDB.PBT01A.Where(p => p.INV == noBuktiPB).ToList();
                                                if (checkheader.Count() > 0)
                                                {
                                                    EDB.ExecuteSQL("Constring", CommandType.Text, "UPDATE PBT01A SET BIAYA_LAIN = " + TempOngkir + " , BRUTO = " + Tempbruto + " , NETTO = " + Tempnetto + " , PPN = " + TempPPNPersen + " , NPPN = " + TempPPN + " WHERE INV = '" + noBuktiPB + "'");
                                                }

                                            }

                                            if (sSQLValues != "")
                                            {
                                                sSQLValues = sSQLValues.Substring(0, sSQLValues.Length - 1);
                                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + sSQLValues);
                                                sSQLValues = "";
                                                new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                            }

                                            Tempbruto = 0;
                                            Tempnetto = 0;
                                            TempPPNPersen = 0;
                                            TempPPN = 0;
                                            TempOngkir = 0;
                                            TempTotalHargaBarang = 0;
                                        }
                                    }
                                }
                                #endregion
                                // end add by fauzi 23/09/2020

                                // END PROSES UPLOAD EXCEL INVOICE PEMBELIAN VERSI 1 04/03/2021 by Fauzi
                                #endregion PROSES_UPLOAD_EXCEL_PEMBELIAN_VERSI2

                            }
                            catch (Exception ex)
                            {
                                tw.WriteLine(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
                                //transaction.Rollback();
                                //new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, username);
                                ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
                            }

                            tw.Close();
                            //tw.Dispose();
                            //}
                        }
                    }
                }
            }

            //}
            //catch (Exception ex)
            //{
            //    ret.Errors.Add(ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            //}

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
                                                    var brg_mp = worksheet.Cells[j, 13].Value == null ? "" : worksheet.Cells[j, 13].Value.ToString();
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
                                                        var kd_brg_mp = worksheet.Cells[i, 13].Value == null ? "" : worksheet.Cells[i, 13].Value.ToString();
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
                                                                        current_brg.SELLER_SKU = worksheet.Cells[i, 3].Value == null ? "" : worksheet.Cells[i, 3].Value.ToString().Trim();
                                                                        current_brg.KODE_BRG_INDUK = worksheet.Cells[i, 4].Value == null ? "" : worksheet.Cells[i, 4].Value.ToString().Trim();
                                                                        //change 14 juni 2019, kode kategori mo disimpan di avalue_40, kode kategory mp tetap di category_code
                                                                        //current_brg.CATEGORY_CODE = worksheet.Cells[i, 5].Value == null ? "" : worksheet.Cells[i, 5].Value.ToString();
                                                                        current_brg.AVALUE_40 = worksheet.Cells[i, 7].Value == null ? "" : worksheet.Cells[i, 7].Value.ToString().Trim();
                                                                        //end change 14 juni 2019, kode kategori mo disimpan di avalue_40, kode kategory mp tetap di category_code
                                                                        current_brg.MEREK = worksheet.Cells[i, 8].Value == null ? "" : worksheet.Cells[i, 8].Value.ToString().Trim();
                                                                        //current_brg.HJUAL_MP = Convert.ToDouble(worksheet.Cells[i, 7].Value == null ? "0" : worksheet.Cells[i, 7].Value.ToString());
                                                                        //current_brg.HJUAL = Convert.ToDouble(worksheet.Cells[i, 7].Value == null ? "0" : worksheet.Cells[i, 7].Value.ToString());
                                                                        //current_brg.BERAT = Convert.ToDouble(worksheet.Cells[i, 9].Value == null ? "0" : worksheet.Cells[i, 9].Value.ToString());
                                                                        //current_brg.IMAGE = worksheet.Cells[i, 10].Value == null ? "" : worksheet.Cells[i, 10].Value.ToString();
                                                                        current_brg.AVALUE_36 = "Auto Process";// barang yg akan di transfer ke master hasil upload excel saja
                                                                        current_brg.BARCODE = worksheet.Cells[i, 5].Value == null ? "" : worksheet.Cells[i, 5].Value.ToString().Trim();
                                                                        current_brg.RAK = worksheet.Cells[i, 6].Value == null ? "" : worksheet.Cells[i, 6].Value.ToString().Trim();

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
                        //change by nurul 31/3/2021, ubah discount header ambil dr NILAI_DISC
                        //"ALAMAT_KIRIM, A.TERM AS [TOP],  SHIPMENT AS KURIR, TGL_JTH_TEMPO AS TGL_JATUH_TEMPO, A.KET AS KETERANGAN, A.BRUTO, A.DISCOUNT AS DISC, A.PPN, A.NILAI_PPN, A.ONGKOS_KIRIM, A.NETTO, " +
                        "ALAMAT_KIRIM, A.TERM AS [TOP],  SHIPMENT AS KURIR, TGL_JTH_TEMPO AS TGL_JATUH_TEMPO, A.KET AS KETERANGAN, A.BRUTO, A.NILAI_DISC AS DISC, A.PPN, A.NILAI_PPN, A.ONGKOS_KIRIM, A.NETTO, " +
                        //end change by nurul 31/3/2021, ubah discount header ambil dr NILAI_DISC
                        "A.STATUS_TRANSAKSI AS STATUS_PESANAN, B.BRG AS KODE_BRG, ISNULL(C.NAMA,'') + ' ' + ISNULL(C.NAMA2, '') AS NAMA_BARANG, QTY, " +
                        "H_SATUAN AS HARGA_SATUAN, B.DISCOUNT AS DISC1, B.NILAI_DISC_1 AS NDISC1, B.DISCOUNT_2 AS DISC2, B.NILAI_DISC_2 AS NDISC2, HARGA AS TOTAL " +
                        "FROM SOT01A A(NOLOCK) INNER JOIN SOT01B B(NOLOCK) ON A.NO_BUKTI = B.NO_BUKTI " +
                        "LEFT JOIN STF02 C(NOLOCK) ON B.BRG = C.BRG " +
                        "INNER JOIN ARF01 D(NOLOCK) ON A.CUST = D.CUST " +
                        "INNER JOIN MO..MARKETPLACE E(NOLOCK) ON D.NAMA = E.IDMARKET " +
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
                var dateNow = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd");
                var dateTGLTempo = DateTime.UtcNow.AddDays(3).AddHours(7).AddDays(2).ToString("yyyy-MM-dd");

                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("PESANAN");

                    // SHEET 1
                    //initial for protected
                    worksheet.Protection.IsProtected = true;
                    worksheet.Column(1).Style.Locked = true;
                    worksheet.Column(2).Style.Locked = false;
                    worksheet.Column(3).Style.Locked = false;
                    worksheet.Column(4).Style.Locked = false;
                    worksheet.Column(5).Style.Locked = false;
                    worksheet.Column(6).Style.Locked = false;
                    worksheet.Column(7).Style.Locked = false;
                    worksheet.Column(8).Style.Locked = false;
                    worksheet.Column(9).Style.Locked = false;
                    worksheet.Column(10).Style.Locked = false;
                    worksheet.Column(11).Style.Locked = false;
                    worksheet.Column(12).Style.Locked = false;
                    worksheet.Column(13).Style.Locked = false;
                    worksheet.Column(14).Style.Locked = false;
                    worksheet.Column(15).Style.Locked = false;
                    worksheet.Column(16).Style.Locked = false;
                    worksheet.Column(17).Style.Locked = false;
                    worksheet.Column(18).Style.Locked = false;
                    worksheet.Column(19).Style.Locked = false;
                    worksheet.Column(20).Style.Locked = false;
                    worksheet.Column(21).Style.Locked = false;
                    worksheet.Column(22).Style.Locked = false;
                    worksheet.Column(23).Style.Locked = false;
                    //worksheet.Column(24).Style.Locked = false;

                    using (var rangePackage = worksheet.Cells[2, 1])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }

                    using (var rangePackage = worksheet.Cells[4, 1])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Silver);
                    }

                    using (var rangePackage = worksheet.Cells[4, 2, 4, 9])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }

                    using (var rangePackage = worksheet.Cells[4, 11])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }

                    using (var rangePackage = worksheet.Cells[4, 15, 4, 17])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }

                    using (var rangePackage = worksheet.Cells[4, 19, 4, 20])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }

                    using (var rangePackage = worksheet.Cells[4, 22])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }

                    worksheet.Cells["A1"].Value = "Pesanan : SUDAH BAYAR";
                    worksheet.Cells["A2"].Value = "Keterangan: Kolom warna kuning harus diisi.";

                    //add formula


                    for (int i = 0; i < 5; i++)
                    {
                        //worksheet.Cells["X" + (5 + i)].Formula = "=S" + (5 + i) + "*R" + (5 + i) + "";
                        //worksheet.Cells["U" + (5 + i)].Formula = "=S" + (5 + i) + "*T" + (5 + i) + "/100";
                        //worksheet.Cells["W" + (5 + i)].Formula = "=S" + (5 + i) + "*V" + (5 + i) + "/100";


                        worksheet.Cells[5 + i, 1].Value = "-"; //NO_PESANAN
                        worksheet.Cells[5 + i, 2].Value = ""; //NO_REFERENSI
                        worksheet.Cells[5 + i, 3].Value = dateNow; //TGL 
                        worksheet.Cells[5 + i, 4].Value = "-- Silahkan Pilih Marketplace --"; //MARKETPLACE
                        //worksheet.Cells[5 + i, 5].Value = "001028"; //KODE_PEMBELI
                        worksheet.Cells[5 + i, 5].Value = ""; //PEMBELI
                        worksheet.Cells[5 + i, 6].Value = ""; //ALAMAT_KIRIM
                        worksheet.Cells[5 + i, 7].Value = ""; //NO TELEPHONE
                        worksheet.Cells[5 + i, 8].Value = "-- Silahkan Pilih Kurir --"; //KURIR
                        worksheet.Cells[5 + i, 9].Value = "2"; //TOP
                        //worksheet.Cells[5 + i, 9].Value = dateTGLTempo; //TGL_JATUH_TEMPO
                        worksheet.Cells[5 + i, 10].Value = ""; //KETERANGAN
                        worksheet.Cells[5 + i, 11].Value = 0; //BRUTO
                        worksheet.Cells[5 + i, 12].Value = 0; //DISC
                        worksheet.Cells[5 + i, 13].Value = 0; //PPN
                        worksheet.Cells[5 + i, 14].Value = 0; //NILAI_PPN
                        worksheet.Cells[5 + i, 15].Value = 0; //ONGKOS_KIRIM
                        worksheet.Cells[5 + i, 16].Value = 0; //NETTO
                        //worksheet.Cells[5 + i, 17].Value = "SUDAH BAYAR"; //STATUS_PESANAN
                        worksheet.Cells[5 + i, 17].Value = ""; //KODE_BRG
                        worksheet.Cells[5 + i, 18].Value = ""; //NAMA_BARANG
                        worksheet.Cells[5 + i, 19].Value = 0; //QTY
                        worksheet.Cells[5 + i, 20].Value = 0; //HARGA_SATUAN
                        //worksheet.Cells[5 + i, 21].Value = 20; //DISC1
                        worksheet.Cells[5 + i, 21].Value = 0; //NDISC1
                        //worksheet.Cells[5 + i, 22].Value = 30; //DISC2
                        //worksheet.Cells[5 + i, 23].Value = 0; //NDISC2
                        worksheet.Cells[5 + i, 22].Value = 0;//TOTAL
                    }

                    ExcelRange rg0 = worksheet.Cells[4, 1, worksheet.Dimension.End.Row, 22];
                    string tableName0 = "TablePesanan";
                    ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);

                    table0.Columns[0].Name = "NO PESANAN";
                    table0.Columns[1].Name = "NO REFERENSI";
                    table0.Columns[2].Name = "TGL";
                    table0.Columns[3].Name = "MARKETPLACE";
                    //table0.Columns[4].Name = "KODE PEMBELI";
                    table0.Columns[4].Name = "PEMBELI";
                    table0.Columns[5].Name = "ALAMAT KIRIM";
                    table0.Columns[6].Name = "NO TELP PEMBELI";
                    table0.Columns[7].Name = "KURIR";
                    table0.Columns[8].Name = "TOP";
                    //table0.Columns[8].Name = "TGL JATUH TEMPO";
                    table0.Columns[9].Name = "KETERANGAN";
                    table0.Columns[10].Name = "BRUTO";
                    table0.Columns[11].Name = "DISC";
                    table0.Columns[12].Name = "PPN";
                    table0.Columns[13].Name = "NILAI PPN";
                    table0.Columns[14].Name = "ONGKOS KIRIM";
                    table0.Columns[15].Name = "NETTO";
                    //table0.Columns[16].Name = "STATUS PESANAN";
                    table0.Columns[16].Name = "KODE BRG";
                    table0.Columns[17].Name = "NAMA BARANG";
                    table0.Columns[18].Name = "QTY";
                    table0.Columns[19].Name = "HARGA SATUAN";
                    //table0.Columns[20].Name = "DISC1";
                    table0.Columns[20].Name = "NDISC1";
                    //table0.Columns[22].Name = "DISC2";
                    //table0.Columns[23].Name = "NDISC2";
                    table0.Columns[21].Name = "TOTAL";

                    using (var range = worksheet.Cells[4, 1, 4, 22])
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
                        "FROM ARF01 A (NOLOCK) " +
                        "LEFT JOIN MO..MARKETPLACE B(NOLOCK) ON A.NAMA = B.IDMARKET";

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
                        foreach (var itemKurir in dataKurir)
                        {
                            sheet2.Cells[4 + j, 6].Value = itemKurir.RecNum;
                            sheet2.Cells[4 + j, 7].Value = itemKurir.RecNum + ";" + itemKurir.NamaEkspedisi;
                            j += 1;
                        }
                    }

                    var validation2 = worksheet.DataValidations.AddListValidation(worksheet.Cells[5, 8, worksheet.Dimension.End.Row, 8].Address);
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

                    ExcelRange rg2 = sheet2.Cells[3, 6, worksheet.Dimension.End.Row, 8];
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

        //add by fauzi 25 Februari 2021, download example template for upload invoice pembelian
        public ActionResult ExampleTemplateInvoicePembelianExcel()
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };

            try
            {
                var dateNow = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd");
                var dateTGLTempo = DateTime.UtcNow.AddDays(3).AddHours(7).AddDays(2).ToString("yyyy-MM-dd");

                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("INVOICE_PEMBELIAN");

                    // SHEET 1
                    //initial for protected
                    worksheet.Protection.IsProtected = true;
                    worksheet.Row(2).Style.Locked = true;
                    worksheet.Row(3).Style.Locked = true;
                    worksheet.Row(4).Style.Locked = true;
                    worksheet.Row(5).Style.Locked = true;
                    worksheet.Row(6).Style.Locked = true;
                    worksheet.Row(9).Style.Locked = true;

                    for (int i = 10; i < 100000; i++)
                    {
                        worksheet.Row(i).Style.Locked = false;
                    }

                    //worksheet.Column(1).Style.Locked = true;


                    using (var rangePackage = worksheet.Cells[8, 1])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }

                    //using (var rangePackage = worksheet.Cells[9, 1])
                    //{
                    //    rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //    rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Silver);
                    //}

                    using (var rangePackage = worksheet.Cells[9, 1, 9, 3])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }

                    using (var rangePackage = worksheet.Cells[9, 7])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }

                    using (var rangePackage = worksheet.Cells[9, 9, 9, 11])
                    {
                        rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    }

                    //using (var rangePackage = worksheet.Cells[9, 12])
                    //{
                    //    rangePackage.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //    rangePackage.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    //}

                    worksheet.Cells["F9"].AddComment("Untuk nomor bukti invoice pembelian yang sama ongkos kirim cukup isi 1x di kode barang pertama dalam no bukti tersebut.", "MasterOnline");
                    var comment = worksheet.Cells["F9"].Comment;
                    comment.Text = "Untuk nomor bukti invoice pembelian yang sama ongkos kirim cukup isi 1x di kode barang pertama dalam no bukti tersebut.";
                    comment.Author = "MasterOnline";

                    worksheet.Cells["E9"].AddComment("Untuk nomor bukti invoice pembelian yang sama ongkos kirim cukup isi 1x di kode barang pertama dalam no bukti tersebut.", "MasterOnline");
                    var comment2 = worksheet.Cells["E9"].Comment;
                    comment2.Text = "Untuk nomor bukti invoice pembelian yang sama PPN % cukup isi 1x di kode barang pertama dalam no bukti tersebut.";
                    comment2.Author = "MasterOnline";


                    worksheet.Cells["A8"].Value = "Keterangan: Kolom warna kuning harus diisi.";

                    worksheet.Cells["A2"].Value = "Contoh Pengisian";
                    worksheet.Cells["A3"].Value = "NOMOR BUKTI";
                    worksheet.Cells["B3"].Value = "TANGGAL";
                    worksheet.Cells["C3"].Value = "KODE SUPPLIER";
                    worksheet.Cells["D3"].Value = "TERM OF PAYMENT";
                    worksheet.Cells["E3"].Value = "PPN (%)";
                    //worksheet.Cells["F3"].Value = "NILAI PPN";
                    worksheet.Cells["F3"].Value = "ONGKOS KIRIM";
                    worksheet.Cells["G3"].Value = "KODE BARANG";
                    worksheet.Cells["H3"].Value = "NAMA BARANG";
                    worksheet.Cells["I3"].Value = "GUDANG";
                    worksheet.Cells["J3"].Value = "QTY";
                    worksheet.Cells["K3"].Value = "HARGA SATUAN";
                    worksheet.Cells["L3"].Value = "TOTAL NILAI DISC";
                    //worksheet.Cells["M3"].Value = "TOTAL";

                    //ISI ROW 1
                    worksheet.Cells["A4"].Value = "INVABC";
                    worksheet.Cells["B4"].Value = "2021-01-07";
                    worksheet.Cells["C4"].Value = "PT X";
                    worksheet.Cells["D4"].Value = "10";
                    worksheet.Cells["E4"].Value = "10";
                    //worksheet.Cells["F4"].Value = "2900";
                    worksheet.Cells["F4"].Value = "10000";
                    worksheet.Cells["G4"].Value = "ABC";
                    worksheet.Cells["H4"].Value = "BATERE ABC";
                    worksheet.Cells["I4"].Value = "001";
                    worksheet.Cells["J4"].Value = "1";
                    worksheet.Cells["K4"].Value = "15000";
                    worksheet.Cells["L4"].Value = "0";
                    //worksheet.Cells["M4"].Value = "15000";

                    //ISI ROW 2
                    worksheet.Cells["A5"].Value = "INVABC";
                    worksheet.Cells["B5"].Value = "2021-01-07";
                    worksheet.Cells["C5"].Value = "PT X";
                    worksheet.Cells["D5"].Value = "10";
                    worksheet.Cells["E5"].Value = "0";
                    //worksheet.Cells["F5"].Value = "2900";
                    worksheet.Cells["F5"].Value = "0";
                    worksheet.Cells["G5"].Value = "ALKALINE";
                    worksheet.Cells["H5"].Value = "BATERE ALKALINE";
                    worksheet.Cells["I5"].Value = "001";
                    worksheet.Cells["J5"].Value = "1";
                    worksheet.Cells["K5"].Value = "14000";
                    worksheet.Cells["L5"].Value = "0";
                    //worksheet.Cells["M5"].Value = "14000";

                    //ISI ROW 3
                    worksheet.Cells["A6"].Value = "INVXYZ";
                    worksheet.Cells["B6"].Value = "2021-01-07";
                    worksheet.Cells["C6"].Value = "PT X";
                    worksheet.Cells["D6"].Value = "10";
                    worksheet.Cells["E6"].Value = "0";
                    //worksheet.Cells["F6"].Value = "0";
                    worksheet.Cells["F6"].Value = "9000";
                    worksheet.Cells["G6"].Value = "ALKALINE";
                    worksheet.Cells["H6"].Value = "BATERE ABC";
                    worksheet.Cells["I6"].Value = "001";
                    worksheet.Cells["J6"].Value = "1";
                    worksheet.Cells["K6"].Value = "15000";
                    worksheet.Cells["L6"].Value = "0";
                    //worksheet.Cells["M6"].Value = "15000";

                    
                    

                    //add formula


                    for (int i = 0; i < 5; i++)
                    {
                        //worksheet.Cells["X" + (5 + i)].Formula = "=S" + (5 + i) + "*R" + (5 + i) + "";
                        //worksheet.Cells["U" + (5 + i)].Formula = "=S" + (5 + i) + "*T" + (5 + i) + "/100";
                        //worksheet.Cells["W" + (5 + i)].Formula = "=S" + (5 + i) + "*V" + (5 + i) + "/100";


                        worksheet.Cells[10 + i, 1].Value = ""; //NOMOR BUKTI
                        worksheet.Cells[10 + i, 2].Value = ""; //TANGGAL
                        worksheet.Cells[10 + i, 3].Value = "-- Silahkan Pilih Supplier --"; //KODE SUPPLIER 
                        worksheet.Cells[10 + i, 4].Value = ""; //TERM OF PAYMENT
                        worksheet.Cells[10 + i, 5].Value = 0; //PPN
                        //worksheet.Cells[10 + i, 6].Value = 0; //NILAI PPN
                        worksheet.Cells[10 + i, 6].Value = 0; //ONGKOS KIRIM
                        worksheet.Cells[10 + i, 7].Value = ""; //KODE BARANG
                        worksheet.Cells[10 + i, 8].Value = ""; //NAMA BARANG
                        worksheet.Cells[10 + i, 9].Value = "-- Silahkan Pilih Gudang --"; //GUDANG
                        worksheet.Cells[10 + i, 10].Value = 0; //QTY
                        worksheet.Cells[10 + i, 11].Value = 0; //HARGA SATUAN
                        worksheet.Cells[10 + i, 12].Value = 0; //TOTAL NILAI DISC
                        //worksheet.Cells[10 + i, 13].Value = 0; //TOTAL
                    }

                    ExcelRange rg0 = worksheet.Cells[9, 1, worksheet.Dimension.End.Row, 12];
                    string tableName0 = "TableInvoicePembelian";
                    ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);

                    table0.Columns[0].Name = "NOMOR BUKTI";
                    table0.Columns[1].Name = "TANGGAL";
                    table0.Columns[2].Name = "KODE SUPPLIER";
                    table0.Columns[3].Name = "TERM OF PAYMENT";
                    table0.Columns[4].Name = "PPN (%)";
                    //table0.Columns[5].Name = "NILAI PPN";
                    table0.Columns[5].Name = "ONGKOS KIRIM";
                    table0.Columns[6].Name = "KODE BARANG";
                    table0.Columns[7].Name = "NAMA BARANG";
                    table0.Columns[8].Name = "GUDANG";
                    table0.Columns[9].Name = "QTY";
                    table0.Columns[10].Name = "HARGA SATUAN";
                    table0.Columns[11].Name = "TOTAL NILAI DISC";
                    //table0.Columns[12].Name = "TOTAL";

                    using (var range = worksheet.Cells[9, 1, 9, 12])
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
                    var sheet2 = worksheet.Workbook.Worksheets.Add("master_supplier");

                    sheet2.Cells[2, 1].Value = "MASTER SUPPLIER";

                    // MASTER SUPPLIER
                    var sSQL = "SELECT SUPP, NAMA, AL, PERSO, TLP FROM APF01 (NOLOCK)";

                    var resultSupplier = EDB.GetDataSet("CString", "APF01", sSQL);
                    if (resultSupplier.Tables[0].Rows.Count > 0)
                    {
                        for (int j = 0; j < resultSupplier.Tables[0].Rows.Count; j++)
                        {
                            sheet2.Cells[4 + j, 1].Value = resultSupplier.Tables[0].Rows[j]["SUPP"];
                            sheet2.Cells[4 + j, 2].Value = resultSupplier.Tables[0].Rows[j]["NAMA"];
                        }
                    }

                    var validation = worksheet.DataValidations.AddListValidation(worksheet.Cells[10, 3, worksheet.Dimension.End.Row, 3].Address);
                    validation.ShowErrorMessage = true;
                    validation.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                    validation.ErrorTitle = "An invalid value was entered";
                    validation.Formula.ExcelFormula = string.Format("=master_supplier!${0}${1}:${2}${3}", "A", 4, "A", 4 + resultSupplier.Tables[0].Rows.Count);

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
                    string tableName = "TableSupplier";
                    ExcelTable table = sheet2.Tables.Add(rg, tableName);
                    table.Columns[0].Name = "KODE_SUPPLIER";
                    table.Columns[1].Name = "NAMA_SUPPLIER";
                    table.ShowHeader = true;
                    table.ShowFilter = true;
                    table.ShowRowStripes = false;
                    // END MARKETPLACES

                    // SHEET 3
                    var sheet3 = worksheet.Workbook.Worksheets.Add("master_gudang");

                    sheet3.Cells[2, 1].Value = "MASTER GUDANG";

                    // GUDANG
                    var gudang = ErasoftDbContext.STF18.ToList();

                    if (gudang.Count() > 0)
                    {
                        var j = 0;
                        foreach(var itemGudang in gudang)
                        {
                            sheet3.Cells[4 + j, 1].Value = itemGudang.Kode_Gudang;
                            sheet3.Cells[4 + j, 2].Value = itemGudang.Nama_Gudang;
                            j += 1;
                        }
                    }

                    var validation2 = worksheet.DataValidations.AddListValidation(worksheet.Cells[10, 9, worksheet.Dimension.End.Row, 9].Address);
                    validation2.ShowErrorMessage = true;
                    validation2.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                    validation2.ErrorTitle = "An invalid value was entered";
                    validation2.Formula.ExcelFormula = string.Format("=master_gudang!${0}${1}:${2}${3}", "A", 4, "A", 4 + gudang.Count());

                    using (var range = sheet3.Cells[3, 1, 3, 2])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                    }

                    ExcelRange rg2 = sheet3.Cells[3, 1, worksheet.Dimension.End.Row, 2];
                    string tableName2 = "TableGudang";
                    ExcelTable table2 = sheet3.Tables.Add(rg2, tableName2);
                    table2.Columns[0].Name = "KODE_GUDANG";
                    table2.Columns[1].Name = "NAMA_GUDANG";
                    table2.ShowHeader = true;
                    table2.ShowFilter = true;
                    table2.ShowRowStripes = false;
                    // END GUDANG

                    ret.byteExcel = package.GetAsByteArray();
                    ret.namaFile = username + "_template_upload_invoice_pembelian.xlsx";

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
                        //"ISNULL(D.KET, '') AS KETERANGAN, ISNULL(A.BRUTO, '') AS BRUTO, ISNULL(A.DISCOUNT,'') AS DISC, ISNULL(A.PPN, '') AS PPN, ISNULL(A.NILAI_PPN, '') AS NILAI_PPN, " +
                        //"ISNULL(D.ONGKOS_KIRIM, '') AS ONGKOS_KIRIM, ISNULL(A.NETTO, '') AS NETTO, ISNULL(D.STATUS_TRANSAKSI, '') AS STATUS_PESANAN, " +
                        "ISNULL(D.KET, '') AS KETERANGAN, ISNULL(A.BRUTO, '') AS BRUTO, ISNULL(A.NILAI_DISC, '') AS DISC, ISNULL(A.PPN, '') AS PPN, ISNULL(A.NILAI_PPN, '') AS NILAI_PPN, " +
                        "ISNULL(A.MATERAI, '') AS ONGKOS_KIRIM, ISNULL(A.NETTO, '') AS NETTO, ISNULL(D.STATUS_TRANSAKSI, '') AS STATUS_PESANAN, " +
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

        //add by otniel 15/09/2020
        public ActionResult ListFakturToCsv()
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };

            try
            {
                string EDBConnID = EDB.GetConnectionString("ConnId");
                var sqlStorage = new SqlServerStorage(EDBConnID);
                var client = new BackgroundJobClient(sqlStorage);

                RecurringJobManager recurJobM = new RecurringJobManager(sqlStorage);
                RecurringJobOptions recurJobOpt = new RecurringJobOptions()
                {
                    QueueName = "1_manage_pesanan",
                };


                var dataLinkFTP = ErasoftDbContext.LINKFTP.ToList();

                var connection_id_upload_file_ftp = dbPathEra + "_job_upload_file_ftp";
                var connection_id_upload_file_ftp1 = dbPathEra + "_job_upload_file_ftp_1";
                var connection_id_upload_file_ftp2 = dbPathEra + "_job_upload_file_ftp_2";

                if (dataLinkFTP.Count() > 0)
                {
                    if (dataLinkFTP[0].STATUS_FTP == "1")
                    {

#if (DEBUG || Debug_AWS)
                        var job = new TransferFTPControllerJob();
                        Task.Run(() => job.FTP_listFakturJob(dbPathEra, "CSV", "000000", "FTP", "UPLOADFTP", username).Wait());
                        //new TransferFTPControllerJob().FTP_listFakturJob(dbPathEra, "CSV", "000000", "FTP", "UPLOADFTP", username);
#else

                        //client.Enqueue<TransferFTPControllerJob>(x => x.FTP_listFakturJob(dbPathEra, "CSV", "000000", "FTP", "UPLOADFTP", username));


                        if (dataLinkFTP != null)
                        {
                            //var dtNow = DateTime.UtcNow.AddHours(7); //yyyyMMddhhmmss
                            recurJobM.RemoveIfExists(connection_id_upload_file_ftp);
                            if (dataLinkFTP[0].JAM1 != null)
                            {
                                string[] splitTime = Convert.ToString(dataLinkFTP[0].JAM1).Split(':');
                                int jam1 = Convert.ToInt32(splitTime[0]) - 7;
                                int menit1 = Convert.ToInt32(splitTime[1]);
                                recurJobM.RemoveIfExists(connection_id_upload_file_ftp1);
                                recurJobM.AddOrUpdate(connection_id_upload_file_ftp1, Hangfire.Common.Job.FromExpression<TransferFTPControllerJob>(x => x.FTP_listFakturJob(dbPathEra, "CSV", "000000", "FTP", "UPLOADFTP", username)), Cron.Daily(jam1), recurJobOpt);
                            }

                            if (dataLinkFTP[0].JAM2 != null)
                            {
                                string[] splitTime = Convert.ToString(dataLinkFTP[0].JAM2).Split(':');
                                int jam2 = Convert.ToInt32(splitTime[0]) - 7;
                                int menit2 = Convert.ToInt32(splitTime[1]);
                                recurJobM.RemoveIfExists(connection_id_upload_file_ftp2);
                                recurJobM.AddOrUpdate(connection_id_upload_file_ftp2, Hangfire.Common.Job.FromExpression<TransferFTPControllerJob>(x => x.FTP_listFakturJob(dbPathEra, "CSV", "000000", "FTP", "UPLOADFTP", username)), Cron.Daily(jam2), recurJobOpt);
                            }

                        }
#endif
                    }
                    else
                    {
                        recurJobM.RemoveIfExists(connection_id_upload_file_ftp);
                        recurJobM.RemoveIfExists(connection_id_upload_file_ftp1);
                        recurJobM.RemoveIfExists(connection_id_upload_file_ftp2);
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
        //end by otniel

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

                    //change by nurul 9/11/2020, bundling
                    //string sSQL = "select brg, nama + ' ' + isnull(nama2, '') as nama from stf02 where type = 3 order by nama,nama2";
                    string sSQL = "select a.brg, nama + ' ' + isnull(nama2, '') as nama from stf02 a (nolock) " +
                                  "left join (select distinct unit from stf03 (nolock)) b on a.brg=b.unit " +
                                  "where type = 3 and isnull(b.unit,'')='' order by nama,nama2";
                    //end change by nurul 9/11/2020, bundling

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

            //try
            //{

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
                            using (System.Data.Entity.DbContextTransaction transaction = eraDB.Database.BeginTransaction())
                            {
                                try
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

                                            //add by nurul 9/11/2020, bundling
                                            var listTempBundling = eraDB.STF03.Select(a => a.Unit).Distinct().ToList();
                                            //end add by nurul 9/11/2020, bundling
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
                                                                transaction.Rollback();
                                                                var errMsg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                                                ret.Errors.Add(errMsg);
                                                                ret.statusSuccess = true;
                                                                return Json(ret, JsonRequestBehavior.AllowGet);
                                                            }
                                                        }
                                                    }
                                                }

                                                ret.countAll = worksheet.Dimension.End.Row;
                                                if (Convert.ToInt32(prog[1]) == 0)
                                                {
                                                    prog[1] = "0";
                                                }

                                                ret.progress = Convert.ToInt32(prog[1]);
                                                var kodeBarangTemp = new List<string>();

                                                if (ret.countAll > 5)
                                                {
                                                    for (int i = Convert.ToInt32(prog[1]); i <= ret.countAll; i++)
                                                    {
                                                        ret.statusLoop = true;
                                                        //ret.progress = i;
                                                        ret.progress += 1;
                                                        //ret.percent = (i * 100) / ret.countAll;
                                                        Functions.SendProgress("Process in progress...", ret.progress, ret.countAll);

                                                        var kd_brg = worksheet.Cells[i, 1].Value == null ? "" : worksheet.Cells[i, 1].Value.ToString();
                                                        if (!string.IsNullOrEmpty(kd_brg))
                                                        {
                                                            var current_brg = listTemp.Where(m => m.BRG == kd_brg).SingleOrDefault();
                                                            if (current_brg != null)
                                                            {
                                                                if (!listTempBundling.Contains(kd_brg))
                                                                {
                                                                    if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 3].Value)))
                                                                    {
                                                                        //stok 0 juga bisa masuk //replace(/(?!-)[^0-9.]/g, "")
                                                                        var valStock = worksheet.Cells[i, 3].Value.ToString();
                                                                        valStock = Regex.Replace(valStock, "[^0-9]", "");

                                                                        //if (Convert.ToInt32(worksheet.Cells[i, 3].Value) >= 0)
                                                                        if (!string.IsNullOrEmpty(valStock))
                                                                            if (Convert.ToInt32(valStock) >= 0)
                                                                            {
                                                                                var checkDBDetail = kodeBarangTemp.Where(p => p == current_brg.BRG).ToList();
                                                                                if (checkDBDetail.Count() == 0)
                                                                                {
                                                                                    var stt04b = new STT04B
                                                                                    {
                                                                                        Gud = gd,
                                                                                        Brg = current_brg.BRG,
                                                                                        //Qty = Convert.ToInt32(worksheet.Cells[i, 3].Value),
                                                                                        Qty = Convert.ToInt32(valStock),
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
                                                                                    eraDB.STT04B.Add(stt04b);
                                                                                    eraDB.SaveChanges();
                                                                                    kodeBarangTemp.Add(current_brg.BRG);
                                                                                }
                                                                                else
                                                                                {
                                                                                    transaction.Rollback();
                                                                                    ret.Errors.Add("Proses Gagal. Kode Barang (" + kd_brg + ") tidak boleh duplikat.");
                                                                                    ret.statusSuccess = true;
                                                                                    return Json(ret, JsonRequestBehavior.AllowGet);
                                                                                }
                                                                            }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    transaction.Rollback();
                                                                    ret.Errors.Add("Proses Gagal. Kode Barang (" + kd_brg + ") merupakan barang bundling");
                                                                    ret.statusSuccess = true;
                                                                    ret.lastRow[file_index] = i;
                                                                    i = ret.countAll;
                                                                    return Json(ret, JsonRequestBehavior.AllowGet);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                transaction.Rollback();
                                                                ret.Errors.Add("Proses Gagal. Kode Barang (" + kd_brg + ") tidak ditemukan");
                                                                ret.statusSuccess = true;
                                                                ret.lastRow[file_index] = i;
                                                                i = ret.countAll;
                                                                return Json(ret, JsonRequestBehavior.AllowGet);
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    transaction.Rollback();
                                                    ret.Errors.Add("Proses Gagal. Data Excel masih kosong");
                                                    ret.statusSuccess = true;
                                                    return Json(ret, JsonRequestBehavior.AllowGet);
                                                }
                                            }
                                            else
                                            {
                                                transaction.Rollback();
                                                ret.Errors.Add("Proses Gagal. Master Barang kosong");
                                                ret.statusSuccess = true;
                                                return Json(ret, JsonRequestBehavior.AllowGet);
                                            }
                                        }
                                        else
                                        {
                                            transaction.Rollback();
                                            ret.Errors.Add("Proses Gagal. Kode gudang tidak ditemukan");
                                            ret.statusSuccess = true;
                                            return Json(ret, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    else
                                    {
                                        transaction.Rollback();
                                        ret.Errors.Add("Proses Gagal. Kode gudang tidak ditemukan");
                                        ret.statusSuccess = true;
                                        return Json(ret, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ret.statusSuccess = true;
                                    transaction.Rollback();
                                    ret.Errors.Add(ex.InnerException == null ? ex.Message : "Data tidak berhasil diproses, " + ex.InnerException.Message);
                                    return Json(ret, JsonRequestBehavior.AllowGet);
                                }

                                try
                                {
                                    transaction.Commit();
                                    ret.statusSuccess = true;
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    ret.Errors.Add(ex.InnerException == null ? ex.Message : "Data tidak berhasil diproses, " + ex.InnerException.Message);
                                    ret.statusSuccess = true;
                                    return Json(ret, JsonRequestBehavior.AllowGet);
                                }
                            }


                        }
                    }
                }

            }
            //}
            //catch (Exception ex)
            //{
            //    ret.statusSuccess = true;
            //    ret.Errors.Add(ex.InnerException == null ? ex.Message : "Data tidak berhasil diproses, " + ex.InnerException.Message);
            //}

            return Json(ret, JsonRequestBehavior.AllowGet);

        }
        //end by Indra 16 apr 2020, upload stokopname

        //ADD BY NURUL 23/7/2020
        public ActionResult ListPackingListtoExcel(string noPackingList, string mode, string tgl)
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };

            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    if (noPackingList != null && noPackingList != "undefined" && noPackingList != "")
                    {
                        if (mode == "1")
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Packing List");
                            //change by nurul 27/9/2020
                            //string sSQL = "SELECT A.NO_PESANAN, A.BRG, B.NAMA + ' ' + (ISNULL(NAMA2, '')) NAMA_BARANG, QTY, PEMBELI, MARKETPLACE, ISNULL(D.NO_REFERENSI,'') AS NO_REFERENSI " +
                            //    //add by nurul 18/9/2020, Barang Multi SKU
                            //    ", ISNULL(E.BRG_MULTISKU,'') BRG_MULTISKU , ISNULL(E.NAMA_BRG_MULTISKU,'') NAMA_BRG_MULTISKU " +
                            //    //end add by nurul 18/9/2020, Barang Multi SKU
                            //    "FROM SOT03C A INNER JOIN STF02 B ON A.BRG = B.BRG " +
                            //    "INNER JOIN SOT03B C ON A.NO_BUKTI = C.NO_BUKTI AND A.NO_PESANAN = C.NO_PESANAN " +
                            //    "INNER JOIN SOT01A D ON A.NO_PESANAN = D.NO_BUKTI  " +
                            //    //add by nurul 18/9/2020, Barang Multi SKU
                            //    "LEFT JOIN (SELECT B.NO_BUKTI, ISNULL(B.BRG_MULTISKU,'') BRG_MULTISKU, ISNULL(C.NAMA + ' ' + (ISNULL(C.NAMA2, '')),'') NAMA_BRG_MULTISKU FROM SOT03C A INNER JOIN SOT01B B ON A.NO_PESANAN = B.NO_BUKTI INNER JOIN STF02 C ON B.BRG_MULTISKU = C.BRG where A.NO_BUKTI = '" + noPackingList + "')E ON A.NO_PESANAN = E.NO_BUKTI " +
                            //    //end add by nurul 18/9/2020, Barang Multi SKU
                            //    "WHERE A.NO_BUKTI = '" + noPackingList + "' GROUP BY A.NO_PESANAN,A.BRG,B.NAMA,B.NAMA2,QTY, PEMBELI, MARKETPLACE,D.NO_REFERENSI ,E.BRG_MULTISKU,E.NAMA_BRG_MULTISKU ORDER BY A.NO_PESANAN, NAMA_BARANG ";

                            string sSQL = "SELECT A.NO_BUKTI AS NO_PESANAN, B.BRG,C.NAMA + ' ' + (ISNULL(C.NAMA2, '')) NAMA_BARANG,B.QTY,A.NAMAPEMESAN AS PEMBELI,F.NAMAMARKET + ' (' + E.PERSO +')' AS MARKETPLACE, ISNULL(A.NO_REFERENSI,'')NO_REFERENSI, ISNULL(B.BRG_MULTISKU,'')BRG_MULTISKU, ISNULL(D.NAMA + ' ' + (ISNULL(D.NAMA2, '')),'') NAMA_BRG_MULTISKU " +
                            "FROM SOT01A A(nolock) INNER JOIN SOT01B B(nolock) ON A.NO_BUKTI=B.NO_BUKTI LEFT JOIN STF02 C(nolock) ON B.BRG=C.BRG LEFT JOIN STF02 D ON D.BRG=B.BRG_MULTISKU  " +
                            "LEFT JOIN ARF01 E(nolock) ON A.CUST=E.CUST LEFT JOIN MO..MARKETPLACE F(nolock) ON E.NAMA=F.IDMARKET " +
                            "WHERE A.NO_BUKTI IN (SELECT NO_PESANAN FROM SOT03C WHERE NO_BUKTI='" + noPackingList + "')  and isnull(A.status_kirim,'') <> '5' ";
                            //end change by nurul 27/9/2020
                            var lsPacking = EDB.GetDataSet("CString", "SO", sSQL);
                            if (lsPacking.Tables[0].Rows.Count > 0)
                            {
                                worksheet.Cells["A1"].Value = "PACKING LIST";
                                worksheet.Cells["A2"].Value = "NO. BUKTI    : " + noPackingList;
                                worksheet.Cells["A3"].Value = "Tanggal      : " + tgl;
                                for (int i = 0; i < lsPacking.Tables[0].Rows.Count; i++)
                                {
                                    worksheet.Cells[6 + i, 1].Value = lsPacking.Tables[0].Rows[i]["NO_PESANAN"];
                                    worksheet.Cells[6 + i, 2].Value = lsPacking.Tables[0].Rows[i]["NO_REFERENSI"];
                                    worksheet.Cells[6 + i, 3].Value = lsPacking.Tables[0].Rows[i]["BRG"];
                                    worksheet.Cells[6 + i, 4].Value = lsPacking.Tables[0].Rows[i]["NAMA_BARANG"];
                                    //add by nurul 18/9/2020, Barang Multi SKU
                                    worksheet.Cells[6 + i, 5].Value = lsPacking.Tables[0].Rows[i]["BRG_MULTISKU"];
                                    worksheet.Cells[6 + i, 6].Value = lsPacking.Tables[0].Rows[i]["NAMA_BRG_MULTISKU"];
                                    //end add by nurul 18/9/2020, Barang Multi SKU
                                    worksheet.Cells[6 + i, 7].Value = lsPacking.Tables[0].Rows[i]["QTY"];
                                    worksheet.Cells[6 + i, 8].Value = lsPacking.Tables[0].Rows[i]["PEMBELI"];
                                    worksheet.Cells[6 + i, 9].Value = lsPacking.Tables[0].Rows[i]["MARKETPLACE"];

                                }
                                ExcelRange rg0 = worksheet.Cells[5, 1, worksheet.Dimension.End.Row, 9];
                                string tableName0 = "TablePackingList";
                                ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);

                                table0.Columns[0].Name = "NO PESANAN";
                                table0.Columns[1].Name = "NO REFERENSI";
                                table0.Columns[2].Name = "KODE BARANG";
                                table0.Columns[3].Name = "NAMA BARANG";
                                table0.Columns[4].Name = "KODE BARANG MULTI SKU";
                                table0.Columns[5].Name = "NAMA BARANG MULTI SKU";
                                table0.Columns[6].Name = "QTY";
                                table0.Columns[7].Name = "PEMBELI";
                                table0.Columns[8].Name = "MARKETPLACE";

                                using (var range = worksheet.Cells[5, 1, 5, 9])
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
                                ret.namaFile = username + "_PackingList_" + noPackingList + ".xlsx";

                            }
                            else
                            {
                                ret.Errors.Add("Tidak ada data packing list");
                            }
                        }
                        else
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Picking List");
                            string sSQL = "SELECT A.BRG, B.NAMA + ' ' + (ISNULL(NAMA2, '')) NAMA_BARANG, sum(QTY) QTY, ISNULL(A.BARCODE,'') as BARCODE, ISNULL(B.LKS,'') as RAK " +
                                "from SOT03C A (nolock) INNER JOIN STF02 B(nolock) ON A.BRG = B.BRG LEFT JOIN SOT01A C(NOLOCK) ON A.NO_PESANAN=C.NO_BUKTI " +
                                "WHERE A.NO_BUKTI = '" + noPackingList + "' and isnull(C.status_kirim,'') <> '5' GROUP BY A.BRG, B.NAMA, B.NAMA2, A.BARCODE, B.LKS ";
                            var lsPicking = EDB.GetDataSet("CString", "SO", sSQL);
                            if (lsPicking.Tables[0].Rows.Count > 0)
                            {
                                worksheet.Cells["A1"].Value = "PICKING LIST";
                                worksheet.Cells["A2"].Value = "NO. BUKTI    : " + noPackingList;
                                worksheet.Cells["A3"].Value = "Tanggal      : " + tgl;
                                for (int i = 0; i < lsPicking.Tables[0].Rows.Count; i++)
                                {
                                    worksheet.Cells[6 + i, 1].Value = lsPicking.Tables[0].Rows[i]["BRG"];
                                    worksheet.Cells[6 + i, 2].Value = lsPicking.Tables[0].Rows[i]["BARCODE"];
                                    worksheet.Cells[6 + i, 3].Value = lsPicking.Tables[0].Rows[i]["NAMA_BARANG"];
                                    worksheet.Cells[6 + i, 4].Value = lsPicking.Tables[0].Rows[i]["RAK"];
                                    worksheet.Cells[6 + i, 5].Value = lsPicking.Tables[0].Rows[i]["QTY"];

                                    //add by nurul 9/2/2021
                                    var tempRef = "";
                                    var getListNoref = EDB.GetDataSet("CString", "SOT01A", "select brg, ISNULL(SUM(qty),0) AS QTY,isnull(no_referensi,'') as no_referensi from sot01a a (nolock) inner join sot01b b (nolock) on a.no_bukti=b.no_bukti where brg='" + lsPicking.Tables[0].Rows[i]["BRG"] + "' and a.no_bukti in (select no_pesanan from sot03b where no_bukti='" + noPackingList + "') GROUP BY brg,no_referensi");
                                    if (getListNoref.Tables[0].Rows.Count > 0)
                                    {
                                        for (int a = 0; a < getListNoref.Tables[0].Rows.Count; a++)
                                        {
                                            if (!string.IsNullOrEmpty(getListNoref.Tables[0].Rows[a]["no_referensi"].ToString()))
                                            {
                                                tempRef = tempRef + getListNoref.Tables[0].Rows[a]["no_referensi"].ToString() + " (" + getListNoref.Tables[0].Rows[a]["qty"].ToString() + ") " + Environment.NewLine + " ";
                                            }
                                        }
                                    }
                                    if (tempRef != "")
                                    {
                                        tempRef = tempRef.Substring(0, tempRef.Length - 4);
                                    }
                                    worksheet.Cells[6 + i, 6].Value = tempRef;
                                    //end add by nurul 9/2/2021
                                }
                                //ExcelRange rg0 = worksheet.Cells[5, 1, worksheet.Dimension.End.Row, 5];
                                ExcelRange rg0 = worksheet.Cells[5, 1, worksheet.Dimension.End.Row, 6];
                                string tableName0 = "TablePackingList";
                                ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);

                                table0.Columns[0].Name = "KODE BARANG";
                                table0.Columns[1].Name = "KODE BARCODE";
                                table0.Columns[2].Name = "NAMA BARANG";
                                table0.Columns[3].Name = "LOKASI RAK";
                                table0.Columns[4].Name = "QTY";
                                //add by nurul 9/2/2021
                                table0.Columns[5].Name = "NO REFERENSI";
                                //end add by nurul 9/2/2021

                                //using (var range = worksheet.Cells[5, 1, 5, 5])
                                using (var range = worksheet.Cells[5, 1, 5, 6])
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
                                ret.namaFile = username + "_PickingList_" + noPackingList + ".xlsx";

                            }
                            else
                            {
                                ret.Errors.Add("Tidak ada data picking list");
                            }
                        }
                    }
                    else
                    {
                        ret.Errors.Add("Tidak ada no bukti packing list");
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
        //END ADD BY NURUL 23/7/2020

        //add by nurul 24/2/2021
        public ActionResult ListRekapBarangToExcel(string search, string filter, string filtervalue, string drTgl, string sdTgl)
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };

            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Rekap Barang");
                    var Drtgl = (drTgl != "" ? DateTime.ParseExact(drTgl, "dd/MM/yyyy",
                        System.Globalization.CultureInfo.InvariantCulture) : DateTime.Today);
                    var Sdtgl = (sdTgl != "" ? DateTime.ParseExact(sdTgl, "dd/MM/yyyy",
                        System.Globalization.CultureInfo.InvariantCulture) : DateTime.Today);
                    var tempDrtgl = Drtgl.ToString("yyyy-MM-dd") + " 00:00:00.000";
                    var tempSdtgl = Sdtgl.ToString("yyyy-MM-dd") + " 23:59:59.999";

                    string[] getkata = search.Split(' ');
                    string sSQLnama = "";
                    string sSQLkode = "";
                    string sSQLrak = "";
                    if (getkata.Length > 0)
                    {
                        if (search != "")
                        {
                            for (int i = 0; i < getkata.Length; i++)
                            {
                                if (i > 0)
                                {
                                    sSQLkode += " and ";
                                    sSQLnama += " and ";
                                    sSQLrak += " and ";
                                }

                                sSQLkode += " ( A.BRG like '%" + getkata[i] + "%' ) ";
                                sSQLnama += "  ( (ISNULL(B.NAMA,'') + ' ' + ISNULL(B.NAMA2,'')) like '%" + getkata[i] + "%' ) ";
                                sSQLrak += " ( B.LKS like '%" + getkata[i] + "%' ) ";
                            }
                        }
                    }

                    string sSQL = "";
                    sSQL += "SELECT A.BRG, (ISNULL(B.NAMA,'') + ' ' + ISNULL(B.NAMA2,'')) AS NAMA_BARANG, ISNULL(sum(ISNULL(QTY,0)),0) QTY, ISNULL(B.LKS,'') as RAK ";
                    sSQL += "from SOT03A C(NOLOCK) INNER JOIN SOT03C A(NOLOCK) ON A.NO_BUKTI=C.NO_BUKTI INNER JOIN STF02 B(NOLOCK) ON A.BRG = B.BRG ";
                    sSQL += "LEFT JOIN SOT01A D(NOLOCK) ON A.NO_PESANAN=D.NO_BUKTI ";
                    sSQL += "WHERE C.TGL BETWEEN '" + tempDrtgl + "' AND '" + tempSdtgl + "' AND ISNULL(D.STATUS,'') <> '2' AND ISNULL(STATUS_KIRIM,'') <> '5' ";
                    if (search != "")
                    {
                        sSQL += " AND ( " + sSQLkode + " or " + sSQLnama + " or " + sSQLrak + " ) ";
                    }
                    var sSQLGrouping = "GROUP BY A.BRG, B.NAMA, B.NAMA2, B.LKS ";
                    string ssqlOrder = "";
                    switch (filter)
                    {
                        case "kode":
                            {
                                ssqlOrder += "order by A.BRG ";
                            }
                            break;
                        case "nama":
                            {
                                ssqlOrder += "order by (ISNULL(B.NAMA,'') + ' ' + ISNULL(B.NAMA2,'')) ";
                            }
                            break;
                        case "rak":
                            {
                                ssqlOrder += "order by B.LKS ";
                            }
                            break;
                        case "qty":
                            {
                                ssqlOrder += "order by sum(QTY) ";
                            }
                            break;
                        default:
                            {
                                ssqlOrder += "order by B.LKS ";
                            }
                            break;
                    }
                    if (filtervalue == "desc")
                    {
                        ssqlOrder += " DESC ";
                    }
                    else if (filtervalue == "asc")
                    {
                        ssqlOrder += " ASC ";
                    }
                    else
                    {
                        ssqlOrder += ", A.BRG ASC ";
                    }
                    var lsRekapBrg = EDB.GetDataSet("CString", "SO", sSQL + sSQLGrouping + ssqlOrder);
                    if (lsRekapBrg.Tables[0].Rows.Count > 0)
                    {
                        worksheet.Cells["A1"].Value = "REKAP BARANG";
                        worksheet.Cells["A2"].Value = "Dari Tanggal    : " + Drtgl.ToString("dd/MM/yyyy");
                        worksheet.Cells["A3"].Value = "S/d Tanggal      : " + Sdtgl.ToString("dd/MM/yyyy");
                        for (int i = 0; i < lsRekapBrg.Tables[0].Rows.Count; i++)
                        {
                            worksheet.Cells[6 + i, 1].Value = lsRekapBrg.Tables[0].Rows[i]["BRG"];
                            worksheet.Cells[6 + i, 2].Value = lsRekapBrg.Tables[0].Rows[i]["NAMA_BARANG"];
                            worksheet.Cells[6 + i, 3].Value = lsRekapBrg.Tables[0].Rows[i]["RAK"];
                            worksheet.Cells[6 + i, 4].Value = lsRekapBrg.Tables[0].Rows[i]["QTY"];
                        }
                        ExcelRange rg0 = worksheet.Cells[5, 1, worksheet.Dimension.End.Row, 4];
                        string tableName0 = "TableRekapBarang";
                        ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);

                        table0.Columns[0].Name = "KODE BARANG";
                        table0.Columns[1].Name = "NAMA BARANG";
                        table0.Columns[2].Name = "LOKASI RAK";
                        table0.Columns[3].Name = "QTY";

                        using (var range = worksheet.Cells[5, 1, 5, 4])
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
                        ret.namaFile = username + "_RekapBarang_" + Drtgl.ToString("dd-MM-yyyy") + "_To_" + Sdtgl.ToString("dd-MM-yyyy") + ".xlsx";

                    }
                    else
                    {
                        ret.Errors.Add("Tidak ada data rekap barang");
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
        //end add by nurul 24/2/2021

        public ActionResult DownloadExcelHJual(string cust)
        {
            var ret = new BindDownloadExcel
            {
                Errors = new List<string>()
            };
            try
            {
                var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                var idMarket = Convert.ToInt32(customer.NAMA);
                var mp = MoDbContext.Marketplaces.Where(m => m.IdMarket == idMarket).FirstOrDefault();

                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Harga Jual");

                    //worksheet.Protection.IsProtected = true;
                    //worksheet.Column(5).Style.Locked = false;

                    string sSQL = "SELECT S.BRG, ";
                    //sSQL += "replace(replace(S.NAMA, char(10), ''), char(13), '') + ISNULL(replace(replace(S.NAMA2, char(10), ''), char(13), ''), '') AS NAMA, ";
                    //sSQL += "M.NAMAMARKET + '(' + replace(replace(A.PERSO, char(10), ''), char(13), '') + ')' AS AKUN,H.HJUAL, M.IDMARKET, ISNULL(STF10.HPOKOK, 0) AS HPOKOK ";
                    //sSQL += "FROM STF02 S INNER JOIN STF02H H ON S.BRG = H.BRG INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM ";
                    //sSQL += "INNER JOIN MO..MARKETPLACE M ON A.NAMA = M.IDMARKET LEFT JOIN STF10 ON S.BRG = STF10.BRG WHERE TYPE = '3' ORDER BY NAMA,M.IDMARKET";
                    sSQL += "replace(replace(S.NAMA, char(10), ''), char(13), '') + ISNULL(replace(replace(S.NAMA2, char(10), ''), char(13), ''), '') AS NAMA, ";
                    sSQL += "H.HJUAL, ISNULL(BRG_MP, '') BRG_MP, ISNULL(E.KET, '') KET ";
                    sSQL += "FROM STF02 S INNER JOIN STF02H H ON S.BRG = H.BRG INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM ";
                    //CHANGE BY NURUL 16/10/2020, BRG BUNDLING TIDAK BOLEH UBAH HARGA JUAL DARI SINI
                    //sSQL += "LEFT JOIN STF02E E ON S.SORT1 = E.KODE AND E.LEVEL = 1 WHERE TYPE in ('3', '6') AND CUST = '" + cust + "' ORDER BY NAMA";
                    sSQL += "LEFT JOIN STF02E E ON S.SORT1 = E.KODE AND E.LEVEL = 1 ";
                    sSQL += "LEFT JOIN (SELECT DISTINCT UNIT FROM STF03) B ON S.BRG=B.UNIT ";
                    sSQL += "WHERE TYPE in ('3', '6') AND CUST = '" + cust + "' AND ISNULL(B.UNIT,'')='' ";
                    sSQL += "ORDER BY NAMA";

                    //CHANGE BY NURUL 16/10/2020, BRG BUNDLING TIDAK BOLEH UBAH HARGA JUAL DARI SINI
                    var dsBarang = EDB.GetDataSet("CString", "STF02", sSQL);

                    worksheet.Cells["A1"].Value = "Akun Marketplace :";
                    worksheet.Cells[1, 2].Value = mp.NamaMarket + "(" + customer.PERSO + ")";
                    worksheet.Cells[1, 3].Value = cust;
                    var dataCount = "Data barang yang link dengan MP / Total Data : ";
                    int dataWithLink = 0;
                    if (dsBarang.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsBarang.Tables[0].Rows.Count; i++)
                        {
                            worksheet.Cells[4 + i, 1].Value = dsBarang.Tables[0].Rows[i]["BRG"].ToString();
                            worksheet.Cells[4 + i, 2].Value = dsBarang.Tables[0].Rows[i]["NAMA"].ToString();
                            worksheet.Cells[4 + i, 3].Value = dsBarang.Tables[0].Rows[i]["KET"].ToString();
                            worksheet.Cells[4 + i, 4].Value = dsBarang.Tables[0].Rows[i]["BRG_MP"].ToString();
                            worksheet.Cells[4 + i, 5].Value = dsBarang.Tables[0].Rows[i]["HJUAL"].ToString();
                            if (!string.IsNullOrEmpty(dsBarang.Tables[0].Rows[i]["BRG_MP"].ToString()))
                            {
                                dataWithLink++;
                            }
                        }
                        worksheet.Cells[2, 2].Value = dataCount + dataWithLink + " / " + dsBarang.Tables[0].Rows.Count;
                        ExcelRange rg0 = worksheet.Cells[3, 1, worksheet.Dimension.End.Row, 6];
                        string tableName0 = "TableBarang";
                        ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);
                        table0.Columns[0].Name = "KODE BARANG";
                        table0.Columns[1].Name = "NAMA BARANG";
                        table0.Columns[2].Name = "KATEGORI BARANG";
                        table0.Columns[3].Name = "KODE BARANG MP";
                        table0.Columns[4].Name = "HARGA JUAL LAMA";
                        table0.Columns[5].Name = "HARGA JUAL BARU";

                        table0.ShowHeader = true;
                        table0.ShowFilter = true;
                        table0.ShowRowStripes = false;
                        worksheet.Cells.AutoFitColumns(0);

                        //return File(package.GetAsByteArray(), System.Net.Mime.MediaTypeNames.Application.Octet, username + "_hargaJual" + ".xlsx");
                        ret.byteExcel = package.GetAsByteArray();
                        ret.namaFile = "HJual_" + mp.NamaMarket + "_" + customer.PERSO + ".xlsx";
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

        public ActionResult UploadExcelHJual(int page, int dataProcessed, int NH, int NL, int index_file)
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
            ret.progress = dataProcessed;
            ret.jmlNH = NH;
            ret.jmlNL = NL;
            var filename = "";
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
                            filename = file.FileName;
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
                                        string cust = worksheet.Cells[1, 3].Value == null ? "" : worksheet.Cells[1, 3].Value.ToString();
                                        if (!string.IsNullOrEmpty(cust))
                                        {
                                            var customer = eraDB.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                                            if (customer != null)
                                            {
                                                if (page == 0)
                                                {
                                                    var cekData = EDB.GetDataSet("CString", "TEMP_UPDATE_HJUAL", " SELECT * FROM TEMP_UPDATE_HJUAL WHERE INDEX_FILE <> " + index_file + " AND IDMARKET = " + customer.RecNum);
                                                    if (cekData.Tables[0].Rows.Count > 0)
                                                    {
                                                        ret.Errors.Add("Anda sudah mengupload excel harga jual untuk akun ini. Silahkan upload excel untuk akun marketplace lain.");
                                                        return Json(ret, JsonRequestBehavior.AllowGet);
                                                    }
                                                    //var resetDataTemp = EDB.ExecuteSQL("CString", System.Data.CommandType.Text, "DELETE FROM TEMP_UPDATE_HJUAL WHERE IDMARKET = " + customer.RecNum);
                                                    var resetDataTemp = EDB.ExecuteSQL("CString", System.Data.CommandType.Text, "DELETE FROM TEMP_UPDATE_HJUAL WHERE INDEX_FILE = " + index_file);

                                                }
                                                string namaMP = mp.Where(m => m.IdMarket.ToString() == customer.NAMA).SingleOrDefault().NamaMarket;
                                                ret.cust.Add(cust);
                                                ret.namaCust.Add(namaMP + "(" + customer.PERSO + ")");
                                                int dataPerPage = 300;
                                                int maxData = 4 + (page * dataPerPage) + dataPerPage;
                                                if (4 + (page * dataPerPage) + dataPerPage >= worksheet.Dimension.End.Row)
                                                {
                                                    ret.nextFile = true;
                                                    maxData = worksheet.Dimension.End.Row;
                                                }

                                                //add by nurul 9/11/2020, bundling
                                                var listTempBundling = eraDB.STF03.Select(a => a.Unit).Distinct().ToList();
                                                //end add by nurul 9/11/2020, bundling
                                                var sSQL = "INSERT INTO TEMP_UPDATE_HJUAL(BRG, IDMARKET, INDEX_FILE, HJUAL, TGL_INPUT, USERNAME) VALUES ";
                                                //loop all rows
                                                for (int i = 4 + (page * dataPerPage); i <= maxData; i++)
                                                {
                                                    var kd_brg = worksheet.Cells[i, 1].Value == null ? "" : worksheet.Cells[i, 1].Value.ToString();
                                                    if (!string.IsNullOrEmpty(kd_brg))
                                                    {
                                                        if (!listTempBundling.Contains(kd_brg))
                                                        {
                                                            if (string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 6].Value)))
                                                            {
                                                                ret.jmlNH++;
                                                                // barang varian tapi tidak diisi kode brg induk di excel
                                                                //break;
                                                            }
                                                            else
                                                            {
                                                                if (string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 4].Value)))
                                                                {
                                                                    ret.jmlNL++;
                                                                }
                                                                var sSQL2 = " ('" + worksheet.Cells[i, 1].Value.ToString() + "',";
                                                                sSQL2 += customer.RecNum + "," + index_file + "," + worksheet.Cells[i, 6].Value.ToString() + ", '";
                                                                sSQL2 += DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "')";
                                                                var result = EDB.ExecuteSQL("CString", CommandType.Text, sSQL + sSQL2);
                                                                if (result == 1 && !string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 4].Value)))
                                                                {
                                                                    ret.progress++;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //ret.nextFile = true;
                                                            ret.Errors.Add(namaMP + "(" + customer.PERSO + ") : Kode barang (" + kd_brg + ") merupakan barang bundling di baris " + i);
                                                            //ret.lastRow[file_index] = i;
                                                            //break;
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
            var sSQL3 = "UPDATE LOG_HARGAJUAL_A SET FILE_" + index_file + " = '" + filename + "', JML_BRG_" + index_file + " = '" + ret.progress + "/" + ret.lastRow[0] + "', ";
            sSQL3 += "JML_BRG_NH_" + index_file + " = " + ret.jmlNH + ", JML_BRG_NL_" + index_file + " = " + ret.jmlNL + " WHERE STATUS = 0";
            EDB.ExecuteSQL("CString", CommandType.Text, sSQL3);

            return Json(ret, JsonRequestBehavior.AllowGet);
        }
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

        public int jmlNH { get; set; }
        public int jmlNL { get; set; }
    }

    public class BindDownloadExcel
    {
        public List<string> Errors { get; set; }
        public byte[] byteExcel { get; set; }
        public string namaFile { get; set; }
    }

}