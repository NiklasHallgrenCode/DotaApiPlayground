using System.Text;
using System.Text.Json;
using DotNetEnv;

public class Program
{
    private const int MatchLimit = 10000;
    private const int DaysPeer = 10000;
    private const int LobbyType = 7;
    private const int NumberOfPeers = 5;

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
            .Take(NumberOfPeers)
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

        var peerTitles = string.Join(",", wonGamesWithPlayer.Select(w => $"{w.PlayerName} ({w.AccountId})"));

        var titleString =
            $"MatchId,IsWin,WonGames,MatchDate,AverageRank,Teammate1,Teammate2,Teammate3,Teammate4,{peerTitles}";

        sb.AppendLine(titleString);

        foreach (var match in playerMatches)
        {
            string matchDate = ConvertUnixTimeToDateString(match.StartTime);
            bool isWin = winningMatchIds.Contains(match.MatchId);

            var teammateIds = new List<string>();
            foreach (var peerHistory in peerMatchHistories)
            {
                if (peerHistory.Matches.Any(m => m.MatchId == match.MatchId))
                {
                    if (peersDictionary.TryGetValue(peerHistory.AccountId, out string? personaName))
                    {
                        teammateIds.Add(peerHistory.AccountId);
                    }
                    else
                    {
                        teammateIds.Add("Unknown Peer");
                    }
                }
            }

            wonGames += isWin ? 1 : -1;

            foreach (var teammateId in teammateIds)
            {
                var player = wonGamesWithPlayer.FirstOrDefault(w => w.AccountId.Value.ToString() == teammateId);
                if (player != null)
                {
                    player.Wins += isWin ? 1 : -1;
                }
            }

            string teamString = BuildTeammateString(teammateIds, peersDictionary);
            string winsString = BuildWinsString(wonGamesWithPlayer);

            string stringToWrite = $"{match.MatchId},{(isWin ? "true" : "false")},{wonGames},{matchDate},{(match.AverageRank == null ? "0" : match.AverageRank)},{teamString},{winsString}";
            sb.AppendLine(stringToWrite);
        }
        Console.WriteLine(sb);

        // Define the file path
        string filePath = "output.csv";

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

        var nameWithCounts = wonGamesWithPlayer.Select(player =>
        {
            //if (teammateNames.Contains(dict.Key))
            //{
            //return $"{player.AccountId},{player.PlayerName},{player.Wins}";
            return player.Wins.ToString();
            //}
            //return $"{dict.Key};";
        }).ToList();

        return string.Join(",", nameWithCounts);
    }

    private static string BuildTeammateString(List<string> teammateIds, Dictionary<string, string> peersDictionary)
    {

        List<string> teammatesList = new List<string>();


        for (int i = 0; i < 4; i++)
        {
            if (teammateIds.Count <= i)
            {
                teammatesList.Add("RANDOM");
            }
            else
            {
                teammatesList.Add($"{peersDictionary[teammateIds[i]]} ({teammateIds[i]})");
            }
        }
        return string.Join(",", teammatesList);
    }
}