namespace Plugin.Maui.LoadingHud;

// Shared entry points. Each platform implements PresentCore/DismissCore in Platforms/<OS>/PlatformHud.cs.
// On targets with no implementation (plain net10.0) the partial methods are removed by the compiler = no-op.
internal sealed partial class PlatformHud
{
    public void Present(LoadingOptions options, HudContent content) => PresentCore(options, content);
    public void Dismiss() => DismissCore();

    partial void PresentCore(LoadingOptions options, HudContent content);
    partial void DismissCore();
}
