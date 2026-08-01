using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhasmoHunt.Models;

namespace PhasmoHunt.Services;

public static class EvidenceIconService
{
    private static readonly Dictionary<EvidenceType, ImageSource> Icons = CreateIcons();

    public static ImageSource? GetIcon(EvidenceType evidence) =>
        Icons.TryGetValue(evidence, out var icon) ? icon : null;

    public static IReadOnlyList<ImageSource> GetIcons(GhostInfo ghost)
    {
        var types = ghost.HasFakeGhostOrb
            ? ghost.Evidences.Append(EvidenceType.GhostOrb).Distinct()
            : ghost.Evidences;

        return types
            .Select(GetIcon)
            .Where(icon => icon is not null)
            .Cast<ImageSource>()
            .ToArray();
    }

    private static Dictionary<EvidenceType, ImageSource> CreateIcons()
    {
        var map = new Dictionary<EvidenceType, string>
        {
            [EvidenceType.Emf5] = "emf5-icon.png",
            [EvidenceType.SpiritBox] = "spirit-box-icon.png",
            [EvidenceType.Ultraviolet] = "fingerprints-icon.png",
            [EvidenceType.GhostOrb] = "orbs-icon.png",
            [EvidenceType.GhostWriting] = "writing-icon.png",
            [EvidenceType.Freezing] = "freezing-icon.png",
            [EvidenceType.Dots] = "dots-icon.png"
        };

        var result = new Dictionary<EvidenceType, ImageSource>();
        foreach (var (type, fileName) in map)
        {
            var uri = new Uri($"pack://application:,,,/Assets/Evidence/{fileName}", UriKind.Absolute);
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                result[type] = bitmap;
            }
            catch
            {
                // Ícone ausente nos assets — fantasma fica sem esse ícone.
            }
        }

        return result;
    }
}
