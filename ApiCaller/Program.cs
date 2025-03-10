using System.Text;
using System.Text.Json;
using DotNetEnv;

public static class Program
{
    private const int MatchLimit = 50;
    private const int DaysPeer = int.MaxValue;
    private const int NumberOfPeers = 1000;

    private enum LobbyTypeEnum
    {
        Normal = 0,
        Ranked = 7
    }

    private enum GameModeEnum
    {
        AllPick = 1,
        AllDraft = 22,
        Turbo = 23
    }

    private const int LobbyType = (int)LobbyTypeEnum.Normal;
    private const int GameMode = (int)GameModeEnum.AllDraft;

    private static readonly Dictionary<string, string> MedalToRankDictionary = RankToMmr.GetRankMmrMap();
    private static readonly HttpClient HttpClient = new HttpClient();
    private static readonly string PlayerId;
    private static readonly string BaseUrl = "https://api.opendota.com/api/players";
    private static readonly string BaseMatchesUrl = "https://api.opendota.com/api/matches";

    static Program()
    {
        Env.Load();
        PlayerId = Env.GetString("PLAYER_ID")
            ?? throw new Exception("PLAYER_ID is missing or not set in .env");
    }

    public static async Task Main()
    {
        var topPeers = await GetTopPeersAsync(PlayerId, NumberOfPeers, DaysPeer, LobbyType);

        var excludedQueryString = string.Join("&", topPeers
            .Where(tp => tp.AccountId.HasValue)
            .Select(tp => $"excluded_account_id={tp.AccountId.Value}"));

        string playerMatchesUrl = BuildMatchesUrl(PlayerId, MatchLimit, LobbyType, GameMode, excludedQueryString);
        string playerWinUrl = playerMatchesUrl + "&win=1";

        var playerMatchesTask = FetchListAsync<PlayerMatch>(playerMatchesUrl);
        var playerWinMatchesTask = FetchListAsync<PlayerMatch>(playerWinUrl);

        await Task.WhenAll(playerMatchesTask, playerWinMatchesTask);

        var playerMatches = playerMatchesTask.Result;
        var playerWinMatches = playerWinMatchesTask.Result;

        playerMatches.Reverse();

        var matchDataTasks = playerMatches
            .Select(m => FetchSingleAsync<MatchData>($"{BaseMatchesUrl}/{m.MatchId}"))
            .ToList();

        var matchData = await Task.WhenAll(matchDataTasks);

        ProcessMatchesWithPeers(matchData, new List<MatchHistory>(), playerWinMatches);
    }

    public static async Task OnlyPlayer()
    {
        string playerMatchesUrl = BuildMatchesUrl(PlayerId, MatchLimit, LobbyType, GameMode);
        string peersUrl = $"{BaseUrl}/{PlayerId}/peers?lobby_type={LobbyType}&date={DaysPeer}";
        string playerWinUrl = playerMatchesUrl + "&win=1";

        var playerMatchesTask = FetchListAsync<PlayerMatch>(playerMatchesUrl);
        var peersTask = FetchListAsync<PlayerData>(peersUrl);
        var playerWinMatchesTask = FetchListAsync<PlayerMatch>(playerWinUrl);

        await Task.WhenAll(playerMatchesTask, peersTask, playerWinMatchesTask);

        var playerMatches = playerMatchesTask.Result;
        var peersData = peersTask.Result;
        var playerWinMatches = playerWinMatchesTask.Result;

        var winningMatchIds = new HashSet<long>(playerWinMatches.Select(m => m.MatchId));

        var topPeers = peersData
            .Where(p => p.AccountId.HasValue)
            .Take(NumberOfPeers)
            .ToList();

        var peerMatchHistories = await FetchPeerMatchHistories(topPeers);

        playerMatches.Reverse();

        var sb = new StringBuilder();
        int games = 0;

        foreach (var match in playerMatches)
        {
            bool skip = peerMatchHistories.Any(ph => ph.Matches.Any(m => m.MatchId == match.MatchId));
            if (skip) continue;

            games++;

            string matchDate = ConvertUnixTimeToDateString(match.StartTime);
            bool isWin = winningMatchIds.Contains(match.MatchId);

            var avgRankKey = match.AverageRank?.ToString() ?? "";
            MedalToRankDictionary.TryGetValue(avgRankKey, out string mmrString);
            mmrString ??= "0";

            string line = $"{match.MatchId} / {matchDate} / {(isWin ? "W" : "L")} / {avgRankKey} ({mmrString})";
            Console.WriteLine(line);
            sb.AppendLine(line);
        }

        Console.WriteLine(sb.ToString());
        File.WriteAllText("output1.csv", sb.ToString());
        Console.WriteLine($"Found {games} matches");
    }

