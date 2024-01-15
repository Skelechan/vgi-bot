using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OAuth;

namespace VGI.Helpers.Twitter;

public class TwitterClient(IConfiguration configuration)
{
    private const string _twitterPostEndpoint = "https://api.twitter.com/2/tweets";
    private readonly string _consumerKey = configuration.GetValue<string>("TWITTER:APP_KEY");
    private readonly string _consumerSecret = configuration.GetValue<string>("TWITTER:APP_SECRET");
    private readonly string _accessToken = configuration.GetValue<string>("TWITTER:TOKEN_KEY");
    private readonly string _accessTokenSecret = configuration.GetValue<string>("TWITTER:TOKEN_SECRET");

    public async Task<string> PostQuote(TweetRequest request)
    {
        var oauthClient = OAuthRequest.ForProtectedResource("POST", _consumerKey, _consumerSecret, _accessToken, _accessTokenSecret);
        oauthClient.RequestUrl = _twitterPostEndpoint;
        var authHeader = oauthClient.GetAuthorizationHeader();

        using (var httpClient = new HttpClient())
        {
            var messageRequest = new HttpRequestMessage();
            messageRequest.Method = HttpMethod.Post;
            messageRequest.RequestUri = new Uri(_twitterPostEndpoint);
            messageRequest.Content = JsonContent.Create(request);
            messageRequest.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader);

            var messageResponse = await httpClient.SendAsync(messageRequest);
            if (!messageResponse.IsSuccessStatusCode) 
                return null;
            
            var response = await messageResponse.Content.ReadFromJsonAsync<TweetResponse>();
            return response?.Data?.Id;
        }
    }
}