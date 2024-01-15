using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using Discord;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace VGI.Helpers.Osu;

public class OsuMapper(IConfiguration configuration)
{
    private readonly Color _embedColor = new(0xA65973);
    private readonly string _historicalLeaderboardPath = "osu_leaderboard.json";
    private readonly string _a_rank_emoji = configuration.GetValue<string>("BANCHO:A_RANK_EMOJI");
    private readonly string _b_rank_emoji = configuration.GetValue<string>("BANCHO:B_RANK_EMOJI");
    private readonly string _c_rank_emoji = configuration.GetValue<string>("BANCHO:C_RANK_EMOJI");
    private readonly string _d_rank_emoji = configuration.GetValue<string>("BANCHO:D_RANK_EMOJI");
    private readonly string _f_rank_emoji = configuration.GetValue<string>("BANCHO:F_RANK_EMOJI");
    private readonly string _s_rank_emoji = configuration.GetValue<string>("BANCHO:S_RANK_EMOJI");
    private readonly string _s_plus_rank_emoji = configuration.GetValue<string>("BANCHO:SPlus_RANK_EMOJI");
    private readonly string _ss_rank_emoji = configuration.GetValue<string>("BANCHO:SS_RANK_EMOJI");
    private readonly string _ss_plus_rank_emoji = configuration.GetValue<string>("BANCHO:SSPlus_RANK_EMOJI");
    private readonly string _bancho_url = configuration.GetValue<string>("BANCHO:URL");

