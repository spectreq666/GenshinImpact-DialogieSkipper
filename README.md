# Genshin Impact Dialogue Skipper

Automated dialogue skipping utility for Genshin Impact using computer vision and image recognition.
Use at tour own risk.

## 📋 Description

This application automatically detects active dialogues in Genshin Impact and presses the F key to skip them. It uses OpenCV for template matching, making it more reliable than pixel-based methods.

## ✨ Features

- 🖼️ **Computer Vision** - Dialogue detection using template matching
- ⌨️ **Hotkey Controls** - Simple management with F8, F9, F12 keys
- ⏱️ **Randomized Intervals** - Natural behavior to avoid detection
- ☕ **Smart Breaks** - Random pauses for human-like behavior
- 🔧 **Auto Resolution Detection** - Automatically detects and configures screen resolution
- 💾 **Configuration Persistence** - Saves settings to `.env` file

## 🚀 Installation

### Setup

1. Download the latest release from the [Releases page](https://github.com/spectreq666/GenshinImpact-DialogieSkipper/releases)
2. Extract the ZIP file to your preferred location
3. Launch an .exe file with admin rights.

## 🎮 Usage

1. **Launch Genshin Impact** and enter the game world
2. **Run the application** as Administrator (important for screen capture)
3. **Configure resolution** on first run if needed
4. **Use hotkeys** to control the application:

| Key | Action | Description |
|-----|--------|-------------|
| F8 | Start | Begin automatic dialogue skipping |
| F9 | Pause | Temporarily stop skipping |
| F12 | Exit | Close the application |

5. **Focus Genshin Impact window** and start dialogues - the app will automatically skip them
