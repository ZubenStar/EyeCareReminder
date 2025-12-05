# Eye Care Reminder 👁️

A WinUI 3 desktop application that helps protect your eyes using the **20-20-20 rule**: Every 20 minutes, take a 20-second break and look at something 20 feet (6 meters) away.

## Features

✨ **Core Functionality**
- Automatic 20-minute work timer with countdown
- 20-second rest reminder with visual and audio notifications
- Auto-start next cycle after each phase completes
- Always-on-top compact window design

🎨 **User Interface**
- Clean, modern WinUI 3 interface with Mica backdrop
- Real-time countdown display (minutes:seconds)
- Visual progress ring showing time remaining
- Color-coded status indicators (blue for work, green for rest)
- Compact window size (320x280) that doesn't obstruct work

🎛️ **Controls**
- **Start**: Begin the timer
- **Pause**: Temporarily stop the timer
- **Reset**: Return to initial 20-minute work state
- **Settings**: Customize work and rest durations

⚙️ **Customization**
- Adjustable work duration (5-60 minutes)
- Adjustable rest duration (10-60 seconds)
- Settings persist during current session

## Requirements

- Windows 10 version 1809 (Build 17763) or later
- Windows 11 recommended for best experience
- .NET 8.0 SDK
- Windows App SDK 1.6

## Installation & Setup

### Prerequisites

1. Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Install [Visual Studio 2022](https://visualstudio.microsoft.com/) with:
   - .NET Desktop Development workload
   - Windows application development workload
   - Windows App SDK C# Templates

### Build Instructions

1. **Clone or download this repository**

2. **Open the project**
   ```bash
   cd EyeCareReminder
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

### Visual Studio

1. Open `EyeCareReminder.csproj` in Visual Studio 2022
2. Press F5 to build and run
3. The application window will appear on top of all other windows

## Usage

### Getting Started

1. **Launch the application** - The window will appear in a compact size, always on top
2. **Click "▶️ Start"** to begin the 20-minute work timer
3. **Work notification** - After 20 minutes, you'll receive a notification to take a break
4. **Rest period** - Look at something 20 feet away for 20 seconds
5. **Auto-continue** - The app automatically starts the next work cycle

### Controls

- **▶️ Start**: Begins the countdown timer
- **⏸️ Pause**: Pauses the current timer (can resume with Start)
- **🔄 Reset**: Resets to the initial state (20-minute work timer)
- **⚙️ Settings**: Opens the settings dialog to customize durations

### Settings

Click the **⚙️ Settings** button to customize:

- **Work Duration**: 5-60 minutes (default: 20 minutes)
- **Rest Duration**: 10-60 seconds (default: 20 seconds)

Changes apply immediately when the dialog is closed.

## Technical Details

### Architecture

- **Framework**: WinUI 3 (.NET 8.0)
- **UI Pattern**: XAML with code-behind
- **Timer**: DispatcherTimer for precise 1-second intervals
- **Window Management**: Win32 APIs for always-on-top functionality

### Key Components

- [`MainWindow.xaml`](MainWindow.xaml): UI layout and styling
- [`MainWindow.xaml.cs`](MainWindow.xaml.cs): Timer logic and window management
- [`App.xaml`](App.xaml): Application resources
- [`App.xaml.cs`](App.xaml.cs): Application entry point

### Features Implementation

**Always On Top**
```csharp
SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
```

**Precise Timing**
```csharp
timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
```

**Notifications**
- Visual: ContentDialog with phase-specific messages
- Audio: System notification sound
- Auto-advance to next phase

## Troubleshooting

### Application won't start
- Ensure .NET 8.0 SDK is installed
- Verify Windows App SDK is properly installed
- Check Windows version (minimum 10.0.17763)

### Timer not visible
- The window is always on top - check if it's behind other windows
- Try clicking the taskbar icon to bring focus

### No notification sound
- Check Windows sound settings
- Verify system volume is not muted
- Some systems may require additional permissions

### Window positioning issues
- The app automatically centers on primary display
- If off-screen, use Reset button or restart the app

## Contributing

Feel free to submit issues and enhancement requests!

## License

This project is provided as-is for personal and educational use.

## Credits

Based on the **20-20-20 rule** recommended by optometrists to reduce digital eye strain.

---

**Stay healthy! Remember to give your eyes regular breaks. 👁️✨**