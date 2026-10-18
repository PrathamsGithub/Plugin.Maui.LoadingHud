namespace Plugin.Maui.LoadingHud;

/// <summary>What to render for one Show() call (after merging per-call values with defaults).</summary>
internal readonly record struct HudContent(string? Text, string? Lottie, string? Image);

public static class LoadingHud
{
    internal static LoadingOptions Options { get; set; } = new();

    private static PlatformHud? _hud;
    private static int _count;       // nested Show() calls (main thread only)
    private static int _generation;  // bumped by Hide() so stale tokens are ignored

    /// <summary>
    /// Shows the loader. Everything is optional. Priority: per-call Lottie → per-call Image →
    /// default Lottie → default Image → activity indicator. Text falls back to <c>LoadingOptions.Text</c>
    /// when null; pass "" to show no text.
    /// Dispose the result (or use <c>using</c>) to hide it. Nested calls are reference counted.
    /// </summary>
    public static IDisposable Show(string? text = null, string? lottie = null, string? image = null)
    {
        var content = Resolve(text ?? Options.Text, lottie, image);
        int gen = Volatile.Read(ref _generation);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_count++ == 0)
                (_hud ??= new PlatformHud()).Present(Options, content);
        });

        return new Token(gen);
    }

    /// <summary>Force-hides the loader no matter how many Show() calls are outstanding.</summary>
    public static void Hide() => MainThread.BeginInvokeOnMainThread(() =>
    {
        Interlocked.Increment(ref _generation);
        _count = 0;
        _hud?.Dismiss();
    });

    private static HudContent Resolve(string? text, string? lottie, string? image)
    {
        // Per-call asset wins over defaults; within a level, Lottie wins over Image.
        if (!string.IsNullOrWhiteSpace(lottie)) return new(text, lottie, null);
        if (!string.IsNullOrWhiteSpace(image)) return new(text, null, image);
        if (!string.IsNullOrWhiteSpace(Options.Lottie)) return new(text, Options.Lottie, null);
        if (!string.IsNullOrWhiteSpace(Options.Image)) return new(text, null, Options.Image);
        return new(text, null, null); // activity indicator
    }

    private sealed class Token(int generation) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (generation != _generation || _count == 0) return;
                if (--_count == 0) _hud?.Dismiss();
            });
        }
    }
}

public static class LoadingHudExtensions
{
    /// <summary>Call in MauiProgram: <c>builder.UseLoadingHud(o => o.Lottie = "loader.json");</c></summary>
    public static MauiAppBuilder UseLoadingHud(this MauiAppBuilder builder, Action<LoadingOptions>? configure = null)
    {
        var options = new LoadingOptions();
        configure?.Invoke(options);
        LoadingHud.Options = options;
        return builder;
    }
}
