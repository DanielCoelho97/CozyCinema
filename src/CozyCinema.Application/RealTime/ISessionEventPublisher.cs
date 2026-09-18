using CozyCinema.Application.RealTime.Events;

namespace CozyCinema.Application.RealTime;

public interface ISessionEventPublisher
{
    Task UserJoinedSessionAsync(UserJoinedSessionEvent evt, CancellationToken cancellationToken);

    Task UserLeftSessionAsync(UserLeftSessionEvent evt, CancellationToken cancellationToken);

    Task MovieSuggestedAsync(MovieSuggestedEvent evt, CancellationToken cancellationToken);

    Task ReadyToVoteAsync(ReadyToVoteEvent evt, CancellationToken cancellationToken);

    Task VotingStartedAsync(VotingStartedEvent evt, CancellationToken cancellationToken);

    Task VoteSubmittedAsync(VoteSubmittedEvent evt, CancellationToken cancellationToken);

    Task VotingCompletedAsync(VotingCompletedEvent evt, CancellationToken cancellationToken);

    Task MovieSelectedAsync(MovieSelectedEvent evt, CancellationToken cancellationToken);

    Task TurnPassedAsync(TurnPassedEvent evt, CancellationToken cancellationToken);

    Task MovieWatchedAsync(MovieWatchedEvent evt, CancellationToken cancellationToken);

    Task SessionErrorAsync(SessionErrorEvent evt, CancellationToken cancellationToken);
}
