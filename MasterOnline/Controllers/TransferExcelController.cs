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

        public class dataByte
        {
            public byte[] data { get; set; }
        }

        public ActionResult UploadXcelSaldoAwal()
        {
            //var file = Request.Files[0];
            //List<string> excelData = new List<string>();
            //var listCust = new List<string>();
            BindUploadExcel ret = new BindUploadExcel();
            ret.Errors = new List<string>();
            ret.namaGudang = new List<string>();
            ret.lastRow = new List<int>();
            try
            {
                var mp = MoDbContext.Marketplaces.ToList();

                byte[] dataByte = UploadFileServices.UploadFile(Request.Files[0]);
                
                for (int file_index = 0; file_index < Request.Files.Count; file_index++)
                {
                    //remark by fauzi for upload with method Server Side Rendering
                    //var file = Request.Files[file_index];
                    //if (file != null && file.ContentLength > 0)
                    //{
                    //    byte[] data;
                    ret.lastRow.Add(0);
                    //    using (Stream inputStream = file.InputStream)
                    //    {
                    //        MemoryStream memoryStream = inputStream as MemoryStream;
                    //        if (memoryStream == null)
                    //        {
                    //            memoryStream = new MemoryStream();
                    //            inputStream.CopyTo(memoryStream);
                    //        }
                    //        data = memoryStream.ToArray();
                    //    }
                    //end remark

                    using (MemoryStream stream = new MemoryStream(dataByte))
                        {
                            using (ExcelPackage excelPackage = new ExcelPackage(stream))

                            //FileInfo existingFile = new FileInfo("C:\\Users\\Agashi\\source\\repos\\MODev\\MasterOnline\\Content\\Uploaded\\Setiawan_qty_hargamodal.xlsx");
                            //using (ExcelPackage excelPackage = new ExcelPackage(existingFile))
                            {
                                using (ErasoftContext eraDB = new ErasoftContext(DataSourcePath, dbPathEra))
                                {
                                    eraDB.Database.CommandTimeout = 180;
                                    //loop all worksheets
                                    var worksheet = excelPackage.Workbook.Worksheets[1];
                                    //foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                                    //{
                                    string gd = worksheet.Cells[2, 1].Value == null ? "" : worksheet.Cells[2, 1].Value.ToString();
                                    if (!string.IsNullOrEmpty(gd))
                                    {
                                        var gudang = eraDB.STF18.Where(m => m.Kode_Gudang == gd).FirstOrDefault();
                                        if (gudang != null)
                                        {
                                            //string namaMP = mp.Where(m => m.IdMarket.ToString() == customer.NAMA).SingleOrDefault().NamaMarket;
                                            ret.namaGudang.Add(gudang.Nama_Gudang);
                                            //ret.namaCust.Add(namaMP + "(" + customer.PERSO + ")");

                                            var listTemp = eraDB.STF02.Where(m => m.TYPE == "3").ToList();
                                            if (listTemp.Count > 0)
                                            {
                                                #region create induk
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

                                                //change by nurul 23/12/2019, perbaikan no bukti
                                                //var listStokInDb = eraDB.STT01A.OrderBy(p => p.ID).ToList();
                                                //var digitAkhir = "";
                                                //var noStok = "";

                                                //if (listStokInDb.Count == 0)
                                                //{
                                                //    digitAkhir = "000001";
                                                //    noStok = $"ST{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                                                //    eraDB.Database.ExecuteSqlCommand("DBCC CHECKIDENT (STT01A, RESEED, 0)");
                                                //}
                                                //else
                                                //{
                                                //    var lastRecNum = listStokInDb.Last().ID;
                                                //    var lastKode = listStokInDb.Last().Nobuk;
                                                //    lastRecNum++;

                                                //    digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                                                //    noStok = $"ST{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";

                                                //    if (noStok == lastKode)
                                                //    {
                                                //        lastRecNum++;
                                                //        digitAkhir = lastRecNum.ToString().PadLeft(6, '0');
                                                //        noStok = $"ST{DateTime.Now.Year.ToString().Substring(2, 2)}{digitAkhir}";
                                                //    }
                                                //}
                                                var lastBukti = new ManageController().GenerateAutoNumber(ErasoftDbContext, "ST", "STT01A", "Nobuk");
                                                //var lastBukti = ManageController().GenerateAutoNumber(ErasoftDbContext, "ST", "STT01A", "Nobuk");
                                                var noStok = "ST" + DateTime.UtcNow.AddHours(7).Year.ToString().Substring(2, 2) + Convert.ToString(Convert.ToInt32(lastBukti) + 1).PadLeft(6, '0');
                                                //end change by nurul 23/12/2019, perbaikan no bukti

                                                stt01a.Nobuk = noStok;


                                                //change by nurul 23/12/2019, perbaikan no_bukti
                                                //eraDB.STT01A.Add(stt01a);
                                                //try
                                                //{
                                                //    //save header
                                                //    eraDB.SaveChanges();
                                                //}
                                                //catch (Exception ex)
                                                //{
                                                //    var errMsg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                                //    ret.Errors.Add(errMsg);
                                                //    return Json(ret, JsonRequestBehavior.AllowGet);
                                                //}
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
                                                            eraDB.STT01A.Add(stt01a);
                                                            eraDB.SaveChanges();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var errMsg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                                        ret.Errors.Add(errMsg);
                                                        return Json(ret, JsonRequestBehavior.AllowGet);
                                                    }
                                                }
                                                //end change by nurul 23/12/2019, perbaikan no bukti

                                                
                                                #endregion
                                                //loop all rows
                                                for (int i = 5; i <= worksheet.Dimension.End.Row; i++)
                                                {
                                                    var kd_brg = worksheet.Cells[i, 1].Value == null ? "" : worksheet.Cells[i, 1].Value.ToString();
                                                    if (!string.IsNullOrEmpty(kd_brg))
                                                    {
                                                        var current_brg = listTemp.Where(m => m.BRG == kd_brg).SingleOrDefault();
                                                        if (current_brg != null)
                                                        {
                                                            //if (worksheet.Cells[i, 3].Value != null)
                                                            if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 3].Value)))
                                                            {
                                                                //change 7 Nov 2019, stok 0 juga bisa masuk
                                                                //if (Convert.ToInt32(worksheet.Cells[i, 3].Value) > 0)
                                                                if (Convert.ToInt32(worksheet.Cells[i, 3].Value) >= 0)
                                                                //end change 7 Nov 2019, stok 0 juga bisa masuk
                                                                {
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
                                                                        Nobuk = stt01a.Nobuk,
                                                                        Satuan = "2",
                                                                    };
                                                                    stt01b.Kobar = current_brg.BRG;
                                                                    stt01b.Ke_Gd = gd;
                                                                    //stt01b.Harsat = Convert.ToDouble(worksheet.Cells[i, 4].Value);
                                                                    if (!string.IsNullOrEmpty(Convert.ToString(worksheet.Cells[i, 4].Value)))
                                                                    {
                                                                        stt01b.Harsat = Convert.ToDouble(worksheet.Cells[i, 4].Value);
                                                                    }
                                                                    else
                                                                    {
                                                                        //stt01b.Harsat = current_brg.HJUAL;
                                                                        stt01b.Harsat = 0;
                                                                    }
                                                                    stt01b.Qty = Convert.ToInt32(worksheet.Cells[i, 3].Value);
                                                                    stt01b.Harga = stt01b.Harsat * stt01b.Qty;
                                                                    eraDB.STT01B.Add(stt01b);
                                                                    eraDB.SaveChanges();
                                                            }
                                                            }

                                                            //eraDB.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            ret.Errors.Add("Kode Barang (" + kd_brg + ") tidak ditemukan");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ret.Errors.Add("Kode barang tidak ditemukan lagi di baris " + i);
                                                        ret.lastRow[file_index] = i;
                                                        i = worksheet.Dimension.End.Row;
                                                        //break;
                                                    }
                                                }
                                                //eraDB.SaveChanges();
                                                if (ret.lastRow[file_index] == 0)
                                                    ret.lastRow[file_index] = worksheet.Dimension.End.Row;

                                                var doUpdateStock = new ManageController().MarketplaceLogRetryStock();
                                            }
                                            else
                                            {
                                                ret.Errors.Add("Data Barang tidak ditemukan");
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
                        //        }

                    //}
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
    }

    public class BindDownloadExcel
    {
        public List<string> Errors { get; set; }
        public byte[] byteExcel { get; set; }
        public string namaFile { get; set; }
    }

}