    public static async Task Teammates()
    {
        string playerMatchesUrl = $"{BaseUrl}/{PlayerId}/matches?limit={MatchLimit}&lobby_type={LobbyType}";
        string peersUrl = $"{BaseUrl}/{PlayerId}/peers?lobby_type={LobbyType}&date={DaysPeer}";
        string playerWinUrl = playerMatchesUrl + "&win=1";

        var playerMatchesTask = FetchListAsync<PlayerMatch>(playerMatchesUrl);
        var peersTask = FetchListAsync<PlayerData>(peersUrl);
        var playerWinMatchesTask = FetchListAsync<PlayerMatch>(playerWinUrl);

        await Task.WhenAll(playerMatchesTask, peersTask, playerWinMatchesTask);

        var playerMatches = playerMatchesTask.Result;
        var peersData = peersTask.Result;
        var playerWinMatches = playerWinMatchesTask.Result;

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

        var peerMatchHistories = await FetchPeerMatchHistories(topPeers);

        playerMatches.Reverse();

        var winningMatchIds = new HashSet<long>(playerWinMatches.Select(m => m.MatchId));
        int wonGames = 0;

        var wonGamesWithPlayer = topPeers
            .Select(p => new PlayerStats
            {
                AccountId = p.AccountId,
                PlayerName = p.Personaname
            })
            .ToList();

        var sb = new StringBuilder();
        var peerTitles = string.Join(",", wonGamesWithPlayer.Select(w => $"{w.PlayerName} ({w.AccountId})"));
        var titleString = $"MatchId,IsWin,WonGamesSoFar,MatchDate,AverageRank,Teammate1,Teammate2,Teammate3,Teammate4,{peerTitles}";

        sb.AppendLine(titleString);

        foreach (var match in playerMatches)
        {
            string matchDate = ConvertUnixTimeToDateString(match.StartTime);
            bool isWin = winningMatchIds.Contains(match.MatchId);

            var teammateIds = new List<string>();
            foreach (var pmh in peerMatchHistories)
            {
                bool peerInMatch = pmh.Matches.Any(m => m.MatchId == match.MatchId);
                if (peerInMatch)
                {
                    teammateIds.Add(pmh.AccountId);
                }
            }

            wonGames += isWin ? 1 : -1;

            foreach (var tId in teammateIds)
            {
                var peerStats = wonGamesWithPlayer.FirstOrDefault(w => w.AccountId?.ToString() == tId);
                if (peerStats != null)
                {
                    peerStats.Wins += isWin ? 1 : -1;
                }
            }

            string teammatesCsv = BuildTeammateCsvRow(teammateIds, peersDictionary);
            string winsString = BuildWinsString(wonGamesWithPlayer);

            string averageRank = match.AverageRank?.ToString() ?? "0";
            string line = $"{match.MatchId},{isWin},{wonGames},{matchDate},{averageRank},{teammatesCsv},{winsString}";

            sb.AppendLine(line);
        }

        Console.WriteLine(sb.ToString());
        File.WriteAllText("output2.csv", sb.ToString());
        Console.WriteLine($"Found {playerMatches.Count} matches");
    }

    private static async Task<List<T>> FetchListAsync<T>(string url)
    {
        string responseString = await HttpClient.GetStringAsync(url);

        return JsonSerializer.Deserialize<List<T>>(
            responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ) ?? new List<T>();
    }

