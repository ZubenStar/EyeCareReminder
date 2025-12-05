using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace EyeCareReminder.Services
{
    /// <summary>
    /// Service for displaying notifications and playing sounds
    /// </summary>
    public class NotificationService
    {
        private readonly XamlRoot _xamlRoot;

        public NotificationService(XamlRoot xamlRoot)
        {
            _xamlRoot = xamlRoot ?? throw new ArgumentNullException(nameof(xamlRoot));
        }

        /// <summary>
        /// Shows a notification when work phase ends
        /// </summary>
        public async Task ShowRestNotificationAsync()
        {
            await ShowNotificationAsync(
                "⏰ Time to Rest!",
                "Take a 20-second break and look at something 20 feet away!"
            );
        }

        /// <summary>
        /// Shows a notification when rest phase ends
        /// </summary>
        public async Task ShowWorkNotificationAsync()
        {
            await ShowNotificationAsync(
                "✅ Rest Complete!",
                "Great job! Starting next work session."
            );
        }

        /// <summary>
        /// Shows a custom notification dialog
        /// </summary>
        public async Task ShowNotificationAsync(string title, string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = _xamlRoot,
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to show notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Plays the system notification sound
        /// </summary>
        public void PlayNotificationSound()
        {
            try
            {
                var mediaPlayer = new MediaPlayer();
                mediaPlayer.Source = MediaSource.CreateFromUri(
                    new Uri("ms-winsoundevent:Notification.Default"));
                mediaPlayer.Play();
                
                // Auto-dispose after playing
                mediaPlayer.MediaEnded += (s, e) => mediaPlayer.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play notification sound: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows phase completion notification with sound
        /// </summary>
        public async Task ShowPhaseCompletionAsync(bool wasWorkPhase)
        {
            PlayNotificationSound();
            
            if (wasWorkPhase)
            {
                await ShowRestNotificationAsync();
            }
            else
            {
                await ShowWorkNotificationAsync();
            }
        }
    }
}