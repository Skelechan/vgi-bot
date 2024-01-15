using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using VGI.Helpers.Twitter;

namespace VGI.Modules.Osu;

[Group("twitter", "Commands related to the VGI Twitter Account")]
public class TwitterModule(IConfiguration config, TwitterClient twitterClient, TwitterMapper twitterMapper) : InteractionModuleBase<SocketInteractionContext>
{
    private IConfiguration _config = config;
    private TwitterClient _twitterClient = twitterClient;
    private TwitterMapper _twitterMapper = twitterMapper;
    private string[] _twitterWhitlist = config.GetValue<string>("TWITTER:WHITELIST").Split(',');

    [SlashCommand("retweet", "Post on main")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task Retweet([Summary(name: "URL", description: "The URL you want to retweet")] string url, [Summary(name: "Message", description: "The message to QRT with")] string message)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(message))
        {
            await RespondAsync(embed: _twitterMapper.CouldNotTweet($"Missing parameters").Build());
            return;
        }
        
        var match = Regex.Match(url, @"(x\.com|twitter\.com)\/(.*)\/.*\/(\d+)$");
        if (!match.Success || match.Groups.Count != 4)
        {
            await RespondAsync(embed: _twitterMapper.CouldNotTweet($"Invalid Twitter link! Expecting something like: `{_config.GetValue<string>("TWITTER:EXAMPLE_TWEET")}`").Build());
            return;
        }

        var userId = match.Groups[2].Value;
        var tweetId = match.Groups[3].Value;

        if (_twitterWhitlist.All(x => !string.Equals(x, userId, StringComparison.CurrentCultureIgnoreCase)))
        {
            await RespondAsync(embed: _twitterMapper.CouldNotTweet($"User {userId} not on whitelist").Build());
            return;
        }
        
        var qrtRequest = new TweetRequest { QuoteId = tweetId, Text = message };
        var qrtResponse = await _twitterClient.PostQuote(qrtRequest);
        if (qrtResponse != null)
        {
            await RespondAsync(embed: _twitterMapper.TweetCreated(qrtResponse).Build());
            return;
        }
        
        await RespondAsync(embed: _twitterMapper.CouldNotTweet("Twitter said no, DM Skelly.").Build());
    }
}