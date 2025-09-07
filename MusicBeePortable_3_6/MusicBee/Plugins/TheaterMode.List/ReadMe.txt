Notes for customising the setting .xml files - see Default.xml as an example

Setting Attributes:
  - enableScaling: when enabled, fonts, pictures and the layout are scaled to the display size (see notes at end for more details on how the scaling factor is calculated). You should enable this setting if you are planning to make your layout available to others
  - backgroundImage: optional override for the background image. By default, a .jpg picture with the same filename as this settings file will be loaded. But when this element is set, the image will be loaded from the SharedArtwork sub-folder eg. <settings backgroundImage="xxx.jpg" /> will load "Plugins\TheaterMode.List\SharedArtwork\xxx.jpg"
  - rotationFolder: optional override for the rotation sub-folder. By default, images in "<settings filename>.Rotation" folder will be loaded for rotation. But when this element is set, the images will be loaded from the SharedArtwork sub-folder eg. <settings rotationFolder="ZZZ" /> will load rotation images from "Plugins\TheaterMode.List\SharedArtwork\ZZZ\"
  - rotationPeriod: if there are images in the rotation sub-folder, then rotationPeriod determines the duration each image randomly chosen will be displayed
                    1..n = number of seconds to display each image
                    EndOfTrack = rotate at the end of each track
  settings for when activated in theater mode:
    - bdr: colour for a border, remove this element if you dont want a border
  settings for when activated as a screen saver:
    - idlePeriod: 0 = disable, 1..n = trigger after n seconds of idle time where MusicBee is playing a track
    - monitor: starting from 1, the index of a single monitor the screen saver should display in
    - otherMonitors: blackout = other monitors will display black;
                     active = other montitors will stay active and usable
         - this setting only applies when a specific monitor is set and the screen saver is manually activated. If activated by the idle timer, the other monitors always black out

Elements:
  - co-ordinates can be relative to another element or field eg. xAnchor="Panel.Right" x="-66" yAnchor="Title.Bottom" y="4" means this element will be located 66px to the left of the panel right and 4px below the Title field
  - or absolute eg. x="10" y="20" with no xAnchor
  - an element width can be constrained eg. widthDock="AlbumCover" width="-4" constrains the width to the width of the album cover less 4px
  - or alternatively constrained as the width between its X location and another point eg. widthDock="X:Panel.Right" width="-10" means the element will always cut-off 10px from the right panel
  - the album cover size is scaled using 1152 as the reference width (eg. panel is docked in musicbee and musicbee is 800px wide, then the album cover picture is scaled by 300 x 800/1152) and constrained by the min and max attribute values
  - fonts are scaled in proportion to the scaling applied to the album cover, but wont go below 7.5pt
  - for AlbumCover, the brightness attribute is between 0 and 1, where 0 is completely black and 1 is normal brightness
  - for Gallery, a set of images from the web will be retrieved matching: tags="XXXX,YYYY" and changes picture on interval="n" seconds - see the Landscape example
  - for Gallery, photos for the current playing artist will be retrieved: tags="artist"
  - for ArtistPicture, the fade attribute  is between 0 and 1, where 1 is full dimming and 0 is no dimming - see the Artist Pictures example
  - for Gallery and ArtistPicture, aspectRatio="keep/stretch/zoomKeep/zoomStretch": stretch=the picture is stretched to fill the entire area; keep=keep aspect ratio and fillColor="200,0,0,0" defines the colour to be used to fill the gaps (if any); zoomKeep=keeps the apspect ratio but allows the sides of the image to be cut off so no vertical gaps; zoomStretch=similar to zoomKeep, but stretches rather than keeps the apect ratio
  - for PlayerBar, extras="ProgressBar": includes a progress bar in the player bar
  - for ProgressButton, style="Diamond" shows a diamond shaped button otherwise the default is a round button
  - colours can have 3 (R,G,B) or 4(Alpha,R,G,B) values, where alpha determines how much the colour is blended into the background picture (0=fully transparent .. 255=opaque ie. no blending)
  - fields and text can be combined into one line using <child> elements
  - child elements can optionally have font and colour attributes. If not provided, the colour and font default to the main element colour and font respectively. Rating child elements can also optionally have a size attribute
  - an element or a child element can contain a "onClick" or "onDoubleClick" attribute and on clicking/ double clicking the element, an action is performed. Supported values:  OpenNowPlayingAssistant, OpenAutoHidePanel  eg. onDoubleClick="OpenNowPlayingAssistant"
  - a SoundGraph is available but commented out in this default settings file
  - because retrieving the SoundGraph data is very expensive it is only done once at the start of a track and uses the SoundGraph width from the default ScreenSaver.Settings file. So if you have rotation enabled and change the width of the sound graph, then the existing sound data will be scaled to that size
  - Rating and RatingAlbum are displayed as stars whose size can be changed by setting the size attribute eg. size="16" means the star will be 16pixels high
  - Rating and RatingLove can take a fg2 colour attribute to set "Love" colour or background rating stars. When fg2 is set for Rating, the rating can be clicked to set a new value
  - Rating and RatingLove can be set so they display only if they are configured to display in the main MusicBee player:  visible="MusicBeeSetting"
  - the following fields are available:
#
Artist
Title
Album
AlbumAndYear
AlbumArtist
BPM
DiscNo
DiscCount
DiscAndTrackNo
Duration
Genre
GenreCategory
Composer
Comment
Custom1 .. Custom9
Grouping
Kind
Mood
Occasion
PlayCount
Publisher
Quality
Rating
RatingAlbum
RatingLove
SkipCount
Tempo
TrackNo
TrackCount
Virtual1 .. Virtual9
Year
Year(yyyy)


credits:
default landscape image: "Waterfront" by "~ieStudio" @ http://iestudio.deviantart.com/gallery/#/d4rxpkp
