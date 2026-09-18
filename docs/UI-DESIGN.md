# Design System — "Cozy Cinema"

Guia visual e de interação para o frontend mobile-first. Toda implementação de UI deve seguir este documento; tokens de cor citados aqui devem existir literalmente no `tailwind.config`.

## 1. Atmosfera

"Cozy Home Cinema": sensação de estar assistindo filme numa sala aconchegante, à meia-luz, sofá confortável. Tons escuros e quentes, nada de branco puro ou cores frias/neon. Bordas bastante arredondadas, sombras suaves (nunca duras/hard-edge), áreas de toque generosas para uso com o polegar.

## 2. Tokens de cor (Tailwind)

Adicionar em `tailwind.config` (`theme.extend.colors`):

```js
colors: {
  cinema: {
    // Fundo principal — vinho escuro
    background: '#1E0F14',
    // Superfícies elevadas (cards) — cinza estofado, levemente amadeirado
    surface: '#2B2226',
    surfaceElevated: '#352A2F',
    // Destaque primário — bordô/burgundy
    primary: '#7A1F2B',
    primaryHover: '#921F2E',
    // Acento — amarelo pipoca / âmbar
    accent: '#E8A83C',
    accentHover: '#F2B958',
    // Texto — bege macio
    text: '#F1E4D8',
    textMuted: '#C9B8AC',
    // Estados
    success: '#5C8A5A',
    danger: '#C1443C',
  },
}
```

- **Fundo (`cinema.background`):** usado na tela toda, transmite penumbra.
- **Superfície (`cinema.surface` / `surfaceElevated`):** cards, bottom sheets, modais.
- **Primária (`cinema.primary`):** botões de ação principal, cabeçalhos de destaque.
- **Acento (`cinema.accent`):** CTA de maior prioridade (ex: "Pronto para Votar", vencedor da votação), badges, indicadores de "é sua vez".
- **Texto (`cinema.text` / `textMuted`):** nunca usar branco puro (`#FFFFFF`) sobre o fundo escuro — usar sempre o bege macio para manter a atmosfera "à meia-luz".

## 3. Bordas, sombras e formas

- Cards de filme, bottom sheets e modais: `rounded-3xl`.
- Botões, inputs, badges: `rounded-2xl`.
- Sombras suaves e difusas, nunca duras:
  ```js
  boxShadow: {
    cozy: '0 8px 24px -6px rgba(0, 0, 0, 0.45)',
    cozyLg: '0 16px 40px -8px rgba(0, 0, 0, 0.55)',
  }
  ```
- Botões de ação principal (ex: "Sugerir Filme", "Pronto para Votar", "Assistido!"): altura mínima de toque `h-14` (56px), padding horizontal generoso (`px-6`), texto `text-base font-semibold`.

## 4. Tipografia

- Fonte arredondada e amigável para títulos (ex: `Poppins` ou `Nunito`), fonte legível e neutra para corpo de texto (ex: `Inter`).
- Hierarquia: `text-2xl font-bold` (título de tela) → `text-lg font-semibold` (título de card) → `text-sm text-cinema-textMuted` (metadados: gênero, diretor, data).

## 5. Layouts de referência (mobile-first)

### Bottom Sheets
- Usados para: detalhe de filme, confirmação de "Assistido!", ajustes de sessão.
- Sobem do rodapé cobrindo até ~85% da altura da tela, fundo `cinema.surfaceElevated`, `rounded-t-3xl`, handle (alça) centralizado no topo.
- Fecham por swipe-down ou toque fora da área.

### Cards de filme
- Formato retrato (poster) com `rounded-3xl`, sombra `shadow-cozy`.
- Overlay de gradiente escuro na base da imagem para garantir legibilidade do título sobreposto.
- Metadados (nota TMDB, gênero, ano) como badges pequenos com `cinema.accent` para a nota.
- No Modo Grupo durante a votação cega: card não exibe nenhum indício de quem sugeriu o filme (sem avatar, sem nome).

### Barras de ação inferiores
- Fixas na base da tela (`fixed bottom-0`), fundo `cinema.surface`, borda superior sutil, padding respeitando safe-area (`env(safe-area-inset-bottom)`).
- Contêm a ação principal do momento (ex: "Sugerir Filme", "Pronto para Votar", "Confirmar Voto") — sempre um único CTA primário visível por vez para reduzir a paralisia de escolha também na própria UI.

## 6. Micro-animações (Framer Motion)

### Troca de turno — Modo Casal
- Ao `TurnPassed`: card indicador "É a vez de {Nome}" faz um `crossfade + slight scale` (sai o card anterior com `opacity: 1 → 0, scale: 1 → 0.96`, entra o novo com `opacity: 0 → 1, scale: 1.04 → 1`), duração ~350ms, easing suave (`ease-in-out` custom, tipo "cozy" sem bounce agressivo).
- Um leve highlight pulsante em `cinema.accent` ao redor do avatar de quem escolhe, para reforçar de forma calorosa (não abrupta) que é a vez da pessoa.

### Revelação do vencedor — Modo Grupo
- Ao `VotingCompleted`: os cards não vencedores encolhem e desaparecem (`scale: 1 → 0.9, opacity: 1 → 0`, stagger leve entre eles).
- O card vencedor se expande suavemente do tamanho de card de grid para um card de destaque central (`scale + layout animation` do Framer Motion via `layoutId` compartilhado entre o card na grade de votação e o card de revelação), com um brilho/glow sutil em `cinema.accent` ao redor da borda.
- Duração total da sequência de revelação: ~600-800ms, para transmitir "celebração calma", não abrupta.

### Regras gerais de animação
- Nunca usar easing linear ou transições abruptas — sempre curvas suaves (`ease-in-out`/curvas customizadas), reforçando a atmosfera aconchegante.
- Toda transição de fase de sessão (Seleção → Prontidão → Votação → Concluído) deve ter uma animação de entrada/saída própria, nunca "pular" de estado sem transição visual.
- Respeitar `prefers-reduced-motion`: quando ativo, reduzir animações a simples fades curtos (~150ms), sem scale/movimento.
