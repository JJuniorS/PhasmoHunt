using System.ComponentModel;
using System.Runtime.CompilerServices;
using PhasmoHunt.Models;

namespace PhasmoHunt.Services;

public enum AppLanguage
{
    PtBr,
    En
}

/// <summary>
/// Runtime UI strings for pt-BR / en. Bind via
/// {Binding Path=Clear, Source={x:Static services:LocalizationService.Instance}}.
/// </summary>
public sealed class LocalizationService : INotifyPropertyChanged
{
    public static LocalizationService Instance { get; } = new();

    public const string CodePtBr = "pt-BR";
    public const string CodeEn = "en";

    private AppLanguage _language = AppLanguage.PtBr;

    private LocalizationService()
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? LanguageChanged;

    public AppLanguage Language => _language;

    public bool IsPtBr => _language == AppLanguage.PtBr;
    public bool IsEn => _language == AppLanguage.En;

    public void SetLanguage(AppLanguage language)
    {
        if (_language == language)
        {
            return;
        }

        _language = language;
        OnPropertyChanged(string.Empty);
        LanguageChanged?.Invoke();
    }

    public void SetLanguage(string? code) => SetLanguage(Parse(code));

    public static AppLanguage Parse(string? code) =>
        string.Equals(code, CodeEn, StringComparison.OrdinalIgnoreCase)
        || string.Equals(code, "en-US", StringComparison.OrdinalIgnoreCase)
        || string.Equals(code, "en-GB", StringComparison.OrdinalIgnoreCase)
            ? AppLanguage.En
            : AppLanguage.PtBr;

    public static string ToCode(AppLanguage language) =>
        language == AppLanguage.En ? CodeEn : CodePtBr;

    public string T(string key) => (_language == AppLanguage.En ? En : PtBr).TryGetValue(key, out var v)
        ? v
        : key;

    public string Format(string key, params object[] args) => string.Format(T(key), args);

    // —— Bound properties (notify via SetLanguage → PropertyChanged(null)) ——

    public string Settings => T("settings");
    public string CompactToggle => T("compact_toggle");
    public string AverageOverall => T("average_overall");
    public string Confidence => T("confidence");
    public string Step => T("step");
    public string Evidences => T("evidences");
    public string Clear => T("clear");
    public string EvidencesFound => T("evidences_found");
    public string EvidencesHint => T("evidences_hint");
    public string Ghosts => T("ghosts");
    public string GhostsLegend => T("ghosts_legend");
    public string Peculiarities => T("peculiarities");
    public string Readings => T("readings");
    public string Opacity => T("opacity");
    public string UiScale => T("ui_scale");
    public string GhostSpeedPercent => T("ghost_speed_percent");
    public string Hotkeys => T("hotkeys");
    public string Demon => T("demon");
    public string Incense => T("incense");
    public string Obambo => T("obambo");
    public string Save => T("save");
    public string LanguageLabel => T("language");
    public string ClearHotkeyHint => T("clear_hotkey_hint");
    public string HideIneligible => T("hide_ineligible");
    public string HideIneligibleHint => T("hide_ineligible_hint");
    public string GhostToggleTip => T("ghost_toggle_tip");
    public string FanDisclaimer => T("fan_disclaimer");

    public string EvidenceDisplayName(EvidenceType type) => type switch
    {
        EvidenceType.Emf5 => T("ev_emf5"),
        EvidenceType.SpiritBox => T("ev_spirit_box"),
        EvidenceType.Ultraviolet => T("ev_ultraviolet"),
        EvidenceType.GhostOrb => T("ev_orb"),
        EvidenceType.GhostWriting => T("ev_writing"),
        EvidenceType.Freezing => T("ev_freezing"),
        EvidenceType.Dots => T("ev_dots"),
        _ => type.ToString()
    };

    public string PatternText(SpeedPattern pattern, string parts) => pattern switch
    {
        SpeedPattern.Stable => Format("pattern_stable", parts),
        SpeedPattern.Accelerating => Format("pattern_accelerating", parts),
        SpeedPattern.Decelerating => Format("pattern_decelerating", parts),
        _ => Format("pattern_irregular", parts)
    };

