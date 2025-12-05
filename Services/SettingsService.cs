using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using EyeCareReminder.Models;

namespace EyeCareReminder.Services
{
    /// <summary>
    /// Service for persisting and loading application settings
    /// </summary>
    public class SettingsService
    {
        private static readonly string SettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EyeCareReminder"
        );
        
        private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.json");

        /// <summary>
        /// Saves settings to disk
        /// </summary>
        public async Task SaveSettingsAsync(TimerSettings settings)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(SettingsFolder);

                // Validate before saving
                settings.Validate();

                // Serialize and save
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                await File.WriteAllTextAsync(SettingsFile, json);
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads settings from disk, or returns default if not found
        /// </summary>
        public async Task<TimerSettings> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(SettingsFile))
                {
                    return new TimerSettings();
                }

                var json = await File.ReadAllTextAsync(SettingsFile);
                var settings = JsonSerializer.Deserialize<TimerSettings>(json) ?? new TimerSettings();
                
                // Validate loaded settings
                settings.Validate();
                
                return settings;
            }
            catch (Exception ex)
            {
                // Log error and return defaults
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
                return new TimerSettings();
            }
        }

        /// <summary>
        /// Deletes saved settings file
        /// </summary>
        public void ResetSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    File.Delete(SettingsFile);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reset settings: {ex.Message}");
            }
        }
    }
}