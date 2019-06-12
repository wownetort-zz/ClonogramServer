using System;
using System.Collections.Generic;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using MassTransit;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace ClonogramTests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private const string bucketName = "clonogram-photos";
        private const string keyName = "test";
        private const string filePath = @"C:\Users\wownetort\Pictures\desctop\CR40WeJ.jpg";
        // Specify your bucket region (an example region is shown).
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUWest1;
        private static IAmazonS3 s3Client;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async void Test1()
        {
            try
            {
                AWSCredentials credentials = new BasicAWSCredentials("c7cS25L80Inr1OueEeUn", "zAQAnjAq6dAMZ8xKobsqe9O5Hriskcpp0LXxDp2O");
                s3Client = new AmazonS3Client(credentials, new AmazonS3Config(){ServiceURL = "https://storage.yandexcloud.net" });

                var fileTransferUtility =
                    new TransferUtility(s3Client);

                // Option 1. Upload a file. The file name is used as the object key name.
                await fileTransferUtility.UploadAsync(filePath, bucketName);
                Console.WriteLine("Upload 1 completed");


                // Option 2. Specify object key name explicitly.
                await fileTransferUtility.UploadAsync(filePath, bucketName, keyName);
                Console.WriteLine("Upload 2 completed");

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
