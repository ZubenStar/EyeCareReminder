using System;

namespace EyeCareReminder.Models
{
    /// <summary>
    /// Represents the timer configuration settings
    /// </summary>
    public class TimerSettings
    {
        public const int DefaultWorkMinutes = 20;
        public const int DefaultRestSeconds = 20;
        public const int MinWorkMinutes = 5;
        public const int MaxWorkMinutes = 60;
        public const int MinRestSeconds = 10;
        public const int MaxRestSeconds = 60;

        /// <summary>
        /// Work duration in seconds
        /// </summary>
        public int WorkDurationSeconds { get; set; } = DefaultWorkMinutes * 60;

        /// <summary>
        /// Rest duration in seconds
        /// </summary>
        public int RestDurationSeconds { get; set; } = DefaultRestSeconds;

        /// <summary>
        /// Gets work duration in minutes
        /// </summary>
        public int WorkDurationMinutes
        {
            get => WorkDurationSeconds / 60;
            set => WorkDurationSeconds = Math.Clamp(value, MinWorkMinutes, MaxWorkMinutes) * 60;
        }

        /// <summary>
        /// Validates and clamps settings to acceptable ranges
        /// </summary>
        public void Validate()
        {
            WorkDurationSeconds = Math.Clamp(WorkDurationSeconds, MinWorkMinutes * 60, MaxWorkMinutes * 60);
            RestDurationSeconds = Math.Clamp(RestDurationSeconds, MinRestSeconds, MaxRestSeconds);
        }

        /// <summary>
        /// Creates a copy of the settings
        /// </summary>
        public TimerSettings Clone()
        {
            return new TimerSettings
            {
                WorkDurationSeconds = this.WorkDurationSeconds,
                RestDurationSeconds = this.RestDurationSeconds
            };
        }
    }
}