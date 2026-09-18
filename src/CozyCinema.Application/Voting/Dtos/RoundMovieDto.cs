namespace CozyCinema.Application.Voting.Dtos;

public class RoundMovieDto
{
    public Guid Id { get; set; }
    public int TmdbMovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }

    /// <summary>Calculado a partir do usuário chamador — nunca serializa o SuggestedByUserId real de terceiros.</summary>
    public bool IsSuggestedByCurrentUser { get; set; }

    /// <summary>Só preenchido fora da fase de votação cega (Selecting/Completed); sempre null durante Voting.</summary>
    public string? SuggestedByName { get; set; }

    /// <summary>Só preenchido após a apuração (Status == Completed).</summary>
    public int? VoteCount { get; set; }
}
