using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Platform;
using Xamarin.AirBnB.Lottie;
using Orientation = Android.Widget.Orientation;

namespace Plugin.Maui.LoadingHud;

internal sealed partial class PlatformHud
{
    private Dialog? _dialog;
    private LottieAnimationView? _lottie;

    private static int Dp(Context c, double dp) => (int)Math.Round(dp * c.Resources!.DisplayMetrics!.Density);

    partial void PresentCore(LoadingOptions o, HudContent content)
    {
        var ctx = Platform.CurrentActivity;
        if (ctx == null || _dialog != null) return;

        _dialog = new Dialog(ctx, Android.Resource.Style.ThemeTranslucentNoTitleBar);

        var root = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        root.SetGravity(GravityFlags.Center);
        root.SetBackgroundColor(o.OverlayColor.ToPlatform());

        int pad = Dp(ctx, 16);
        var box = new LinearLayout(ctx) { Orientation = Orientation.Vertical };
        box.SetGravity(GravityFlags.Center);
        box.SetPadding(pad, pad, pad, pad);
        var bg = new GradientDrawable();
        bg.SetColor(o.BackgroundColor.ToPlatform());
        bg.SetCornerRadius(Dp(ctx, 12));
        box.Background = bg;

        box.AddView(CreateLoader(ctx, o, content));

        if (!string.IsNullOrWhiteSpace(content.Text))
        {
            var label = new TextView(ctx) { Text = content.Text, TextSize = 16f, Gravity = GravityFlags.CenterHorizontal };
            label.SetIncludeFontPadding(false);   // no hidden extra space around the text
            label.SetSingleLine(false);           // allow word wrap
            label.SetHorizontallyScrolling(false);
            label.Ellipsize = null;               // never truncate with "..."
            label.SetTextColor(o.TextColor.ToPlatform());
            label.SetPadding(0, Dp(ctx, o.Spacing), 0, 0);
            label.SetMaxWidth(Dp(ctx, 200));
            label.LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            box.AddView(label);
        }

        root.AddView(box, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
        _dialog.SetContentView(root);
        _dialog.SetCancelable(false);
        _dialog.Show();
    }

    private Android.Views.View CreateLoader(Context ctx, LoadingOptions o, HudContent c)
    {
        // 1. Lottie (Resources/Raw -> Android assets). Lottie caches compositions by name.
        if (c.Lottie is { } json && AssetExists(ctx, json))
        {
            int px = Dp(ctx, o.LottieSize);
            _lottie = new LottieAnimationView(ctx)
            {
                LayoutParameters = new LinearLayout.LayoutParams(px, px),
                RepeatCount = LottieDrawable.Infinite
            };
            _lottie.SetAnimation(json);
            _lottie.PlayAnimation();
            return _lottie;
        }

        // 2. Static image (Resources/Images -> drawable)
        if (c.Image is { } img)
        {
            int id = ctx.Resources!.GetIdentifier(Path.GetFileNameWithoutExtension(img).ToLowerInvariant(), "drawable", ctx.PackageName);
            if (id != 0)
            {
                int px = Dp(ctx, o.ImageSize);
                var iv = new ImageView(ctx) { LayoutParameters = new LinearLayout.LayoutParams(px, px) };
                iv.SetScaleType(ImageView.ScaleType.FitCenter);
                iv.SetImageResource(id);
                return iv;
            }
        }

        // 3. Activity indicator fallback
        int size = Dp(ctx, o.IndicatorSize);
        var pb = new Android.Widget.ProgressBar(ctx) { Indeterminate = true, LayoutParameters = new LinearLayout.LayoutParams(size, size) };
        pb.IndeterminateTintList = ColorStateList.ValueOf(o.IndicatorColor.ToPlatform());
        return pb;
    }

    private static bool AssetExists(Context ctx, string name)
    {
        try { using var s = ctx.Assets!.Open(name); return true; }
        catch { return false; }
    }

    partial void DismissCore()
    {
        if (_dialog == null) return;
        _lottie?.CancelAnimation();
        _lottie?.Dispose();
        _lottie = null;
        _dialog.Dismiss();
        _dialog.Dispose();
        _dialog = null;
    }
}
