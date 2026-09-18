# Modelo de Dados — Cozy Cinema

Diagrama textual das entidades do Entity Framework Core e suas relações, para PostgreSQL (Supabase).

## Diagrama textual (relacionamentos)

```
User ─┬───────────────< SessionMember >───────────────┬─ Session
      │                                                 │
      ├──< Vote                                         ├─ LastPickerUserId (FK → User, nullable)
      │                                                 │
      └──< WatchedMovie (UserId)                        └──< VotingRound >──< RoundMovie >──< Vote
                                                                                     │
                                                          WatchedMovie (SessionId) ──┘ (histórico gerado a partir do vencedor)
```

- `Session` 1—N `SessionMember` N—1 `User` (tabela associativa, um usuário pode participar de várias sessões).
- `Session` 1—N `VotingRound` (Modo Grupo; Modo Casal tipicamente usa 0 ou 1 rodada simplificada, ou nenhuma).
- `VotingRound` 1—N `RoundMovie` (filmes sugeridos naquela rodada).
- `RoundMovie` 1—N `Vote`, `Vote` N—1 `User`.
- `Session` 1—N `WatchedMovie` (histórico); `WatchedMovie` N—1 `User` (quem assistiu, no caso de histórico pessoal fora de uma sessão específica).

## Entidades

### `User`

| Campo | Tipo | Observações |
|---|---|---|
| `Id` | `Guid` | PK. |
| `Name` | `string` | Obrigatório. |
| `Email` | `string` | Obrigatório, único (index unique). |
| `PasswordHash` | `string` | Hash da senha (nunca texto plano). |

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<SessionMember> SessionMemberships { get; set; } = new List<SessionMember>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<WatchedMovie> WatchedMovies { get; set; } = new List<WatchedMovie>();
}
```

### `Session`

| Campo | Tipo | Observações |
|---|---|---|
| `Id` | `Guid` | PK. |
| `Title` | `string` | Nome da sessão (ex: "Sexta à noite"). |
| `IconUrl` | `string?` | Ícone/capa opcional da sessão. |
| `Mode` | `enum SessionMode { Casal, Grupo }` | Define o fluxo (revezamento vs. votação). |
| `LastPickerUserId` | `Guid?` | FK → `User`. Só relevante no Modo Casal; indica quem escolheu por último para calcular o próximo turno. |

```csharp
public enum SessionMode
{
    Casal,
    Grupo
}

public class Session
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public SessionMode Mode { get; set; }

    public Guid? LastPickerUserId { get; set; }
    public User? LastPicker { get; set; }

    public ICollection<SessionMember> Members { get; set; } = new List<SessionMember>();
    public ICollection<VotingRound> VotingRounds { get; set; } = new List<VotingRound>();
    public ICollection<WatchedMovie> WatchedMovies { get; set; } = new List<WatchedMovie>();
}
```

### `SessionMember`

Entidade associativa entre `Session` e `User`, com status de participação (útil para o fluxo de "Pronto para Votar" e para saber quem está ativo na sessão).

| Campo | Tipo | Observações |
|---|---|---|
| `SessionId` | `Guid` | FK → `Session`. Parte da PK composta. |
| `UserId` | `Guid` | FK → `User`. Parte da PK composta. |
| `Status` | `enum MemberStatus { Active, ReadyToVote, Left }` | Estado do membro dentro da sessão/rodada atual. |

```csharp
public enum MemberStatus
{
    Active,
    ReadyToVote,
    Left
}

public class SessionMember
{
    public Guid SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public MemberStatus Status { get; set; } = MemberStatus.Active;
}
```

> Chave primária composta `(SessionId, UserId)` configurada via Fluent API (`HasKey(sm => new { sm.SessionId, sm.UserId })`).

### `VotingRound`

Representa uma rodada de votação do Modo Grupo (Etapas 1–3: Seleção, Prontidão, Votação).

| Campo | Tipo | Observações |
|---|---|---|
| `Id` | `Guid` | PK. |
| `SessionId` | `Guid` | FK → `Session`. |
| `Status` | `enum RoundStatus { Selecting, ReadyCheck, Voting, Completed }` | Fase atual da rodada. |
| `WinnerMovieId` | `Guid?` | FK → `RoundMovie`. Preenchido quando `Status = Completed`. |

```csharp
public enum RoundStatus
{
    Selecting,
    ReadyCheck,
    Voting,
    Completed
}

