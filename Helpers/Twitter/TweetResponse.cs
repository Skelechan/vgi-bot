using System.Collections.Generic;
using Newtonsoft.Json;

namespace VGI.Helpers.Twitter;

public class TweetResponse
{
    [JsonProperty("data")]
    public TweetData Data { get; set; }
    
    public class TweetData
    {
        [JsonProperty("edit_history_tweet_ids")]
        public List<string> EditHistoryTweetIds { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}

