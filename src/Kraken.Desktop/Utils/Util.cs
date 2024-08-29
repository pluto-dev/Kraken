using System.Linq;
using System.Threading.Tasks;
using Kraken.Desktop.Services;

namespace Kraken.Desktop.Utils;

public static class Util
{
    public static async Task InitializeAsync()
    {
        await InitializeKrakenAsync();
    }

    private static async Task InitializeKrakenAsync()
    {
        var storage = StorageService.Instance;
        var krakenService = KrakenService.Instance;

        var device = await krakenService.GetDeviceAsync(8199, 7793);
        var colorModes = await storage.ReadColorModeSettingsAsync();
        var selectedRingMode = await storage.ReadSettingAsync<string>(
            StorageService.SelectedRingModeKey
        );
        var selectedLogoMode = await storage.ReadSettingAsync<string>(
            StorageService.SelectedLogoModeKey
        );

        var ringMode = colorModes?["ring"][selectedRingMode];
        var logoMode = colorModes?["logo"][selectedLogoMode];

        await krakenService.SetColorMode(
            device?.Id,
            "ring",
            ringMode?.Mode.ToLower(),
            ringMode?.Colors?.ToList(),
            ringMode?.AnimationSpeed?.ToLower(),
            ringMode?.Direction?.ToLower()
        );

        await krakenService.SetColorMode(
            device?.Id,
            "logo",
            logoMode?.Mode.ToLower(),
            logoMode?.Colors?.ToList(),
            logoMode?.AnimationSpeed?.ToLower(),
            logoMode?.Direction?.ToLower()
        );
    }
}
