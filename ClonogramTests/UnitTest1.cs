using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using MassTransit;
using Npgsql;
using StackExchange.Redis;
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
        public async Task Redis()
        {
      /*      var muxer = ConnectionMultiplexer.Connect("rc1b-53udswomgfzm0jm3.mdb.yandexcloud.net:6379,password=Wfhmljns-2");
            var conn = muxer.GetDatabase();
*/
            var options = new ConfigurationOptions
            {
                EndPoints = { "rc1b-53udswomgfzm0jm3.mdb.yandexcloud.net:26379" },
                Password = "Wfhmljns-2"
            };

            var muxer = ConnectionMultiplexer.Connect(options);
            var conn = muxer.GetDatabase();
        }
    }
}
