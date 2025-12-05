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
                ApplyMiniMode();
            }
            else
            {
                ApplyNormalMode();
            }
        }

        private void ApplyMiniMode()
        {
            // Hide all normal mode elements
            foreach (var element in _normalModeElements)
            {
                element.Visibility = Visibility.Collapsed;
            }

            // Configure timer for mini mode
            if (_timerContainer is FrameworkElement timerFramework)
            {
                timerFramework.Margin = new Thickness(4);
                timerFramework.VerticalAlignment = VerticalAlignment.Center;
                timerFramework.HorizontalAlignment = HorizontalAlignment.Center;
            }

            _timerText.FontSize = 28;
            _timerText.FontWeight = new Windows.UI.Text.FontWeight(700);
            _phaseText.Visibility = Visibility.Collapsed;
            _progressRing.Width = 50;
            _progressRing.Height = 50;
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