using CozyCinema.Application.RealTime;
using CozyCinema.Application.RealTime.Events;
using CozyCinema.Application.Voting;
using CozyCinema.Application.Voting.Dtos;
using CozyCinema.Application.Voting.Exceptions;
using CozyCinema.Application.Sessions.Exceptions;
using CozyCinema.Domain.Entities;
using CozyCinema.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CozyCinema.Infrastructure.Voting;

public class VotingService(CozyCinemaDbContext dbContext, ISessionEventPublisher eventPublisher) : IVotingService
{
    public async Task<VotingRoundDto> SuggestMovieAsync(Guid userId, Guid sessionId, SuggestMovieRequestDto request, CancellationToken cancellationToken)
    {
        var session = await LoadGrupoSessionAsync(userId, sessionId, cancellationToken);
        var round = await GetOrCreateSelectingRoundAsync(session, cancellationToken);

        if (round.Movies.Any(m => m.SuggestedByUserId == userId))
        {
            var ex = new AlreadySuggestedInRoundException();
            await PublishErrorAsync(session.Id, ex, cancellationToken);
            throw ex;
        }

        var suggester = session.Members.Single(m => m.UserId == userId).User;

        var roundMovie = new RoundMovie
        {
            Id = Guid.NewGuid(),
            VotingRoundId = round.Id,
            VotingRound = round,
            SuggestedByUserId = userId,
            SuggestedByUser = suggester,
            TmdbMovieId = request.TmdbMovieId,
            Title = request.Title.Trim(),
            CoverUrl = request.CoverUrl,
        };

        round.Movies.Add(roundMovie);
        dbContext.RoundMovies.Add(roundMovie);

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.MovieSuggestedAsync(
            new MovieSuggestedEvent(session.Id, roundMovie.Id, roundMovie.TmdbMovieId, roundMovie.Title, roundMovie.CoverUrl),
            cancellationToken);

        return ToDto(round, session, userId);
    }

    public async Task<VotingRoundDto> MarkReadyAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await LoadGrupoSessionAsync(userId, sessionId, cancellationToken);
        var round = await GetOrCreateSelectingRoundAsync(session, cancellationToken);

        var member = session.Members.Single(m => m.UserId == userId);
        member.Status = MemberStatus.ReadyToVote;

        var activeMembers = session.Members.Where(m => m.Status != MemberStatus.Left).ToList();
        var readyCount = activeMembers.Count(m => m.Status == MemberStatus.ReadyToVote);

        RoundMovie? completedWinner = null;
        var startedVoting = false;

