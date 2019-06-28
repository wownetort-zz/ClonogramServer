using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Clonogram.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Clonogram.Repositories
{
    public class AmazonS3Repository : IAmazonS3Repository
    {
        private readonly AWSCredentials _credentials;
        private readonly S3Settings _s3Settings;

        public AmazonS3Repository(IOptions<S3Settings> s3Settings)
        {
            _s3Settings = s3Settings.Value;
            _credentials = new BasicAWSCredentials(_s3Settings.AccessKey, _s3Settings.SecretKey);
        }

        public async Task Upload(IFormFile file, string name)
        {
            using var client = new AmazonS3Client(_credentials, new AmazonS3Config { ServiceURL = _s3Settings.ServiceURL });
            using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = name,
                BucketName = _s3Settings.BucketName,
                CannedACL = S3CannedACL.PublicRead
            };

            var fileTransferUtility = new TransferUtility(client);
            await fileTransferUtility.UploadAsync(uploadRequest);
        }

        public async Task Delete(string name)
        {
            using var client = new AmazonS3Client(_credentials, new AmazonS3Config { ServiceURL = _s3Settings.ServiceURL });
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = name
            };

            await client.DeleteObjectAsync(deleteObjectRequest);
        }
    }
}
