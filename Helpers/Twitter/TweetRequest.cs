using System.Text.Json.Serialization;

namespace VGI.Helpers.Twitter;

public class TweetRequest
{
    [JsonPropertyName("text")] public string Text { get; set; }
    [JsonPropertyName("quote_tweet_id")] public string QuoteId { get; set; }
}