    public EmbedBuilder NoData()
    {
        var statsEmbed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = $"Could not find results for criteria",
            },
            Description = "Oopsie, talk to Skelly is this is wrong",
            Color = Color.Red
        };
        return statsEmbed;
    }
    
    public EmbedBuilder ToProfileEmbed(OsuGameTypes gameType, MySqlDataReader reader)
    {
        reader.Read();

        var userId = reader.GetInt64("user_id");
        var name = reader.GetString("user_name");
        var country = reader.GetString("user_country");
        var ranking = reader.GetInt64("stats_ranking");
        var pp = reader.GetInt64("stats_pp");
        var accuracy = reader.GetFloat("stats_acc");
        var plays = reader.GetInt64("stats_plays");
        var playtime = reader.GetInt64("stats_playtime");
        var a_rank_count = reader.GetInt64("stats_a_count");
        var s_rank_count = reader.GetInt64("stats_sh_count");
        var s_plus_rank_count = reader.GetInt64("stats_s_count");
        var ss_rank_count = reader.GetInt64("stats_xh_count");
        var ss_plus_rank_count = reader.GetInt64("stats_x_count");
        var max_combo = reader.GetInt64("stats_max_combo");

        var stats = new List<string>
        {
            $"• **Leaderboard Rank:** #{ranking}",
            $"• **PP:** {pp}pp",
            $"• **Accuracy:** {accuracy:n2}%",
            $"• **Playcount:** {plays} ({playtime / 3600} hours)",
            $"• **Max Combo:** x{max_combo}",
            $"<:SS:1195386023014834176> `{ss_plus_rank_count}`\t" +
            $"<:SSPlus:1195386025602715648> `{ss_rank_count}`\t" +
            $"<:S_:1195386018765996072> `{s_plus_rank_count}`\t" +
            $"<:SPlus:1195386021580382401> `{s_rank_count}`\t" +
            $"<:A_:1195386010289324145> `{a_rank_count}`\t"
        };

        var statsEmbed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = $"{gameType.ToDisplayText()} profile for {name}",
                IconUrl = $"https://{_bancho_url}/static/images/flags/{country.ToUpper()}.png",
                Url = $"https://{_bancho_url}/u/{userId}"
            },
            ThumbnailUrl = $"https://a.{_bancho_url}/{userId}",
            Description = string.Join("\r\n", stats),
            Color = _embedColor
        };
        return statsEmbed;
    }

    public EmbedBuilder ToTopScoresEmbed(bool singleUser, OsuGameTypes gameType, MySqlDataReader reader)
    {
        long primaryId = 0;
        string primaryName = null;
        string primaryLocation = null;
        var stats = new List<string>();

        var index = 0;
        while (reader.Read())
        {
            index++;
            var userId = reader.GetInt64("user_id");
            var name = reader.GetString("user_name");
            var country = reader.GetString("user_country");
            var songSetId = reader.GetInt64("map_set_id");
            var songId = reader.GetInt64("map_id");
            var songSet = reader.GetString("map_version");
            var songName = reader.GetString("map_title");
            var mapMaxCombo = reader.GetInt64("map_max_combo");
            var mapDiff = reader.GetFloat("map_diff");
            var mods = reader.IsDBNull("mod_name") ? "" : $"**+{reader.GetString("mod_name")}**";
            var mode = (OsuGameTypes)reader.GetInt64("score_mode");
            var score = reader.GetInt64("score_score");
            var grade = reader.GetString("score_grade");
            var pp = reader.GetInt64("score_pp");
            var acc = reader.GetFloat("score_acc");
            var maxCombo = reader.GetInt64("score_max_combo");
            var h300 = reader.GetInt64("score_n300");
            var h100 = reader.GetInt64("score_n100");
            var h50 = reader.GetInt64("score_n50");
            var hMiss = reader.GetInt64("score_nmiss");
            var playDate = reader.GetDateTime("score_play_time");

            //Set author values once
            if (primaryName == null)
            {
                primaryId = userId;
                primaryName = name;
                primaryLocation = country;
            }

            stats.AddRange(new[]
            {
                $"**{index}.** [**{songName} [{songSet}]**](https://osu.ppy.sh/beatmapsets/{songSetId}#{mode}/{songId}) {mods} [\u2605 {mapDiff:N2}]",
                $"• {OsuGrade(grade)} • {pp:N0}PP • {acc:N2}%",
                $"• {score:N0} • x{maxCombo:N0}/{mapMaxCombo:N0} • [{h300:N0}/{h100:N0}/{h50:N0}/{hMiss:N0}]",
                $"• {TimestampTag.FromDateTime(playDate, TimestampTagStyles.Relative)} by {name}",
                ""
            });
        }

        var userFilter = string.Empty;

        if (singleUser)
        {
            userFilter = $"for {primaryName}";
        }

        var statsEmbed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = $"Top 5 {gameType.ToDisplayText()} scores {userFilter}",
                IconUrl = $"https://{_bancho_url}/static/images/flags/{primaryLocation.ToUpper()}.png",
                Url = singleUser ? $"https://{_bancho_url}/u/{primaryId}" : null
            },
            ThumbnailUrl = $"https://a.{_bancho_url}/{primaryId}",
            Description = string.Join("\r\n", stats),
            Color = _embedColor
        };

        return statsEmbed;
    }

    public EmbedBuilder ToRecentScoresEmbed(bool singleUser, OsuGameTypes gameType, MySqlDataReader reader)
    {
        long primaryId = 0;
        string primaryName = null;
        string primaryLocation = null;
        var stats = new List<string>();

        var index = 0;
        while (reader.Read())
        {
            index++;
            var userId = reader.GetInt64("user_id");
            var name = reader.GetString("user_name");
            var country = reader.GetString("user_country");
            var songSetId = reader.GetInt64("map_set_id");
            var songId = reader.GetInt64("map_id");
            var songSet = reader.GetString("map_version");
            var songName = reader.GetString("map_title");
            var mapMaxCombo = reader.GetInt64("map_max_combo");
            var mapDiff = reader.GetFloat("map_diff");
            var mods = reader.IsDBNull("mod_name") ? "" : $"**+{reader.GetString("mod_name")}**";
            var mode = (OsuGameTypes)reader.GetInt64("score_mode");
            var score = reader.GetInt64("score_score");
            var grade = reader.GetString("score_grade");
            var pp = reader.GetInt64("score_pp");
            var acc = reader.GetFloat("score_acc");
            var maxCombo = reader.GetInt64("score_max_combo");
            var h300 = reader.GetInt64("score_n300");
            var h100 = reader.GetInt64("score_n100");
            var h50 = reader.GetInt64("score_n50");
            var hMiss = reader.GetInt64("score_nmiss");
            var playDate = reader.GetDateTime("score_play_time");

            //Set author values once
            if (primaryName == null)
            {
                primaryId = userId;
                primaryName = name;
                primaryLocation = country;
            }

            stats.AddRange(new[]
            {
                $"**{index}.** [**{songName} [{songSet}]**](https://osu.ppy.sh/beatmapsets/{songSetId}#{mode}/{songId}) {mods} [\u2605 {mapDiff:N2}]",
                $"• {OsuGrade(grade)} • {pp:N0}PP • {acc:N2}%",
                $"• {score:N0} • x{maxCombo:N0}/{mapMaxCombo:N0} • [{h300:N0}/{h100:N0}/{h50:N0}/{hMiss:N0}]",
                $"• {TimestampTag.FromDateTime(playDate, TimestampTagStyles.Relative)} by {name}",
                ""
            });
        }

        var userFilter = string.Empty;

        if (singleUser)
        {
            userFilter = $"for {primaryName}";
        }

        var statsEmbed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = $"Recent {gameType.ToDisplayText()} scores {userFilter}",
                IconUrl = $"https://{_bancho_url}/static/images/flags/{primaryLocation.ToUpper()}.png",
                Url = singleUser ? $"https://{_bancho_url}/u/{primaryId}" : null
            },
            ThumbnailUrl = $"https://a.{_bancho_url}/{primaryId}",
            Description = string.Join("\r\n", stats),
            Color = _embedColor
        };

        return statsEmbed;
    }

    public EmbedBuilder ToLeaderboardEmbed(OsuGameTypes gameType, MySqlDataReader reader)
    {
        long primaryId = 0;
        string primaryName = null;
        string primaryLocation = null;
        var stats = new List<string>();
        var statsCollection = new Dictionary<long, int>();
        var historicalData = new Dictionary<long, int>();
        if (File.Exists(_historicalLeaderboardPath))
        {
            var fileContents = File.ReadAllText(_historicalLeaderboardPath);
            historicalData = JsonSerializer.Deserialize<Dictionary<long, int>>(fileContents);
        }

        var index = 0;
        while (reader.Read())
        {
            index++;
            var userId = reader.GetInt64("user_id");
            var name = reader.GetString("user_name");
            var country = reader.GetString("user_country");
            var pp = reader.GetInt64("stats_pp");
            var acc = reader.GetFloat("stats_acc");
            var maxCombo = reader.GetInt64("stats_max_combo");
            statsCollection[userId] = index;

            //Set author values once
            if (primaryName == null)
            {
                primaryId = userId;
                primaryName = name;
                primaryLocation = country;
            }

            var positionChange = " ";
            if (historicalData.TryGetValue(userId, out var historialIndex))
            {
                var change = index - historialIndex;
                positionChange = change switch
                {
                    > 0 => $"\u25bc {Math.Abs(change)}",
                    < 0 => $"\u25b2 {Math.Abs(change)}",
                    _ => positionChange
                };
            }
            else
            {
                positionChange = "NEW";
            }

            stats.AddRange(new[]
            {
                $"**{index}.** **{name}** **{positionChange}**",
                $"• {pp:N0}PP • {acc:N2}% • x{maxCombo}",
                ""
            });
        }

        //Save the data for next time
        var serialisedLeaderboard = JsonSerializer.Serialize(statsCollection);
        File.WriteAllText(_historicalLeaderboardPath, serialisedLeaderboard);

        var statsEmbed = new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = $"{gameType.ToDisplayText()} leaderboard",
                IconUrl = $"https://{_bancho_url}/static/images/flags/{primaryLocation.ToUpper()}.png",
                Url = $"https://{_bancho_url}/leaderboard/std/rscore/vn"
            },
            ThumbnailUrl = $"https://a.{_bancho_url}/{primaryId}",
            Description = string.Join("\r\n", stats),
            Color = _embedColor
        };

        return statsEmbed;
    }

    private string OsuGrade(string source)
    {
        
        return source switch
        {
            "A" => _a_rank_emoji,
            "B" => _b_rank_emoji,
            "C" => _c_rank_emoji,
            "D" => _d_rank_emoji,
            "F" => _f_rank_emoji,
            "S" => _s_rank_emoji,
            "SH" => _s_plus_rank_emoji,
            "SS" => _ss_rank_emoji,
            "SSH" => _ss_plus_rank_emoji,
            _ => source
        };
    }
}