    public string MouseSideBack => T("mouse_side_back");
    public string MouseSideFront => T("mouse_side_front");
    public string KeySpace => T("key_space");

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private static readonly Dictionary<string, string> PtBr = new(StringComparer.Ordinal)
    {
        ["settings"] = "Configurações",
        ["compact_toggle"] = "Minimizar / Expandir",
        ["average_overall"] = "Média geral",
        ["confidence"] = "Confiabilidade",
        ["step"] = "Passo",
        ["evidences"] = "Evidências",
        ["clear"] = "Limpar",
        ["evidences_found"] = "Evidências encontradas",
        ["evidences_hint"] = "Filtra fantasmas junto com a velocidade. Mimic inclui Orb falsa.",
        ["ghosts"] = "Fantasmas",
        ["ghosts_legend"] = "verde apto · vermelho fora",
        ["peculiarities"] = "Peculiaridades",
        ["readings"] = "Leituras",
        ["opacity"] = "Transparência",
        ["ui_scale"] = "Escala da interface",
        ["ghost_speed_percent"] = "Velocidade do fantasma (%)",
        ["hotkeys"] = "Hotkeys",
        ["demon"] = "Demônio",
        ["incense"] = "Incenso",
        ["obambo"] = "Obambo",
        ["save"] = "Salvar",
        ["language"] = "Idioma",
        ["clear_hotkey_hint"] = "Limpar: Shift + L (fixo)",
        ["hide_ineligible"] = "Ocultar fantasmas fora",
        ["hide_ineligible_hint"] = "Quando ativo, fantasmas vermelhos (fora) somem da lista.",
        ["ev_emf5"] = "EMF Nível 5",
        ["ev_spirit_box"] = "Spirit Box",
        ["ev_ultraviolet"] = "Ultravioleta",
        ["ev_orb"] = "Orbe Fantasma",
        ["ev_writing"] = "Escrita Fantasma",
        ["ev_freezing"] = "Temperatura Baixa",
        ["ev_dots"] = "Projetor D.O.T.S.",
        ["pattern_stable"] = "Estável ({0})",
        ["pattern_accelerating"] = "Acelerando ({0})",
        ["pattern_decelerating"] = "Desacelerando ({0})",
        ["pattern_irregular"] = "Irregular ({0})",
        ["mouse_side_back"] = "Botão lateral (atrás)",
        ["mouse_side_front"] = "Botão lateral (frente)",
        ["key_space"] = "Espaço",
        ["steps_count"] = "{0} passos · {1}/parte ({2} usados)",
        ["steps_min"] = "{0} passos · mín. {1}",
        ["catalog_count"] = "{0} fantasmas no catálogo",
        ["no_catalog_match"] = "Nenhum no catálogo atual",
        ["hotkey_fail_title"] = "Phasmo Hunt",
        ["hotkey_fail_body"] =
            "Não foi possível registrar a(s) hotkey(s): {0}.\nOutro aplicativo pode estar usando a mesma combinação. A preferência foi salva mesmo assim.",
        ["invalid_speed_percent"] = "Informe uma porcentagem de velocidade válida.",
        ["speed_percent_gt_zero"] = "A porcentagem de velocidade deve ser maior que 0.",
        ["hotkey_conflict"] = "Hotkey em conflito: {0} e {1}.",
        ["hotkey_reserved_clear"] = "Shift + L é reservado para Limpar e não pode ser usado.",
        ["session_no_readings"] = "Nenhuma leitura na sessão.",
        ["session_one_reading"] = "1 leitura · {0}",
        ["session_many"] = "{0} leituras · média {1:F2} m/s ({2:F2}–{3:F2}) · {4}",
        ["session_spread_wide"] = "Partes da sessão cobrem faixa larga (possível fantasma variável).",
        ["session_consistent"] = "Leituras consistentes entre si.",
        ["session_diverge"] = "Leituras divergem — compare as 3 partes de cada uma.",
        ["session_no_consensus"] = "Sem consenso ainda",
        ["ghost_toggle_tip"] = "Clique para marcar apto / fora",
        ["fan_disclaimer"] = "Phasmophobia © Kinetic Games Limited. Ferramenta criada pela comunidade"
    };

