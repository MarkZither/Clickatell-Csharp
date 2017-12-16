using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Google.Apis.Urlshortener.v1.Data;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Services;
using System.Threading.Tasks;

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
            
            UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer
            {
                ApplicationName = "UrlShortener.ShortenURL sample",
                ApiKey = ""
            });
            
            // Shorten URL
            Url response = await service.Url.Insert(new Url { LongUrl = urlToShorten }).ExecuteAsync();
            // Display response
            return response.Id;
        }
        /*
        public static async Task<string> GetAllShortURLs()
        {
            UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer
            {
                ApplicationName = "UrlShortener.ShortenURL sample",
                ApiKey = ""
            });

            // Shorten URL
            UrlHistory response = service.Url.List().Execute();
            // Display response
            return ;
        }
        */
    }
}
