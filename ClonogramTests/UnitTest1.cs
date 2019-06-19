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
        public async Task RedisYandex26379()
        {
            var muxer = await ConnectionMultiplexer.ConnectAsync("rc1b-53udswomgfzm0jm3.mdb.yandexcloud.net:26379,password=Wfhmljns-2");
            var conn = muxer.GetDatabase();

            var guid = NewId.Next().ToGuid().ToString();
            conn.ListLeftPush(guid, 1);
            conn.ListLeftPush(guid, 2);
            conn.ListLeftPush(guid, 3);

            var list = conn.ListRange(guid);
            foreach (var value in list)
            {
                _testOutputHelper.WriteLine(value.ToString());
            }
        }

        [Fact]
        public async Task RedisYandex6379()
        {
            var muxer = await ConnectionMultiplexer.ConnectAsync("rc1b-53udswomgfzm0jm3.mdb.yandexcloud.net:6379,password=Wfhmljns-2");
            var conn = muxer.GetDatabase();

            var guid = NewId.Next().ToGuid().ToString();
            conn.ListLeftPush(guid, 1);
            conn.ListLeftPush(guid, 2);
            conn.ListLeftPush(guid, 3);

            var list = conn.ListRange(guid);
            foreach (var value in list)
            {
                _testOutputHelper.WriteLine(value.ToString());
            }
        }

        [Fact]
        public async Task RedisLocalhost()
        {
            var muxer = await ConnectionMultiplexer.ConnectAsync("127.0.0.1:6379");
            var conn = muxer.GetDatabase();

            var guid = NewId.Next().ToGuid().ToString();
            conn.ListLeftPush(guid, 1);
            conn.ListLeftPush(guid, 2);
            conn.ListLeftPush(guid, 3);

            var list = conn.ListRange(guid);
            foreach (var value in list)
            {
                _testOutputHelper.WriteLine(value.ToString());
            }
        }
    }
}
