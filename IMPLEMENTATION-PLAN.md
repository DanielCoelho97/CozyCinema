# Plano de Implementação — Cozy Cinema

Plano de execução do MVP em fases numeradas. Marque `[x]` conforme cada etapa for concluída. Este plano segue o escopo e as decisões registradas em `CLAUDE.md`, `docs/ARCHITECTURE.md`, `docs/DATA-MODEL.md` e `docs/UI-DESIGN.md`.

---

## Fase 0 — Scaffolding do repositório

- [x] Criar solution .NET (`CozyCinema.sln`) com projetos `src/CozyCinema.Api`, `src/CozyCinema.Application` (Services), `src/CozyCinema.Infrastructure` (EF Core/DbContext), `src/CozyCinema.Domain` (entidades)
- [x] Criar projeto React + TypeScript em `/frontend` (Vite), com Tailwind CSS e Framer Motion instalados
- [x] Configurar `tailwind.config` com os tokens de cor/sombra definidos em `docs/UI-DESIGN.md`
- [x] Configurar `appsettings.Development.json` / variáveis de ambiente (connection string Supabase, TMDB API key) fora do controle de versão
- [x] Configurar CORS na API para o dev server do frontend
- [x] Subir "Hello World" nos dois lados (endpoint de health check + tela inicial) para validar que o scaffolding builda e roda

## Fase 1 — Fundação de dados (Backend)

- [x] Implementar entidades do EF Core conforme `docs/DATA-MODEL.md` (`User`, `Session`, `SessionMember`, `VotingRound`, `RoundMovie`, `Vote`, `WatchedMovie`)
- [x] Configurar `DbContext` com Fluent API (chaves compostas, índices únicos, `DeleteBehavior`)
- [x] Conectar ao PostgreSQL do Supabase via `Npgsql.EntityFrameworkCore.PostgreSQL`
- [x] Gerar migration inicial (`dotnet ef migrations add InitialCreate`)
- [x] Aplicar a migration inicial (`dotnet ef database update`)
- [x] Validar schema criado diretamente no Supabase

## Fase 2 — Autenticação simples

- [x] Endpoint de registro (`POST /api/auth/register`) com DTO de request/response e hash de senha
- [x] Endpoint de login (`POST /api/auth/login`) retornando token (JWT)
- [x] Middleware/autorização JWT na API (`[Authorize]` nos controllers que exigem usuário autenticado)
- [x] Fluxo de login/registro no frontend (telas + hook `useAuth`) com persistência do token

> **Pendência:** smoke test manual end-to-end (subir a API localmente e validar `register` → `login` → `GET /api/auth/me` com token, e o fluxo de UI de login/registro no frontend). Ainda não executado nesta sessão — só validado via `dotnet build`, `npm run build` e `npm run lint`.

## Fase 3 — Sessões e Modo Casal (revezamento por turnos)

- [x] Endpoints de sessão: criar sessão, entrar em sessão (`SessionService`)
- [x] Regra de cálculo do próximo `LastPickerUserId` no `SessionService` (nunca no frontend)
- [x] Endpoint para registrar a escolha do filme da vez (Modo Casal) e persistir `LastPickerUserId`
- [x] Telas do Modo Casal: tela de sessão, indicador "é a vez de X", tela de busca/escolha de filme
- [ ] Integração desses fluxos com eventos SignalR (`TurnPassed`, `MovieSelected`) — ver Fase 5

