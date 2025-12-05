using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Runtime.InteropServices;
using Windows.Media.Core;
using Windows.Media.Playback;
using WinRT.Interop;
using Windows.Graphics;

namespace EyeCareReminder
{
    public sealed partial class MainWindow : Window
    {
        // Timer settings (in seconds)
        private int workDuration = 20 * 60; // 20 minutes
        private int restDuration = 20; // 20 seconds
        
        // Current state
        private int remainingSeconds;
        private int totalSeconds;
        private bool isWorkPhase = true;
        private bool isRunning = false;
        
        // Timer
        private DispatcherTimer timer = default!;
        
        // Window handle for topmost
        private IntPtr hWnd;
        private AppWindow appWindow = default!;
        
        // Window size states
        private RectInt32 normalSize;
        private RectInt32 miniSize;
        private bool isMiniMode = false;

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeWindow();
            InitializeTimer();
            ResetTimer();
        }

        private void InitializeWindow()
        {
            // Get window handle
            hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            appWindow = AppWindow.GetFromWindowId(windowId);

            // Set window to always on top
            SetWindowTopMost(true);

            // Center the window and set sizes
            if (appWindow != null)
            {
                var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
                var workArea = displayArea.WorkArea;
                
                // Normal size
                var normalWidth = 320;
                var normalHeight = 280;
                var x = (workArea.Width - normalWidth) / 2;
                var y = (workArea.Height - normalHeight) / 2;
                
                normalSize = new RectInt32(x, y, normalWidth, normalHeight);
                
                // Mini mode size (small timer display only) - positioned in bottom right corner
                var miniWidth = 180;
                var miniHeight = 80;
                miniSize = new RectInt32(workArea.Width - miniWidth - 20, workArea.Height - miniHeight - 60, miniWidth, miniHeight);
                
                appWindow.MoveAndResize(normalSize);
                
                // Listen for window resize
                appWindow.Changed += AppWindow_Changed;
            }
            
            // Add double-click handler to switch modes
            this.Content.DoubleTapped += Content_DoubleTapped;
            
            // Initialize mini mode button text
            UpdateMiniModeButtonText();
        }

