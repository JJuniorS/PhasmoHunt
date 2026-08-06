(() => {
  const STORAGE_KEY = "phasmohunt-lang";

  const strings = {
    pt: {
      "nav.features": "Funcionalidades",
      "nav.how": "Como usar",
      "nav.download": "Download",
      "nav.github": "GitHub",
      "hero.eyebrow": "CASE FILE · WINDOWS · OFFLINE",
      "hero.title": "Overlay manual para identificar o fantasma",
      "hero.lede":
        "Meça a velocidade dos passos, cruze com evidências e acompanhe peculiaridades. Tudo inserido por você — nada lê o processo do jogo.",
      "hero.cta": "Baixar para Windows",
      "hero.secondary": "Não é cheat",
      "hero.caption": "Prévia do overlay em uso",
      "features.eyebrow": "INVESTIGATION TOOLS",
      "features.title": "O que o Phasmo Hunt faz",
      "features.sub":
        "Assistente Always-On-Top — catálogo offline, filtros manuais, atalhos globais, zero acesso ao jogo.",
      "features.speed.title": "Velocidade dos passos",
      "features.speed.body":
        "Registre passos com a tecla 1 ou o botão lateral. Após 3s sem cliques, a leitura fecha sozinha em 3 médias (±0,10 m/s).",
      "features.reliability.title": "Confiabilidade",
      "features.reliability.body":
        "Porcentagem (0–100%) que mostra o quão regulares foram os intervalos entre os passos. Alta = ritmo estável e leitura mais segura para filtrar; baixa = vale repetir a medição. Não identifica o fantasma — avalia a qualidade do clique.",
      "features.evidence.title": "Evidências + catálogo",
      "features.evidence.body":
        "Filtre 30 fantasmas por evidência e velocidade. Tooltips nos ícones; The Mimic inclui Orb falsa.",
      "features.mark.title": "Marcar apto / fora",
      "features.mark.body":
        "Clique no nome do fantasma para alternar entre verde (apto) e vermelho (fora). Override manual na lista.",
      "features.hide.title": "Ocultar fora",
      "features.hide.body":
        "Opção na lista e nas configurações: com ativos, fantasmas vermelhos somem e só ficam os candidatos.",
      "features.pec.title": "Peculiaridades",
      "features.pec.body":
        "Shift+1 Demon cooldown, Shift+2 Incenso (3 min), Shift+3 ciclo Obambo paz/agressivo.",
      "features.ui.title": "Overlay + idioma",
      "features.ui.body":
        "Always-On-Top, modo compacto, transparência e escala salvos localmente. Interface em pt-BR e EN. Limpar com Shift+L.",
      "how.eyebrow": "FIELD GUIDE",
      "how.title": "Como usar em caçada",
      "how.sub": "Fluxo rápido para uma leitura de velocidade confiável.",
      "how.s1.title": "Ouça os passos",
      "how.s1.body":
        "Pressione 1 ou o botão lateral do mouse a cada passo (mínimo útil: 6 cliques).",
      "how.s2.title": "Espere 3 segundos",
      "how.s2.body": "Sem novos toques, a leitura finaliza e divide em P1 / P2 / P3.",
      "how.s3.title": "Confira os candidatos",
      "how.s3.body":
        "Veja média, padrão (estável/acelerando/…), confiabilidade e fantasmas compatíveis. Clique no nome para marcar apto ou fora.",
      "how.s4.title": "Marque evidências",
      "how.s4.body":
        "Refine o filtro e use peculiaridades (Shift+1/2/3). Oculte os fora se quiser só os candidatos. Shift+L limpa a sessão.",
      "download.eyebrow": "RELEASE",
      "download.title": "Baixe o overlay",
      "download.sub":
        "Windows · funciona offline. O binário é hospedado fora do GitHub; o histórico de versões fica nas Releases.",
      "download.cta": "Baixar para Windows",
      "download.cta.soon": "Download em breve",
      "download.releases": "Versões no GitHub",
      "download.version": "Versão {version} · {platform}",
      "download.meta1": "Sem instalador complexo — execute e investigue",
      "download.meta2": "Catálogo embutido; não precisa de internet na partida",
      "download.meta3": "Código aberto em github.com/JJuniorS/PhasmoHunt",
      "disclaimer.eyebrow": "CLEARANCE",
      "disclaimer.title": "O que esta aplicação NÃO faz",
      "disclaimer.i1": "Não lê memória do jogo",
      "disclaimer.i2": "Não acessa processos do Phasmophobia",
      "disclaimer.i3": "Não faz hooking gráfico / injeção de DLL",
      "disclaimer.i4": "Não captura áudio ou tela do jogo",
      "disclaimer.i5": "Não automatiza ações no jogo",
      "disclaimer.note":
        "É um overlay de apoio manual — não é cheat client. Toda informação é inserida por você.",
      "disclaimer.fan":
        "Phasmophobia © Kinetic Games Limited. Ferramenta não oficial da comunidade — não afiliada, endossada ou patrocinada pela Kinetic Games.",
      "footer.github": "GitHub — JJuniorS/PhasmoHunt",
      "footer.copy":
        "Phasmophobia © Kinetic Games Limited. Ferramenta não oficial da comunidade (fan-made) — não afiliada, endossada ou patrocinada pela Kinetic Games.",
    },
    en: {
      "nav.features": "Features",
      "nav.how": "How to use",
      "nav.download": "Download",
      "nav.github": "GitHub",
      "hero.eyebrow": "CASE FILE · WINDOWS · OFFLINE",
      "hero.title": "A manual overlay to identify the ghost",
      "hero.lede":
        "Measure footstep speed, cross-check evidence, and track hunt quirks. Everything is entered by you — nothing reads the game process.",
      "hero.cta": "Download for Windows",
      "hero.secondary": "Not a cheat",
      "hero.caption": "Overlay preview in use",
      "features.eyebrow": "INVESTIGATION TOOLS",
      "features.title": "What Phasmo Hunt does",
      "features.sub":
        "Always-On-Top community assistant — offline catalog, manual filters, global hotkeys, zero game access.",
      "features.speed.title": "Footstep speed",
      "features.speed.body":
        "Log steps with key 1 or the mouse side button. After 3s idle, the reading closes into 3 averages (±0.10 m/s).",
      "features.reliability.title": "Reliability",
      "features.reliability.body":
        "A 0–100% score of how regular the intervals between your step clicks were. High = steady rhythm and a safer reading for filtering; low = worth measuring again. It does not name the ghost — it rates the quality of the tap timing.",
      "features.evidence.title": "Evidence + catalog",
      "features.evidence.body":
        "Filter 30 ghosts by evidence and speed. Icon tooltips; The Mimic includes the fake Orb.",
      "features.mark.title": "Mark match / out",
      "features.mark.body":
        "Click a ghost name to toggle green (match) and red (out). Manual override in the list.",
      "features.hide.title": "Hide ruled-out",
      "features.hide.body":
        "Toggle on the list and in settings: when on, red ghosts disappear so only candidates remain.",
      "features.pec.title": "Peculiarities",
      "features.pec.body":
        "Shift+1 Demon cooldown, Shift+2 Incense (3 min), Shift+3 Obambo peace/angry cycle.",
      "features.ui.title": "Overlay + language",
      "features.ui.body":
        "Always-On-Top, compact mode, opacity and scale saved locally. UI in pt-BR and EN. Clear with Shift+L.",
      "how.eyebrow": "FIELD GUIDE",
      "how.title": "How to use on a hunt",
      "how.sub": "A quick flow for a reliable speed reading.",
      "how.s1.title": "Listen for footsteps",
      "how.s1.body":
        "Press 1 or the mouse side button on each step (useful minimum: 6 clicks).",
      "how.s2.title": "Wait 3 seconds",
      "how.s2.body": "With no new taps, the reading finishes and splits into P1 / P2 / P3.",
      "how.s3.title": "Check candidates",
      "how.s3.body":
        "See average, pattern (stable/speeding up/…), reliability, and matching ghosts. Click a name to mark match or out.",
      "how.s4.title": "Mark evidence",
      "how.s4.body":
        "Tighten the filter and use peculiarities (Shift+1/2/3). Hide ruled-out ghosts if you want candidates only. Shift+L clears the session.",
      "download.eyebrow": "RELEASE",
      "download.title": "Download the overlay",
      "download.sub":
        "Windows · works offline. The binary is hosted outside GitHub; version history lives in Releases.",
      "download.cta": "Download for Windows",
      "download.cta.soon": "Download coming soon",
      "download.releases": "Versions on GitHub",
      "download.version": "Version {version} · {platform}",
      "download.meta1": "No heavy installer — run and investigate",
      "download.meta2": "Embedded catalog; no internet needed in-match",
      "download.meta3": "Open source at github.com/JJuniorS/PhasmoHunt",
      "disclaimer.eyebrow": "CLEARANCE",
      "disclaimer.title": "What this app does NOT do",
      "disclaimer.i1": "Does not read game memory",
      "disclaimer.i2": "Does not access the Phasmophobia process",
      "disclaimer.i3": "Does not do graphics hooking / DLL injection",
      "disclaimer.i4": "Does not capture game audio or screen",
      "disclaimer.i5": "Does not automate in-game actions",
      "disclaimer.note":
        "It is a manual helper overlay — not a cheat client. Every input comes from you.",
      "disclaimer.fan":
        "Phasmophobia © Kinetic Games Limited. Unofficial community tool — not affiliated with, endorsed by, or sponsored by Kinetic Games.",
      "footer.github": "GitHub — JJuniorS/PhasmoHunt",
      "footer.copy":
        "Phasmophobia © Kinetic Games Limited. Unofficial fan-made community tool — not affiliated with, endorsed by, or sponsored by Kinetic Games.",
    },
  };

  let releaseInfo = null;
  let currentLang = "pt";

  function t(key, fallback) {
    const dict = strings[currentLang] || strings.pt;
    return dict[key] != null ? dict[key] : fallback ?? key;
  }

  function applyI18n(lang) {
    const dict = strings[lang] || strings.pt;
    document.querySelectorAll("[data-i18n]").forEach((el) => {
      const key = el.getAttribute("data-i18n");
      if (key && dict[key] != null) {
        el.textContent = dict[key];
      }
    });
    applyReleaseUi();
  }

  function setLang(lang) {
    const next = lang === "en" ? "en" : "pt";
    currentLang = next;
    document.documentElement.lang = next;
    localStorage.setItem(STORAGE_KEY, next);
    applyI18n(next);
    document.querySelectorAll(".lang-btn").forEach((btn) => {
      const active = btn.getAttribute("data-lang") === next;
      btn.setAttribute("aria-pressed", active ? "true" : "false");
    });
  }

  function initLang() {
    const saved = localStorage.getItem(STORAGE_KEY);
    setLang(saved === "en" || saved === "pt" ? saved : "pt");
  }

  function initLangButtons() {
    document.querySelectorAll(".lang-btn").forEach((btn) => {
      btn.addEventListener("click", () => {
        setLang(btn.getAttribute("data-lang"));
      });
    });
  }

  function hasDownloadUrl() {
    return Boolean(releaseInfo && releaseInfo.downloadUrl && releaseInfo.downloadUrl.trim());
  }

  function applyReleaseUi() {
    const versionEl = document.getElementById("download-version");
    const releasesBtn = document.getElementById("releases-btn");
    const ctas = document.querySelectorAll("[data-download-cta]");

    if (releaseInfo) {
      if (versionEl) {
        versionEl.hidden = false;
        versionEl.textContent = t("download.version")
          .replace("{version}", releaseInfo.version || "—")
          .replace("{platform}", releaseInfo.platform || "Windows");
      }

      if (releasesBtn && releaseInfo.githubReleasesUrl) {
        releasesBtn.href = releaseInfo.githubReleasesUrl;
      }
    }

    const ready = hasDownloadUrl();
    ctas.forEach((btn) => {
      if (ready) {
        btn.href = releaseInfo.downloadUrl;
        btn.removeAttribute("aria-disabled");
        btn.classList.remove("is-disabled");
        if (btn.hasAttribute("data-i18n")) {
          btn.setAttribute("data-i18n", "download.cta");
        }
        btn.textContent = t("download.cta");
        btn.target = "_blank";
        btn.rel = "noopener noreferrer";
      } else {
        btn.href = "#download";
        btn.removeAttribute("target");
        btn.removeAttribute("rel");
        btn.setAttribute("aria-disabled", "true");
        btn.classList.add("is-disabled");
        if (btn.hasAttribute("data-i18n")) {
          btn.setAttribute("data-i18n", "download.cta.soon");
        }
        btn.textContent = t("download.cta.soon");
      }
    });
  }

  async function initRelease() {
    try {
      const res = await fetch("release.json", { cache: "no-cache" });
      if (!res.ok) throw new Error(`release.json ${res.status}`);
      releaseInfo = await res.json();
    } catch {
      releaseInfo = null;
    }
    applyReleaseUi();
  }

  function initSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
      anchor.addEventListener("click", (event) => {
        if (anchor.getAttribute("aria-disabled") === "true") {
          event.preventDefault();
          return;
        }
        const id = anchor.getAttribute("href");
        if (!id || id === "#" || !id.startsWith("#")) return;
        const target = document.querySelector(id);
        if (!target) return;
        event.preventDefault();
        target.scrollIntoView({ behavior: "smooth", block: "start" });
      });
    });
  }

  function initReveal() {
    const nodes = document.querySelectorAll(".reveal");
    if (!("IntersectionObserver" in window)) {
      nodes.forEach((n) => n.classList.add("is-visible"));
      return;
    }
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.12, rootMargin: "0px 0px -40px 0px" }
    );
    nodes.forEach((n) => observer.observe(n));
  }

  document.addEventListener("DOMContentLoaded", () => {
    initLang();
    initLangButtons();
    initSmoothScroll();
    initReveal();
    initRelease();
  });
})();