    private static readonly Dictionary<string, string> En = new(StringComparer.Ordinal)
    {
        ["settings"] = "Settings",
        ["compact_toggle"] = "Minimize / Expand",
        ["average_overall"] = "Overall avg",
        ["confidence"] = "Reliability",
        ["step"] = "Step",
        ["evidences"] = "Evidence",
        ["clear"] = "Clear",
        ["evidences_found"] = "Evidence found",
        ["evidences_hint"] = "Filters ghosts with speed. Mimic includes fake Orb.",
        ["ghosts"] = "Ghosts",
        ["ghosts_legend"] = "green match · red out",
        ["peculiarities"] = "Traits",
        ["readings"] = "Readings",
        ["opacity"] = "Opacity",
        ["ui_scale"] = "UI scale",
        ["ghost_speed_percent"] = "Ghost speed (%)",
        ["hotkeys"] = "Hotkeys",
        ["demon"] = "Demon",
        ["incense"] = "Incense",
        ["obambo"] = "Obambo",
        ["save"] = "Save",
        ["language"] = "Language",
        ["clear_hotkey_hint"] = "Clear: Shift + L (fixed)",
        ["hide_ineligible"] = "Hide ruled-out ghosts",
        ["hide_ineligible_hint"] = "When on, red (out) ghosts disappear from the list.",
        ["ev_emf5"] = "EMF Level 5",
        ["ev_spirit_box"] = "Spirit Box",
        ["ev_ultraviolet"] = "Ultraviolet",
        ["ev_orb"] = "Ghost Orb",
        ["ev_writing"] = "Ghost Writing",
        ["ev_freezing"] = "Freezing Temperatures",
        ["ev_dots"] = "D.O.T.S. Projector",
        ["pattern_stable"] = "Stable ({0})",
        ["pattern_accelerating"] = "Speeding up ({0})",
        ["pattern_decelerating"] = "Slowing down ({0})",
        ["pattern_irregular"] = "Irregular ({0})",
        ["mouse_side_back"] = "Side button (back)",
        ["mouse_side_front"] = "Side button (forward)",
        ["key_space"] = "Space",
        ["steps_count"] = "{0} steps · {1}/part ({2} used)",
        ["steps_min"] = "{0} steps · min. {1}",
        ["catalog_count"] = "{0} ghosts in catalog",
        ["no_catalog_match"] = "None in current catalog",
        ["hotkey_fail_title"] = "Phasmo Hunt",
        ["hotkey_fail_body"] =
            "Could not register hotkey(s): {0}.\nAnother app may be using the same combination. Your preference was saved anyway.",
        ["invalid_speed_percent"] = "Enter a valid speed percentage.",
        ["speed_percent_gt_zero"] = "Speed percentage must be greater than 0.",
        ["hotkey_conflict"] = "Hotkey conflict: {0} and {1}.",
        ["hotkey_reserved_clear"] = "Shift + L is reserved for Clear and cannot be used.",
        ["session_no_readings"] = "No readings in this session.",
        ["session_one_reading"] = "1 reading · {0}",
        ["session_many"] = "{0} readings · avg {1:F2} m/s ({2:F2}–{3:F2}) · {4}",
        ["session_spread_wide"] = "Session parts cover a wide range (possible variable ghost).",
        ["session_consistent"] = "Readings are consistent with each other.",
        ["session_diverge"] = "Readings diverge — compare each reading's 3 parts.",
        ["session_no_consensus"] = "No consensus yet",
        ["ghost_toggle_tip"] = "Click to mark match / out",
        ["fan_disclaimer"] = "Phasmophobia © Kinetic Games Limited. Tool created by community",
    };
}