> **Decisões de schema tomadas nesta fase** (não previstas originalmente no `docs/DATA-MODEL.md`): `Session.InviteCode` (código curto de convite, genérico para Casal e Grupo — resolve entrada na sessão para os dois modos) e `Session.CurrentMovieTmdbId`/`CurrentMovieTitle`/`CurrentMovieCoverUrl` (o filme selecionado precisa sobreviver a um refresh de página antes de ser confirmado como assistido na Fase 7). O primeiro turno do Modo Casal é sorteado aleatoriamente quando o 2º membro entra na sessão, e persistido em `LastPickerUserId` (que passa a ter dupla função: "quem escolheu por último" e "quem escolheu o filme atual").
>
> **Pendência:** a tela de escolha de filme usa um formulário manual temporário (TMDB ID, título, URL da capa) — será substituído pelo componente de busca real na Fase 6 (TMDB). Smoke test do fluxo completo (registrar 2 usuários → criar sessão → entrar com invite code → sorteio do primeiro turno → bloqueio de escolha fora da vez → escolha do filme → troca de turno → persistência após refresh) validado via `curl` ponta a ponta contra a API local. A verificação visual da UI no navegador não foi executada nesta sessão (usuário optou por não instalar a extensão Claude in Chrome) — `dotnet run --project src/CozyCinema.Api` e `npm run dev` (frontend) ficaram rodando ao final desta sessão para o usuário validar manualmente em http://localhost:5173.

## Fase 4 — Modo Grupo (votação cega)

- [x] Etapa 1 (Seleção): endpoint para sugerir filme na rodada (`RoundMovie`) — usa o mesmo formulário manual temporário do Modo Casal (TMDB ID/título/capa); a busca real via TMDB chega na Fase 6
- [x] Etapa 2 (Prontidão): endpoint "Pronto para Votar" (`SessionMember.Status = ReadyToVote`) e checagem de 100% dos membros no backend
- [x] Transição automática Prontidão → Votação no backend quando todos estiverem prontos
- [x] Etapa 3 (Votação cega): endpoint de voto (`VotingService`) com **bloqueio obrigatório de auto-voto no backend**
- [x] Apuração do resultado e definição do `WinnerMovieId`
- [x] Telas do Modo Grupo: sugestão de filme, tela de espera de prontidão, grid de votação cega, tela de revelação do vencedor

> **Decisões de regra de negócio tomadas nesta fase** (não previstas originalmente no `docs/DATA-MODEL.md`/`docs/ARCHITECTURE.md`):
> - **100% prontos + exatamente 1 filme sugerido:** pula a votação inteira — esse filme já é o vencedor direto (nunca passa por `Status = Voting`).
> - **100% prontos + 0 filmes sugeridos:** a rodada **não** transiciona para votação e permanece em `Selecting` (sem sorteio/fallback — evitamos inventar um "sugestor" de sistema, já que `RoundMovie.SuggestedByUserId` não é anulável, e evitamos depender da TMDB antes da Fase 6). O sinal de "pronto" já fica registrado; a rodada só avança quando alguém sugere ao menos 1 filme e o grupo confirma prontidão de novo.
> - **Empate na apuração:** sorteio aleatório (`Random.Shared`) entre os filmes empatados.
> - **Nova rodada após `Completed`:** automática — a primeira sugestão ou sinal de "pronto" depois que a última rodada da sessão fechou cria uma `VotingRound` nova (`VotingRound.CreatedAt`, adicionado nesta fase, é o que permite ao backend identificar a rodada "atual" quando existem várias `Completed` acumuladas). Não existe endpoint dedicado de "jogar de novo".
> - **`Session.LastPickerUserId`** passa a ser populado também no Modo Grupo ao fechar uma rodada, com o significado de "quem sugeriu o filme vencedor" (`RoundMovie.SuggestedByUserId` do vencedor) — necessário porque `SessionDto.CurrentMovie.PickedByUserId` é não-anulável e é preenchido sempre que `Session.CurrentMovie*` está populado.
> - **`RoundStatus.ReadyCheck`** (enum já existente em `docs/DATA-MODEL.md`) não é usado como estado persistido — "quantos já estão prontos" é dado calculado (`ReadyCount`/`TotalActiveMembers`) exposto no DTO da rodada, não uma transição de `Status` própria. A transição é sempre `Selecting → Voting` ou `Selecting → Completed`.
> - **"Marcar filme como assistido"** (Fase 7, ainda não implementada em nenhum modo): decisão pré-registrada de que qualquer membro ativo poderá marcar, sem conceito de moderador/dono de sessão — `Session`/`SessionMember` não têm e não ganharão esse conceito.
>
> **Pendência:** mesma observação da Fase 3 — a tela de sugestão de filme usa o formulário manual temporário, substituído na Fase 6. Smoke test do fluxo completo (0 filmes bloqueando votação, 1 filme pulando votação, fluxo normal com auto-voto/duplicidade/apuração, nova rodada automática, empate) validado via `curl` ponta a ponta contra a API local, cobrindo os 3 casos de sessão de Grupo. A verificação visual da UI no navegador não foi executada nesta sessão (usuário optou novamente por não instalar a extensão Claude in Chrome) — `dotnet run --project src/CozyCinema.Api --launch-profile https` e `npm run dev` (frontend) ficaram rodando ao final desta sessão para validação manual em http://localhost:5173.

