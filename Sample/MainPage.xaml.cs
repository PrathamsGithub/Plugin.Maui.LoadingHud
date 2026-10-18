using Plugin.Maui.LoadingHud;

namespace Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void Loader_without_Text_Clicked(object sender, EventArgs e)
	{
		using (LoadingHud.Show())
			await Task.Delay(3000);
	}

	private async void Loader_with_Text_Clicked(object sender, EventArgs e)
	{
		using (LoadingHud.Show("Loading..."))
			await Task.Delay(3000);
	}

	private async void Loader_with_Lottie_Clicked(object sender, EventArgs e)
	{
		LoadingHud.Show("Loading...", "loadinglottie.json");
		await Task.Delay(3000);
		LoadingHud.Hide();
	}

	private async void Loader_with_Image_Clicked(object sender, EventArgs e)
	{
		LoadingHud.Show("Loading...", null, "dotnet_bot.png");
		await Task.Delay(3000);
		LoadingHud.Hide();
	}
}