    private static async Task<T?> FetchSingleAsync<T>(string url)
    {
        try
        {
            var responseString = await HttpClient.GetStringAsync(url);

            if (string.IsNullOrWhiteSpace(responseString))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(
                responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (HttpRequestException httpEx)
        {
            return default;
        }
        catch (JsonException jsonEx)
        {
            return default;
        }
    }



    private static async Task<List<PlayerData>> GetTopPeersAsync(string playerId, int numberOfPeers, int daysPeer, int lobbyType)
    {
        string peersUrl = $"{BaseUrl}/{playerId}/peers?lobby_type={lobbyType}&date={daysPeer}";
        var peersData = await FetchListAsync<PlayerData>(peersUrl);

        return peersData
            .Where(p => p.AccountId.HasValue)
            .Take(numberOfPeers)
            .ToList();
    }

    private static async Task<MatchHistory[]> FetchPeerMatchHistories(IEnumerable<PlayerData> topPeers)
    {
        var tasks = topPeers.Select(async peer =>
        {
            string accountIdString = peer.AccountId?.ToString() ?? "Unknown";
            string url = $"{BaseUrl}/{accountIdString}/matches?limit={MatchLimit}&lobby_type={LobbyType}";
            var matches = await FetchListAsync<PlayerMatch>(url);

            return new MatchHistory
            {
                AccountId = accountIdString,
                Matches = matches
            };
        });

        return await Task.WhenAll(tasks);
    }

    private static string BuildMatchesUrl(
        string playerId,
        int matchLimit,
        int lobbyType,
        int gameMode,
        string excludedQueryString = null
    )
    {
        var url = $"{BaseUrl}/{playerId}/matches?limit={matchLimit}&lobby_type={lobbyType}&game_mode={gameMode}";

        if (!string.IsNullOrWhiteSpace(excludedQueryString))
        {
            url += "&" + excludedQueryString;
        }

        return url;
    }

    private static void ProcessMatchesWithPeers(
        MatchData[] matches,
        IEnumerable<MatchHistory> peerMatchHistories,
        List<PlayerMatch> playerWinMatches
    )
    {
        var winningMatchIds = new HashSet<long>(playerWinMatches.Select(m => m.MatchId));
        var sb = new StringBuilder();
        int games = 0;

        foreach (var match in matches)
        {
            if (match == null) continue;

            bool skip = peerMatchHistories.Any(ph => ph.Matches.Any(m => m.MatchId == match.MatchId));
            if (skip) continue;

            games++;

            var userPlayer = match.Players.FirstOrDefault(p => p.AccountId?.ToString() == PlayerId);
            if (userPlayer == null) continue;

            bool userIsRadiant = userPlayer.IsRadiant ?? false;

            var opponentMmrList = match.Players
                .Where(p => p.IsRadiant != userIsRadiant)
                .Where(p => p.RankTier != null && MedalToRankDictionary.ContainsKey(p.RankTier.Value.ToString()))
                .Select(p => int.Parse(MedalToRankDictionary[p.RankTier.Value.ToString()]))
                .ToList();

            var opponentImmortalListCount = match.Players.Where(p => p.IsRadiant != userIsRadiant).Count(p => p.RankTier != null && p.RankTier.Value == 80);

            bool isWin = winningMatchIds.Contains(match.MatchId.Value);
            int averageOpponentMmr = opponentMmrList.Any() ? (int)opponentMmrList.Average() : 0;
            string approxMedal = GetClosestRank(averageOpponentMmr);

            string matchDate = ConvertUnixTimeToDateString(match.StartTime);
            string line = $"{match.MatchId} / {matchDate} / {(isWin ? "W" : "L")} / {approxMedal} ({averageOpponentMmr}) ({opponentMmrList.Count}) ({opponentImmortalListCount})";

            Console.WriteLine(line);
            sb.AppendLine(line);
        }

        Console.WriteLine(sb.ToString());
        Console.WriteLine($"Found {games} matches after peer-exclusion filtering");
        File.WriteAllText("output1.csv", sb.ToString());
    }

    private static string ConvertUnixTimeToDateString(long? unixTimeSeconds)
    {
        if (unixTimeSeconds == null) return "UnknownDate";

        var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds.Value).UtcDateTime;
        return dateTime.ToString("yyyy-MM-dd");
    }

    private static string BuildTeammateCsvRow(List<string> teammateIds, Dictionary<string, string> peersDictionary)
    {
        var list = new List<string>();

        for (int i = 0; i < 4; i++)
        {
            if (i < teammateIds.Count)
            {
                string tId = teammateIds[i];
                if (peersDictionary.TryGetValue(tId, out var personaName))
                {
                    list.Add($"{personaName} ({tId})");
                }
                else
                {
                    list.Add($"UnknownPeer ({tId})");
                }
            }
            else
            {
                list.Add("RANDOM");
            }
        }

        return string.Join(",", list);
    }

    private static string BuildWinsString(List<PlayerStats> playerStats)
    {
        var counts = playerStats.Select(p => p.Wins.ToString());
        return string.Join(",", counts);
    }

    public static string GetClosestRank(int targetMmr)
    {
        if (targetMmr >= 5620)
            return "80";

        var validEntries = MedalToRankDictionary
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => new
            {
                RankCode = kv.Key,
                ApproxMmr = int.Parse(kv.Value)
            })
            .ToList();

        var bestMatch = validEntries
            .OrderBy(x => Math.Abs(x.ApproxMmr - targetMmr))
            .First();

        return bestMatch.RankCode;
    }
}
