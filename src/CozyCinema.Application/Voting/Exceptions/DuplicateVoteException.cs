namespace CozyCinema.Application.Voting.Exceptions;

public class DuplicateVoteException()
    : Exception("Você já votou nesta rodada.");
