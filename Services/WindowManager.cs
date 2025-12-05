using System;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using Windows.Graphics;

namespace EyeCareReminder.Services
{
    /// <summary>
    /// Manages window positioning, sizing, and always-on-top behavior
    /// </summary>
    public class WindowManager
    {
        private readonly Window _window;
        private readonly IntPtr _hWnd;
        private readonly AppWindow _appWindow;
        
        // Window dimensions
        public const int NormalWidth = 320;
        public const int NormalHeight = 280;
        public const int MiniWidth = 200;
        public const int MiniHeight = 100;
        public const int MiniMarginRight = 20;
        public const int MiniMarginBottom = 60;

        private RectInt32 _normalSize;
        private RectInt32 _miniSize;

        // P/Invoke for always-on-top
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;

        public WindowManager(Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            
            // Get window handle
            _hWnd = WindowNative.GetWindowHandle(_window);
            var windowId = Win32Interop.GetWindowIdFromWindow(_hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            if (_appWindow == null)
            {
                throw new InvalidOperationException("Failed to get AppWindow");
            }

            InitializeWindowSizes();
            SetAlwaysOnTop(true);
            
            // Listen for window changes
            _appWindow.Changed += OnAppWindowChanged;
        }

        private void InitializeWindowSizes()
        {
            var windowId = Win32Interop.GetWindowIdFromWindow(_hWnd);
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            var workArea = displayArea.WorkArea;

            // Calculate centered normal position
            var x = (workArea.Width - NormalWidth) / 2;
            var y = (workArea.Height - NormalHeight) / 2;
            _normalSize = new RectInt32(x, y, NormalWidth, NormalHeight);

            // Calculate bottom-right mini position
            var miniX = workArea.Width - MiniWidth - MiniMarginRight;
            var miniY = workArea.Height - MiniHeight - MiniMarginBottom;
            _miniSize = new RectInt32(miniX, miniY, MiniWidth, MiniHeight);

            // Set initial size
            _appWindow.MoveAndResize(_normalSize);
        }

        private void OnAppWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
        {
            // Maintain always-on-top when window state changes
            if (args.DidPresenterChange || args.DidSizeChange)
            {
                SetAlwaysOnTop(true);
            }
        }

        /// <summary>
        /// Sets the window to always be on top
        /// </summary>
        public void SetAlwaysOnTop(bool topmost)
        {
            SetWindowPos(_hWnd, topmost ? HWND_TOPMOST : IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        /// <summary>
        /// Switches to normal mode
        /// </summary>
        public void SwitchToNormalMode()
        {
            try
            {
                if (_appWindow == null) return;
                _appWindow.MoveAndResize(_normalSize);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error switching to normal mode: {ex.Message}");
            }
        }

        /// <summary>
        /// Switches to mini mode
        /// </summary>
        public async System.Threading.Tasks.Task SwitchToMiniMode()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("WindowManager: Starting switch to mini mode");
                
                // Check if window is still valid
                if (_appWindow == null)
                {
                    System.Diagnostics.Debug.WriteLine("WindowManager: AppWindow is null");
                    return;
                }
                
                // Create a slightly larger mini mode size to ensure content is visible
                var windowId = Win32Interop.GetWindowIdFromWindow(_hWnd);
                var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
                var workArea = displayArea.WorkArea;
                
                // Calculate bottom-right mini position with larger size
                var largerMiniWidth = 220;
                var largerMiniHeight = 120;
                var miniX = workArea.Width - largerMiniWidth - MiniMarginRight;
                var miniY = workArea.Height - largerMiniHeight - MiniMarginBottom;
                var largerMiniSize = new RectInt32(miniX, miniY, largerMiniWidth, largerMiniHeight);
                
                // First ensure the window content is ready for mini mode
                await System.Threading.Tasks.Task.Delay(50);
                
                // Check again before resizing
                if (_appWindow == null)
                {
                    System.Diagnostics.Debug.WriteLine("WindowManager: AppWindow became null during delay");
                    return;
                }
                
                // Then resize the window
                System.Diagnostics.Debug.WriteLine($"WindowManager: Resizing to mini mode: {largerMiniSize.Width}x{largerMiniSize.Height} at ({largerMiniSize.X},{largerMiniSize.Y})");
                _appWindow.MoveAndResize(largerMiniSize);
                
                // Force a layout update after resize
                await System.Threading.Tasks.Task.Delay(50);
                
                System.Diagnostics.Debug.WriteLine("WindowManager: Successfully switched to mini mode");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowManager: Error switching to mini mode: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"WindowManager: Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Dispose()
        {
            if (_appWindow != null)
            {
                _appWindow.Changed -= OnAppWindowChanged;
            }
        }
    }
}