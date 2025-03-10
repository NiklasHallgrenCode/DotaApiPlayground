using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Profile
{
    [JsonPropertyName("account_id")]
    public int? AccountId { get; set; }

    [JsonPropertyName("personaname")]
    public string PersonaName { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("plus")]
    public bool? Plus { get; set; }

    [JsonPropertyName("cheese")]
    public int? Cheese { get; set; }

    [JsonPropertyName("steamid")]
    public string SteamId { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }

    [JsonPropertyName("avatarmedium")]
    public string AvatarMedium { get; set; }

    [JsonPropertyName("avatarfull")]
    public string AvatarFull { get; set; }

    [JsonPropertyName("profileurl")]
    public string ProfileUrl { get; set; }

    [JsonPropertyName("last_login")]
    public DateTime? LastLogin { get; set; }

    [JsonPropertyName("loccountrycode")]
    public string LocCountryCode { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("fh_unavailable")]
    public bool? FhUnavailable { get; set; }

    [JsonPropertyName("is_contributor")]
    public bool? IsContributor { get; set; }

    [JsonPropertyName("is_subscriber")]
    public bool? IsSubscriber { get; set; }
}

public class PlayerMatch
{
    [JsonPropertyName("match_id")]
    public long MatchId { get; set; }

    [JsonPropertyName("player_slot")]
    public int? PlayerSlot { get; set; }

    [JsonPropertyName("radiant_win")]
    public bool? RadiantWin { get; set; }

    [JsonPropertyName("hero_id")]
    public int? HeroId { get; set; }

    [JsonPropertyName("start_time")]
    public long StartTime { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("game_mode")]
    public int? GameMode { get; set; }

    [JsonPropertyName("lobby_type")]
    public int? LobbyType { get; set; }

    [JsonPropertyName("version")]
    public int? Version { get; set; } // Nullable

    [JsonPropertyName("kills")]
    public int? Kills { get; set; }

    [JsonPropertyName("deaths")]
    public int? Deaths { get; set; }

    [JsonPropertyName("assists")]
    public int? Assists { get; set; }

    [JsonPropertyName("average_rank")]
    public int? AverageRank { get; set; }

    [JsonPropertyName("xp_per_min")]
    public int? XpPerMin { get; set; }

    [JsonPropertyName("gold_per_min")]
    public int? GoldPerMin { get; set; }

    [JsonPropertyName("hero_damage")]
    public int? HeroDamage { get; set; }

    [JsonPropertyName("tower_damage")]
    public int? TowerDamage { get; set; }

    [JsonPropertyName("hero_healing")]
    public int? HeroHealing { get; set; }

    [JsonPropertyName("last_hits")]
    public int? LastHits { get; set; }

    [JsonPropertyName("lane")]
    public int? Lane { get; set; } // Nullable

    [JsonPropertyName("lane_role")]
    public int? LaneRole { get; set; } // Nullable

    [JsonPropertyName("is_roaming")]
    public bool? IsRoaming { get; set; } // Changed to nullable bool

    [JsonPropertyName("cluster")]
    public int? Cluster { get; set; }

    [JsonPropertyName("leaver_status")]
    public int? LeaverStatus { get; set; }

    [JsonPropertyName("party_size")]
    public int? PartySize { get; set; } // Nullable

    [JsonPropertyName("hero_variant")]
    public int? HeroVariant { get; set; } // Nullable
}

public class MatchHistory
{
    public string AccountId { get; set; }
    public List<PlayerMatch> Matches { get; set; }
}
public class Player
{
    [JsonPropertyName("account_id")]
    public int? AccountId { get; set; }

    [JsonPropertyName("player_slot")]
    public int? PlayerSlot { get; set; }

    [JsonPropertyName("team_number")]
    public int? TeamNumber { get; set; }

    [JsonPropertyName("team_slot")]
    public int? TeamSlot { get; set; }

    [JsonPropertyName("hero_id")]
    public int? HeroId { get; set; }

    [JsonPropertyName("hero_variant")]
    public int? HeroVariant { get; set; }

    [JsonPropertyName("kills")]
    public int? Kills { get; set; }

    [JsonPropertyName("deaths")]
    public int? Deaths { get; set; }

    [JsonPropertyName("assists")]
    public int? Assists { get; set; }

    [JsonPropertyName("leaver_status")]
    public int? LeaverStatus { get; set; }

    [JsonPropertyName("last_hits")]
    public int? LastHits { get; set; }

    [JsonPropertyName("denies")]
    public int? Denies { get; set; }

    [JsonPropertyName("gold_per_min")]
    public int? GoldPerMin { get; set; }

