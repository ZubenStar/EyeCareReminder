using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace EyeCareReminder.Helpers
{
    /// <summary>
    /// Manages UI mode transitions between normal and mini modes
    /// </summary>
    public class UIModeManager
    {
        private readonly UIElement[] _normalModeElements;
        private readonly UIElement _timerContainer;
        private readonly TextBlock _timerText;
        private readonly TextBlock _phaseText;
        private readonly ProgressRing _progressRing;
        private readonly Button _miniModeButton;

        private bool _isMiniMode;

        public bool IsMiniMode => _isMiniMode;

        public UIModeManager(
            UIElement[] normalModeElements,
            UIElement timerContainer,
            TextBlock timerText,
            TextBlock phaseText,
            ProgressRing progressRing,
            Button miniModeButton)
        {
            _normalModeElements = normalModeElements ?? throw new ArgumentNullException(nameof(normalModeElements));
            _timerContainer = timerContainer ?? throw new ArgumentNullException(nameof(timerContainer));
            _timerText = timerText ?? throw new ArgumentNullException(nameof(timerText));
            _phaseText = phaseText ?? throw new ArgumentNullException(nameof(phaseText));
            _progressRing = progressRing ?? throw new ArgumentNullException(nameof(progressRing));
            _miniModeButton = miniModeButton ?? throw new ArgumentNullException(nameof(miniModeButton));
        }

        /// <summary>
        /// Toggles between normal and mini mode
        /// </summary>
        public void ToggleMode()
        {
            _isMiniMode = !_isMiniMode;
            System.Diagnostics.Debug.WriteLine($"UIModeManager: Toggling to {(_isMiniMode ? "mini" : "normal")} mode");
            ApplyMode();
        }

        /// <summary>
        /// Switches to normal mode
        /// </summary>
        public void SwitchToNormalMode()
        {
            if (_isMiniMode)
            {
                _isMiniMode = false;
                ApplyMode();
            }
        }

        /// <summary>
        /// Switches to mini mode
        /// </summary>
        public void SwitchToMiniMode()
        {
            if (!_isMiniMode)
            {
                _isMiniMode = true;
                ApplyMode();
            }
        }

        private void ApplyMode()
        {
            UpdateButtonText();

            if (_isMiniMode)
            {
                System.Diagnostics.Debug.WriteLine("UIModeManager: Applying mini mode");
                ApplyMiniMode();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("UIModeManager: Applying normal mode");
                ApplyNormalMode();
            }
        }

        private void ApplyMiniMode()
        {
            System.Diagnostics.Debug.WriteLine("UIModeManager: Applying mini mode");
            
            // Hide all normal mode elements except the mini mode button
            foreach (var element in _normalModeElements)
            {
                if (element != _miniModeButton)
                {
                    element.Visibility = Visibility.Collapsed;
                }
            }

            // Ensure the mini mode button is visible and positioned
            _miniModeButton.Visibility = Visibility.Visible;
            _miniModeButton.Margin = new Thickness(5);
            _miniModeButton.HorizontalAlignment = HorizontalAlignment.Center;

            // Ensure timer container is visible
            _timerContainer.Visibility = Visibility.Visible;

            // Configure timer for mini mode with better sizing
            if (_timerContainer is FrameworkElement timerFramework)
            {
                timerFramework.Margin = new Thickness(5);
                timerFramework.VerticalAlignment = VerticalAlignment.Center;
                timerFramework.HorizontalAlignment = HorizontalAlignment.Center;
            }

            // Configure timer text
            _timerText.Visibility = Visibility.Visible;
            _timerText.FontSize = 20;
            _timerText.FontWeight = new Windows.UI.Text.FontWeight(700);
            _timerText.Margin = new Thickness(0);

            // Hide phase text in mini mode
            _phaseText.Visibility = Visibility.Collapsed;

            // Configure progress ring
            _progressRing.Visibility = Visibility.Visible;
            _progressRing.Width = 50;
            _progressRing.Height = 50;
            _progressRing.Margin = new Thickness(0);
            
            System.Diagnostics.Debug.WriteLine("UIModeManager: Mini mode applied successfully");
        }

        private void ApplyNormalMode()
        {
            // Show all normal mode elements
            foreach (var element in _normalModeElements)
            {
                element.Visibility = Visibility.Visible;
            }

            // Restore timer to normal mode
            if (_timerContainer is FrameworkElement timerFramework)
            {
                timerFramework.Margin = new Thickness(0, 12, 0, 12);
                timerFramework.VerticalAlignment = VerticalAlignment.Stretch;
                timerFramework.HorizontalAlignment = HorizontalAlignment.Stretch;
            }

            _timerText.FontSize = 32;
            _timerText.FontWeight = new Windows.UI.Text.FontWeight(700);
            _phaseText.Visibility = Visibility.Visible;
            _progressRing.Width = 120;
            _progressRing.Height = 120;
        }

        private void UpdateButtonText()
        {
            _miniModeButton.Content = _isMiniMode ? "🖥️ Normal Mode" : "🪟 Mini Mode";
        }
    }
}