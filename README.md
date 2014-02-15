KanColleViewer! (提督業も忙しい！)
--
**--This is the horizontal branch for those on computers not vertically endowed. Max width must be under 1440px**

KCV is a browser utility that makes it easy to play Kantai Collection.
This is the translation project of the original [KanColleViewer](http://grabacr.net/kancolleviewer)

##### Main Contributors
* [@Zharay](http://twitter.com/Zharay) -- English Version
* [@Grabacr07](https://twitter.com/Grabacr07) -- Original author
* [silfumus](https://github.com/silfumus) -- Continued Work (defunct)
* [southrop](https://github.com/southrop) -- Original text file translation code (defunct)
* [m-kc](https://github.com/m-kc) -- Rankings section
* [taihou](https://github.com/taihou) -- Logging option

## Downloads
All builds can be found in the following links below.

* [SkyDrive](http://sdrv.ms/1b01S24) -- Major revision binaries

## About This Project
KanColleViewer uses the web browser component of the Windows Presentation Foundation (WPF) in combination of [FiddlerCore](http://fiddler2.com/fiddlercore) to capture communication packets inbetween the server and the page itself.

Of course, we do not change the contents of the packets in anyway and is used to provide information to the program's components.

### About The Translation
Main work on the translation of ships, equipment, and quests fall solely on the now defunct [silfumus' version of KanColleViewer](https://github.com/silfumus/KanColleViewer) and those who contributed to it. I manually merged the changes to this version of the fork and went from there.

The way the translation works is that it manually loads the translation from several text files which holds both the Japanese name and the English translation of the item in question. This can theoretically be used to translate the contents of the game easily to any language needed.

### Main Functions
* Real-time display of elements such as instant repair and build materials.
* Real-time display of the number of Ship Girls and equipment held at the HQ
* List of all ship girls currently in your fleets
* Repair and Construction end timers and notifications.
* List of all quests currently active as well as a listing of available daily and weekly quests.
* Screenshot saving
* Sound muting

### Requirements
* Windows 8 or later
* Windows 7 (limited functionality)
* [.NET Framework 4.5](http://www.microsoft.com/ja-jp/download/details.aspx?id=30653)

Developed and tested mainly in Windows 8.1 Pro. Windows 7 can also be used to run the program, but you will not have the built in notification system that is found in Windows 8. 

Windows 7 requires that you install .NET Framework 4.5. Windows 8 already has this installed by default.

### Development Environment, Language, Libraries
This was mainly developed using C# + WPF in Windows 8.1 Pro and Visual Studio Premium 2013.

* [Reactive Extensions](http://rx.codeplex.com/)
* Interactive Extensions
* [Windows API Code Pack](http://archive.msdn.microsoft.com/WindowsAPICodePack)
* [Livet](http://ugaya40.net/livet) (MVVM Infrastructure)
* [DynamicJson](http://dynamicjson.codeplex.com/) (JSON deserialization processing)
* [FiddlerCore](http://fiddler2.com/fiddlercore) (Network capture)


#### Liscense
* MIT License

To be released under the MIT license as an open source / free software.
