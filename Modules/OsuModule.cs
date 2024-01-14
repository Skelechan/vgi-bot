using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using VGI.Helpers.Osu;

namespace VGI.Modules.Osu;

[Group("osu", "Commands related to the VGI Osu! server")]
public class OsuModule(IConfiguration config, OsuMapper mapper) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly string _banchoConnectionString = config["BANCHO:CONNECTION_STRING"];
    private readonly string _secretCode = config["BANCHO:SECRET_CODE"];

    [SlashCommand("stats", "Get a user profile")]
    public Task GetProfile([Summary(name: "User", description: "The user you wish to lookup stats for")] SocketGuildUser user)
    {
        using var connection = new MySqlConnection(_banchoConnectionString);
        connection.Open();
        var command = new MySqlCommand("SELECT user_id, user_name, user_country, " +
                                       "stats_mode, stats_tscore, stats_rscore, stats_pp, stats_plays, stats_playtime, stats_acc, stats_max_combo, stats_xh_count, stats_x_count, stats_sh_count, stats_s_count, stats_a_count, stats_ranking " +
                                       "FROM v_leaderboard " +
                                       "WHERE user_discord_id = @discordID AND stats_mode = @gameMode " +
                                       "ORDER BY stats_pp DESC", connection);
        command.Parameters.AddWithValue("discordID", user.Id);
        command.Parameters.AddWithValue("gameMode", (int)OsuGameTypes.Osu);

        using (var reader = command.ExecuteReader())
        {
            if (!reader.HasRows)
                return RespondAsync(embed: mapper.NoData().Build());

            return RespondAsync(embed: mapper.ToProfileEmbed(OsuGameTypes.Osu, reader).Build());
        }
    }

    [SlashCommand("top", "Get top plays.")]
    public Task GetTopScores([Summary(name: "User", description: "Filter top plays by user")] SocketUser? user = null)
    {
        using (var connection = new MySqlConnection(_banchoConnectionString))
        {
            connection.Open();
            var command = new MySqlCommand("SELECT user_id, user_name,user_country, " +
                                           "map_set_id, map_id, map_version, map_title, map_max_combo, map_diff, " +
                                           "mod_name, " +
                                           "score_mode, score_score, score_grade, score_pp, score_acc, score_max_combo, score_n300, score_n100, score_n50, score_nmiss, score_play_time " +
                                           "FROM v_scores " +
                                           "WHERE score_mode = @gameMode " +
                                           "AND score_grade != 'F' ", connection);
            command.Parameters.AddWithValue("gameMode", (int)OsuGameTypes.Osu);
            if (user != null)
            {
                command.CommandText += " AND user_discord_id = @discordId ";
                command.Parameters.AddWithValue("discordId", user.Id);
            }

            command.CommandText += "ORDER BY score_pp DESC " +
                                   "LIMIT 5 ";

            using (var reader = command.ExecuteReader())
            {
                if (!reader.HasRows)
                    return RespondAsync(embed: mapper.NoData().Build());

                return RespondAsync(embed: mapper.ToTopScoresEmbed(user != null, OsuGameTypes.Osu, reader).Build());
            }
        }
    }

    [SlashCommand("recent", "Get most recent plays.")]
    public Task GetRecent([Summary(name: "User", description: "Filter recent plays by user")] SocketUser? user = null)
    {
        using (var connection = new MySqlConnection(_banchoConnectionString))
        {
            connection.Open();
            var command = new MySqlCommand("SELECT user_id, user_name,user_country, " +
                                           "map_set_id, map_id, map_version, map_title, map_max_combo, map_diff, " +
                                           "mod_name, " +
                                           "score_mode, score_score, score_grade, score_pp, score_acc, score_max_combo, score_n300, score_n100, score_n50, score_nmiss, score_play_time " +
                                           "FROM v_scores " +
                                           "WHERE score_mode = @gameMode ", connection);
            command.Parameters.AddWithValue("gameMode", (int)OsuGameTypes.Osu);
            if (user != null)
            {
                command.CommandText += " AND user_discord_id = @discordId ";
                command.Parameters.AddWithValue("discordId", user.Id);
            }

            command.CommandText += "ORDER BY score_pp DESC " +
                                   "LIMIT 5 ";

            using (var reader = command.ExecuteReader())
            {
                if (!reader.HasRows)
                    return RespondAsync(embed: mapper.NoData().Build());

                return RespondAsync(embed: mapper.ToRecentScoresEmbed(user != null, OsuGameTypes.Osu, reader).Build());
            }
        }
    }

    [SlashCommand("leaderboard", "Get current leaderboard.")]
    public Task Leaderboard()
    {
        using var connection = new MySqlConnection(_banchoConnectionString);
        connection.Open();
        var command = new MySqlCommand("SELECT user_id, user_name, user_country, " +
                                       "stats_pp, stats_acc, stats_max_combo " +
                                       "FROM v_leaderboard " +
                                       "WHERE stats_mode = @gameMode ", connection);
        command.Parameters.AddWithValue("gameMode", (int)OsuGameTypes.Osu);

        using (var reader = command.ExecuteReader())
        {
            if (!reader.HasRows)
                return RespondAsync(embed: mapper.NoData().Build());

            return RespondAsync(embed: mapper.ToLeaderboardEmbed(OsuGameTypes.Osu, reader).Build());
        }
    }

    [SlashCommand("code", "Get secret code for registration.")]
    public Task SecretCode()
    {
        var response = new EmbedBuilder
        {
            Description = _secretCode
        };
        return RespondAsync(embed: response.Build(), ephemeral:true);
    }
}