using System;
using System.IO;
using System.Web;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MasterOnline.Utils;
using System.Collections.Generic;



namespace MasterOnline.Services
{
    public class UploadFileServices
    {
        private static readonly string _awsAccessKey = AwsConfig._awsAccessKey;
        private static readonly string _awsSecretKey = AwsConfig._awsSecretKey;
        private static readonly string _bucketName = AwsConfig._bucketName;
        private static readonly string _amazonS3PublicUrl = AwsConfig._amazonS3PublicUrl;
        private static readonly string _amazonAwsUrl = AwsConfig._amazonAwsUrl;
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSoutheast1;
        private const string keyName = "upload_1";
        private static IAmazonS3 s3Client;

        public class BindUploadExcelFile
        {
            public double ContentLength { get; set; }
            public List<ResponseStreamResult> ResponseStream { get; set; }
        }

        public class ResponseStreamResult
        {
            public bool CanRead { get; set; }
            public bool CanSeek { get; set; }
            public bool CanTimeout { get; set; }
            public bool CanWrite { get; set; }
            public long Length { get; set; }
            public long Position { get; set; }
            public int ReadTimeout { get; set; }
            public int WriteTimeout { get; set; }
        }

        public static BindUploadExcelFile UploadFile(HttpPostedFileBase file)
        {
            var fileName = Guid.NewGuid().ToString();
            //object ret = null;
            BindUploadExcelFile responseData = new BindUploadExcelFile();

            try
            {
                Stream inputSteram = file.InputStream;

                IAmazonS3 client;
                using (client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1))
                {
                    PutObjectRequest putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = string.Format(file.FileName),
                        CannedACL = S3CannedACL.PublicRead,//PERMISSION TO FILE PUBLIC ACCESIBLE
                        ContentType = file.ContentType,
                        InputStream = inputSteram
                    };

                    //ret = _amazonS3PublicUrl + "uploaded-file/" + string.Format(file.FileName);
                    client.PutObject(putRequest);
                    //ret = client.GetObject("masteronlinebucket", "uploaded-file/" + string.Format(file.FileName));

                    using (GetObjectResponse response = client.GetObject(_bucketName, "uploaded-file/" + string.Format(file.FileName)))
                    {
                        using (StreamReader reader = new StreamReader(response.ResponseStream))
                        {
                            string contents = reader.ReadToEnd();
                            responseData.ContentLength = response.ContentLength;
                            responseData.ResponseStream = new List<ResponseStreamResult>();

                            ResponseStreamResult responseStream = new ResponseStreamResult()
                            {
                                CanRead = response.ResponseStream.CanRead,
                                CanSeek = response.ResponseStream.CanSeek,
                                CanWrite = response.ResponseStream.CanWrite,
                                CanTimeout = response.ResponseStream.CanTimeout,
                                Length = response.ResponseStream.Length,
                                Position = response.ResponseStream.Position,
                                ReadTimeout = response.ResponseStream.ReadTimeout,
                                WriteTimeout = response.ResponseStream.WriteTimeout
                            };

                            responseData.ResponseStream.Add(responseStream);

                            Console.WriteLine("Object - " + response.Key);
                            Console.WriteLine(" Version Id - " + response.VersionId);
                            Console.WriteLine(" Contents - " + contents);
                        }
                    }

                }
                            
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    //throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
            return responseData;
        }
    }
}