# Phasmo Hunt

Assistente manual externo para Phasmophobia (Windows, .NET 8 + WPF).

## O que é

Overlay Always-On-Top com cronômetro e medidor de velocidade do fantasma. **Toda informação é inserida manualmente** (botão lateral do mouse ou UI).

## Uso rápido

1. Ao ouvir passos, pressione o **botão lateral** (cada toque = 1 passo).
2. Pare de clicar por **3 segundos** — a leitura finaliza sozinha.
3. Os cliques são divididos em **3 médias** (`floor(n/3)` passos por parte; resto descartado).
4. Mínimo útil: **6 cliques** (2 por parte). Ex.: 6→2/2/2, 24→8/8/8, 7→2/2/2 (descarta 1).
5. Marque **Evidências** para filtrar fantasmas junto com a velocidade.

## O que esta aplicação NÃO faz

- Não lê memória do jogo
- Não acessa processos do Phasmophobia
- Não faz hooking gráfico / injeção de DLL
- Não captura áudio ou tela do jogo
- Não automatiza ações no jogo

## Catálogo de fantasmas

Velocidades de caçada ficam embutidas offline (`GhostCatalogService`), com base em dados públicos da comunidade/wiki ([Ghost](https://phasmophobia.fandom.com/wiki/Ghost)). Nada é lido do cliente do jogo.

## Desenvolvimento

```bash
dotnet build src/PhasmoHunt/PhasmoHunt.csproj
dotnet run --project src/PhasmoHunt/PhasmoHunt.csproj
```
