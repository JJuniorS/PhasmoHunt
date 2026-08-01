using PhasmoHunt.Models;

namespace PhasmoHunt.Services;

/// <summary>
/// Offline reference catalog (speeds + evidence) from community/wiki data.
/// Never reads data from the game process.
/// </summary>
public sealed class GhostCatalogService
{
    private readonly IReadOnlyList<GhostInfo> _ghosts =
    [
        Fixed("aswang", "Aswang", 1.53, [E.Freezing, E.GhostWriting, E.Dots], "Base mais lenta; acelera mais rápido com LoS."),
        Fixed("banshee", "Banshee", 1.7, [E.Dots, E.GhostOrb, E.Ultraviolet], "Velocidade padrão com LoS."),
        Range("dayan", "Dayan", 1.7, 1.2, 2.25, [E.Emf5, E.GhostOrb, E.SpiritBox], "2.25 andando ≤10 m; 1.2 parado ≤10 m; 1.7 além."),
        Fixed("demon", "Demon", 1.7, [E.GhostWriting, E.Ultraviolet, E.Freezing], "Velocidade padrão com LoS."),
        Range("deogen", "Deogen", 3.0, 0.4, 3.0, [E.Dots, E.GhostWriting, E.SpiritBox], "3.0 longe; ~0.4 perto."),
        Range("deildegast", "Deildegast", 3.0, 0.4, 3.0, [E.Emf5, E.GhostWriting, E.Dots], "Rápido com poucas interações; lento após muitas."),
        Range("gallu", "Gallu", 1.7, 1.36, 1.96, [E.Emf5, E.Ultraviolet, E.SpiritBox], "Normal/Enraivecido/Enfraquecido."),
        Fixed("goryo", "Goryo", 1.7, [E.Dots, E.Emf5, E.Ultraviolet], "Velocidade padrão com LoS."),
        Range("hantu", "Hantu", 2.0, 1.44, 2.7, [E.GhostOrb, E.Ultraviolet, E.Freezing], "Mais lento no calor; mais rápido no frio."),
        Range("jinn", "Jinn", 1.7, 1.7, 2.5, [E.Emf5, E.Ultraviolet, E.Freezing], "2.5 com fusível + LoS + distância."),
        Range("kormos", "Kormos", 1.7, 1.7, 2.21, [E.GhostOrb, E.SpiritBox, E.Ultraviolet], "Pode acelerar se detectar passos à distância."),
        Fixed("mare", "Mare", 1.7, [E.GhostWriting, E.GhostOrb, E.SpiritBox], "Velocidade padrão com LoS."),
        Range("moroi", "Moroi", 3.71, 1.5, 3.71, [E.GhostWriting, E.Freezing, E.SpiritBox], "Sobe com sanidade baixa (até 3.71 m/s)."),
        Fixed("myling", "Myling", 1.7, [E.GhostWriting, E.Emf5, E.Ultraviolet], "Passos mais silenciosos."),
        Fixed("obake", "Obake", 1.7, [E.Emf5, E.GhostOrb, E.Ultraviolet], "Velocidade padrão com LoS."),
        Range("obambo", "Obambo", 1.7, 1.45, 1.96, [E.GhostWriting, E.Ultraviolet, E.Dots], "Calmo ~1.45 / agressivo ~1.96."),
        Fixed("oni", "Oni", 1.7, [E.Dots, E.Emf5, E.Freezing], "Velocidade padrão com LoS."),
        Fixed("onryo", "Onryo", 1.7, [E.GhostOrb, E.Freezing, E.SpiritBox], "Velocidade padrão com LoS."),
        Fixed("phantom", "Phantom", 1.7, [E.Dots, E.Ultraviolet, E.SpiritBox], "Velocidade padrão com LoS."),
        Fixed("poltergeist", "Poltergeist", 1.7, [E.GhostWriting, E.Ultraviolet, E.SpiritBox], "Velocidade padrão com LoS."),
        Range("raiju", "Raiju", 1.7, 1.7, 2.5, [E.Dots, E.Emf5, E.GhostOrb], "2.5 perto de eletrônicos ativos."),
        Range("revenant", "Revenant", 1.0, 1.0, 3.0, [E.GhostWriting, E.GhostOrb, E.Freezing], "1.0 sem alvo; 3.0 ao detectar."),
        Fixed("shade", "Shade", 1.7, [E.GhostWriting, E.Emf5, E.Freezing], "Velocidade padrão com LoS."),
        Fixed("spirit", "Spirit", 1.7, [E.GhostWriting, E.Emf5, E.SpiritBox], "Velocidade padrão com LoS."),
        Range("thaye", "Thaye", 2.75, 1.0, 2.75, [E.Dots, E.GhostWriting, E.GhostOrb], "2.75 jovem → 1.0 velho."),
        Mimic("the-mimic", "The Mimic", 1.7, 0.4, 3.71, [E.Ultraviolet, E.Freezing, E.SpiritBox], "Copia velocidade; Orb falsa sempre."),
        Range("the-twins", "The Twins", 1.5, 1.5, 1.9, [E.Emf5, E.Freezing, E.SpiritBox], "Principal ~1.5; decoy ~1.9."),
        Fixed("wraith", "Wraith", 1.7, [E.Dots, E.Emf5, E.SpiritBox], "Velocidade padrão com LoS."),
        Fixed("yokai", "Yokai", 1.7, [E.Dots, E.GhostOrb, E.SpiritBox], "Velocidade padrão com LoS."),
        Fixed("yurei", "Yurei", 1.7, [E.Dots, E.GhostOrb, E.Freezing], "Velocidade padrão com LoS.")
    ];