        if (activeMembers.Count > 0 && readyCount == activeMembers.Count)
        {
            if (round.Movies.Count == 1)
            {
                completedWinner = round.Movies.Single();
                CompleteRound(session, round, completedWinner, activeMembers);
            }
            else if (round.Movies.Count >= 2)
            {
                round.Status = RoundStatus.Voting;
                startedVoting = true;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.ReadyToVoteAsync(
            new ReadyToVoteEvent(session.Id, userId, readyCount, activeMembers.Count),
            cancellationToken);

        if (startedVoting)
        {
            await eventPublisher.VotingStartedAsync(
                new VotingStartedEvent(
                    session.Id,
                    round.Id,
                    round.Movies.Select(m => new VotingStartedMovieDto(m.Id, m.TmdbMovieId, m.Title, m.CoverUrl)).ToList()),
                cancellationToken);
        }

        if (completedWinner is not null)
        {
            await PublishRoundCompletionAsync(session, round, completedWinner, cancellationToken);
        }

        return ToDto(round, session, userId);
    }

    public async Task<VotingRoundDto> CastVoteAsync(Guid userId, Guid sessionId, CastVoteRequestDto request, CancellationToken cancellationToken)
    {
        var session = await LoadGrupoSessionAsync(userId, sessionId, cancellationToken);

        var round = session.VotingRounds.SingleOrDefault(vr => vr.Status != RoundStatus.Completed);

        if (round is null || round.Status != RoundStatus.Voting)
        {
            var ex = new RoundNotInVotingPhaseException();
            await PublishErrorAsync(session.Id, ex, cancellationToken);
            throw ex;
        }

        var roundMovie = round.Movies.SingleOrDefault(m => m.Id == request.RoundMovieId);

        if (roundMovie is null)
        {
            var ex = new RoundMovieNotFoundException();
            await PublishErrorAsync(session.Id, ex, cancellationToken);
            throw ex;
        }

        if (roundMovie.SuggestedByUserId == userId)
        {
            var ex = new SelfVoteNotAllowedException();
            await PublishErrorAsync(session.Id, ex, cancellationToken);
            throw ex;
        }

        if (round.Movies.SelectMany(m => m.Votes).Any(v => v.UserId == userId))
        {
            var ex = new DuplicateVoteException();
            await PublishErrorAsync(session.Id, ex, cancellationToken);
            throw ex;
        }

        var vote = new Vote
        {
            Id = Guid.NewGuid(),
            RoundMovieId = roundMovie.Id,
            RoundMovie = roundMovie,
            UserId = userId,
            User = session.Members.Single(m => m.UserId == userId).User,
        };

        roundMovie.Votes.Add(vote);
        dbContext.Votes.Add(vote);

        var activeMembers = session.Members.Where(m => m.Status != MemberStatus.Left).ToList();
        var distinctVoterIds = round.Movies.SelectMany(m => m.Votes).Select(v => v.UserId).Distinct().ToList();

        RoundMovie? winner = null;

        if (distinctVoterIds.Count >= activeMembers.Count)
        {
            var maxVotes = round.Movies.Max(m => m.Votes.Count);
            var topMovies = round.Movies.Where(m => m.Votes.Count == maxVotes).ToList();
            winner = topMovies.Count == 1 ? topMovies[0] : topMovies[Random.Shared.Next(topMovies.Count)];

            CompleteRound(session, round, winner, activeMembers);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.VoteSubmittedAsync(
            new VoteSubmittedEvent(session.Id, round.Id, userId, distinctVoterIds.Count, activeMembers.Count),
            cancellationToken);

        if (winner is not null)
        {
            await PublishRoundCompletionAsync(session, round, winner, cancellationToken);
        }

        return ToDto(round, session, userId);
    }

    private async Task PublishRoundCompletionAsync(Session session, VotingRound round, RoundMovie winner, CancellationToken cancellationToken)
    {
        var tally = round.Movies
            .Select(m => new MovieTallyDto(m.Id, m.TmdbMovieId, m.Title, m.CoverUrl, m.Votes.Count))
            .ToList();

        await eventPublisher.VotingCompletedAsync(
            new VotingCompletedEvent(session.Id, round.Id, winner.Id, tally),
            cancellationToken);

        await eventPublisher.MovieSelectedAsync(
            new MovieSelectedEvent(session.Id, winner.TmdbMovieId, winner.Title, winner.SuggestedByUserId, SessionMode.Grupo),
            cancellationToken);
    }

    private Task PublishErrorAsync(Guid sessionId, Exception exception, CancellationToken cancellationToken)
    {
        var code = exception.GetType().Name.Replace("Exception", string.Empty);
        return eventPublisher.SessionErrorAsync(new SessionErrorEvent(sessionId, code, exception.Message), cancellationToken);
    }

    public async Task<VotingRoundDto?> GetCurrentRoundAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await LoadGrupoSessionAsync(userId, sessionId, cancellationToken);

        var round = session.VotingRounds.SingleOrDefault(vr => vr.Status != RoundStatus.Completed)
            ?? session.VotingRounds.OrderByDescending(vr => vr.CreatedAt).FirstOrDefault();

        if (round is null)
        {
            return null;
        }

        // Uma rodada Completed só é relevante para exibição enquanto o filme vencedor ainda for o
        // CurrentMovie da sessão (tela de revelação / marcar como assistido). Depois de marcado como
        // assistido, CurrentMovie* é limpo e a rodada antiga não deve mais "prender" o cliente na tela
        // de revelação — o frontend volta para a seleção, pronta para a próxima sugestão.
        if (round.Status == RoundStatus.Completed && session.CurrentMovieTmdbId is null)
        {
            return null;
        }

        return ToDto(round, session, userId);
    }

    private async Task<Session> LoadGrupoSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await dbContext.Sessions
            .Include(s => s.Members)
            .ThenInclude(m => m.User)
            .Include(s => s.VotingRounds)
            .ThenInclude(vr => vr.Movies)
            .ThenInclude(rm => rm.SuggestedByUser)
            .Include(s => s.VotingRounds)
            .ThenInclude(vr => vr.Movies)
            .ThenInclude(rm => rm.Votes)
            .SingleOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session is null)
        {
            throw new SessionNotFoundException();
        }

