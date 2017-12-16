using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Google.Apis.Urlshortener.v1.Data;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Services;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Util.Store;
using System.IO;

namespace Clickatell.Core.lib
{
    public class Api
    {
        //This function is in charge of converting the data into a json array and sending it to the rest sending controller.
        public static string SendSMS(string Token, Dictionary<string, string> Params)
        {
            Params["to"] = CreateRecipientList(Params["to"]);
            string JsonArray = JsonConvert.SerializeObject(Params, Formatting.None);
            JsonArray = JsonArray.Replace("\\\"", "\"").Replace("\"[", "[").Replace("]\"", "]");
            return Rest.Post(Token, JsonArray);
        }

        //This function converts the recipients list into an array string so it can be parsed correctly by the json array.
        public static string CreateRecipientList(string to)
        {
            string[] tmp = to.Split(',');
            to = "[\"";
            to = to + string.Join("\",\"", tmp);
            to = to + "\"]";
            return to;
        }

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

        public static async Task<string> GetAllShortURLs()
        {
            UserCredential credential = await Authenticate();

            UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "UrlShortener.ShortenURL sample",
                //ApiKey = ""
            });

            // Shorten URL
            UrlHistory response = service.Url.List().Execute();
            // Display response
            return JsonConvert.SerializeObject(response);
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
    }
}
