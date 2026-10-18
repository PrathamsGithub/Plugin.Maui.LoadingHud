# Plugin.Maui.LoadingHud

A tiny loading overlay for .NET MAUI (Android + iOS). Show a Lottie, an image or a spinner, with optional text.

```csharp
using (LoadingHud.Show("Loading..."))
    await api.GetDataAsync();
```

## Demo

| Android | iOS |
|:---:|:---:|
| ![Android](https://github.com/user-attachments/assets/4cb3ff17-f499-4821-a44b-c7172e040167) | ![iOS](https://github.com/user-attachments/assets/84998022-7c5c-48ef-9b22-9fb24745f482) |

## Why use it

- **No page events:** it's a native overlay, so `OnAppearing` / `OnDisappearing` don't fire and there's no navigation to deadlock.
- **No overlapping loaders:** nested `Show()` calls are reference counted, and the loader hides when the last one is disposed.
- **Configure once:** set defaults in `MauiProgram` and override per call only when needed.
- **Lightweight:** views are disposed on hide, so nothing stays in memory.

## Features

- Lottie, image or spinner (spinner is the automatic fallback, including when a file is missing)
- Global defaults: text, Lottie, image, colors, sizes, spacing
- Per-call override of text, Lottie and image
- Word-wrapped text, thread-safe, blocks touches while loading

## Install

```
dotnet add package Plugin.Maui.LoadingHud
```

## Setup (`MauiProgram.cs`)

All options are optional. With none set you get a white spinner.

```csharp
builder.UseLoadingHud(o =>
{
    o.Text            = "Please wait...";               // default text
    o.Lottie          = "loader.json";                  // Resources/Raw
    o.Image           = "logo.png";                     // Resources/Images (used if no Lottie)
    o.IndicatorColor  = Colors.White;
    o.BackgroundColor = Color.FromArgb("#80000000");
    o.OverlayColor    = Color.FromArgb("#66000000");
    o.TextColor       = Colors.White;
    o.IndicatorSize   = 40;
    o.LottieSize      = 100;
    o.ImageSize       = 60;
    o.Spacing         = 8;                              // gap between loader and text
});
```

## Usage

```csharp
using (LoadingHud.Show()) { ... }                                  // defaults
using (LoadingHud.Show("Saving...")) { ... }                       // custom text
using (LoadingHud.Show("")) { ... }                                // no text
using (LoadingHud.Show("Uploading", lottie: "upload.json")) { ... }
using (LoadingHud.Show(image: "logo.png")) { ... }

LoadingHud.Hide();                                                 // force-hide all
```

`Show(string? text = null, string? lottie = null, string? image = null)`

**Loader priority:** per-call Lottie, per-call image, default Lottie, default image, spinner.
**Text:** per-call text, otherwise `o.Text`. Pass `""` for none.

## Sample

```csharp
private async void Loader_without_Text_Clicked(object s, EventArgs e)
{
    using (LoadingHud.Show("")) await Task.Delay(3000);
}

private async void Loader_with_Text_Clicked(object s, EventArgs e)
{
    using (LoadingHud.Show("Loading...")) await Task.Delay(3000);
}

private async void Loader_with_Lottie_Clicked(object s, EventArgs e)
{
    using (LoadingHud.Show("Loading...", lottie: "loadinglottie.json")) await Task.Delay(3000);
}

private async void Loader_with_Image_Clicked(object s, EventArgs e)
{
    using (LoadingHud.Show("Loading...", image: "dotnet_bot.png")) await Task.Delay(3000);
}
```

```xml
<VerticalStackLayout Padding="30,0" Spacing="25">
    <Button Text="Loader without Text" Clicked="Loader_without_Text_Clicked"/>
    <Button Text="Loader with Text"    Clicked="Loader_with_Text_Clicked"/>
    <Button Text="Loader with Lottie"  Clicked="Loader_with_Lottie_Clicked"/>
    <Button Text="Loader with Image"   Clicked="Loader_with_Image_Clicked"/>
</VerticalStackLayout>
```

## Options

| Option | Default | Description |
|---|---|---|
| `Text` | none | Default message |
| `Lottie` | none | Lottie JSON name (`Resources/Raw`) |
| `Image` | none | Image name (`Resources/Images`) |
| `IndicatorColor` | White | Spinner color |
| `BackgroundColor` | `#80000000` | Box behind the loader |
| `OverlayColor` | `#66000000` | Dim layer over the screen |
| `TextColor` | White | Text color |
| `IndicatorSize` | 40 | Spinner size |
| `LottieSize` | 100 | Lottie size |
| `ImageSize` | 60 | Image size |
| `Spacing` | 8 | Gap between loader and text |

## Notes

- Android image names must be lowercase (`logo.png`).
- Platforms: Android and iOS. Other targets compile but show nothing.
