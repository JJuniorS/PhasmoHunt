# Phasmo Hunt

**Idioma / Language:** [Português](README.md) | [English](README.en.md)

Assistente manual externo para **Phasmophobia** — overlay Always-On-Top no Windows (.NET 8 + WPF).

Ajuda a identificar o fantasma medindo a velocidade dos passos, cruzando com evidências e acompanhando peculiaridades de caçada. **Toda informação é inserida por você** (atalhos / interface). Nada é lido do processo do jogo.

---

## Funcionalidades

### Medição de velocidade

- Cada toque no atalho de passo registra um timestamp (o primeiro toque também inicia a sessão).
- Após **3 segundos** sem novos passos, a leitura finaliza sozinha.
- Os cliques são divididos em **3 médias** (`P1` / `P2` / `P3`): `floor(n/3)` passos por parte; o resto é descartado.
- Mínimo útil: **6 cliques** (2 por parte). Ex.: 6 → 2/2/2 · 24 → 8/8/8 · 7 → 2/2/2 (descarta 1).
- Velocidade em m/s alinhada ao BPM finder da comunidade Zero-Network.
- Tolerância de correspondência: **±0,10 m/s**.
- Mostra padrão da leitura (estável, acelerando, desacelerando, irregular) e confiança.

### Catálogo e filtros

- Catálogo offline com **30 fantasmas** (velocidades + evidências), baseado em dados públicos da [wiki](https://phasmophobia.fandom.com/wiki/Ghost).
- Filtra por velocidade medida **e** evidências selecionadas.
- **The Mimic** inclui Orb falsa no filtro (como no jogo).
- Lista visual: verde = apto · vermelho = fora · neutro = pendente.
- Histórico de leituras com consenso entre medições da sessão.

### Peculiaridades

Timers e marcadores úteis durante a investigação:

| Peculiaridade | Atalho | Comportamento |
|---------------|--------|----------------|
| **Demon** | `Shift + 1` | Cronômetro de cooldown. Se parar abaixo de 25 s, filtra o catálogo para Demon. |
| **Incenso** | `Shift + 2` | Contagem regressiva de 3 minutos (reinicia a cada toque). |
| **Obambo** | `Shift + 3` | Ciclo paz / agressivo a cada 2 minutos. |

### Interface

- Janela Always-On-Top, compactável para ocupar menos espaço na tela.
- Painel de **evidências** para filtrar junto com a velocidade.
- **Configurações:** transparência (0,3–1,0) e escala da interface (0,8–1,5), salvas em `%AppData%\PhasmoHunt\settings.json`.
- Botão **Limpar** zera medição, evidências, histórico e peculiaridades.

### Atalhos

| Ação | Atalho |
|------|--------|
| Registrar passo | Tecla `1` ou **botão lateral** do mouse (atrás) |
| Demon | `Shift + 1` |
| Incenso | `Shift + 2` |
| Obambo | `Shift + 3` |

---

## Uso rápido

1. Ao ouvir passos, pressione `1` ou o **botão lateral** (cada toque = 1 passo).
2. Pare de clicar por **3 segundos** — a leitura finaliza sozinha.
3. Confira a média geral, as 3 partes e os fantasmas compatíveis.
4. Marque **Evidências** para refinar o filtro.
5. Use as peculiaridades (`Shift + 1/2/3`) conforme a investigação.

---

## O que esta aplicação NÃO faz

- Não lê memória do jogo
- Não acessa processos do Phasmophobia
- Não faz hooking gráfico / injeção de DLL
- Não captura áudio ou tela do jogo
- Não automatiza ações no jogo

É um overlay de apoio manual — não é cheat client.

---

## Catálogo de fantasmas

Velocidades de caçada ficam embutidas em `GhostCatalogService`. Nada é lido do cliente do jogo. Referência pública: [Ghost — Phasmophobia Wiki](https://phasmophobia.fandom.com/wiki/Ghost).

---

## Desenvolvimento

Requisitos: .NET 8 SDK (Windows).

```bash
dotnet build src/PhasmoHunt/PhasmoHunt.csproj
dotnet run --project src/PhasmoHunt/PhasmoHunt.csproj
```

### Estrutura

```
src/PhasmoHunt/
  Models/       # Fantasmas, evidências, medições, settings
  Services/     # Catálogo, cálculo de velocidade, hotkeys, persistência
  ViewModels/   # Lógica da UI
  Views/        # MainWindow (WPF)
  Themes/       # Tema escuro
  Assets/       # Ícones de evidências e peculiaridades
src/website/    # Landing page (GitHub Pages)
```

### Assets / ícones

Ícones de evidência, peculiaridades e do app foram **criados para este projeto** (arte gerada com IA / original). Não incluem assets oficiais do Phasmophobia nem da Kinetic Games.

---

## Site e download

| O quê | Onde |
|-------|------|
| Site estático | `src/website/` |
| Deploy | GitHub Actions → **GitHub Pages** (workflow `Deploy website`) |
| Link do binário | campo `downloadUrl` em [`src/website/release.json`](src/website/release.json) (Google Drive, etc.) |
| Histórico de versões | [CHANGELOG.md](CHANGELOG.md) + [GitHub Releases](https://github.com/JJuniorS/PhasmoHunt/releases) (tags `v*`) |

O botão de download do site lê `release.json` em runtime. Enquanto `downloadUrl` estiver vazio, o CTA fica como “em breve”.

---

## Licença

Código sob a [licença MIT](LICENSE).

Phasmophobia e Kinetic Games são marcas de seus respectivos proprietários. O Phasmo Hunt é uma ferramenta não oficial da comunidade e **não é afiliado, endossado ou patrocinado** pela Kinetic Games.