## Fase 5 — Tempo real (SignalR)

- [x] Implementar `CinemaSessionHub` com grupos por sessão (`session:{sessionId}`)
- [x] Implementar todos os eventos definidos em `docs/ARCHITECTURE.md` (`UserJoinedSession`, `UserLeftSession`, `MovieSuggested`, `ReadyToVote`, `VotingStarted`, `VoteSubmitted`, `VotingCompleted`, `MovieSelected`, `TurnPassed`, `SessionError`)
- [x] Garantir que todo evento seja disparado a partir dos Services (nunca dos Controllers)
- [x] `RealtimeProvider`/`useRealtime` no frontend para conectar/desconectar do hub e expor a conexão reativa da sessão (o hook de dados de sessão continua sendo `useSession`; a conexão SignalR vive num contexto próprio consumido por ele)
- [x] Remover qualquer polling remanescente do frontend, substituindo por reação a eventos do hub

> **Decisões de arquitetura tomadas nesta fase** (não previstas originalmente no `docs/ARCHITECTURE.md`):
> - **`ISessionEventPublisher`** (porta definida em `CozyCinema.Application/RealTime`) é injetado nos Services (`SessionService`, `VotingService`), que vivem em `CozyCinema.Infrastructure` e não podem referenciar a API. A implementação real (`SignalRSessionEventPublisher`, usando `IHubContext<CinemaSessionHub>`) mora em `CozyCinema.Api` e é registrada via DI no `Program.cs` — isso preserva a regra "todo evento é disparado a partir dos Services" sem criar referência circular Infrastructure→Api.
> - **`CinemaSessionHub`** não recebe comandos de domínio dos clientes; seu único método é `OnConnectedAsync`, que lê `?sessionId=` da query string (autenticada via JWT, aceito também via `access_token` na query string só para rotas `/hubs/*`) e adiciona a conexão ao grupo `session:{sessionId}`. Toda a lógica de domínio continua chegando via REST.
> - **`UserLeftSession` precisava de um gatilho real**: como `MemberStatus.Left` já existia no modelo (usado no rejoin) mas nenhuma ação setava esse status, foi adicionado `POST /api/sessions/{id}/leave` (`SessionService.LeaveSessionAsync`) — o botão "Trocar de sessão", que já existia no frontend e só limpava estado local, agora chama esse endpoint antes de limpar o `localStorage`.
> - **`SessionError`** é publicado pelos Services logo antes de lançar as exceções de validação "em fluxo de sessão ativa" (`NotYourTurnException`, `SessionNotReadyException`, `RoundNotInSelectionPhaseException`, `RoundNotInVotingPhaseException`, `SelfVoteNotAllowedException`, `DuplicateVoteException`, `RoundMovieNotFoundException`), com `code` = nome da exceção sem o sufixo `Exception` e `message` = a mesma mensagem devolvida no corpo do erro REST. Isso dá ao evento um disparo real e consistente sem duplicar validação.
> - **Presença por desconexão de socket (fechar aba sem clicar em "Trocar de sessão") ficou fora de escopo**: `UserLeftSession` só é emitido por ação explícita de saída, não por `OnDisconnectedAsync`. Rastrear presença por conexão exigiria mapear `connectionId → userId` com suporte a múltiplas abas, o que não foi considerado necessário para o MVP.
> - **Estratégia do frontend: eventos como sinal de invalidação, não como fonte de estado.** Cada handler de evento do hub simplesmente dispara um refetch REST (`refresh()`/`refreshRound()`) em vez de reconstruir o estado local a partir do payload do evento — mantém uma única fonte de verdade (a resposta REST, já mapeada para os DTOs existentes) e evita duplicar lógica de mapeamento em TypeScript. `VotingRoundContext` teve o polling de 3s removido e substituído por essa reação a eventos.
> - **Contextos React**: `RealtimeProvider` foi inserido entre `SessionProvider` e as telas (`App.tsx`), pois precisa de `sessionId`/`token` de `useSession`/`useAuth`. Como `SessionContext` não pode consumir um contexto fornecido por seu próprio descendente, a reação de `SessionContext` a eventos (`UserJoinedSession`, `UserLeftSession`, `MovieSelected`, `TurnPassed`, `SessionError`) foi isolada num componente-ponte (`SessionRealtimeBridge`, renderizado dentro de `RealtimeProvider`) em vez de morar dentro do próprio `SessionProvider`.
>
> **Pendência:** smoke test manual end-to-end no navegador (duas sessões/abas, Modo Casal e Modo Grupo, validando que os eventos chegam sem polling e que o banner de `SessionError` aparece) não foi executado nesta sessão. Validado apenas via `dotnet build`, `npm run build` e `npm run lint`. `dotnet run --project src/CozyCinema.Api` e `npm run dev` não foram deixados rodando ao final desta sessão.