    [JsonPropertyName("xp_per_min")]
    public int? XpPerMin { get; set; }

    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("net_worth")]
    public int? NetWorth { get; set; }

    [JsonPropertyName("aghanims_scepter")]
    public int? AghanimsScepter { get; set; }

    [JsonPropertyName("aghanims_shard")]
    public int? AghanimsShard { get; set; }

    [JsonPropertyName("moonshard")]
    public int? Moonshard { get; set; }

    [JsonPropertyName("hero_damage")]
    public int? HeroDamage { get; set; }

    [JsonPropertyName("tower_damage")]
    public int? TowerDamage { get; set; }

    [JsonPropertyName("hero_healing")]
    public int? HeroHealing { get; set; }

    [JsonPropertyName("gold")]
    public int? Gold { get; set; }

    [JsonPropertyName("gold_spent")]
    public int? GoldSpent { get; set; }

    [JsonPropertyName("ability_upgrades_arr")]
    public List<int?> AbilityUpgrades { get; set; }

    [JsonPropertyName("personaname")]
    public string PersonaName { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("last_login")]
    public DateTime? LastLogin { get; set; }

    [JsonPropertyName("rank_tier")]
    public int? RankTier { get; set; }

    [JsonPropertyName("is_subscriber")]
    public bool? IsSubscriber { get; set; }

    [JsonPropertyName("radiant_win")]
    public bool? RadiantWin { get; set; }

    [JsonPropertyName("start_time")]
    public long? StartTime { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("cluster")]
    public int? Cluster { get; set; }

    [JsonPropertyName("lobby_type")]
    public int? LobbyType { get; set; }

    [JsonPropertyName("game_mode")]
    public int? GameMode { get; set; }

    [JsonPropertyName("is_contributor")]
    public bool? IsContributor { get; set; }

    [JsonPropertyName("patch")]
    public int? Patch { get; set; }

    [JsonPropertyName("region")]
    public int? Region { get; set; }

    [JsonPropertyName("isRadiant")]
    public bool? IsRadiant { get; set; }

    [JsonPropertyName("win")]
    public int? Win { get; set; }

    [JsonPropertyName("lose")]
    public int? Lose { get; set; }

    [JsonPropertyName("total_gold")]
    public int? TotalGold { get; set; }

    [JsonPropertyName("total_xp")]
    public int? TotalXp { get; set; }

    [JsonPropertyName("kills_per_min")]
    public double? KillsPerMin { get; set; }

    [JsonPropertyName("kda")]
    public double? Kda { get; set; }

    [JsonPropertyName("abandons")]
    public int? Abandons { get; set; }

    [JsonPropertyName("benchmarks")]
    public Benchmarks Benchmarks { get; set; }
}

public class Benchmarks
{
    [JsonPropertyName("gold_per_min")]
    public BenchmarkData GoldPerMin { get; set; }

    [JsonPropertyName("xp_per_min")]
    public BenchmarkData XpPerMin { get; set; }

    [JsonPropertyName("kills_per_min")]
    public BenchmarkData KillsPerMin { get; set; }

    [JsonPropertyName("last_hits_per_min")]
    public BenchmarkData LastHitsPerMin { get; set; }

    [JsonPropertyName("hero_damage_per_min")]
    public BenchmarkData HeroDamagePerMin { get; set; }

    [JsonPropertyName("hero_healing_per_min")]
    public BenchmarkData HeroHealingPerMin { get; set; }

    [JsonPropertyName("tower_damage")]
    public BenchmarkData TowerDamage { get; set; }
}

public class BenchmarkData
{
    [JsonPropertyName("raw")]
    public double? Raw { get; set; }

    [JsonPropertyName("pct")]
    public double? Pct { get; set; }
}

public class MatchData
{
    [JsonPropertyName("players")]
    public List<Player> Players { get; set; }

    [JsonPropertyName("radiant_win")]
    public bool? RadiantWin { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("start_time")]
    public long? StartTime { get; set; }

    [JsonPropertyName("match_id")]
    public long? MatchId { get; set; }

    [JsonPropertyName("game_mode")]
    public int? GameMode { get; set; }

    [JsonPropertyName("patch")]
    public int? Patch { get; set; }

    [JsonPropertyName("region")]
    public int? Region { get; set; }

    [JsonPropertyName("radiant_score")]
    public int? RadiantScore { get; set; }

    [JsonPropertyName("dire_score")]
    public int? DireScore { get; set; }

    [JsonPropertyName("picks_bans")]
    public List<PickBan> PicksBans { get; set; }
}

public class PickBan
{
    [JsonPropertyName("is_pick")]
    public bool? IsPick { get; set; }

    [JsonPropertyName("hero_id")]
    public int? HeroId { get; set; }

    [JsonPropertyName("team")]
    public int? Team { get; set; }

    [JsonPropertyName("order")]
    public int? Order { get; set; }
}
public class PlayerData
{
    [JsonPropertyName("account_id")]
    public int? AccountId { get; set; }

    [JsonPropertyName("last_played")]
    public int? LastPlayed { get; set; }

    [JsonPropertyName("win")]
    public int? Win { get; set; }

    [JsonPropertyName("games")]
    public int? Games { get; set; }

    [JsonPropertyName("with_win")]
    public int? WithWin { get; set; }

    [JsonPropertyName("with_games")]
    public int? WithGames { get; set; }

    [JsonPropertyName("against_win")]
    public int? AgainstWin { get; set; }

    [JsonPropertyName("against_games")]
    public int? AgainstGames { get; set; }

    [JsonPropertyName("with_gpm_sum")]
    public int? WithGpmSum { get; set; }

    [JsonPropertyName("with_xpm_sum")]
    public int? WithXpmSum { get; set; }

    [JsonPropertyName("personaname")]
    public string Personaname { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("is_contributor")]
    public bool? IsContributor { get; set; }

    [JsonPropertyName("is_subscriber")]
    public bool? IsSubscriber { get; set; }

    [JsonPropertyName("last_login")]
    public DateTime? LastLogin { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }

    [JsonPropertyName("avatarfull")]
    public string AvatarFull { get; set; }
}

public class PlayerStats
{
    public int? AccountId { get; set; }
    public string PlayerName { get; set; }
    public int Wins { get; set; }
}

