namespace TurtlePath.Studio.App;

public partial class App : Microsoft.Maui.Controls.Application
{
	private const int DefaultWindowWidth = 1440;
	private const int DefaultWindowHeight = 900;

	private readonly MainPage mainPage;

	public App(MainPage mainPage)
	{
		this.mainPage = mainPage;
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(mainPage)
		{
			Title = "TurtlePath Studio",
			Width = DefaultWindowWidth,
			Height = DefaultWindowHeight,
			MinimumWidth = 1180,
			MinimumHeight = 760
		};

#if WINDOWS
		window.Created += (_, _) => CenterWindow(window);
#endif

		return window;
	}

#if WINDOWS
	private static void CenterWindow(Window window)
	{
		if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window platformWindow)
			return;

		var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(platformWindow);
		var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
		var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
		var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(
			windowId,
			Microsoft.UI.Windowing.DisplayAreaFallback.Primary);

		var workArea = displayArea.WorkArea;
		var width = appWindow.Size.Width > 0 ? appWindow.Size.Width : DefaultWindowWidth;
		var height = appWindow.Size.Height > 0 ? appWindow.Size.Height : DefaultWindowHeight;
		var x = workArea.X + Math.Max(0, (workArea.Width - width) / 2);
		var y = workArea.Y + Math.Max(0, (workArea.Height - height) / 2);

		appWindow.MoveAndResize(new Windows.Graphics.RectInt32(x, y, width, height));
	}
#endif
}
