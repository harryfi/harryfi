using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MasterOnline.Utils;

namespace MasterOnline.Services
{
    public class ManageImageService
    {
        private static readonly string _awsAccessKey = AwsConfig._awsAccessKey;
        private static readonly string _awsSecretKey = AwsConfig._awsSecretKey;
        private static readonly string _bucketName = AwsConfig._bucketName;

        public static void DeleteObjectNonVersionedBucketAsync(string namaFile)
        {
            try
            {
                using (IAmazonS3 client =
                    new AmazonS3Client(_awsAccessKey, _awsSecretKey, Amazon.RegionEndpoint.APSoutheast1))
                {
                    var deleteObjectRequest = new DeleteObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = namaFile,
                    };

                    client.DeleteObject(deleteObjectRequest);
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }
    }
}