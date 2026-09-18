# CLAUDE.md — Cozy Cinema

Guia de contexto para trabalhar neste repositório. Leia também `docs/ARCHITECTURE.md`, `docs/DATA-MODEL.md` e `docs/UI-DESIGN.md` antes de implementar features novas.

## O que é o Cozy Cinema

App mobile-first para resolver a "paralisia de escolha" de filmes em casal ou em grupo. Dois modos de sessão (Casal = revezamento por turnos, Grupo = votação cega) mais um histórico de filmes assistidos (Movie Log). Ver `setup-harness-instructions.md` para o briefing original do produto.

## Plano de implementação

O progresso do MVP é rastreado em fases numeradas em `IMPLEMENTATION-PLAN.md`. Consulte esse arquivo para saber o que já foi feito e o que vem a seguir, e marque as caixas (`[x]`) conforme as etapas forem concluídas.

## Stack

- **Backend:** .NET 8/9, ASP.NET Core Web API, Entity Framework Core, Npgsql (PostgreSQL/Supabase).
- **Tempo real:** SignalR (`CinemaSessionHub`).
- **Frontend:** React + TypeScript, Tailwind CSS, Framer Motion. Mobile-first.
- **API externa:** TMDB, consumida só pelo backend (nunca direto do frontend), com `MemoryCache`.

## Comandos

### Backend (.NET)
```bash
dotnet build                                    # build da solution
dotnet run --project src/CozyCinema.Api         # roda a API localmente
dotnet watch --project src/CozyCinema.Api run   # hot reload
dotnet test                                     # roda todos os testes
dotnet ef migrations add <Nome> -p src/CozyCinema.Infrastructure -s src/CozyCinema.Api
dotnet ef database update -p src/CozyCinema.Infrastructure -s src/CozyCinema.Api
```

### Frontend (React)
```bash
npm install     # instala dependências (dentro de /frontend)
npm run dev     # servidor de desenvolvimento
npm run build   # build de produção
npm run lint    # eslint
npm run test    # testes
```

> Os caminhos de projeto acima (`src/CozyCinema.Api`, `/frontend`) são a convenção alvo deste repositório. Ajuste este arquivo se a estrutura real divergir quando o código for criado.

## Diretrizes de estilo C# (Backend)

- **Controllers magros:** controllers só recebem request, chamam um Service e retornam o resultado. Nenhuma regra de negócio dentro de um controller.
- **Services para lógica de negócio:** toda regra (ex: validação de auto-voto, cálculo de próximo `LastPickerUserId`, apuração de votação) vive em uma classe de Service, injetada via DI.
- **DTOs obrigatórios:** nunca expor entidades do EF Core diretamente na API. Toda request/response tem um DTO próprio. Mapeamento explícito (ou AutoMapper, se adotado) entre Entity ↔ DTO.
- Entidades do EF Core ficam na camada de Infrastructure/Domain, não referenciadas fora dos Services e do `DbContext`.
- Validação de input via `DataAnnotations`/`FluentValidation` nos DTOs de request, não em getters/setters da entidade.
- Toda chamada assíncrona usa `async`/`await` com `CancellationToken` propagado até o repositório/`DbContext`.

## Diretrizes de estilo React/Tailwind (Frontend)

- **Componentes funcionais TypeScript** apenas. Sem class components. Props sempre tipadas com `interface`/`type`.
- **Tailwind mobile-first:** escrever classes para mobile primeiro, usar prefixos (`sm:`, `md:`, `lg:`) apenas para adaptar em telas maiores — nunca o contrário.
- **Framer Motion** para toda transição de estado visível ao usuário (troca de turno, revelação de vencedor, entrada/saída de bottom sheets). Ver `docs/UI-DESIGN.md` para as especificações de animação.
- Tokens de cor/espaçamento customizados do design system "Cozy Cinema" ficam centralizados no `tailwind.config`, nunca hardcoded como hex solto em componentes.
- Hooks de dados (fetch de API, subscrição SignalR) isolados em hooks próprios (`useSession`, `useVoting`, etc.), não misturados com lógica de apresentação do componente.

## Regras arquiteturais críticas

- **Validação de auto-voto é responsabilidade do backend**, sempre — nunca confiar apenas em desabilitar o botão no frontend. Um usuário não pode votar em um filme que ele mesmo sugeriu na rodada (ver `docs/DATA-MODEL.md`, entidade `Vote`).
- **SignalR é a fonte de eventos de mudança de estado** de uma sessão (novo membro, pronto para votar, filme selecionado, voto registrado, votação concluída). O frontend não deve fazer polling para essas mudanças — deve reagir a eventos do `CinemaSessionHub` (ver `docs/ARCHITECTURE.md`).
- **TMDB nunca é chamado diretamente pelo frontend.** Toda busca de filme passa por um endpoint do backend que encapsula a chamada à TMDB API com cache em memória.
- **Transição de Prontidão → Votação** só ocorre quando 100% dos membros da sessão sinalizarem "Pronto para Votar" — essa contagem é feita no backend, não no cliente.
- **Persistência de turno:** o campo `LastPickerUserId` na `Session` é a única fonte de verdade sobre de quem é a vez no Modo Casal; ele é atualizado pelo backend ao final de cada escolha, nunca inferido no frontend.
