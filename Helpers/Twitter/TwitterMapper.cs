using Discord;
using Microsoft.Extensions.Configuration;

namespace VGI.Helpers.Twitter;

public class TwitterMapper(IConfiguration configuration)
{
    private readonly string _accountName = configuration.GetValue<string>("TWITTER:ACCOUNT_NAME");
    private readonly string _accountImage = configuration.GetValue<string>("TWITTER:ACCOUNT_IMAGE");

    public EmbedBuilder CouldNotTweet(string reason)
    {
        var tweetError = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = _accountName,
                IconUrl = _accountImage
            },
            Description = reason,
            Color = Color.Red
        };
        return tweetError;
    }

    public EmbedBuilder TweetCreated(string twitterResponse)
    {
        var tweetEmbed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = _accountName,
                IconUrl = _accountImage
            },
            Description = $"https://twitter.com/{_accountName}/status/{twitterResponse}",
            Color = new(0x1DA1F2)
        };
        return tweetEmbed;
    }
}