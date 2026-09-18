using CozyCinema.Application.RealTime;
using CozyCinema.Application.RealTime.Events;
using CozyCinema.Application.Sessions;
using CozyCinema.Application.Sessions.Dtos;
using CozyCinema.Application.Sessions.Exceptions;
using CozyCinema.Domain.Entities;
using CozyCinema.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CozyCinema.Infrastructure.Sessions;

public class SessionService(CozyCinemaDbContext dbContext, ISessionEventPublisher eventPublisher) : ISessionService
{
    private const int InviteCodeLength = 6;
    private const string InviteCodeAlphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    public async Task<SessionDto> CreateSessionAsync(Guid userId, CreateSessionRequestDto request, CancellationToken cancellationToken)
    {
        var creator = await dbContext.Users.SingleAsync(u => u.Id == userId, cancellationToken);

        var session = new Session
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            IconUrl = request.IconUrl,
            Mode = request.Mode,
            InviteCode = await GenerateUniqueInviteCodeAsync(cancellationToken),
        };

        session.Members.Add(new SessionMember
        {
            SessionId = session.Id,
            Session = session,
            UserId = creator.Id,
            User = creator,
            Status = MemberStatus.Active,
        });

        dbContext.Sessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(session);
    }

    public async Task<IReadOnlyList<SessionSummaryDto>> GetMySessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await dbContext.Sessions
            .Include(s => s.Members)
            .Where(s => s.Members.Any(m => m.UserId == userId && m.Status != MemberStatus.Left))
            .ToListAsync(cancellationToken);

        return sessions
            .Select(s => new SessionSummaryDto
            {
                Id = s.Id,
                Title = s.Title,
                IconUrl = s.IconUrl,
                Mode = s.Mode,
                InviteCode = s.InviteCode,
                ActiveMemberCount = s.Members.Count(m => m.Status != MemberStatus.Left),
            })
            .ToList();
    }

    public async Task<SessionDto> JoinSessionAsync(Guid userId, JoinSessionRequestDto request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.InviteCode.Trim().ToUpperInvariant();

        var session = await dbContext.Sessions
            .Include(s => s.Members)
            .ThenInclude(m => m.User)
            .SingleOrDefaultAsync(s => s.InviteCode == normalizedCode, cancellationToken);

        if (session is null)
        {
            throw new SessionNotFoundException();
        }

        var existingMembership = session.Members.SingleOrDefault(m => m.UserId == userId);

        if (existingMembership is not null)
        {
            if (existingMembership.Status == MemberStatus.Left)
            {
                existingMembership.Status = MemberStatus.Active;
            }
        }
        else
        {
            var activeMemberCount = session.Members.Count(m => m.Status != MemberStatus.Left);

            if (session.Mode == SessionMode.Casal && activeMemberCount >= 2)
            {
                throw new SessionFullException();
            }

            var joiningUser = await dbContext.Users.SingleAsync(u => u.Id == userId, cancellationToken);

            session.Members.Add(new SessionMember
            {
                SessionId = session.Id,
                Session = session,
                UserId = joiningUser.Id,
                User = joiningUser,
                Status = MemberStatus.Active,
            });
        }

        var initialPickerAssigned = AssignInitialPickerIfReady(session);

        await dbContext.SaveChangesAsync(cancellationToken);

        var joinedMember = session.Members.Single(m => m.UserId == userId);
        await eventPublisher.UserJoinedSessionAsync(
            new UserJoinedSessionEvent(session.Id, userId, joinedMember.User.Name),
            cancellationToken);

        if (initialPickerAssigned)
        {
            var nextPickerUserId = CalculateNextPickerUserId(session);

            if (nextPickerUserId is not null)
            {
                await eventPublisher.TurnPassedAsync(
                    new TurnPassedEvent(session.Id, nextPickerUserId.Value),
                    cancellationToken);
            }
        }

        return ToDto(session);
    }

    public async Task LeaveSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await LoadSessionForMemberAsync(userId, sessionId, cancellationToken);

        var member = session.Members.Single(m => m.UserId == userId);
        member.Status = MemberStatus.Left;

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.UserLeftSessionAsync(new UserLeftSessionEvent(session.Id, userId), cancellationToken);
    }

    public async Task<SessionDto> GetSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await LoadSessionForMemberAsync(userId, sessionId, cancellationToken);

        return ToDto(session);
    }

    public async Task<SessionDto> SelectMovieAsync(Guid userId, Guid sessionId, SelectMovieRequestDto request, CancellationToken cancellationToken)
    {
        var session = await LoadSessionForMemberAsync(userId, sessionId, cancellationToken);

        if (session.Mode != SessionMode.Casal)
        {
            throw new InvalidSessionModeException();
        }

        var activeMembers = session.Members.Where(m => m.Status != MemberStatus.Left).ToList();

        if (activeMembers.Count != 2)
        {
            var notReadyException = new SessionNotReadyException();
            await PublishErrorAsync(session.Id, notReadyException, cancellationToken);
            throw notReadyException;
        }

        var currentPickerUserId = CalculateNextPickerUserId(session);

        if (currentPickerUserId != userId)
        {
            var notYourTurnException = new NotYourTurnException();
            await PublishErrorAsync(session.Id, notYourTurnException, cancellationToken);
            throw notYourTurnException;
        }

        if (session.CurrentMovieTmdbId is not null)
        {
            var previousMovieNotWatchedException = new PreviousMovieNotWatchedException();
            await PublishErrorAsync(session.Id, previousMovieNotWatchedException, cancellationToken);
            throw previousMovieNotWatchedException;
        }

        session.CurrentMovieTmdbId = request.TmdbMovieId;
        session.CurrentMovieTitle = request.Title.Trim();
        session.CurrentMovieCoverUrl = request.CoverUrl;
        session.LastPickerUserId = userId;

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.MovieSelectedAsync(
            new MovieSelectedEvent(session.Id, session.CurrentMovieTmdbId.Value, session.CurrentMovieTitle!, userId, SessionMode.Casal),
            cancellationToken);

        var nextPickerUserId = CalculateNextPickerUserId(session);

        if (nextPickerUserId is not null)
        {
            await eventPublisher.TurnPassedAsync(new TurnPassedEvent(session.Id, nextPickerUserId.Value), cancellationToken);
        }

        return ToDto(session);
    }

    private Task PublishErrorAsync(Guid sessionId, Exception exception, CancellationToken cancellationToken)
    {
        var code = exception.GetType().Name.Replace("Exception", string.Empty);
        return eventPublisher.SessionErrorAsync(new SessionErrorEvent(sessionId, code, exception.Message), cancellationToken);
    }

    private async Task<Session> LoadSessionForMemberAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await dbContext.Sessions
            .Include(s => s.Members)
            .ThenInclude(m => m.User)
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

        return session;
    }

    private static bool AssignInitialPickerIfReady(Session session)
    {
        if (session.Mode != SessionMode.Casal || session.LastPickerUserId is not null)
        {
            return false;
        }

        var activeMembers = session.Members.Where(m => m.Status != MemberStatus.Left).ToList();

        if (activeMembers.Count != 2)
        {
            return false;
        }

        var memberWhoGoesSecond = activeMembers[Random.Shared.Next(activeMembers.Count)];
        session.LastPickerUserId = memberWhoGoesSecond.UserId;
        return true;
    }

    private static Guid? CalculateNextPickerUserId(Session session)
    {
        if (session.Mode != SessionMode.Casal || session.LastPickerUserId is null)
        {
            return null;
        }

        var activeMembers = session.Members.Where(m => m.Status != MemberStatus.Left).ToList();

        if (activeMembers.Count != 2)
        {
            return null;
        }

        return activeMembers.SingleOrDefault(m => m.UserId != session.LastPickerUserId)?.UserId;
    }

    private async Task<string> GenerateUniqueInviteCodeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var code = GenerateInviteCode();
            var codeInUse = await dbContext.Sessions.AnyAsync(s => s.InviteCode == code, cancellationToken);

            if (!codeInUse)
            {
                return code;
            }
        }
    }

    private static string GenerateInviteCode()
    {
        var chars = new char[InviteCodeLength];

        for (var i = 0; i < InviteCodeLength; i++)
        {
            chars[i] = InviteCodeAlphabet[Random.Shared.Next(InviteCodeAlphabet.Length)];
        }

        return new string(chars);
    }

    private static SessionDto ToDto(Session session) => new()
    {
        Id = session.Id,
        Title = session.Title,
        IconUrl = session.IconUrl,
        Mode = session.Mode,
        InviteCode = session.InviteCode,
        LastPickerUserId = session.LastPickerUserId,
        NextPickerUserId = CalculateNextPickerUserId(session),
        CurrentMovie = session.CurrentMovieTmdbId is null
            ? null
            : new SessionCurrentMovieDto
            {
                TmdbMovieId = session.CurrentMovieTmdbId.Value,
                Title = session.CurrentMovieTitle ?? string.Empty,
                CoverUrl = session.CurrentMovieCoverUrl,
                PickedByUserId = session.LastPickerUserId!.Value,
            },
        Members = session.Members
            .Where(m => m.Status != MemberStatus.Left)
            .Select(m => new SessionMemberDto
            {
                UserId = m.UserId,
                Name = m.User.Name,
                Status = m.Status,
            })
            .ToList(),
    };
}
