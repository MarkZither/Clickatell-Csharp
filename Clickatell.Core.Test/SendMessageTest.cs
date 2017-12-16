using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Clickatell.Core.Test
{
    public class SendMessageTest
    {
        [Fact]
        public void Test1()
        {
            //creating a dictionary to store all the parameters that needs to be sent
            Dictionary<string, string> Params = new Dictionary<string, string>();

            //adding the parameters to the dictionary
            Params.Add("content", "test message test");
            Params.Add("to", "+48500871366");
            string result = Clickatell.Core.lib.Api.SendSMS("PKNcLhyiQj-kI6hVjGfkSw==", Params);
            Assert.True(true);
        }

        [Fact]
        public async Task<string> GetShortUrlReturnsGoog()
        {
            string shortUrl = await Clickatell.Core.lib.Api.CreateShortURL("https://blog.noknok.pl");
            return shortUrl;
        }
        
        [Fact]
        public async Task GetShortenedURLListReturnsUrlHistory()
        {
            string UrlHistory = await Clickatell.Core.lib.Api.GetAllShortURLs();
            Assert.
        }
    }
}
