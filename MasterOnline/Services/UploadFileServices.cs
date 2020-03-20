using System;
using Amazon.S3;
using Amazon.S3.Model;
using MasterOnline.Utils;

namespace MasterOnline.Services
{
    public class UploadFileServices
    {
        private static readonly string _awsAccessKey = AwsConfig._awsAccessKey;
        private static readonly string _awsSecretKey = AwsConfig._awsSecretKey;
        private static readonly string _bucketName = AwsConfig._bucketFileName;
        private static readonly string _amazonS3PublicUrl = AwsConfig._amazonS3PrivateUrl;
        private static readonly string _amazonAwsUrl = AwsConfig._amazonAwsUrl;

        private static AmazonS3Client client;

        public UploadFileServices()
        {
            client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1);
        }

        public static void UploadFile(string filePath)
        {
            var result = "";
            try
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = _awsAccessKey,
                    FilePath = filePath,
                    ContentType = "plain/text"
                };

                PutObjectResponse response = client.PutObject(putRequest);
                result = response.ResponseMetadata.Metadata.Values.ToString();
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
            //return result;
        }
    }
}