        private void Content_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            ToggleMiniMode();
        }

        private void UpdateMiniModeButtonText()
        {
            if (MiniModeButton != null)
            {
                MiniModeButton.Content = isMiniMode ? "🖥️ Normal Mode" : "🪟 Mini Mode";
            }
        }

        private void MiniModeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMiniMode();
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            // Keep window always on top
            if (args.DidPresenterChange || args.DidSizeChange)
            {
                SetWindowTopMost(true);
            }
        }

        private void ToggleMiniMode()
        {
            isMiniMode = !isMiniMode;
            UpdateMiniModeButtonText();
            
            if (isMiniMode)
            {
                // Switch to mini mode - show only timer
                appWindow.MoveAndResize(miniSize);
                // Hide all UI elements except timer
                TitleTextBlock.Visibility = Visibility.Collapsed;
                StatusBorder.Visibility = Visibility.Collapsed;
                ControlButtonsPanel.Visibility = Visibility.Collapsed;
                MiniModeButton.Visibility = Visibility.Collapsed;
                SettingsButton.Visibility = Visibility.Collapsed;
                
                // Adjust timer for mini mode - center it with minimal padding
                TimerViewbox.Margin = new Thickness(4);
                TimerViewbox.VerticalAlignment = VerticalAlignment.Center;
                TimerViewbox.HorizontalAlignment = HorizontalAlignment.Center;
                
                // Make timer text larger and more prominent for mini mode
                TimerText.FontSize = 28;
                TimerText.FontWeight = new Windows.UI.Text.FontWeight(700);
                PhaseText.Visibility = Visibility.Collapsed;
                ProgressRing.Width = 50;
                ProgressRing.Height = 50;
            }
            else
            {
                // Switch to normal mode
                appWindow.MoveAndResize(normalSize);
                // Show all UI elements
                TitleTextBlock.Visibility = Visibility.Visible;
                StatusBorder.Visibility = Visibility.Visible;
                ControlButtonsPanel.Visibility = Visibility.Visible;
                MiniModeButton.Visibility = Visibility.Visible;
                SettingsButton.Visibility = Visibility.Visible;
                
                // Restore normal timer layout
                TimerViewbox.Margin = new Thickness(0, 12, 0, 12);
                TimerViewbox.VerticalAlignment = VerticalAlignment.Stretch;
                TimerViewbox.HorizontalAlignment = HorizontalAlignment.Stretch;
                
                // Restore timer text size and weight
                TimerText.FontSize = 32;
                TimerText.FontWeight = new Windows.UI.Text.FontWeight(700);
                PhaseText.Visibility = Visibility.Visible;
                ProgressRing.Width = 120;
                ProgressRing.Height = 120;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;

        private void SetWindowTopMost(bool topmost)
        {
            SetWindowPos(hWnd, topmost ? HWND_TOPMOST : IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, object e)
        {
            remainingSeconds--;
            UpdateDisplay();

            if (remainingSeconds <= 0)
            {
                timer.Stop();
                isRunning = false;
                OnPhaseComplete();
            }
        }

        private void UpdateDisplay()
        {
            // Update timer text
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            TimerText.Text = $"{minutes:D2}:{seconds:D2}";

            // Update progress ring
            double progress = ((double)(totalSeconds - remainingSeconds) / totalSeconds) * 100;
            ProgressRing.Value = progress;

            // Update status and colors based on phase
            if (isWorkPhase)
            {
                StatusText.Text = "💼 Work Time";
                PhaseText.Text = "Minutes Left";
                StatusText.Foreground = new SolidColorBrush(Colors.DodgerBlue);
                ProgressRing.Foreground = new SolidColorBrush(Colors.DodgerBlue);
            }
            else
            {
                StatusText.Text = "😌 Rest Time - Look Away!";
                PhaseText.Text = "Seconds Left";
                StatusText.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
                ProgressRing.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
            }
        }

        private async void OnPhaseComplete()
        {
            // Show notification
            var dialog = new ContentDialog
            {
                XamlRoot = this.Content.XamlRoot,
                Title = isWorkPhase ? "⏰ Time to Rest!" : "✅ Rest Complete!",
                Content = isWorkPhase 
                    ? "Take a 20-second break and look at something 20 feet away!" 
                    : "Great job! Starting next work session.",
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close
            };

            // Play notification sound
            try
            {
                var mediaPlayer = new MediaPlayer();
                mediaPlayer.Source = MediaSource.CreateFromUri(
                    new Uri("ms-winsoundevent:Notification.Default"));
                mediaPlayer.Play();
            }
            catch { }

            await dialog.ShowAsync();

            // Switch phase and auto-start
            isWorkPhase = !isWorkPhase;
            ResetTimer();
            StartTimer();
        }

        private void ResetTimer()
        {
            if (isWorkPhase)
            {
                remainingSeconds = workDuration;
                totalSeconds = workDuration;
            }
            else
            {
                remainingSeconds = restDuration;
                totalSeconds = restDuration;
            }
            
            UpdateDisplay();
        }

        private void StartTimer()
        {
            isRunning = true;
            timer.Start();
            StartButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
        }

        private void StopTimer()
        {
            isRunning = false;
            timer.Stop();
            StartButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartTimer();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            StopTimer();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            StopTimer();
            isWorkPhase = true;
            ResetTimer();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = this.Content.XamlRoot,
                Title = "⚙️ Settings",
                CloseButtonText = "Close"
            };

            var panel = new StackPanel { Spacing = 16 };
            
            // Work duration setting
            var workPanel = new StackPanel { Spacing = 8 };
            workPanel.Children.Add(new TextBlock { Text = "Work Duration (minutes):", FontWeight = new Windows.UI.Text.FontWeight(600) });
            var workSlider = new Slider
            {
                Minimum = 5,
                Maximum = 60,
                Value = workDuration / 60,
                StepFrequency = 5,
                TickFrequency = 5,
                TickPlacement = Microsoft.UI.Xaml.Controls.Primitives.TickPlacement.BottomRight
            };
            var workValueText = new TextBlock { Text = $"{workDuration / 60} minutes" };
            workSlider.ValueChanged += (s, e) => workValueText.Text = $"{e.NewValue} minutes";
            workPanel.Children.Add(workSlider);
            workPanel.Children.Add(workValueText);
            
            // Rest duration setting
            var restPanel = new StackPanel { Spacing = 8 };
            restPanel.Children.Add(new TextBlock { Text = "Rest Duration (seconds):", FontWeight = new Windows.UI.Text.FontWeight(600) });
            var restSlider = new Slider
            {
                Minimum = 10,
                Maximum = 60,
                Value = restDuration,
                StepFrequency = 10,
                TickFrequency = 10,
                TickPlacement = Microsoft.UI.Xaml.Controls.Primitives.TickPlacement.BottomRight
            };
            var restValueText = new TextBlock { Text = $"{restDuration} seconds" };
            restSlider.ValueChanged += (s, e) => restValueText.Text = $"{e.NewValue} seconds";
            restPanel.Children.Add(restSlider);
            restPanel.Children.Add(restValueText);

            // Info text
            var infoText = new TextBlock
            {
                Text = "📝 20-20-20 Rule: Every 20 minutes, take a 20-second break and look at something 20 feet away.",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.7,
                Margin = new Thickness(0, 8, 0, 0)
            };

            panel.Children.Add(workPanel);
            panel.Children.Add(restPanel);
            panel.Children.Add(infoText);

            dialog.Content = panel;
            
            var result = await dialog.ShowAsync();
            
            // Apply settings
            workDuration = (int)workSlider.Value * 60;
            restDuration = (int)restSlider.Value;
            
            if (!isRunning)
            {
                ResetTimer();
            }
        }
    }
}