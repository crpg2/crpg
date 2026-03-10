using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace LauncherV3;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    private readonly ConcurrentQueue<string> _messageQueue = new();
    private readonly DispatcherTimer _flushTimer;
    private static readonly MainViewModel ViewModel = new();

    public MainWindow()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) => Log(args?.ExceptionObject.ToString() ?? string.Empty);
        InitializeComponent();
        DataContext = ViewModel;
        Instance = this;
        _flushTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(2000),
        };
        _flushTimer.Tick += FlushTimer_Tick;
        _flushTimer.Start();
        ViewModel.RequestClose += (sender, e) => Close();
    }

    public void WriteToConsole(string text)
    {
        _messageQueue.Enqueue(text);
    }

    private void Log(string exception)
    {
        File.AppendAllText("appErrorLog.txt", exception);
    }

    private void LogException(Exception ex)
    {
        if (ex != null)
        {
            File.AppendAllText("appErrorLog.txt", ex.ToString());
        }
    }

    private void Window_Loaded(object sender, EventArgs e)
    {
        if (!ViewModel.HasWritePermissionOnConfigDir())
        {
            MessageBox.Show(
                "Please extract the launcher first and put it in a folder where you have write permission.",
                "Write Permission Required",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            Close();
        }

        ViewModel.StartRoutine();
    }

    private async void FlushTimer_Tick(object? sender, EventArgs e)
    {
        _flushTimer?.Stop();
        await FlushTextToTextBoxAsync();
        _flushTimer?.Start();
    }

    private async Task FlushTextToTextBoxAsync()
    {
        StringBuilder sb = new();
        string viewModelText = ViewModel.FlushText();
        if (viewModelText != string.Empty)
        {
            sb.AppendLine(viewModelText);
        }

        while (_messageQueue.TryDequeue(out string? text))
        {
            sb.AppendLine(text);
        }

        consoleTextBox.AppendText(sb.ToString());
        consoleTextBox.ScrollToEnd();
    }

    private void MinimizeButton_Click(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            e.Handled = true;
        }
        else
        {
            WindowState = WindowState.Minimized;
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true,
        });
        e.Handled = true;
    }

    private void CloseButton_Click(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            e.Handled = true;
        }
        else
        {
            Close();
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }

    private void LocationButton_Click(object sender, RoutedEventArgs e)
    {
    }
}