## Fase 6 — Integração com TMDB

- [x] Implementar `ITmdbService`/`TmdbService` com `HttpClientFactory` (client nomeado `TmdbClient`)
- [x] Endpoints backend de busca (`GET /api/movies/search`) e detalhe (`GET /api/movies/{tmdbId}`) que encapsulam a TMDB
- [x] Cache em `IMemoryCache` para busca e detalhe, com chaves e expiração conforme `docs/ARCHITECTURE.md`
- [x] Mapear apenas os campos necessários (título, capa, sinopse, nota, gêneros, diretor, data) para DTOs internos
- [x] Componente de busca de filme no frontend (usado tanto no Modo Casal quanto na Etapa 1 do Modo Grupo)

> **Decisões desta fase** (não previstas originalmente no `docs/ARCHITECTURE.md`):
> - **Autenticação na TMDB via API Key v3** (`api_key` como query string), não o Bearer Token v4 — combina com o formato do campo `Tmdb:ApiKey` já reservado em `appsettings.json` desde o scaffolding (Fase 0). A chave nunca é passada ao frontend; toda montagem de URL acontece dentro de `TmdbService`.
> - **`GET /api/movies/{tmdbId}`** usa `append_to_response=credits` numa única chamada à TMDB para extrair o diretor (`credits.crew` filtrando `job == "Director"`), evitando uma segunda requisição só para créditos.
> - **Cache** com chave `tmdb:search:{query}:{page}` / `tmdb:movie:{tmdbId}` e expiração absoluta de 10 minutos (`IMemoryCache.Set` com `TimeSpan.FromMinutes(10)`), suficiente para não estourar rate limit da TMDB durante uma sessão ativa sem servir dados muito desatualizados.
> - **Resiliência:** timeout de 8s no `HttpClient` nomeado `TmdbClient`; falhas de rede/timeout/JSON viram `TmdbUnavailableException` (HTTP 502 para o frontend) em vez de derrubar a sessão; um `tmdbId` inexistente vira `MovieNotFoundException` (HTTP 404).
> - **`GET /api/movies/search`** exige `[Authorize]` como os demais endpoints (não é uma rota pública) e devolve lista vazia (sem chamar a TMDB) quando `query` vier em branco, evitando uma chamada desnecessária a cada keystroke apagado.
> - **Frontend:** `useMovieSearch` isola fetch/debounce (400ms) da apresentação; `MovieSearchPicker` é o componente reutilizável (usado em `MoviePickForm` do Modo Casal e `MovieSuggestForm` da Etapa 1 do Modo Grupo), substituindo o formulário manual de TMDB ID/título/capa das Fases 3 e 4. A imagem da capa usa o tamanho fixo `w500` do TMDB (`https://image.tmdb.org/t/p/w500`); não há endpoint de configuração dinâmica de imagem por não ser necessário para o MVP.
>
> **Pendência:** smoke test manual end-to-end (buscar filme real, selecionar, confirmar escolha/sugestão no navegador com uma `Tmdb:ApiKey` válida) não foi executado nesta sessão — validado apenas via `dotnet build`, `npm run build` e `npm run lint`. É necessário configurar `Tmdb:ApiKey` (via `dotnet user-secrets` ou variável de ambiente, nunca commitado) antes de rodar a API localmente com esta fase.

