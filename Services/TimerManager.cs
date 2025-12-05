using System;
using Microsoft.UI.Xaml;
using EyeCareReminder.Models;

namespace EyeCareReminder.Services
{
    /// <summary>
    /// Manages timer state and logic for work/rest cycles
    /// </summary>
    public class TimerManager
    {
        private readonly DispatcherTimer _timer;
        private TimerSettings _settings;
        
        // State
        private int _remainingSeconds;
        private int _totalSeconds;
        private bool _isWorkPhase = true;
        private bool _isRunning = false;

        // Events
        public event EventHandler? Tick;
        public event EventHandler? PhaseCompleted;
        public event EventHandler<TimerStateChangedEventArgs>? StateChanged;

        public TimerManager(TimerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;
            
            ResetTimer();
        }

        // Properties
        public int RemainingSeconds => _remainingSeconds;
        public int TotalSeconds => _totalSeconds;
        public bool IsWorkPhase => _isWorkPhase;
        public bool IsRunning => _isRunning;
        public double ProgressPercentage => ((double)(_totalSeconds - _remainingSeconds) / _totalSeconds) * 100;

        /// <summary>
        /// Updates timer settings and resets if not running
        /// </summary>
        public void UpdateSettings(TimerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            
            if (!_isRunning)
            {
                ResetTimer();
            }
        }

        /// <summary>
        /// Starts the timer
        /// </summary>
        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _timer.Start();
                RaiseStateChanged();
            }
        }

        /// <summary>
        /// Pauses the timer
        /// </summary>
        public void Pause()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _timer.Stop();
                RaiseStateChanged();
            }
        }

        /// <summary>
        /// Resets timer to current phase's duration
        /// </summary>
        public void ResetTimer()
        {
            _remainingSeconds = _isWorkPhase ? _settings.WorkDurationSeconds : _settings.RestDurationSeconds;
            _totalSeconds = _remainingSeconds;
            RaiseStateChanged();
        }

        /// <summary>
        /// Resets to work phase
        /// </summary>
        public void ResetToWorkPhase()
        {
            Pause();
            _isWorkPhase = true;
            ResetTimer();
        }

        /// <summary>
        /// Switches to the next phase (work <-> rest)
        /// </summary>
        public void SwitchPhase()
        {
            _isWorkPhase = !_isWorkPhase;
            ResetTimer();
        }

        /// <summary>
        /// Gets formatted time string (MM:SS)
        /// </summary>
        public string GetFormattedTime()
        {
            int minutes = _remainingSeconds / 60;
            int seconds = _remainingSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }

        private void OnTimerTick(object? sender, object e)
        {
            _remainingSeconds--;
            
            Tick?.Invoke(this, EventArgs.Empty);
            RaiseStateChanged();

            if (_remainingSeconds <= 0)
            {
                _timer.Stop();
                _isRunning = false;
                PhaseCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RaiseStateChanged()
        {
            StateChanged?.Invoke(this, new TimerStateChangedEventArgs
            {
                RemainingSeconds = _remainingSeconds,
                TotalSeconds = _totalSeconds,
                IsWorkPhase = _isWorkPhase,
                IsRunning = _isRunning,
                ProgressPercentage = ProgressPercentage
            });
        }

        /// <summary>
        /// Cleans up timer resources
        /// </summary>
        public void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
        }
    }

    /// <summary>
    /// Event args for timer state changes
    /// </summary>
    public class TimerStateChangedEventArgs : EventArgs
    {
        public int RemainingSeconds { get; init; }
        public int TotalSeconds { get; init; }
        public bool IsWorkPhase { get; init; }
        public bool IsRunning { get; init; }
        public double ProgressPercentage { get; init; }
    }
}