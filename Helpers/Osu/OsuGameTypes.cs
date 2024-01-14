namespace VGI.Helpers.Osu;

public enum OsuGameTypes
{
    Osu = 0,
    Taiko = 1,
    Catch = 2,
    Mania = 3
}

public static class OsuGameTypesExtensions
{
    public static string ToDisplayText(this OsuGameTypes gameType)
    {
        return gameType switch
        {
            OsuGameTypes.Taiko => "taiko",
            OsuGameTypes.Catch => "catch",
            OsuGameTypes.Mania => "mania",
            _ => "osu!"
        };
    }
}