## Fase 7 — Histórico / Movie Log

- [x] Endpoint para registrar filme assistido (`WatchedMovie`), tanto a partir do resultado de uma sessão quanto manualmente
- [x] Endpoint de consulta de histórico por sessão e por usuário, com ordenação (ano, nota) e filtros (gênero, diretor)
- [x] Tela de Movie Log no frontend: lista/grid com capa, título, sinopse, nota, gênero, diretor e data assistida
- [x] Controles de ordenação e filtro na UI, refletindo os parâmetros de query do endpoint

> **Decisões de schema tomadas nesta fase** (não previstas originalmente no `docs/DATA-MODEL.md`): `WatchedMovie.Synopsis` (`string?`) e `WatchedMovie.ReleaseYear` (`int?`, extraído de `MovieDetailDto.ReleaseDate.Year`) foram adicionados via migration própria (`AddWatchedMovieSynopsisAndReleaseYear`, já aplicada ao Supabase) — a tela de Movie Log precisa exibir sinopse e permitir ordenar por "ano", e nenhum dos dois campos existia no modelo original (que só previa nota, gêneros, diretor e data assistida).
>
> **Dois endpoints de registro** (`POST /api/movie-log/{sessionId}/from-current` e `POST /api/movie-log/{sessionId}/manual`), ambos em `MovieLogController`/`MovieLogService`:
> - **`from-current`**: usa `Session.CurrentMovieTmdbId` (não aceita `tmdbMovieId` no corpo), busca os metadados completos via `ITmdbService.GetMovieDetailAsync` (a sessão só cacheia título/capa, não sinopse/gêneros/diretor/nota) e, ao concluir, **limpa `Session.CurrentMovieTmdbId/Title/CoverUrl`** (mas não `LastPickerUserId`, que continua sendo a fonte de verdade do turno no Modo Casal). Falha com `NoCurrentMovieToMarkWatchedException` (409) se a sessão não tiver filme selecionado.
> - **`manual`**: recebe `tmdbMovieId` explícito no corpo e não mexe em `Session.CurrentMovie*` — permite registrar qualquer filme no histórico da sessão sem depender do fluxo de escolha/votação (ex: filme assistido fora do app). `WatchedMovie.UserId` é sempre quem chama o endpoint (quem "marcou"), não uma lista de todos os membros presentes.
> - Ambos aceitam `WatchedAt` opcional no corpo (default `DateTime.UtcNow`) para permitir logs retroativos.
> - Consulta de histórico é toda em memória após um único `ToListAsync` (`GET /api/movie-log/session/{sessionId}` e `GET /api/movie-log/me`), já que `WatchedMovie.Genres` é CSV e o filtro de gênero precisa fazer split — não há como traduzir isso para SQL via EF de forma direta, e o volume esperado por sessão/usuário no MVP não justifica otimizar isso agora.
>
> **Novo evento SignalR `MovieWatched`** (`ISessionEventPublisher.MovieWatchedAsync`, adicionado ao catálogo de `docs/ARCHITECTURE.md`): disparado apenas pelo fluxo `from-current`, porque é o único que muda estado compartilhado da sessão (`CurrentMovie` volta a `null` para todos os membros). O fluxo `manual` não dispara evento — é um registro pessoal de histórico que não afeta o que os outros membros veem na tela da sessão.
>
> **Frontend:** navegação para o Movie Log é um simples toggle de estado local em `SessionRouter` (`App.tsx`), sem router — consistente com o resto do app, que não usa nenhuma lib de rotas. O botão "Marcar como assistido" fica dentro do card do filme atual (Modo Casal) ou da tela de revelação do vencedor (Modo Grupo), não na barra de ação inferior fixa, para preservar a regra de "um único CTA primário por vez" de `docs/UI-DESIGN.md` quando também há um CTA de "Escolher filme"/"Sugerir filme" disponível. Nenhum conceito de moderador foi introduzido — qualquer membro ativo pode marcar como assistido ou registrar manualmente, confirmando a decisão pré-registrada na Fase 4.
>
> **Pendência:** smoke test manual end-to-end no navegador (marcar filme atual como assistido em ambos os modos, registrar filme manualmente, conferir ordenação/filtros na tela de Movie Log com dados reais da TMDB) não foi executado nesta sessão — validado apenas via `dotnet build`, `npm run build` e `npm run lint`. A migration desta fase já foi aplicada ao Supabase (`dotnet ef database update`).

