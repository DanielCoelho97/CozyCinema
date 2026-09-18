namespace CozyCinema.Application.Voting.Exceptions;

public class SelfVoteNotAllowedException()
    : Exception("Você não pode votar em um filme que você mesmo sugeriu.");
