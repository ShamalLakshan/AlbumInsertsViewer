# AlbumInsertsViewer Configuration Documentation

## File Location
```
C:\Users\[YourUsername]\AppData\Roaming\MusicBee\albuminsertsviewer.colors.conf
```

**Quick Access:** Open `%appdata%\MusicBee\` in Windows Explorer (Win+R, type path, press Enter)

---

## Configuration Reference

### Color Settings

All colors use RGB format: `R,G,B` where each value is between 0-255.

#### `BackgroundColor=0,0,0`
- **What it does:** Sets the main background color for the plugin window
- **Default:** Black (0,0,0)
- **Example:** White = `255,255,255`, Gray = `128,128,128`

#### `ForegroundColor=255,255,255`
- **What it does:** Sets the text color throughout the plugin
- **Default:** White (255,255,255)
- **Note:** Make sure this contrasts well with BackgroundColor for readability

#### `ButtonBackColor=30,30,30`
- **What it does:** Background color for navigation buttons (Scans/Booklet) when NOT selected
- **Default:** Dark gray (30,30,30)

#### `ButtonForeColor=255,255,255`
- **What it does:** Text color for navigation buttons when NOT selected
- **Default:** White (255,255,255)

#### `ActiveButtonBackColor=60,60,60`
- **What it does:** Background color for the currently selected navigation button
- **Default:** Medium gray (60,60,60)
- **Note:** Should be different from ButtonBackColor to show which tab is active

#### `ActiveButtonForeColor=255,255,255`
- **What it does:** Text color for the currently selected navigation button
- **Default:** White (255,255,255)

#### `PanelBackColor=0,0,0`
- **What it does:** Background color for the main content area (where images/PDFs are displayed)
- **Default:** Black (0,0,0)
- **Note:** This is the area behind your images

---

### Slideshow Settings

#### `SlideshowIntervalSeconds=3`
- **What it does:** Time in seconds between each image in the slideshow
- **Default:** 3 seconds
- **Minimum:** 1 second
- **Recommended:** 3-10 seconds
- **Note:** Only applies when multiple images are available

#### `AutoStartSlideshow=true`
- **What it does:** Whether to automatically start the slideshow when multiple images are found
- **Default:** `true`
- **Options:** `true` or `false`
- **Note:** If set to `false`, only the first image will be displayed

---

### Display Settings

#### `ShowOpenInViewerLink=true`
- **What it does:** Shows/hides the "🔗 Open in viewer" link on images
- **Default:** `true`
- **Options:** `true` or `false`
- **Note:** The link appears in the bottom-right corner when hovering over images

#### `WindowWidth=800`
- **What it does:** Default width of the floating window in pixels
- **Default:** 800 pixels
- **Minimum:** No hard limit, but values below 400 may cause layout issues
- **Note:** Only affects the floating window, not the dockable panel

#### `WindowHeight=600`
- **What it does:** Default height of the floating window in pixels
- **Default:** 600 pixels
- **Minimum:** No hard limit, but values below 300 may cause layout issues
- **Note:** Only affects the floating window, not the dockable panel

---

## Tips for Customization

### Matching Your MusicBee Theme

**Dark Theme Example:**
```ini
BackgroundColor=20,20,20
ForegroundColor=220,220,220
ButtonBackColor=35,35,35
ActiveButtonBackColor=70,70,70
PanelBackColor=15,15,15
```

**Light Theme Example:**
```ini
BackgroundColor=240,240,240
ForegroundColor=30,30,30
ButtonBackColor=220,220,220
ActiveButtonBackColor=180,180,180
PanelBackColor=250,250,250
```

**Blue Theme Example:**
```ini
BackgroundColor=15,25,45
ForegroundColor=220,230,240
ButtonBackColor=25,40,70
ActiveButtonBackColor=40,60,100
PanelBackColor=10,20,35
```

### Finding RGB Values

1. Use any color picker tool (Windows: Paint, online tools, etc.)
2. Get the RGB values
3. Enter them as `R,G,B` in the config file

### Performance Considerations

- **SlideshowIntervalSeconds:** Lower values (1-2 seconds) may cause higher CPU usage with large images
- **WindowWidth/Height:** Very large values may impact performance on lower-end systems

---

## Applying Changes

### For Dockable Panel
1. Edit and save the config file
2. Close and reopen MusicBee (or restart the plugin)

### For Floating Window
1. Edit and save the config file
2. Close and reopen the window

---

## Troubleshooting

**Colors not showing correctly?**
- Make sure RGB values are between 0-255
- Verify the format is exactly `R,G,B` with commas and no spaces
- Check that you saved the file after editing

**Slideshow not working?**
- Ensure `AutoStartSlideshow=true`
- Verify `SlideshowIntervalSeconds` is at least 1
- Make sure you have multiple images in your album folder

**Settings not applying?**
- Check that you edited the correct config file
- For dockable panel: Restart MusicBee or reload the plugin
- For floating window: Completely close and reopen it
- Verify there are no typos in the setting names

**Config file missing?**
- The file is created automatically on first run
- Open the plugin at least once to generate the file
- Check the exact path: `%appdata%\MusicBee\`

---

## Default Configuration

If you want to reset to defaults, delete the config file and restart MusicBee. A new file with default values will be created automatically.

Default theme: Black background, white text, dark gray buttons.