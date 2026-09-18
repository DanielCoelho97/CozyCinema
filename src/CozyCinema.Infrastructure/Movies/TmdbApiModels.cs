using System.Text.Json.Serialization;

namespace CozyCinema.Infrastructure.Movies;

internal sealed class TmdbSearchResponse
{
    [JsonPropertyName("results")]
    public List<TmdbMovieSummary> Results { get; set; } = [];
}

internal sealed class TmdbMovieSummary
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("vote_average")]
    public decimal VoteAverage { get; set; }
}

internal sealed class TmdbMovieDetail
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("overview")]
    public string? Overview { get; set; }

    [JsonPropertyName("vote_average")]
    public decimal VoteAverage { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("genres")]
    public List<TmdbGenre> Genres { get; set; } = [];

    [JsonPropertyName("credits")]
    public TmdbCredits? Credits { get; set; }
}

internal sealed class TmdbGenre
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

internal sealed class TmdbCredits
{
    [JsonPropertyName("crew")]
    public List<TmdbCrewMember> Crew { get; set; } = [];
}

internal sealed class TmdbCrewMember
{
    [JsonPropertyName("job")]
    public string Job { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
