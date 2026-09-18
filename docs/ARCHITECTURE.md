# Arquitetura — Cozy Cinema

## Visão geral

Arquitetura em camadas clássica, com backend .NET como única fonte de verdade e orquestrador de estado; o frontend React é um cliente "burro" que reage a eventos em tempo real.

```
┌─────────────────────────┐
│   React (TypeScript)    │
│   Tailwind + Framer     │
│   Motion — Mobile-First │
└───────────┬──────────────┘
            │ HTTPS (REST)         │ WebSocket (SignalR)
            ▼                      ▼
┌─────────────────────────────────────────────┐
│           ASP.NET Core Web API               │
│  ┌─────────────┐   ┌────────────────────┐   │
│  │ Controllers │──▶│      Services        │   │
│  │  (magros)   │   │  (regras de negócio) │   │
│  └─────────────┘   └─────────┬────────────┘   │
│                     ┌─────────┴────────────┐   │
│                     │  CinemaSessionHub     │   │
│                     │  (SignalR)            │   │
│                     └─────────┬────────────┘   │
│  ┌──────────────────┐  ┌──────┴───────────┐   │
│  │ TmdbService        │  │ EF Core DbContext │   │
│  │ (+ MemoryCache)    │  │                    │   │
│  └─────────┬──────────┘  └──────┬────────────┘   │
└────────────┼─────────────────────┼─────────────────┘
             ▼                     ▼
     ┌───────────────┐   ┌─────────────────────┐
     │  TMDB API       │   │ PostgreSQL (Supabase)│
     │  (externa)      │   │                       │
     └───────────────┘   └─────────────────────┘
```

### Camadas

- **Frontend (React):** UI mobile-first. Consome a API REST para operações pontuais (criar sessão, buscar filme, registrar histórico) e mantém uma conexão SignalR por sessão ativa para refletir mudanças de estado em tempo real.
- **Controllers:** camada fina de entrada HTTP. Validam o DTO de request (model binding + validação) e delegam para o Service correspondente. Não contêm regra de negócio.
- **Services:** contêm toda a lógica de domínio — validação de auto-voto, apuração de votos, cálculo de próximo `LastPickerUserId`, orquestração de transição de fase (Seleção → Prontidão → Votação). São os únicos consumidores do `DbContext` e responsáveis por publicar eventos no `CinemaSessionHub` quando o estado de uma sessão muda.
- **CinemaSessionHub (SignalR):** canal de push de eventos de estado para todos os clientes conectados a uma sessão (grupo SignalR = `session:{sessionId}`).
- **TmdbService:** encapsula toda comunicação com a TMDB API. Único ponto de acesso à TMDB no sistema. Usa `IMemoryCache` para evitar chamadas repetidas de busca/detalhe de filme.
- **Infrastructure / EF Core:** `DbContext`, mapeamentos de entidade e migrations. Conecta ao PostgreSQL hospedado no Supabase via `Npgsql.EntityFrameworkCore.PostgreSQL`.

## CinemaSessionHub — Especificação de eventos SignalR

Grupo de conexão: cada cliente entra no grupo `session:{sessionId}` ao abrir uma sessão. Todos os eventos abaixo são broadcast apenas para o grupo da sessão correspondente, nunca globalmente.

| Evento | Direção | Payload (resumo) | Disparado quando |
|---|---|---|---|
| `UserJoinedSession` | Server → Clients | `{ sessionId, userId, userName }` | Um usuário entra na sessão (Casal ou Grupo). |
| `UserLeftSession` | Server → Clients | `{ sessionId, userId }` | Um usuário sai/desconecta da sessão. |
| `MovieSuggested` | Server → Clients | `{ sessionId, roundMovieId, tmdbMovieId, title, coverUrl }` | (Modo Grupo, Etapa 1) Um participante sugere um filme para a rodada. |
| `ReadyToVote` | Server → Clients | `{ sessionId, userId, readyCount, totalMembers }` | (Modo Grupo, Etapa 2) Um participante sinaliza "Pronto para Votar". |
| `VotingStarted` | Server → Clients | `{ sessionId, roundId, movies: [...] }` | (Modo Grupo) 100% dos membros ficaram prontos; backend transiciona a rodada para Votação e envia os cards sem indicar quem sugeriu. |
| `VoteSubmitted` | Server → Clients | `{ sessionId, roundId, userId, votesCount, totalMembers }` | (Modo Grupo, Etapa 3) Um voto é registrado. Payload não revela em qual filme o usuário votou (apenas contagem de participação). |
| `VotingCompleted` | Server → Clients | `{ sessionId, roundId, winnerMovieId, tally: [...] }` | Todos os membros votaram; backend apura o resultado e anuncia o filme vencedor da rodada. |
| `MovieSelected` | Server → Clients | `{ sessionId, tmdbMovieId, title, pickedByUserId, mode }` | Um filme foi definido para a sessão — seja pela escolha direta do Modo Casal, seja pelo resultado do `VotingCompleted` no Modo Grupo. |
| `TurnPassed` | Server → Clients | `{ sessionId, nextPickerUserId }` | (Modo Casal) Após a escolha, o backend atualiza `LastPickerUserId` e informa de quem é a próxima vez. |
| `MovieWatched` | Server → Clients | `{ sessionId, watchedMovieId, tmdbMovieId, title, userId }` | Um membro marca o filme atual da sessão como assistido (Movie Log); o backend limpa `Session.CurrentMovie*` e avisa os demais membros. Não é disparado ao registrar um filme manualmente no histórico (não altera estado compartilhado da sessão). |
| `SessionError` | Server → Clients | `{ sessionId, code, message }` | Uma operação falha no servidor (ex: tentativa de auto-voto, ação fora de ordem de fase) e precisa ser exibida ao cliente. |

Todos os eventos são disparados pelos Services (nunca diretamente pelos Controllers), garantindo que qualquer alteração de estado feita via API REST também seja refletida em tempo real a todos os membros conectados.

## Estratégia de integração com a TMDB API

- **Service Layer:** `ITmdbService` / `TmdbService` é a única classe autorizada a chamar a TMDB API. Controllers de busca de filme (`GET /api/movies/search`, `GET /api/movies/{tmdbId}`) delegam integralmente para esse service.
- **Configuração:** API key da TMDB via configuração/secret (`appsettings`/variáveis de ambiente), nunca hardcoded, nunca exposta ao frontend.
- **Caching:** respostas de busca e de detalhe de filme são cacheadas via `IMemoryCache`, com chave composta por endpoint + parâmetros (ex: `tmdb:search:{query}:{page}`, `tmdb:movie:{tmdbId}`) e expiração deslizante curta (minutos) para reduzir latência e respeitar rate limits da TMDB sem servir dados fortemente desatualizados.
- **Resiliência:** chamadas HTTP à TMDB via `HttpClientFactory` (client nomeado `TmdbClient`), com timeout configurado. Falhas da TMDB retornam um erro tratado ao frontend (não derrubam a sessão).
- **Fronteira de dados:** apenas os campos necessários da resposta da TMDB (título, capa, sinopse, nota, gêneros, diretor, data) são mapeados para os DTOs internos antes de trafegar para o frontend — o payload bruto da TMDB não é repassado diretamente.
