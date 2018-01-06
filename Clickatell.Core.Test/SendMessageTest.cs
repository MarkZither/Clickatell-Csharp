using Google.Apis.Urlshortener.v1.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Clickatell.Core.Test
{
    public class MyConfig
    {
        public string ApiToken { get; set; }
    }

    public class SendMessageTest
    {
        public IConfigurationRoot Configuration { get; }
        public MyConfig ClickatellConfiguration { get; set; }
        public SendMessageTest()
        {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("client_secrets.json",
                 optional: true, reloadOnChange: true);
            Configuration = builder.Build();
            //ClickatellConfiguration = Configuration.GetValue<MyConfig>("Clickatell");
            Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.RollingFile("log-{Date}.txt")
    .CreateLogger();
        }

        [Fact]
        public async Task NotReallyATestMoreASimpleProgram()
        {
            List<string> phoneNos = await GetPhoneListWithEPPlus();
            foreach (var item in phoneNos)
            {
                //creating a dictionary to store all the parameters that needs to be sent
                Dictionary<string, string> Params = new Dictionary<string, string>();

                //adding the parameters to the dictionary
                //string message = "Konkurs noknok.pl: okragly tysiac klientow to okragly tysiac dla Ciebie i butelka szampana dla Twojej agencji! Pytanie konkursowe: ktore dzielnice Krakowa sa najbardziej i najmniej popularne wsrod klientow noknok.pl? Zaglosuj i zgarnij swiateczny bonus! https://blog.noknok.pl/posts/konkurs?pk_campaign=SMS-Konkurs&pk_kwd=" + item;
                string message = "Wykorzystaj swoja wiedze o krakowskim rynku nieruchomosci i wygraj 1000 zl w konkursie noknok https://blog.noknok.pl/posts/konkurs?pk_campaign=SMS-Konkurs&pk_kwd=" + item;
                message = await Clickatell.Core.lib.GoogleUrl.ShortenLink(message);
                Params.Add("content", message);
                Params.Add("to", item);

                //string token = Configuration..AppSettings.Settings["ClickatellApiToken"];
                string token = Configuration.GetValue<string>("Clickatell:ApiToken");
                //Log.Logger.Information("token");

                string result = Clickatell.Core.lib.Api.SendSMS(token, Params);
                Log.Logger.Information(message);
                Log.Logger.Information(result);
                Assert.True(true);
                Thread.Sleep(1000);
            }
        }

        [Fact]
        public async Task<string> GetNewShortUrlReturnsGoog()
        {
            string shortUrl = await Clickatell.Core.lib.GoogleUrl.GetShortURL("https://blog.noknok.pl??id="+ Guid.NewGuid().ToString());
            return shortUrl;
        }

        [Fact]
        public async Task<string> GetCachedShortUrlReturnsGoog()
        {
            string shortUrl = await Clickatell.Core.lib.GoogleUrl.GetShortURL("https://blog.noknok.pl??id=0d582371-a076-4672-b72b-8e972db24e4e");
            return shortUrl;
        }
        
        [Fact]
        public async Task GetShortenedURLListReturnsUrlHistory()
        {
            IList<Url> UrlHistory = await Clickatell.Core.lib.GoogleUrl.GetAllShortURLs();
            Assert.True(true);
        }

        [Fact]
        public async Task FindandShortentUrl()
        {
            string text = "Konkurs noknok.pl: okragly tysiac klientow to okragly tysiac dla Ciebie i butelka szampana dla Twojej agencji! https://blog.noknok.pl/posts/konkurs?pk_campaign=SMS-Konkurs&pk_kwd=500871366";
            string shortenedLink = await Clickatell.Core.lib.GoogleUrl.ShortenLink(text);
        }


        [Fact]
        public async Task GetPhoneListWithNOPIReturnsCommaSepString()
        {
            ISheet sheet;
            string fullPath = "C:\\Users\\burto\\Downloads\\BrokerNumberProd150.xlsx";
            //https://stackoverflow.com/questions/33579661/encoding-getencoding-cant-work-in-uwp-app
            //encodings in .net core

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string phoneNo = string.Empty;
            List<string> phoneNos = new List<string>();

            using (var stream = File.OpenRead(fullPath))
            {
                //file.CopyTo(stream);
                stream.Position = 0;
                XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                sheet = hssfwb.GetSheetAt(1); //get second sheet from workbook  

                int rowNum = 1;
                while(sheet.GetRow(rowNum) != null)
                {
                    IRow row = sheet.GetRow(rowNum);
                    ICell cell = row.GetCell(1);
                    if (cell != null)
                    {
                        phoneNos.Add(cell.ToString());
                        rowNum++;
                    }
                }
                string phoneNosCat = string.Join(",", phoneNos);
                phoneNo = sheet.GetRow(1).GetCell(1).ToString();
            }

            Assert.Equal("500871366", phoneNo);
        }

        [Fact]
        public async Task GetPhoneListWithNOPIWithoutRegisteringEncodingsReturnsNotSupportedException()
        {
            string FilePath = "C:\\Users\\burto\\Downloads\\BrokerNumberTest.xlsx";
            //string FilePath = "C:\\Users\\burto\\Downloads\\BrokerNumberProd150.xlsx";
            FileInfo existingFile = new FileInfo(FilePath);

            //https://stackoverflow.com/questions/33579661/encoding-getencoding-cant-work-in-uwp-app
            //encodings in .net core

            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Encoding.GetEncoding("windows-1254");

            Assert.Throws<NotSupportedException>(() =>
            {
                using (var stream = File.OpenRead(FilePath))
                {
                    //file.CopyTo(stream);
                    stream.Position = 0;
                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                }
            }); // the using statement automatically calls Dispose() which closes the package.
        }
        [Fact]
        public async Task GetPhoneListWithEPPlusWithoutRegisteringEncodingsReturnsArgumentException()
        {
            string FilePath = "C:\\Users\\burto\\Downloads\\BrokerNumberTest.xlsx";
            //string FilePath = "C:\\Users\\burto\\Downloads\\BrokerNumberProd150.xlsx";
            //string FilePath = "C:\\Users\\burto\\Downloads\\Broker numbers dec 17.ods";
            FileInfo existingFile = new FileInfo(FilePath);

            //https://stackoverflow.com/questions/33579661/encoding-getencoding-cant-work-in-uwp-app
            //encodings in .net core

            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Encoding.GetEncoding("windows-1254");
            string phoneNo = string.Empty;
            List<string> phoneNos = new List<string>();

            Assert.Throws<ArgumentException>(() =>
            {
                using (ExcelPackage package = new ExcelPackage(existingFile))
                {
                    // get the first worksheet in the workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[2];

                }
            }); // the using statement automatically calls Dispose() which closes the package.
        }

        public async Task<List<string>> GetPhoneListWithEPPlus()
        {
            //string FilePath = "C:\\Users\\burto\\Downloads\\BrokerNumberTest.xlsx";
            string FilePath = "C:\\Users\\burto\\Downloads\\BrokerNumberProd150.xlsx";
            //string FilePath = "C:\\Users\\burto\\Downloads\\Broker numbers dec 17.ods";
            FileInfo existingFile = new FileInfo(FilePath);

            //https://stackoverflow.com/questions/33579661/encoding-getencoding-cant-work-in-uwp-app
            //encodings in .net core

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Encoding.GetEncoding("windows-1254");
            string phoneNo = string.Empty;
            List<string> phoneNos = new List<string>();

            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                // get the first worksheet in the workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets[2];
                int rowNum = 2;
                int col = 2; //The item description
                
                while (worksheet.Cells[rowNum, col].Value != null)
                {
                    phoneNos.Add(worksheet.Cells[rowNum, col].Value.ToString());
                    rowNum++;
                }
                // output the data in column 2
                //Console.WriteLine("\tCell({0},{1}).Value={2}", row, col, worksheet.Cells[row, col].Value);
                phoneNo = worksheet.Cells[2, 2].Value.ToString();
                string phoneNosCat = string.Join(",", phoneNos);
                // output the formula in row 5
                //Console.WriteLine("\tCell({0},{1}).Formula={2}", 3, 5, worksheet.Cells[3, 5].Formula);
                //Console.WriteLine("\tCell({0},{1}).FormulaR1C1={2}", 3, 5, worksheet.Cells[3, 5].FormulaR1C1);

                // output the formula in row 5
                //Console.WriteLine("\tCell({0},{1}).Formula={2}", 5, 3, worksheet.Cells[5, 3].Formula);
                //Console.WriteLine("\tCell({0},{1}).FormulaR1C1={2}", 5, 3, worksheet.Cells[5, 3].FormulaR1C1);
            } // the using statement automatically calls Dispose() which closes the package.

            return phoneNos;
        }
    }    
}
