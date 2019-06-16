using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;

namespace Clonogram.Repositories
{
    public class AmazonS3Repository : IAmazonS3Repository
    {
        private readonly AWSCredentials _credentials;

        public AmazonS3Repository()
        {
            _credentials = new BasicAWSCredentials(Constants.AccessKey, Constants.SecretKey);
        }

        public async Task Upload(IFormFile file, string name)
        {
            using var client = new AmazonS3Client(_credentials, new AmazonS3Config { ServiceURL = Constants.ServiceURL });
            using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = name,
                BucketName = Constants.BucketName,
                CannedACL = S3CannedACL.PublicRead
            };

            var fileTransferUtility = new TransferUtility(client);
            await fileTransferUtility.UploadAsync(uploadRequest);
        }

        public async Task Delete(string name)
        {
            using var client = new AmazonS3Client(_credentials, new AmazonS3Config { ServiceURL = Constants.ServiceURL });
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = Constants.BucketName,
                Key = name
            };

            await client.DeleteObjectAsync(deleteObjectRequest);
        }
    }
}
