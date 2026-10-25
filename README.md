# RMS Theme Studio & Interactive Playground

A standalone desktop suite (.NET 8 Windows Forms) for creating, testing, generating, and deploying custom themes, wallpapers, sound effects, and cinematic animations for the **RMS Android Mobile Application**.

---

## Features

1. **Theme Metadata Management (`dbo.tbl_MessageThemes`)**:
   - Register, preview, and update chat themes stored in SQL Server database (`dbRMSAndroid`).
   - Configure theme properties: `ThemeName`, `ThemeBackgroundPath`, `CallBackgroundPath`, `SenderBubble`, `ReceiverBubble`, `DefaultIcon`, `SetIconGroup`, and `Status` (`Public`, `Developing`, `Unavailable`, `Expired`).

2. **AI & Graphic Generator Suite**:
   - Generate high-resolution 1080x1920 diagonal repeating pattern wallpapers and ambient call screen backgrounds.
   - Presets for popular themes:
     - 🎃 **Halloween**: Midnight gothic aura with pumpkins and bats.
     - 📼 **Sadako / Horror**: Well abyss vignette, static glitch lines, and crack overlays.
     - 🎄 **Christmas**: Festive evergreen pine, snowflakes, and warm twilight glows.
     - 🔪 **Freddy Krueger**: Nightmarish crimson slash stripes.
     - 🏒 **Friday the 13th**: Crystal Lake mist and machete icons.
     - 🇵🇭 **Bonifacio Day**: Sunrise revolution banner with katipunan symbols.

3. **Audio Effect Generator**:
   - Synthesizes and tests audio effects (horror screeches, holiday chimes, and bell arpeggios) using pure mathematical WAV synthesis.
   - Automatically exports WAV files to `D:\html\Android\RMS\Notification\` for mobile download.

4. **Interactive 9:16 Smartphone Simulator & Playground**:
   - Real-time rendering of chat bubbles, sender/receiver colors, and background wallpapers.
   - **Snow Particle Simulation**: Interactive testing of `SetIconGroup` multi-emoji cascades with live physics.
   - **Cinematic Sequence Simulation**: Test Sadako horror crawl with glass crack fractals, screen shake, and climax screech at the 60% mark.

5. **Settings & Environment Configuration**:
   - Easily modify SQL Server connection strings, target database/tables, and network server file paths directly via the Settings modal or `theme_studio_settings.json`.

---

## How to Launch

### Option 1: One-Click Launcher
Double-click `Launch_RMSThemeStudio.bat` located in this directory.

### Option 2: .NET CLI
```bash
cd tools/RMSThemeStudio
dotnet run -c Release
```

### Option 3: Publish Standalone Binary
```bash
cd tools/RMSThemeStudio
dotnet publish -c Release -r win-x64 --self-contained false -o bin\Release\net8.0-windows\win-x64\publish
```

---

## Database Schema Reference

```sql
CREATE TABLE dbo.tbl_MessageThemes (
    ThemeID INT IDENTITY(1,1) PRIMARY KEY,
    ThemeName NVARCHAR(100) NOT NULL,
    ThemeBackgroundPath NVARCHAR(500) NULL,
    ReceiverBubble NVARCHAR(20) NOT NULL,
    SenderBubble NVARCHAR(20) NOT NULL,
    DefaultIcon NVARCHAR(20) NOT NULL,
    SetIconGroup NVARCHAR(200) NULL,
    CallBackgroundPath NVARCHAR(500) NULL,
    Status NVARCHAR(30) DEFAULT 'Public',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
```