    public IReadOnlyList<GhostInfo> GetAll() => _ghosts;

    public IReadOnlyList<GhostInfo> FindBySpeed(double speedMps, double toleranceMps = SpeedCalculatorService.SpeedMatchToleranceMps)
    {
        return _ghosts
            .Where(g => g.MatchesSpeed(speedMps, toleranceMps))
            .OrderBy(g => Math.Abs(g.BaseSpeedMps - speedMps))
            .ThenBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static GhostInfo Fixed(string id, string name, double speed, EvidenceType[] evidences, string notes) =>
        new()
        {
            Id = id,
            Name = name,
            BaseSpeedMps = speed,
            MinSpeedMps = speed,
            MaxSpeedMps = speed,
            Evidences = evidences,
            SpeedNotes = notes
        };

    private static GhostInfo Range(
        string id, string name, double baseSpeed, double min, double max,
        EvidenceType[] evidences, string notes) =>
        new()
        {
            Id = id,
            Name = name,
            BaseSpeedMps = baseSpeed,
            MinSpeedMps = min,
            MaxSpeedMps = max,
            Evidences = evidences,
            SpeedNotes = notes
        };

    private static GhostInfo Mimic(
        string id, string name, double baseSpeed, double min, double max,
        EvidenceType[] evidences, string notes) =>
        new()
        {
            Id = id,
            Name = name,
            BaseSpeedMps = baseSpeed,
            MinSpeedMps = min,
            MaxSpeedMps = max,
            Evidences = evidences,
            SpeedNotes = notes,
            HasFakeGhostOrb = true
        };

    private static class E
    {
        public const EvidenceType Emf5 = EvidenceType.Emf5;
        public const EvidenceType SpiritBox = EvidenceType.SpiritBox;
        public const EvidenceType Ultraviolet = EvidenceType.Ultraviolet;
        public const EvidenceType GhostOrb = EvidenceType.GhostOrb;
        public const EvidenceType GhostWriting = EvidenceType.GhostWriting;
        public const EvidenceType Freezing = EvidenceType.Freezing;
        public const EvidenceType Dots = EvidenceType.Dots;
    }
}
