using Microsoft.Extensions.Logging;
using Plugin.Maui.LoadingHud;

namespace Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseLoadingHud(options =>
			{
				// options.IndicatorColor = Colors.Blue;
				// options.BackgroundColor = Colors.Yellow;
				// options.OverlayColor = Color.FromArgb("#400000");
				// options.TextColor = Colors.White;
				// options.Image = "dotnet_bot.png";
				// options.Lottie = "loadinglottie.json";
				options.IndicatorSize = 50;
				// options.Text = "Please Wait...";
				options.Spacing = 10;
				options.ImageSize = 100;
				options.LottieSize = 200;
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
