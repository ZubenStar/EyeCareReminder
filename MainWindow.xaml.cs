using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using EyeCareReminder.Models;
using EyeCareReminder.Services;
using EyeCareReminder.Helpers;

namespace EyeCareReminder
{
    /// <summary>
    /// Main window for the Eye Care Reminder application
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        // Services
        private readonly TimerManager _timerManager;
        private readonly SettingsService _settingsService;
        private readonly WindowManager _windowManager;
        private NotificationService _notificationService = default!;
        private UIModeManager _uiModeManager = default!;
        
        // Settings
        private TimerSettings _settings;

        public MainWindow()
        {
            this.InitializeComponent();
            
            // Initialize settings
            _settings = new TimerSettings();
            _settingsService = new SettingsService();
            
            // Initialize services
            _timerManager = new TimerManager(_settings);
            _windowManager = new WindowManager(this);
            
            // Load saved settings
            LoadSettingsAsync();
            
            // Setup event handlers
            SetupEventHandlers();
        }

        /// <summary>
        /// Loads settings asynchronously after window is initialized
        /// </summary>
        private async void LoadSettingsAsync()
        {
            try
            {
                _settings = await _settingsService.LoadSettingsAsync();
                _timerManager.UpdateSettings(_settings);
                UpdateDisplay(_timerManager);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up all event handlers
        /// </summary>
        private void SetupEventHandlers()
        {
            // Timer events
            _timerManager.StateChanged += OnTimerStateChanged;
            _timerManager.PhaseCompleted += OnPhaseCompleted;
            
            // Window events
            this.Content.DoubleTapped += Content_DoubleTapped;
            this.Activated += Window_Activated;
            
            // Keyboard shortcuts
            this.Content.KeyDown += Content_KeyDown;
        }

        /// <summary>
        /// Initializes services that require XamlRoot after window activation
        /// </summary>
        private void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
        {
            // Initialize only once
            if (_notificationService != null) return;
            
            _notificationService = new NotificationService(this.Content.XamlRoot);
            
            _uiModeManager = new UIModeManager(
                normalModeElements: new UIElement[] 
                { 
                    TitleTextBlock, 
                    StatusBorder, 
                    ControlButtonsPanel, 
                    MiniModeButton, 
                    SettingsButton 
                },
                timerContainer: TimerViewbox,
                timerText: TimerText,
                phaseText: PhaseText,
                progressRing: ProgressRing,
                miniModeButton: MiniModeButton
            );
        }

        /// <summary>
        /// Handles keyboard shortcuts
        /// </summary>
        private void Content_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Ctrl+Space: Start/Pause
            if (e.Key == Windows.System.VirtualKey.Space && 
                Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
                    .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                if (_timerManager.IsRunning)
                {
                    _timerManager.Pause();
                }
                else
                {
                    _timerManager.Start();
                }
                e.Handled = true;
            }
            // Ctrl+R: Reset
            else if (e.Key == Windows.System.VirtualKey.R && 
                Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
                    .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                _timerManager.ResetToWorkPhase();
                e.Handled = true;
            }
            // Ctrl+M: Toggle mini mode
            else if (e.Key == Windows.System.VirtualKey.M && 
                Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
                    .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                ToggleMiniMode();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles timer state changes
        /// </summary>
        private void OnTimerStateChanged(object? sender, TimerStateChangedEventArgs e)
        {
            UpdateDisplay(e);
        }

        /// <summary>
        /// Updates the UI display based on timer state
        /// </summary>
        private void UpdateDisplay(TimerStateChangedEventArgs state)
        {
            // Update timer text
            TimerText.Text = _timerManager.GetFormattedTime();

            // Update progress ring
            ProgressRing.Value = state.ProgressPercentage;

            // Update status and colors based on phase
            if (state.IsWorkPhase)
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

            // Update button states
            StartButton.IsEnabled = !state.IsRunning;
            PauseButton.IsEnabled = state.IsRunning;
        }

        /// <summary>
        /// Overload for TimerManager
        /// </summary>
        private void UpdateDisplay(TimerManager timer)
        {
            var state = new TimerStateChangedEventArgs
            {
                RemainingSeconds = timer.RemainingSeconds,
                TotalSeconds = timer.TotalSeconds,
                IsWorkPhase = timer.IsWorkPhase,
                IsRunning = timer.IsRunning,
                ProgressPercentage = timer.ProgressPercentage
            };
            UpdateDisplay(state);
        }

        /// <summary>
        /// Handles phase completion
        /// </summary>
        private async void OnPhaseCompleted(object? sender, EventArgs e)
        {
            bool wasWorkPhase = _timerManager.IsWorkPhase;
            
            // Show notification
            await _notificationService.ShowPhaseCompletionAsync(wasWorkPhase);

            // Switch phase and auto-start
            _timerManager.SwitchPhase();
            _timerManager.Start();
        }

        /// <summary>
        /// Toggles between normal and mini mode
        /// </summary>
        private void ToggleMiniMode()
        {
            if (_uiModeManager == null) return;

            _uiModeManager.ToggleMode();
            
            if (_uiModeManager.IsMiniMode)
            {
                _windowManager.SwitchToMiniMode();
            }
            else
            {
                _windowManager.SwitchToNormalMode();
            }
        }

        /// <summary>
        /// Double-click handler to toggle mini mode
        /// </summary>
        private void Content_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ToggleMiniMode();
        }

        // Button Click Handlers

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _timerManager.Start();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            _timerManager.Pause();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _timerManager.ResetToWorkPhase();
        }

        private void MiniModeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMiniMode();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowSettingsDialogAsync();
        }

