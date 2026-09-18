using CozyCinema.Domain.Enums;

namespace CozyCinema.Application.RealTime.Events;

public record UserJoinedSessionEvent(Guid SessionId, Guid UserId, string UserName);

public record UserLeftSessionEvent(Guid SessionId, Guid UserId);

public record MovieSuggestedEvent(Guid SessionId, Guid RoundMovieId, int TmdbMovieId, string Title, string? CoverUrl);

public record ReadyToVoteEvent(Guid SessionId, Guid UserId, int ReadyCount, int TotalMembers);

public record VotingStartedMovieDto(Guid RoundMovieId, int TmdbMovieId, string Title, string? CoverUrl);

public record VotingStartedEvent(Guid SessionId, Guid RoundId, IReadOnlyList<VotingStartedMovieDto> Movies);

public record VoteSubmittedEvent(Guid SessionId, Guid RoundId, Guid UserId, int VotesCount, int TotalMembers);

public record MovieTallyDto(Guid RoundMovieId, int TmdbMovieId, string Title, string? CoverUrl, int VoteCount);

public record VotingCompletedEvent(Guid SessionId, Guid RoundId, Guid WinnerMovieId, IReadOnlyList<MovieTallyDto> Tally);

public record MovieSelectedEvent(Guid SessionId, int TmdbMovieId, string Title, Guid PickedByUserId, SessionMode Mode);

public record TurnPassedEvent(Guid SessionId, Guid NextPickerUserId);

public record MovieWatchedEvent(Guid SessionId, Guid WatchedMovieId, int TmdbMovieId, string Title, Guid UserId);

public record SessionErrorEvent(Guid SessionId, string Code, string Message);
