using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Urlshortener.v1.Data;
using Google.Apis.Util.Store;
using MonkeyCache.SQLite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MonkeyCache;
using Clickatell.Core.lib.Models;

namespace Clickatell.Core.lib
{
    public class GoogleUrl
    {
        public static async Task<string> CreateShortURL(string urlToShorten)
        {
            UserCredential credential = await Authenticate();

            UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "UrlShortener.ShortenURL sample",
                //ApiKey = ""
            });

            // Shorten URL
            Url response = await service.Url.Insert(new Url { LongUrl = urlToShorten }).ExecuteAsync();
            // Display response
            return response.Id;
        }

        public static async Task<IList<Url>> GetAllShortURLs()
        {
            var json = string.Empty;
            Barrel.ApplicationId = "GoogleUrl";
            IBarrel barrel = Barrel.Current;
            UserCredential credential = await Authenticate();

            UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "UrlShortener.ShortenURL sample",
                //ApiKey = ""
            });

            if (!Barrel.Current.IsExpired(service.BaseUri))
            {
                return JsonConvert.DeserializeObject<IList<Url>>(Barrel.Current.Get(service.BaseUri));
            }
            else
            {
                List<Url> fullList = new List<Url>();

                // Get All Short URLs
                
                UrlHistory response = service.Url.List().Execute();
                fullList.AddRange(response.Items);
                while(response.NextPageToken != null)
                {
                    var request = service.Url.List();
                    request.StartToken = response.NextPageToken;
                    //UrlHistory result = request.Execute();
                    response = request.Execute(); // service.Url.List().Execute();
                    fullList.AddRange(response.Items);
                }
                Barrel.Current.Add(service.BaseUri, fullList, TimeSpan.FromDays(1));
                // Display response
                return fullList;
            }
        }

        private static async Task<UserCredential> Authenticate()
        {
            UserCredential credential;
            //from https://developers.google.com/api-client-library/dotnet/guide/aaa_oauth
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { UrlshortenerService.Scope.Urlshortener },
                    "user", CancellationToken.None, new FileDataStore("Urlshortener.MyUrls"));
            }
            /*
                        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
                        {
                            ClientId = "837647042410-75ifg...usercontent.com",
                            ClientSecret = "asdlkfjaskd"
                        },
                        new[] { UrlshortenerService.Scope.Urlshortener },
                        "user",
                        CancellationToken.None,
                        new FileDataStore("Urlshortener.MyUrls"));
            */
            return credential;
        }

        private static readonly Regex regex = new Regex("((http://|https://|www\\.)([A-Z0-9.-:]{1,})\\.[0-9A-Z?;~&#=\\-_\\./]{2,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly string link = "{0}{1}";
        public static async Task<string> ShortenLink(string str)
        {
            // based on https://madskristensen.net/blog/resolve-and-shorten-urls-in-csharp
            foreach (Match match in regex.Matches(str))
            {
                string shortUrl = await GetShortURL(match.Value);
                str = str.Replace(match.Value, string.Format(link, string.Empty, shortUrl));
            }
            return str;
        }

        public static async Task<string> GetShortURL(string value)
        {
            var urls = await GetAllShortURLs();
            if (urls.Any(x => x.LongUrl.ToLower().Replace(".pl/??",".pl??").Equals(value.ToLower())))
            {
                return urls.First(x => x.LongUrl.ToLower().Replace(".pl/??", ".pl??").Equals(value.ToLower())).Id;
            }
            else
            {
                return await CreateShortURL(value);
            }
        }
    }
}
