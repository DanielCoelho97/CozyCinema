namespace CozyCinema.Application.Voting.Exceptions;

public class RoundNotInSelectionPhaseException()
    : Exception("Não é possível sugerir filmes ou sinalizar prontidão: a rodada já está em votação.");