## Fase 8 — Design system e polish visual

- [x] Aplicar tokens de cor/sombra/tipografia do `docs/UI-DESIGN.md` de forma consistente em todas as telas
- [x] Implementar bottom sheets, cards de filme e barras de ação inferiores conforme os layouts de referência
- [x] Implementar as micro-animações Framer Motion especificadas (troca de turno, revelação do vencedor, entrada/saída de bottom sheets)
- [x] Suporte a `prefers-reduced-motion`
- [x] Revisão de responsividade mobile-first em telas pequenas e áreas de toque generosas

> **Estado ao entrar nesta fase:** grande parte dos tokens (`tailwind.config`), bottom sheets, cards e as duas animações "carro-chefe" (crossfade de turno no Modo Casal, revelação do vencedor com `layoutId` compartilhado no Modo Grupo) já haviam sido implementados incrementalmente durante as Fases 3–7, junto com cada tela. Esta fase fechou as lacunas reais que restavam, em vez de reescrever telas já conformes ao `docs/UI-DESIGN.md`.
>
> **Lacunas fechadas nesta fase:**
> - **Tipografia não carregava de fato:** `tailwind.config` já declarava `font-heading` (Poppins/Nunito) e `font-body` (Inter), mas nenhum `<link>`/`@import` buscava essas fontes — o app renderizava no fallback `sans-serif` do sistema. Adicionado `preconnect` + `<link>` do Google Fonts (`Inter` 400/500/600, `Poppins` 600/700) em `index.html`.
> - **`prefers-reduced-motion` só existia na tela de revelação do vencedor** (`WinnerRevealScreen`, via `useReducedMotion`). Criado `src/lib/motion.ts` (`cozyMotion`) — helper único que recebe a animação "cheia" (posição/escala) e devolve automaticamente uma versão reduzida a fade puro (~150ms, sem scale/translação) quando `useReducedMotion()` é verdadeiro. Aplicado a todas as telas com Framer Motion (`AuthScreen`, `SessionHome`, `CasalSessionScreen`, os 4 bottom sheets, `MovieLogScreen`, `GrupoSessionScreen`), padronizando o que antes era feito ad-hoc só na revelação do vencedor. Também adicionado um fallback global via CSS (`@media (prefers-reduced-motion: reduce)` em `index.css`) para transições Tailwind puras (hover, etc.) fora do controle do Framer Motion.
> - **Transição de fase da rodada no Modo Grupo "pulava" sem animação:** `GrupoRoundRouter` trocava `SelectionScreen`/`BlindVotingScreen`/`WinnerRevealScreen` por troca direta de JSX, sem `AnimatePresence`. Adicionado wrapper com `AnimatePresence mode="popLayout"` + fade/slide (respeitando `prefers-reduced-motion`) — usa `popLayout` (não `wait`) especificamente para não desmontar a tela de votação antes da tela de revelação montar, preservando a transição de elemento compartilhado (`layoutId`) do card vencedor que já existia entre as duas telas.
> - **Cor de glow hardcoded fora do `tailwind.config`:** `WinnerRevealScreen` tinha um `style={{ boxShadow: 'rgba(232, 168, 60, 0.35)...' }}` inline (o mesmo RGB do token `cinema.accent`, mas duplicado como valor solto). Promovido a token `boxShadow.glowAccent` no `tailwind.config`, usado como `shadow-glowAccent`.
> - **Zero adaptação para telas maiores que mobile:** nenhuma classe `sm:`/`md:`/`lg:` existia no frontend inteiro (confirmado via grep), violando a diretriz de `CLAUDE.md` ("mobile-first... usar prefixos apenas para adaptar em telas maiores"). Adicionados breakpoints pontuais sem alterar o layout mobile: grids de filme (`SelectionScreen`, `BlindVotingScreen`) ganham colunas extras (`sm:grid-cols-3 md:grid-cols-4`); Movie Log vira grid de 2 colunas a partir de `sm:`; telas com `<main>` de largura total ganham `mx-auto max-w-2xl`/`max-w-3xl` para não esticar o conteúdo em viewports largos; botões dentro de barras de ação fixas (que continuam full-width por design) ganham `mx-auto max-w-2xl` interno para não virar um botão gigante em desktop.
> - **`env(safe-area-inset-bottom)` não funcionava de fato:** as barras de ação fixas já usavam `pb-[max(1rem,env(safe-area-inset-bottom))]`, mas a viewport meta tag não tinha `viewport-fit=cover` — sem isso, iOS Safari sempre resolve `env(safe-area-inset-bottom)` como `0`. Corrigido em `index.html`, junto com `<meta name="theme-color">` (`#1E0F14`, mesma cor de `cinema.background`) e `<title>`/`lang="pt-BR"` corrigidos (estavam com os valores padrão do template Vite).
> - **Áreas de toque de links de navegação secundários** ("Histórico", "Trocar de sessão", "Voltar", "Sair") eram texto puro sem padding — adicionado `rounded-lg px-2 py-1` para uma área de toque mais generosa sem mudar a hierarquia visual (continuam `text-xs`/`text-sm` sem fundo).
>
> **Validado nesta fase:** `npm run build` e `npm run lint` limpos após todas as mudanças. Verificação visual no navegador **não foi executada** (mesma limitação das fases anteriores — sem acesso a um navegador neste ambiente); recomenda-se rodar `npm run dev` e conferir visualmente as duas animações principais (troca de turno, revelação do vencedor) e o comportamento com "Reduzir movimento" ativado no SO antes de considerar esta fase fechada em produção.

## Fase 9 — Qualidade e testes

- [ ] Testes unitários dos Services críticos (cálculo de `LastPickerUserId`, bloqueio de auto-voto, apuração de votação, contagem de prontidão)
- [ ] Testes de integração dos endpoints principais (`dotnet test`)
- [ ] Testes de frontend dos hooks de dados (`useSession`, `useVoting`, `useAuth`) e componentes críticos (`npm run test`)
- [ ] Lint limpo (`npm run lint`) e build de produção validado (`npm run build`, `dotnet build`)

## Fase 10 — Deploy inicial

- [ ] Deploy do backend (.NET) no provedor escolhido
- [ ] Deploy do frontend (React) no provedor escolhido
- [ ] Configuração de variáveis de ambiente/secrets de produção (Supabase, TMDB, JWT)
- [ ] Teste ponta a ponta em produção: Modo Casal, Modo Grupo e Movie Log
