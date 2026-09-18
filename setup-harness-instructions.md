# Instrução de Setup do Harness de Contexto (Cozy Cinema App)

Atue como um Engenheiro de Software Principal e Arquiteto de Soluções. Nosso objetivo é criar a estrutura de documentação e Harness de Contexto para este repositório antes de iniciarmos o desenvolvimento do código.

---

## 1. Contexto Geral do Projeto

Estamos construindo o **"Cozy Cinema"**, uma aplicação mobile-first criada para resolver a paralisia de escolha na hora de assistir a filmes em dupla ou em grupo. 

### Conceito do Design System:
- **Atmosfera "Cozy Home Cinema":** Tons escuros e calorosos (vinho escuro, bordô/burgundy, cinza estofado para cards, bege macio para texto e destaques em amarelo pipoca/âmbar).
- **Sensação de iluminação suave / à meia-luz**, bordas bastante arredondadas (`rounded-2xl` / `rounded-3xl`), sombras suaves e botões com áreas de toque generosas para uso fácil com o polegar no celular.
- Micro-animações fluidas e aconchegantes para transições de estado (ex: revezamento de escolha, término da votação).

---

## 2. Escopo Funcional do MVP (Fase 1)

O MVP abordará dois modos de sessão com suporte a autenticação simples e histórico:

1. **Modo Casal (Revezamento por Turnos):**
   - Sistema alternado em que apenas 1 pessoa escolhe por vez.
   - O sistema persiste no banco qual foi o `LastPickerUserId` para passar automaticamente a vez ao parceiro na sessão seguinte.
2. **Modo Grupo (Votação Cega):**
   - **Etapa 1 (Seleção):** Cada participante pode buscar um filme na API pública do **TMDB** e sugerir para a lista da rodada.
   - **Etapa 2 (Prontidão):** Transição para votação assim que 100% dos participantes clicarem em "Pronto para Votar".
   - **Etapa 3 (Votação Cega):** Exibição dos cards dos filmes (sem revelar quem sugeriu cada um). 
   - **Regra de Ouro (Backend):** Bloqueio estrito no backend para impedir que um usuário vote no filme que ele próprio sugeriu.
3. **Histórico / Movie Log:**
   - Registro de filmes assistidos por Sessão e do Usuário no geral.
   - Exibição de capa, título, sinopse, nota TMDB, gênero, diretor e data em que foi assistido.
   - Ordenação (ano, nota) e filtros simples (gênero, diretor).

---

## 3. Stack Tecnológica Confirmada

- **Backend:** .NET 8/9 C# (ASP.NET Core Web API, Entity Framework Core).
- **Comunicação em Tempo Real:** SignalR (para atualização instantânea da sala e fases de votação).
- **Banco de Dados:** PostgreSQL hospedado no Supabase (utilizando o provedor `Npgsql.EntityFrameworkCore.PostgreSQL` no EF Core).
- **Frontend:** React (TypeScript), Tailwind CSS, Framer Motion (Mobile-First UI).
- **API Externa:** TMDB API (The Movie Database) consumida via Backend em C# com `MemoryCache`.

---

## 4. O que você (Claude Code) deve criar agora

Por favor, crie os seguintes arquivos e pastas na raiz deste repositório com o conteúdo detalhado e formatado em Markdown:

### Arquivo 1: `CLAUDE.md` (Arquivo Raiz)
Deve conter:
- Comandos rápidos de build, run e testes para .NET e React.
- Diretrizes de estilo C# (DTOs obrigatórios, Services para lógica de negócio, Controllers magros).
- Diretrizes de estilo React/Tailwind (Componentes funcionais TypeScript, Tailwind mobile-first, Framer Motion para animações).
- Regras arquiteturais críticas (ex: validação de auto-voto no backend, uso do SignalR para eventos de estado).

### Arquivo 2: `docs/ARCHITECTURE.md`
Deve conter:
- Visão geral da arquitetura em camadas (.NET API + React + Supabase Postgres).
- Especificação dos Hubs do SignalR (`CinemaSessionHub`) com lista de eventos (ex: `UserJoinedSession`, `ReadyToVote`, `MovieSelected`, `VoteSubmitted`, `VotingCompleted`).
- Estratégia de integração com a TMDB API (Service Layer + Caching).

### Arquivo 3: `docs/DATA-MODEL.md`
Deve conter o diagrama textual e classes do Entity Framework Core para as entidades:
- `User` (Id, Name, Email, PasswordHash)
- `Session` (Id, Title, IconUrl, Mode [`Casal` | `Grupo`], LastPickerUserId)
- `SessionMember` (SessionId, UserId, Status)
- `VotingRound` e `RoundMovie` (SessionId, Status, WinnerMovieId)
- `Vote` (RoundMovieId, UserId — com restrição de auto-voto)
- `WatchedMovie` (SessionId, UserId, TmdbMovieId, Title, CoverUrl, Rating, Genres, Director, WatchedAt)

### Arquivo 4: `docs/UI-DESIGN.md`
Deve conter:
- Guia do Design System "Cozy Cinema" (tokens de cores Tailwind customizados, fontes e elevação/sombras).
- Layouts de referência mobile-first (bottom sheets, cards de filmes, barras de ação inferiores).
- Especificações das micro-animações (Framer Motion) para troca de turnos no Modo Casal e revelação do vencedor no Modo Grupo.

Por favor, gere esses arquivos no repositório agora.