# AlbumInsertViewer 
### A MusicBee Plugin for Viewing Album Inserts

I've always wanted a way to view album inserts, scanned booklets, and PDFs directly inside MusicBee. It always felt like such a missed opportunity that the beautiful scans were hidden away and only accessible through the file browser—something most people rarely use. So, I created AlbumInsertViewer to solve that problem. This plugin lets you easily view album artwork, booklets, and inserts right within MusicBee, making the music experience even better.


**⚠️ This plugin is in early development and needs your help!** This is a work-in-progress project and we're looking for contributors to help improve and expand it. Check the Issues tab for ways to contribute.

## 🚨 Critical Features Needed - Help Wanted!

We especially need help with these MusicBee-specific features:

### **1. Built-in PDF Viewer (HIGHEST PRIORITY)**
**Currently**: PDFs open in external applications  
**Needed**: Display PDFs directly inside the plugin window  
**Why it matters**: Users want to view liner notes without leaving MusicBee  
**Challenge**: Need to implement or integrate a PDF rendering solution in C#

### **2. Dockable Plugin Panel**
**Currently**: Floating window only  
**Needed**: Make the plugin dockable in MusicBee's interface  
**Why it matters**: Better integration with MusicBee's workflow  
**Challenge**: Requires understanding MusicBee's docking API

### **3. Theme Integration**
**Currently**: Uses default Windows colors  
**Needed**: Match colors from user's active MusicBee theme  
**Why it matters**: Visual consistency with MusicBee's appearance  
**Challenge**: Need to access and apply MusicBee's theme colors dynamically

**If you have experience with MusicBee plugin development, PDF rendering in C#, or Windows Forms docking, your contribution would be invaluable!**

## What it does

This plugin displays album artwork found in your music folders. When you play a track in MusicBee, it automatically:
- Searches the album folder for images and PDFs
- Shows images in a slideshow on the "Images" tab
- Shows PDFs on the "Booklet" tab for viewing liner notes
- Falls back to embedded album art if no files are found

## Screenshots
![](./screenshots/ss01.png)
![](./screenshots/ss02.png)
![](./screenshots/ss03.png)

## Installation

1. Download `AlbumInsertViewer.dll` from the Releases page
2. Copy it to your MusicBee plugins folder: `C:\Program Files (x86)\MusicBee\Plugins\`
3. Restart MusicBee
4. Open the viewer: View → Album Inserts Viewer

**Requirements**: MusicBee 3.0+, Windows, .NET Framework 4.8

## Usage

Just play a track and the plugin will automatically load images from the album folder. Images cycle automatically if there's more than one. Click on images or use the "Open in viewer" link to view them externally.

The plugin searches recursively, so images in subdirectories like "Scans" or "Artwork" will be found automatically.

## Project Structure

- `Plugin.cs` - Main plugin file that handles MusicBee integration
- `Form1.cs` - The UI window with image viewer and PDF tab
- `Form1.Designer.cs` - Auto-generated UI code
- `Form1.resx` - UI resources

The code in `Form1.cs` is fully documented with Doxygen comments to help you understand how it works.

## Building from Source

1. Clone this repository
2. Open `AlbumInsertViewer.sln` in Visual Studio
3. Add a reference to `MusicBeePlugin.dll` from your MusicBee installation folder
4. Build the solution (Release mode)
5. Copy the output DLL to MusicBee's Plugins folder

## Contributing

**This project needs contributors!** Whether you're experienced or just learning, all contributions are welcome.

### How to contribute

1. Look at the [Issues](https://github.com/ShamalLakshan/AlbumInsertsViewer/issues) page to find something to work on
2. Fork the repository
3. Create a new branch for your changes
4. Make your changes and test them with MusicBee
5. Submit a pull request

### Code guidelines

- Use PascalCase for public methods, camelCase for private fields
- Dispose images properly to prevent memory leaks
- Test your changes with different album folder structures
- Use Doxygen-style comments for documentation (optional)

### What needs work

**Priority features** (see Issues for details):

1. **🔴 Built-in PDF viewer** - Display PDFs in the plugin instead of external apps (MOST IMPORTANT!)
2. **🔴 Dockable panel** - Make plugin dockable in MusicBee's interface
3. **🔴 Theme support** - Match MusicBee's active theme colors
4. Manual slideshow controls (next/previous buttons)
5. Configurable slideshow timer
6. Better error handling
7. Keyboard shortcuts
8. UI improvements
9. Testing with edge cases
10. Bug fixes

The top 3 items require MusicBee-specific knowledge. If you've worked with MusicBee plugins before, we'd especially appreciate your help!

Don't see what you want to work on? Open a new issue to discuss it!

## Current Features

- Automatic image slideshow
- Recursive folder search for images
- PDF booklet detection (opens in external viewer)
- Fallback to embedded artwork
- Draggable window
- External viewer launch

**Note**: PDF viewing currently requires an external application. We need contributors to help implement built-in PDF rendering!

## Known Issues

See the [Issues](https://github.com/ShamalLakshan/AlbumInsertsViewer/issues) page for current bugs and limitations.

## Roadmap

**High Priority (need contributors with MusicBee API experience):**
- **PDF viewer embedded in plugin** - Currently opens external apps, need in-app viewing
- **Dockable panel support** - Integrate with MusicBee's docking system
- **Theme color integration** - Dynamically match MusicBee's active theme

**Other features we'd like to add:**
- Previous/next buttons for manual control
- Configurable slideshow speed
- Zoom and pan controls
- Fullscreen mode
- Keyboard shortcuts
- Image sorting options

Have other ideas? Open an issue!

## License

MIT License - see LICENSE file

## Support

- Found a bug? [Open an issue](https://github.com/ShamalLakshan/AlbumInsertsViewer/issues)
- Have a feature idea? [Open an issue](https://github.com/ShamalLakshan/AlbumInsertsViewer/issues)
- Want to contribute? Check the [Issues](https://github.com/ShamalLakshan/AlbumInsertsViewer/issues) page!

---

**This is a community project - your contributions make it better!**