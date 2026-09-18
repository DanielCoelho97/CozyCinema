using CozyCinema.Api.Hubs;
using CozyCinema.Application.RealTime;
using CozyCinema.Application.RealTime.Events;
using Microsoft.AspNetCore.SignalR;

namespace CozyCinema.Api.RealTime;

public class SignalRSessionEventPublisher(IHubContext<CinemaSessionHub> hubContext) : ISessionEventPublisher
{
    public Task UserJoinedSessionAsync(UserJoinedSessionEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.UserJoinedSession, evt, cancellationToken);

    public Task UserLeftSessionAsync(UserLeftSessionEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.UserLeftSession, evt, cancellationToken);

    public Task MovieSuggestedAsync(MovieSuggestedEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.MovieSuggested, evt, cancellationToken);

    public Task ReadyToVoteAsync(ReadyToVoteEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.ReadyToVote, evt, cancellationToken);

    public Task VotingStartedAsync(VotingStartedEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.VotingStarted, evt, cancellationToken);

    public Task VoteSubmittedAsync(VoteSubmittedEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.VoteSubmitted, evt, cancellationToken);

    public Task VotingCompletedAsync(VotingCompletedEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.VotingCompleted, evt, cancellationToken);

    public Task MovieSelectedAsync(MovieSelectedEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.MovieSelected, evt, cancellationToken);

    public Task TurnPassedAsync(TurnPassedEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.TurnPassed, evt, cancellationToken);

    public Task MovieWatchedAsync(MovieWatchedEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.MovieWatched, evt, cancellationToken);

    public Task SessionErrorAsync(SessionErrorEvent evt, CancellationToken cancellationToken) =>
        Group(evt.SessionId).SendAsync(SessionHubEvents.SessionError, evt, cancellationToken);

    private IClientProxy Group(Guid sessionId) => hubContext.Clients.Group(SessionHubEvents.GroupName(sessionId));
}