        var isMember = session.Members.Any(m => m.UserId == userId && m.Status != MemberStatus.Left);

        if (!isMember)
        {
            throw new NotSessionMemberException();
        }

        if (session.Mode != SessionMode.Grupo)
        {
            throw new InvalidSessionModeException();
        }

        return session;
    }

    private async Task<VotingRound> GetOrCreateSelectingRoundAsync(Session session, CancellationToken cancellationToken)
    {
        var round = session.VotingRounds.SingleOrDefault(vr => vr.Status != RoundStatus.Completed);

        if (round is null)
        {
            round = new VotingRound
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Session = session,
                Status = RoundStatus.Selecting,
                CreatedAt = DateTime.UtcNow,
            };
            session.VotingRounds.Add(round);
            dbContext.VotingRounds.Add(round);
            return round;
        }

        if (round.Status == RoundStatus.Voting)
        {
            var ex = new RoundNotInSelectionPhaseException();
            await PublishErrorAsync(session.Id, ex, cancellationToken);
            throw ex;
        }

        return round;
    }

    private static void CompleteRound(Session session, VotingRound round, RoundMovie winner, List<SessionMember> activeMembers)
    {
        round.WinnerMovieId = winner.Id;
        round.Status = RoundStatus.Completed;

        session.CurrentMovieTmdbId = winner.TmdbMovieId;
        session.CurrentMovieTitle = winner.Title;
        session.CurrentMovieCoverUrl = winner.CoverUrl;
        session.LastPickerUserId = winner.SuggestedByUserId;

        foreach (var member in activeMembers)
        {
            member.Status = MemberStatus.Active;
        }
    }

    private static VotingRoundDto ToDto(VotingRound round, Session session, Guid currentUserId)
    {
        var activeMembers = session.Members.Where(m => m.Status != MemberStatus.Left).ToList();
        var currentMember = session.Members.SingleOrDefault(m => m.UserId == currentUserId);
        var hasVoted = round.Movies.SelectMany(m => m.Votes).Any(v => v.UserId == currentUserId);

        return new VotingRoundDto
        {
            Id = round.Id,
            Status = round.Status,
            Movies = round.Movies.Select(m => ToMovieDto(m, round, currentUserId)).ToList(),
            ReadyCount = activeMembers.Count(m => m.Status == MemberStatus.ReadyToVote),
            TotalActiveMembers = activeMembers.Count,
            IsCurrentUserReady = currentMember?.Status == MemberStatus.ReadyToVote,
            HasCurrentUserVoted = hasVoted,
            WinnerMovieId = round.WinnerMovieId,
        };
    }

    private static RoundMovieDto ToMovieDto(RoundMovie movie, VotingRound round, Guid currentUserId) => new()
    {
        Id = movie.Id,
        TmdbMovieId = movie.TmdbMovieId,
        Title = movie.Title,
        CoverUrl = movie.CoverUrl,
        IsSuggestedByCurrentUser = movie.SuggestedByUserId == currentUserId,
        SuggestedByName = round.Status == RoundStatus.Voting ? null : movie.SuggestedByUser.Name,
        VoteCount = round.Status == RoundStatus.Completed ? movie.Votes.Count : null,
    };
}
