using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using Amazon.S3;
using Amazon.S3.Model;
using MasterOnline.Models;
using MasterOnline.Utils;
using Newtonsoft.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace MasterOnline.Services
{
    public class UploadImageService
    {
        private static readonly string _awsAccessKey = AwsConfig._awsAccessKey;
        private static readonly string _awsSecretKey = AwsConfig._awsSecretKey;
        private static readonly string _bucketName = AwsConfig._bucketName;
        private static readonly string _amazonS3PublicUrl = AwsConfig._amazonS3PublicUrl;
        private static readonly string _amazonAwsUrl = AwsConfig._amazonAwsUrl;

        public static ImgurImageResponse UploadMultipleImagesToImgur(IEnumerable<HttpPostedFileBase> files, string albumid)
        {
            var fileName = Guid.NewGuid().ToString();
            var path = albumid + "/" + fileName;
            var imgurImage = new ImgurImageResponse();
            foreach (HttpPostedFileBase file in files)
            {

                try
                {
                    path = path + "." + file.FileName.Split('.').Last();
                    IAmazonS3 client;
                    Stream inputSteram = ResizeImageFile(file.InputStream, 1024);
                    using (client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1))
                    {
                        var request = new PutObjectRequest()
                        {
                            BucketName = _bucketName,
                            CannedACL = S3CannedACL.PublicRead,//PERMISSION TO FILE PUBLIC ACCESIBLE
                            Key = string.Format(path),
                            InputStream = inputSteram//SEND THE FILE STREAM
                        };

                        client.PutObject(request);
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }

                imgurImage.data = new ImgurData();

                imgurImage.data.link = _amazonAwsUrl + "/" + _bucketName + "/" + path;
                imgurImage.data.link_s = _amazonAwsUrl + "/" + _bucketName + "/" + path;
                imgurImage.data.link_m = _amazonAwsUrl + "/" + _bucketName + "/" + path;
                imgurImage.data.link_l = _amazonAwsUrl + "/" + _bucketName + "/" + path;
                imgurImage.data.copyText = "";

            }
            return imgurImage;
        }

        public static ImgurImageResponse UploadSingleImageToImgur(HttpPostedFileBase file, string albumid)
        {
            var fileName = Guid.NewGuid().ToString();
            var path = albumid + "/" + fileName;
            var imgurImage = new ImgurImageResponse();

            try
            {
                path = path + "." + file.FileName.Split('.').Last();
                IAmazonS3 client;
                Stream inputSteram = null;

                if (file.FileName.Split('.')[1] == "gif")
                {
                    inputSteram = file.InputStream;
                }
                else
                {
                    inputSteram = ResizeImageFile(file.InputStream, 1024);
                }

                //add by nurul 13/2/2020, penambahan type file 
                if (file.FileName.Split('.').Last() != "jpeg" && file.FileName.Split('.').Last() != "png" && file.FileName.Split('.').Last() != "jpg" && file.FileName.Split('.').Last() != "gif")
                {
                    path = path + ".jpg";
                }
                //end add by nurul 13/2/2020, penambahan type file 

                using (client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1))
                {
                    var request = new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        CannedACL = S3CannedACL.PublicRead,//PERMISSION TO FILE PUBLIC ACCESIBLE
                        Key = string.Format(path),
                        InputStream = inputSteram,//SEND THE FILE STREAM,                            
                    };


                    if (file.FileName.Split('.')[1] == "gif")
                    {
                        request.ContentType = "image/gif";
                    }

                    client.PutObject(request);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            imgurImage.data = new ImgurData();

            imgurImage.data.link = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_s = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_m = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_l = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.copyText = "";


            return imgurImage;
        }

        //add by nurul 9/2/2021, upload image logo perusahaan tanpa resize 
        public static ImgurImageResponse UploadSingleImageToImgurNotResize(HttpPostedFileBase file, string albumid, string fileNameReq)
        {
            var fileName = Guid.NewGuid().ToString();
            //var path = albumid + "/" + fileName;
            var path = albumid + "/" + fileNameReq;
            var imgurImage = new ImgurImageResponse();

            try
            {
                path = path + "." + file.FileName.Split('.').Last();
                IAmazonS3 client;
                Stream inputSteram = null;

                if (file.FileName.Split('.')[1] == "gif")
                {
                    inputSteram = file.InputStream;
                }
                else
                {
                    //inputSteram = ResizeImageFile(file.InputStream, 1024);
                    inputSteram = file.InputStream;
                }

                //add by nurul 13/2/2020, penambahan type file 
                if (file.FileName.Split('.').Last() != "jpeg" && file.FileName.Split('.').Last() != "png" && file.FileName.Split('.').Last() != "jpg" && file.FileName.Split('.').Last() != "gif")
                {
                    path = path + ".jpg";
                }
                //end add by nurul 13/2/2020, penambahan type file 

                using (client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1))
                {
                    var request = new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        CannedACL = S3CannedACL.PublicRead,//PERMISSION TO FILE PUBLIC ACCESIBLE
                        Key = string.Format(path),
                        InputStream = inputSteram,//SEND THE FILE STREAM,                            
                    };
                    
                    if (file.FileName.Split('.')[1] == "gif")
                    {
                        request.ContentType = "image/gif";
                    }

                    client.PutObject(request);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            imgurImage.data = new ImgurData();

            imgurImage.data.link = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_s = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_m = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_l = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.copyText = "";


            return imgurImage;
        }
        //end add by nurul 9/2/2021

        public static ImgurImageResponse UploadSingleImageToImgurFromUrl(string url, string albumid)
        {
            var fileName = Guid.NewGuid().ToString();
            var path = albumid + "/" + fileName;
            var imgurImage = new ImgurImageResponse();

            try
            {
                path = path + "." + url.Split('.').Last();
                if (path.Contains("?v="))
                {
                    path = albumid + "/" + fileName;
                }
                IAmazonS3 client;
                Stream inputSteram = null;

                //create stream from url
                var req = System.Net.WebRequest.Create(url);
                var imageStream = req.GetResponse().GetResponseStream();
                //end create stream from url

                if (url.Split('.')[1] == "gif")
                {
                    inputSteram = imageStream;
                }
                else
                {
                    inputSteram = ResizeImageFile(imageStream, 1024);
                }

                //add by nurul 13/2/2020, penambahan type file 
                if(url.Split('.').Last() != "jpeg" && url.Split('.').Last() != "png" && url.Split('.').Last() != "jpg" && url.Split('.').Last() != "gif")
                {                    
                    path = path + ".jpg";
                }
                //end add by nurul 13/2/2020, penambahan type file 

                using (client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1))
                {
                    var request = new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        CannedACL = S3CannedACL.PublicRead,//PERMISSION TO FILE PUBLIC ACCESIBLE
                        Key = string.Format(path),
                        InputStream = inputSteram,//SEND THE FILE STREAM,                            
                    };


                    if (url.Split('.')[1] == "gif")
                    {
                        request.ContentType = "image/gif";
                    }

                    client.PutObject(request);
                }
            }
            catch (Exception ex)
            {
                imgurImage.data = new ImgurData();
                imgurImage.data.link_l = "";
                return imgurImage;

                //throw ex;
            }

            imgurImage.data = new ImgurData();

            imgurImage.data.link = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_s = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_m = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_l = _amazonAwsUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.copyText = "";


            return imgurImage;
        }

        public static byte[] StreamToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static Stream GifImageFileWithoutCompression(Stream imageFileStream) // Set targetSize to 1024
        {
            byte[] imageFile = StreamToByteArray(imageFileStream);
            using (System.Drawing.Image oldImage = System.Drawing.Image.FromStream(new MemoryStream(imageFile)))
            {
                using (Bitmap newImage = new Bitmap(oldImage.Width, oldImage.Height, PixelFormat.Format24bppRgb))
                {
                    MemoryStream m = new MemoryStream();
                    newImage.Save(m, ImageFormat.Gif);
                    return new MemoryStream(m.GetBuffer());

                }
            }
        }

        public static Stream ResizeImageFile(Stream imageFileStream, int targetSize) // Set targetSize to 1024
        {
            byte[] imageFile = StreamToByteArray(imageFileStream);
            using (System.Drawing.Image oldImage = System.Drawing.Image.FromStream(new MemoryStream(imageFile)))
            {
                Size newSize = CalculateDimensions(oldImage.Size, targetSize);
                using (Bitmap newImage = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format24bppRgb))
                {
                    using (Graphics canvas = Graphics.FromImage(newImage))
                    {
                        canvas.SmoothingMode = SmoothingMode.AntiAlias;
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        canvas.DrawImage(oldImage, new Rectangle(new Point(0, 0), newSize));
                        MemoryStream m = new MemoryStream();
                        newImage.Save(m, ImageFormat.Jpeg);
                        return new MemoryStream(m.GetBuffer());
                    }
                }
            }
        }

        public static Size CalculateDimensions(Size oldSize, int targetSize)
        {
            Size newSize = new Size();
            if (oldSize.Height > oldSize.Width)
            {
                newSize.Width = (int)(oldSize.Width * ((float)targetSize / (float)oldSize.Height));
                newSize.Height = targetSize;
            }
            else
            {
                newSize.Width = targetSize;
                newSize.Height = (int)(oldSize.Height * ((float)targetSize / (float)oldSize.Width));
            }
            return newSize;
        }

        public static ImgurImageResponse UploadSingleImageToS3FromPath(String imagePath, string albumid, String fileName)
        {


            //var fileName = values[1].Replace(" ", "_") + "_image.png";
            var path = albumid + "/" + fileName;
            var imgurImage = new ImgurImageResponse();

            try
            {
                //path = path + "." + imageType;

                byte[] photo = File.ReadAllBytes(imagePath);
                IAmazonS3 client;
                using (client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1))
                {
                    var request = new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        CannedACL = S3CannedACL.PublicRead,//PERMISSION TO FILE PUBLIC ACCESIBLE
                        Key = string.Format(path),
                        InputStream = new MemoryStream(photo)//SEND THE FILE STREAM
                    };

                    client.PutObject(request);
                    if (File.Exists(@imagePath))
                    {
                        File.Delete(@imagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            imgurImage.data = new ImgurData();

            imgurImage.data.link = _amazonS3PublicUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_s = _amazonS3PublicUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_m = _amazonS3PublicUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.link_l = _amazonS3PublicUrl + "/" + _bucketName + "/" + path;
            imgurImage.data.copyText = "";


            return imgurImage;
        }
        public static String SaveImageOnServer(String imageUrl, String saveLocation)
        {
            byte[] imageBytes;
            HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(imageUrl);
            WebResponse imageResponse = imageRequest.GetResponse();

            Stream responseStream = imageResponse.GetResponseStream();

            using (BinaryReader br = new BinaryReader(responseStream))
            {
                imageBytes = br.ReadBytes(500000);
                br.Close();
            }
            responseStream.Close();
            imageResponse.Close();

            FileStream fs = new FileStream(saveLocation, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            try
            {
                bw.Write(imageBytes);
            }
            finally
            {
                fs.Close();
                bw.Close();
            }

            return saveLocation;
        }

        //add by nurul 14/7/2021
        public static void DeleteImageS3FromPath(String fileName)
        {

            //var fileName = values[1].Replace(" ", "_") + "_image.png";
            //var path = albumid + "/" + fileName;
            //var imgurImage = new ImgurImageResponse();
            var name = fileName.Split(new string[] { "masteronlinebucket/" }, StringSplitOptions.None);
            var path = "";
            if(name.Count() > 1)
            {
                path = name.Last();
            }
            if (path != "")
            {
                try
                {
                    //path = path + "." + imageType;

                    //byte[] photo = File.ReadAllBytes(imagePath);
                    IAmazonS3 client;
                    using (client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1))
                    {
                        var request = new DeleteObjectRequest()
                        {
                            BucketName = _bucketName,
                            //CannedACL = S3CannedACL.PublicRead,//PERMISSION TO FILE PUBLIC ACCESIBLE
                            Key = string.Format(path),
                            //InputStream = new MemoryStream(photo)//SEND THE FILE STREAM
                        };

                        client.DeleteObject(request);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            //imgurImage.data = new ImgurData();

            //imgurImage.data.link = _amazonS3PublicUrl + "/" + _bucketName + "/" + path;
            //imgurImage.data.link_s = _amazonS3PublicUrl + "/" + _bucketName + "/" + path;
            //imgurImage.data.link_m = _amazonS3PublicUrl + "/" + _bucketName + "/" + path;
            //imgurImage.data.link_l = _amazonS3PublicUrl + "/" + _bucketName + "/" + path;
            //imgurImage.data.copyText = "";


            //return imgurImage;
        }
        //end add by nurul 14/7/2021

        public static void InsertToDB(string jsonText, string tableName)
        {
            //DateTime date0 = DateTime.UtcNow;
            //DateTime datenow = date0.AddHours(7);
            //var datetimeDel = date0.AddDays(+2);
            //insert ke dynamodb 
            try
            {
                var client = new AmazonDynamoDBClient(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1);
                var table = Table.LoadTable(client, tableName);
                //long date = (long)datenow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                //long ttlDel = (long)datetimeDel.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                //var id = date.ToString();
                //var data = new tesInsert()
                //{
                //    Chat_Id = date,
                //    Cust = "Tokped",
                //    Date = datenow.ToString("yyyy-MM-dd HH:mm:ss"),
                //    //Result = "{\"msg_id\":419536600,\"message\":\"tes webhook\",\"thumbnail\":\"https://accounts.tokopedia.com/image/v1/u/15305276/user_thumbnail/desktop\",\"full_name\":\"Dhe\",\"shop_id\":2739385,\"user_id\":15305276,\"payload\":{\"attachment_type\":0,\"image\":{\"image_thumbnail\":\"\",\"image_url\":\"\"},\"product\":{\"image_url\":\"\",\"name\":\"\",\"price\":\"\",\"product_id\":0,\"product_url\":\"\"}}}"
                //    Result = json,
                //    ttl = ttlDel.ToString()
                //};
                //var jsonText = JsonConvert.SerializeObject(data);
                var item = Amazon.DynamoDBv2.DocumentModel.Document.FromJson(jsonText);
                table.PutItem(item);
            }
            catch (Exception ex) 
            { 
            }
            //end insert ke dynamodb
        }
        public static List<Dictionary<string, AttributeValue>> selectToDB( string tableName, string sWhere, string sWhereVal)
        {
            var ret = new List<Dictionary<string, AttributeValue>>();
            try
            {
                var client = new AmazonDynamoDBClient(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1);
                var request = new QueryRequest
                {
                    TableName = tableName,
                    KeyConditionExpression = sWhere + " = :v_Id",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
        {":v_Id", new AttributeValue { S =  sWhereVal }}}
                };

                var response = client.Query(request);
                ret = response.Items;
            }
            catch(Exception ex)
            {

            }
            return ret;
        }
    }
}