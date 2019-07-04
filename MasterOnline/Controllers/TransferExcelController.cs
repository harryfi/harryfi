using Erasoft.Function;
using MasterOnline.ViewModels;
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

        public TransferExcelController()
        {
            MoDbContext = new MoDbContext();
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);

                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
                username = sessionData.Account.Username;
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    username = accFromUser.Username;
                }
            }
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
                            worksheet.Cells[5, 2].Value = "tidak boleh lebih dari 11 karakter";
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
                            #endregion
                            //worksheet.Cells[9, 1].Value = "NAMA1";
                            //worksheet.Cells[9, 2].Value = "NAMA2";
                            //worksheet.Cells[9, 3].Value = "KODE_BRG_MO";
                            //worksheet.Cells[9, 4].Value = "KODE_BRG_INDUK_MO";
                            //worksheet.Cells[9, 5].Value = "KODE_KATEGORI_MO";
                            //worksheet.Cells[9, 6].Value = "KODE_MEREK_MO";
                            //worksheet.Cells[9, 7].Value = "HJUAL";
                            //worksheet.Cells[9, 8].Value = "HJUAL_MARKETPLACE";
                            //worksheet.Cells[9, 9].Value = "BERAT";
                            //worksheet.Cells[9, 10].Value = "IMAGE";
                            //worksheet.Cells[9, 11].Value = "KODE_BRG_MARKETPLACE";

                            string sSQL = "SELECT replace(replace(BRG_MP, char(10), ''), char(13), '') AS BRG_MP, ";
                            sSQL += "replace(replace(NAMA, char(10), ''), char(13), '') NAMA, ";
                            sSQL += "replace(replace(NAMA2, char(10), ''), char(13), '') NAMA2, ";
                            sSQL += "replace(replace(NAMA3, char(10), ''), char(13), '') NAMA3, HJUAL, HJUAL_MP, KODE_BRG_INDUK, BERAT, IMAGE, ";
                            sSQL += "replace(replace(DESKRIPSI, char(10), ''), char(13), '') DESKRIPSI, ";
                            sSQL += "replace(replace(CATEGORY_NAME, char(10), ''), char(13), '') CATEGORY_NAME, ";
                            sSQL += "'' SELLER_SKU,'' AS MEREK, '' AS CATEGORY";
                            sSQL += " FROM TEMP_BRG_MP where cust = '" + customer.CUST + "' order by nama";
                            var dsBarang = EDB.GetDataSet("CString", "STF02", sSQL);

                            for (int i = 0; i < dsBarang.Tables[0].Rows.Count; i++)
                            {
                                worksheet.Cells[10 + i, 1].Value = dsBarang.Tables[0].Rows[i]["NAMA"].ToString();
                                worksheet.Cells[10 + i, 2].Value = dsBarang.Tables[0].Rows[i]["NAMA2"].ToString();
                                worksheet.Cells[10 + i, 3].Value = dsBarang.Tables[0].Rows[i]["SELLER_SKU"].ToString();
                                worksheet.Cells[10 + i, 4].Value = dsBarang.Tables[0].Rows[i]["KODE_BRG_INDUK"].ToString();
                                //worksheet.Cells[10 + i, 5].Value = "KODE_KATEGORI_MO";
                                //worksheet.Cells[10 + i, 6].Value = "KODE_MEREK_MO";
                                worksheet.Cells[10 + i, 7].Value = dsBarang.Tables[0].Rows[i]["HJUAL"].ToString();
                                worksheet.Cells[10 + i, 8].Value = dsBarang.Tables[0].Rows[i]["HJUAL_MP"].ToString();
                                worksheet.Cells[10 + i, 9].Value = dsBarang.Tables[0].Rows[i]["BERAT"].ToString();
                                worksheet.Cells[10 + i, 10].Value = dsBarang.Tables[0].Rows[i]["IMAGE"].ToString();
                                worksheet.Cells[10 + i, 11].Value = dsBarang.Tables[0].Rows[i]["BRG_MP"].ToString();
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

                            using (var range = worksheet.Cells[9, 1, 9, 11])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }
                            #endregion

                            ExcelRange rg0 = worksheet.Cells[9, 1, worksheet.Dimension.End.Row, 11];
                            string tableName0 = "TableBarang";
                            ExcelTable table0 = worksheet.Tables.Add(rg0, tableName0);
                            table0.Columns[0].Name = "NAMA1";
                            table0.Columns[1].Name = "NAMA2";
                            table0.Columns[2].Name = "KODE_BRG_MO";
                            table0.Columns[3].Name = "KODE_BRG_INDUK_MO";
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
                            var validation = worksheet.DataValidations.AddListValidation(worksheet.Cells[10, 5, worksheet.Dimension.End.Row, 5].Address);
                            validation.ShowErrorMessage = true;
                            validation.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                            validation.ErrorTitle = "An invalid value was entered";
                            validation.Formula.ExcelFormula = string.Format("=master_Kategori_dan_Merek!${0}${1}:${2}${3}", "A", 4, "A", kategori.Count);

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
                            var validation2 = worksheet.DataValidations.AddListValidation(worksheet.Cells[10, 6, worksheet.Dimension.End.Row, 6].Address);
                            validation2.ShowErrorMessage = true;
                            validation2.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                            validation2.ErrorTitle = "An invalid value was entered";
                            validation2.Formula.ExcelFormula = string.Format("=master_Kategori_dan_Merek!${0}${1}:${2}${3}", "F", 4, "F", merk.Count);

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

            return Json(ret, JsonRequestBehavior.AllowGet);
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
                                //loop all worksheets
                                var worksheet = excelPackage.Workbook.Worksheets[1];
                                //foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                                //{
                                string cust = worksheet.Cells[1, 2].Value == null ? "" : worksheet.Cells[1, 2].Value.ToString();
                                if (!string.IsNullOrEmpty(cust))
                                {
                                    var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                                    if (customer != null)
                                    {
                                        string namaMP = mp.Where(m => m.IdMarket.ToString() == customer.NAMA).SingleOrDefault().NamaMarket;
                                        ret.cust.Add(cust);
                                        ret.namaCust.Add(namaMP + "(" + customer.PERSO + ")");

                                        var listTemp = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.CUST == cust).ToList();
                                        if (listTemp.Count > 0)
                                        {
                                            //loop all rows
                                            for (int i = 10; i <= worksheet.Dimension.End.Row; i++)
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
                                                        current_brg.NAMA = worksheet.Cells[i, 1].Value == null ? "" : worksheet.Cells[i, 1].Value.ToString();
                                                        current_brg.NAMA2 = worksheet.Cells[i, 2].Value == null ? "" : worksheet.Cells[i, 2].Value.ToString();
                                                        current_brg.SELLER_SKU = worksheet.Cells[i, 3].Value == null ? "" : worksheet.Cells[i, 3].Value.ToString();
                                                        current_brg.KODE_BRG_INDUK = worksheet.Cells[i, 4].Value == null ? "" : worksheet.Cells[i, 4].Value.ToString();
                                                        //change 14 juni 2019, kode kategori mo disimpan di avalue_40, kode kategory mp tetap di category_code
                                                        //current_brg.CATEGORY_CODE = worksheet.Cells[i, 5].Value == null ? "" : worksheet.Cells[i, 5].Value.ToString();
                                                        current_brg.AVALUE_40 = worksheet.Cells[i, 5].Value == null ? "" : worksheet.Cells[i, 5].Value.ToString();
                                                        //end change 14 juni 2019, kode kategori mo disimpan di avalue_40, kode kategory mp tetap di category_code
                                                        current_brg.MEREK = worksheet.Cells[i, 6].Value == null ? "" : worksheet.Cells[i, 6].Value.ToString();
                                                        current_brg.HJUAL_MP = Convert.ToDouble(worksheet.Cells[i, 7].Value == null ? "0" : worksheet.Cells[i, 7].Value.ToString());
                                                        current_brg.BERAT = Convert.ToDouble(worksheet.Cells[i, 9].Value == null ? "0" : worksheet.Cells[i, 9].Value.ToString());
                                                        current_brg.IMAGE = worksheet.Cells[i, 10].Value == null ? "" : worksheet.Cells[i, 10].Value.ToString();
                                                        ErasoftDbContext.SaveChanges();
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

            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public FileResult DownloadFileExcel(/*byte[] file, string fileName*//*BindDownloadExcel data*/ string data)
        {
            return File(/*data.byteExcel*/ new byte[1], /*System.Net.Mime.MediaTypeNames.Application.Octet,*/ /*data.namaFile +*/ ".xlsx");
        }
        
    }


    public class BindUploadExcel
    {
        public List<string> Errors { get; set; }
        public List<int> lastRow { get; set; }
        public bool success { get; set; }
        public List<string> cust { get; set; }
        public List<string> namaCust { get; set; }
    }

    public class BindDownloadExcel
    {
        public List<string> Errors { get; set; }
        public byte[] byteExcel { get; set; }
        public string namaFile { get; set; }
    }

}
