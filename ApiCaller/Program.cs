using System.Text;
using System.Text.Json;
using DotNetEnv;

public class Program
{
    private const int MatchLimit = 10000;
    private const int DaysPeer = 10000;
    private const int LobbyType = 7;

    public static async Task Main()
    {
        Env.Load();
        string playerId = Env.GetString("PLAYER_ID");
        using var httpClient = new HttpClient();

        string baseUrl = "https://api.opendota.com/api/players";
        string playerMatchesUrl = $"{baseUrl}/{playerId}/matches?limit={MatchLimit}&lobby_type={LobbyType}";
        string peersUrl = $"{baseUrl}/{playerId}/peers?lobby_type={LobbyType}&date={DaysPeer}";
        string playerWinUrl = $"{playerMatchesUrl}&win=1";

        var playerMatchesTask = FetchDataAsync<Match>(httpClient, playerMatchesUrl);
        var peersTask = FetchDataAsync<PlayerData>(httpClient, peersUrl);
        var playerWinMatchesTask = FetchDataAsync<Match>(httpClient, playerWinUrl);

        await Task.WhenAll(playerMatchesTask, peersTask, playerWinMatchesTask);

        var playerMatches = playerMatchesTask.Result;
        var peersData = peersTask.Result;
        var playerWinMatches = playerWinMatchesTask.Result;

        var winningMatchIds = new HashSet<long>(playerWinMatches.Select(m => m.MatchId));

        var topPeers = peersData
            .Where(p => p.AccountId.HasValue)
            .Take(15)
            .ToList();

        var peersDictionary = topPeers
            .Where(peer => peer.AccountId.HasValue)
            .ToDictionary(
                peer => peer.AccountId.Value.ToString(),
                peer => peer.Personaname
            );

        var peerMatchesTasks = topPeers.Select(async peer =>
        {
            string accountIdString = peer.AccountId?.ToString() ?? "Unknown";
            string url = $"{baseUrl}/{accountIdString}/matches?limit={MatchLimit}&lobby_type={LobbyType}";
            var matches = await FetchDataAsync<Match>(httpClient, url);

            return new MatchHistory
            {
                AccountId = accountIdString,
                Matches = matches
            };
        });

        var peerMatchHistories = await Task.WhenAll(peerMatchesTasks);

        playerMatches.Reverse();

        int wonGames = 0;

        List<PlayerStats> wonGamesWithPlayer = new List<PlayerStats>();

        foreach (var peer in topPeers)
        {
            wonGamesWithPlayer.Add(new PlayerStats { AccountId = peer.AccountId, PlayerName = peer.Personaname });
        }

        StringBuilder sb = new StringBuilder();

        foreach (var match in playerMatches)
        {
            string matchDate = ConvertUnixTimeToDateString(match.StartTime);
            bool isWin = winningMatchIds.Contains(match.MatchId);

            var teammateNames = new List<string>();
            foreach (var peerHistory in peerMatchHistories)
            {
                if (peerHistory.Matches.Any(m => m.MatchId == match.MatchId))
                {
                    if (peersDictionary.TryGetValue(peerHistory.AccountId, out string? personaName))
                    {
                        teammateNames.Add(personaName);
                    }
                    else
                    {
                        teammateNames.Add("Unknown Peer");
                    }
                }
            }

            wonGames += isWin ? 1 : -1;

            foreach (var teammateName in teammateNames)
            {
                var player = wonGamesWithPlayer.FirstOrDefault(w => w.PlayerName == teammateName);
                if (player != null)
                {
                    player.Wins += isWin ? 1 : -1;
                }
            }

            string teamString = BuildTeammateString(teammateNames);
            string winsString = BuildWinsString(wonGamesWithPlayer);

            string stringToWrite = $"{match.MatchId},{(isWin ? "W" : "L")},{wonGames},{matchDate},{(match.AverageRank == null ? "0" : match.AverageRank)},{teamString},{winsString}";
            sb.AppendLine(stringToWrite);
        }
        Console.WriteLine(sb);

        // Define the file path
        string filePath = "output.txt";

        // Write the CSV string to the file
        File.WriteAllText(filePath, sb.ToString());
        Console.WriteLine($"Found {playerMatches.Count} matches");
    }

    private static async Task<List<T>> FetchDataAsync<T>(HttpClient client, string url)
    {
        string responseString = await client.GetStringAsync(url);
        return JsonSerializer.Deserialize<List<T>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<T>();
    }

    private static string ConvertUnixTimeToDateString(long unixTimeSeconds)
    {
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;
        return dateTime.ToString("yyyy-MM-dd");
    }

    private static string BuildWinsString(List<PlayerStats> wonGamesWithPlayer)
    {

        var nameWithCounts = wonGamesWithPlayer.OrderBy(w => w.PlayerName).Select(player =>
        {
            //if (teammateNames.Contains(dict.Key))
            //{
                return $"{player.AccountId},{player.PlayerName},{player.Wins}";
            //}
            //return $"{dict.Key};";
        }).ToList();

        return string.Join(",", nameWithCounts);
    }

    private static string BuildTeammateString(List<string> teammateNames)
    {
        for (int i = 0; i < 5; i++)
        {
            if (teammateNames.Count <= i)
            {
                teammateNames.Add("RANDOM");
            }
        }
        return string.Join(",", teammateNames);
    }
}