public class VotingRound
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public RoundStatus Status { get; set; } = RoundStatus.Selecting;

    public Guid? WinnerMovieId { get; set; }
    public RoundMovie? WinnerMovie { get; set; }

    public ICollection<RoundMovie> Movies { get; set; } = new List<RoundMovie>();
}
```

### `RoundMovie`

Um filme sugerido dentro de uma `VotingRound`.

| Campo | Tipo | Observações |
|---|---|---|
| `Id` | `Guid` | PK. |
| `VotingRoundId` | `Guid` | FK → `VotingRound`. |
| `SuggestedByUserId` | `Guid` | FK → `User`. Usado apenas no backend para a regra de auto-voto; **nunca exposto ao frontend durante a votação cega**. |
| `TmdbMovieId` | `int` | Id do filme na TMDB. |
| `Title` | `string` | Cache do título no momento da sugestão. |
| `CoverUrl` | `string?` | Cache da capa. |

```csharp
public class RoundMovie
{
    public Guid Id { get; set; }
    public Guid VotingRoundId { get; set; }
    public VotingRound VotingRound { get; set; } = null!;

    public Guid SuggestedByUserId { get; set; }
    public User SuggestedByUser { get; set; } = null!;

    public int TmdbMovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
```

### `Vote`

| Campo | Tipo | Observações |
|---|---|---|
| `Id` | `Guid` | PK. |
| `RoundMovieId` | `Guid` | FK → `RoundMovie`. |
| `UserId` | `Guid` | FK → `User`. |

**Regra de Ouro (auto-voto):** um `User` não pode votar em um `RoundMovie` cujo `SuggestedByUserId` seja o próprio `UserId`. Validada no Service antes de persistir o voto (checagem explícita comparando `RoundMovie.SuggestedByUserId == vote.UserId`) — não depende de constraint de banco, pois requer join entre `Vote` e `RoundMovie`. Combinada com um índice único `(RoundMovieId, UserId)` para impedir voto duplicado no mesmo filme.

```csharp
public class Vote
{
    public Guid Id { get; set; }

    public Guid RoundMovieId { get; set; }
    public RoundMovie RoundMovie { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
```

> Fluent API: `HasIndex(v => new { v.RoundMovieId, v.UserId }).IsUnique();`

### `WatchedMovie`

Registro de histórico (Movie Log), tanto por sessão quanto agregável por usuário.

| Campo | Tipo | Observações |
|---|---|---|
| `Id` | `Guid` | PK. |
| `SessionId` | `Guid` | FK → `Session`. |
| `UserId` | `Guid` | FK → `User`. Permite consultas de histórico "geral" por usuário além do histórico por sessão. |
| `TmdbMovieId` | `int` | Id do filme na TMDB (permite re-consultar dados atualizados se necessário). |
| `Title` | `string` | Cache do título. |
| `CoverUrl` | `string?` | Cache da capa. |
| `Rating` | `decimal` | Nota TMDB no momento em que foi assistido. |
| `Genres` | `string` | Lista de gêneros serializada (ex: CSV ou JSON), usada para filtro. |
| `Director` | `string?` | Diretor, usado para filtro. |
| `WatchedAt` | `DateTime` | Data em que foi assistido. |

```csharp
public class WatchedMovie
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int TmdbMovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public decimal Rating { get; set; }
    public string Genres { get; set; } = string.Empty;
    public string? Director { get; set; }
    public DateTime WatchedAt { get; set; }
}
```

## Notas de indexação e integridade

- Índice único em `User.Email`.
- Chave composta em `SessionMember (SessionId, UserId)`.
- Índice único em `Vote (RoundMovieId, UserId)` para impedir voto duplicado.
- `Session.LastPickerUserId` é `nullable` com `OnDelete(DeleteBehavior.SetNull)` — a sessão não deve ser destruída se o usuário referenciado for removido.
- `VotingRound.WinnerMovieId` só é preenchido em `Status = Completed`; validar essa invariante no Service, não via constraint de banco.
