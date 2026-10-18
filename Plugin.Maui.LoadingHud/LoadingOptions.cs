namespace Plugin.Maui.LoadingHud;

/// <summary>Global defaults, set once in MauiProgram via <c>UseLoadingHud</c>.</summary>
public sealed class LoadingOptions
{
    /// <summary>Default message shown under the loader on every screen. A per-call text (even "") overrides it.</summary>
    public string? Text { get; set; }

    /// <summary>Lottie JSON file name in Resources/Raw (e.g. "loader.json"). Highest priority default.</summary>
    public string? Lottie { get; set; }

    /// <summary>MAUI image name from Resources/Images (e.g. "logo.png"). Used when no Lottie is set.</summary>
    public string? Image { get; set; }

    /// <summary>Color of the activity indicator fallback.</summary>
    public Color IndicatorColor { get; set; } = Colors.White;

    /// <summary>Color of the rounded box behind the loader.</summary>
    public Color BackgroundColor { get; set; } = Color.FromArgb("#80000000");

    /// <summary>Full-screen dim color behind the box.</summary>
    public Color OverlayColor { get; set; } = Color.FromArgb("#66000000");

    /// <summary>Message text color.</summary>
    public Color TextColor { get; set; } = Colors.White;

    /// <summary>Activity indicator size (device-independent units).</summary>
    public double IndicatorSize { get; set; } = 40;

    /// <summary>Lottie animation size (device-independent units).</summary>
    public double LottieSize { get; set; } = 100;

    /// <summary>Image size (device-independent units).</summary>
    public double ImageSize { get; set; } = 60;

    /// <summary>Gap between the indicator/lottie/image and the text.</summary>
    public double Spacing { get; set; } = 8;
}