        /// <summary>
        /// Shows the settings dialog
        /// </summary>
        private async System.Threading.Tasks.Task ShowSettingsDialogAsync()
        {
            var dialog = new ContentDialog
            {
                XamlRoot = this.Content.XamlRoot,
                Title = "⚙️ Settings",
                CloseButtonText = "Close",
                PrimaryButtonText = "Save",
                DefaultButton = ContentDialogButton.Primary
            };

            var panel = CreateSettingsPanel(out var workSlider, out var restSlider);
            dialog.Content = panel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await SaveSettingsAsync((int)workSlider.Value, (int)restSlider.Value);
            }
        }

        /// <summary>
        /// Creates the settings panel UI
        /// </summary>
        private StackPanel CreateSettingsPanel(out Slider workSlider, out Slider restSlider)
        {
            var panel = new StackPanel { Spacing = 16 };

            // Work duration setting
            var workPanel = new StackPanel { Spacing = 8 };
            workPanel.Children.Add(new TextBlock 
            { 
                Text = "Work Duration (minutes):", 
                FontWeight = new Windows.UI.Text.FontWeight(600) 
            });
            
            workSlider = new Slider
            {
                Minimum = TimerSettings.MinWorkMinutes,
                Maximum = TimerSettings.MaxWorkMinutes,
                Value = _settings.WorkDurationMinutes,
                StepFrequency = 5,
                TickFrequency = 5,
                TickPlacement = Microsoft.UI.Xaml.Controls.Primitives.TickPlacement.BottomRight
            };
            
            var workValueText = new TextBlock { Text = $"{_settings.WorkDurationMinutes} minutes" };
            workSlider.ValueChanged += (s, e) => workValueText.Text = $"{e.NewValue} minutes";
            
            workPanel.Children.Add(workSlider);
            workPanel.Children.Add(workValueText);

            // Rest duration setting
            var restPanel = new StackPanel { Spacing = 8 };
            restPanel.Children.Add(new TextBlock 
            { 
                Text = "Rest Duration (seconds):", 
                FontWeight = new Windows.UI.Text.FontWeight(600) 
            });
            
            restSlider = new Slider
            {
                Minimum = TimerSettings.MinRestSeconds,
                Maximum = TimerSettings.MaxRestSeconds,
                Value = _settings.RestDurationSeconds,
                StepFrequency = 10,
                TickFrequency = 10,
                TickPlacement = Microsoft.UI.Xaml.Controls.Primitives.TickPlacement.BottomRight
            };
            
            var restValueText = new TextBlock { Text = $"{_settings.RestDurationSeconds} seconds" };
            restSlider.ValueChanged += (s, e) => restValueText.Text = $"{e.NewValue} seconds";
            
            restPanel.Children.Add(restSlider);
            restPanel.Children.Add(restValueText);

            // Info text
            var infoText = new TextBlock
            {
                Text = "📝 20-20-20 Rule: Every 20 minutes, take a 20-second break and look at something 20 feet away.\n\n" +
                       "⌨️ Keyboard Shortcuts:\n" +
                       "Ctrl+Space: Start/Pause\n" +
                       "Ctrl+R: Reset\n" +
                       "Ctrl+M: Toggle Mini Mode",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.7,
                Margin = new Thickness(0, 8, 0, 0)
            };

            panel.Children.Add(workPanel);
            panel.Children.Add(restPanel);
            panel.Children.Add(infoText);

            return panel;
        }

        /// <summary>
        /// Saves settings and updates the timer
        /// </summary>
        private async System.Threading.Tasks.Task SaveSettingsAsync(int workMinutes, int restSeconds)
        {
            try
            {
                _settings.WorkDurationMinutes = workMinutes;
                _settings.RestDurationSeconds = restSeconds;
                
                await _settingsService.SaveSettingsAsync(_settings);
                _timerManager.UpdateSettings(_settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                
                if (_notificationService != null)
                {
                    await _notificationService.ShowNotificationAsync(
                        "❌ Error",
                        "Failed to save settings. Please try again."
                    );
                }
            }
        }
    }
}