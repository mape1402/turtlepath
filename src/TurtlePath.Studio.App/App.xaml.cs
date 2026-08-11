namespace TurtlePath.Studio.App;

public partial class App : Microsoft.Maui.Controls.Application
{
	private readonly MainPage mainPage;

	public App(MainPage mainPage)
	{
		this.mainPage = mainPage;
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(mainPage)
		{
			Title = "TurtlePath Studio",
			Width = 1440,
			Height = 900,
			MinimumWidth = 1180,
			MinimumHeight = 760
		};
	}
}
