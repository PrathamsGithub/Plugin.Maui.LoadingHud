using CoreGraphics;
using Lottie;
using Microsoft.Maui.Platform;
using UIKit;

namespace Plugin.Maui.LoadingHud;

internal sealed partial class PlatformHud
{
    private const double Padding = 16;
    private const double MaxTextWidth = 200;
    private const double SpinnerNativeSize = 37; // UIActivityIndicatorViewStyle.Large

    // Single-slot cache: only the most recently used animation stays in memory.
    private static string? _cachedName;
    private static LottieAnimation? _cachedAnimation;

    private UIWindow? _window;
    private LottieAnimationView? _animationView;
    private int _version; // invalidates in-flight async loads after Dismiss()

    async partial void PresentCore(LoadingOptions o, HudContent content)
    {
        if (_window != null) return;

        var scene = UIApplication.SharedApplication.ConnectedScenes.ToArray()
            .FirstOrDefault(s => s.ActivationState == UISceneActivationState.ForegroundActive) as UIWindowScene;
        if (scene == null) return;

        int version = ++_version;
        _window = new UIWindow(scene)
        {
            WindowLevel = UIWindowLevel.Alert,
            BackgroundColor = o.OverlayColor.ToPlatform(),
            RootViewController = new UIViewController()
        };
        _window.RootViewController.View!.BackgroundColor = UIColor.Clear;
        _window.MakeKeyAndVisible(); // overlay blocks touches immediately

        UIView? asset = null;
        double size = 0;
        Action? start = null;

        // 1. Lottie
        if (content.Lottie is { } json)
        {
            try
            {
                var anim = await LoadAnimationAsync(json);
                if (version != _version || _window == null) return; // dismissed while loading
                var lv = new LottieAnimationView
                {
                    TranslatesAutoresizingMaskIntoConstraints = true,
                    ContentMode = UIViewContentMode.ScaleAspectFit,
                    Animation = anim,
                    LoopMode = LottieLoopMode.Loop
                };
                _animationView = lv;
                asset = lv;
                size = o.LottieSize;
                start = () => lv.Play();
            }
            catch
            {
                _animationView = null;
                if (version != _version || _window == null) return;
            }
        }

        // 2. Static image (Resources/Images)
        if (asset == null && content.Image is { } img)
        {
            var ui = UIImage.FromBundle(Path.GetFileNameWithoutExtension(img));
            if (ui != null)
            {
                asset = new UIImageView(ui) { ContentMode = UIViewContentMode.ScaleAspectFit };
                size = o.ImageSize;
            }
        }

        // 3. Activity indicator fallback
        if (asset == null)
        {
            var spinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Large)
            {
                Color = o.IndicatorColor.ToPlatform(),
                Frame = new CGRect(0, 0, SpinnerNativeSize, SpinnerNativeSize)
            };
            double scale = o.IndicatorSize / SpinnerNativeSize;
            spinner.Transform = CGAffineTransform.MakeScale((nfloat)scale, (nfloat)scale);
            asset = spinner;
            size = o.IndicatorSize;
            start = () => spinner.StartAnimating();
        }

        // Label (measured, so the gap to the asset is exactly Spacing)
        UILabel? label = null;
        double labelW = 0, labelH = 0;
        if (!string.IsNullOrWhiteSpace(content.Text))
        {
            label = new UILabel
            {
                Text = content.Text,
                TextColor = o.TextColor.ToPlatform(),
                TextAlignment = UITextAlignment.Center,
                Font = UIFont.BoldSystemFontOfSize(14),
                Lines = 0,                                   // unlimited lines -> word wrap, never truncate
                LineBreakMode = UILineBreakMode.WordWrap,
                AdjustsFontSizeToFitWidth = false
            };
            var fit = label.SizeThatFits(new CGSize(MaxTextWidth, 10000));

            // Round UP: a fractional width/height makes UILabel clip or truncate the last characters
            labelW = Math.Min(MaxTextWidth, Math.Ceiling((double)fit.Width) + 1);
            labelH = Math.Ceiling((double)fit.Height) + 1;
        }

        double w = Math.Max(size, labelW) + 2 * Padding;
        double h = Padding + size + (label != null ? o.Spacing + labelH : 0) + Padding;

        var box = new UIView
        {
            BackgroundColor = o.BackgroundColor.ToPlatform(),
            ClipsToBounds = true,
            Frame = new CGRect((_window.Bounds.Width - w) / 2, (_window.Bounds.Height - h) / 2, w, h)
        };
        box.Layer.CornerRadius = 16;

        if (asset is UIActivityIndicatorView) { /* frame already set, transform scales it */ }
        else asset.Frame = new CGRect(0, 0, size, size);
        asset.Center = new CGPoint(w / 2, Padding + size / 2);
        box.AddSubview(asset);

        if (label != null)
        {
            label.Frame = new CGRect((w - labelW) / 2, Padding + size + o.Spacing, labelW, labelH);
            box.AddSubview(label);
        }

        _window.RootViewController!.View!.AddSubview(box);
        start?.Invoke(); // play AFTER the view is in the hierarchy
    }

    private static async Task<LottieAnimation?> LoadAnimationAsync(string name)
    {
        if (_cachedName == name && _cachedAnimation != null) return _cachedAnimation;

        using var stream = await FileSystem.OpenAppPackageFileAsync(name);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        _cachedAnimation = LottieAnimation.From(ms.ToArray());
        _cachedName = name;
        return _cachedAnimation;
    }

    partial void DismissCore()
    {
        _version++;
        if (_window == null) return;
        _animationView?.Stop();
        _animationView?.RemoveFromSuperview();
        _animationView = null;
        _window.Hidden = true;
        _window.Dispose();
        _window = null;
    }
}
