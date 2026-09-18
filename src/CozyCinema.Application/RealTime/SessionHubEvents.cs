namespace CozyCinema.Application.RealTime;

public static class SessionHubEvents
{
    public const string UserJoinedSession = nameof(UserJoinedSession);
    public const string UserLeftSession = nameof(UserLeftSession);
    public const string MovieSuggested = nameof(MovieSuggested);
    public const string ReadyToVote = nameof(ReadyToVote);
    public const string VotingStarted = nameof(VotingStarted);
    public const string VoteSubmitted = nameof(VoteSubmitted);
    public const string VotingCompleted = nameof(VotingCompleted);
    public const string MovieSelected = nameof(MovieSelected);
    public const string TurnPassed = nameof(TurnPassed);
    public const string MovieWatched = nameof(MovieWatched);
    public const string SessionError = nameof(SessionError);

    public static string GroupName(Guid sessionId) => $"session:{sessionId}";
}
