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

## Frequently Asked Questions

#### How do I use KCV to play Kancolle without a VPN?
Before or after logging in, press the "Set Regional Cookie" button. This will change your status from being an IP from outside Japan to being a Japanese native one. It doesn't change your IP or do anything else except change a flag value on your visitor cookie.

#### Is this cookie safe?
By all means it is. But it does mean that it makes playing Kancolle a ton more easier for regions that DMM does not support. From their twitter responses, they are alright with foreigners playing the game, but they will not provide any support for those outside of Japan. Until they change this rather open method of delegating foreigners from non-foreigners, this is probably the easiest way to play (and possible register for) the game itself.

#### My game is choppy when it plays fine in Chrome/Firefox/Flash Projector
Update Internet Explorer to the latest version available for your build of Windows. Windows 7 can go all the way to [Internet Explorer 11](http://windows.microsoft.com/en-us/internet-explorer/ie-11-worldwide-languages) which is by far the fastest version of IE they've made in years.

Also be sure to have the latest version of [Flash for Internet Explorer](http://get.adobe.com/flashplayer/otherversions/) installed!

#### The game will not start or KCV makes me download a flash file instead of opening the link
Install [Flash for Internet Explorer](http://get.adobe.com/flashplayer/otherversions/). You may also have to disable any programs that have been installed on to IE without your consent such as antivirus software and other programs.

#### I'm on Windows 7/XP and the program does not run!
You need to have [.NET Framework 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653) installed for your version of Windows. Those on Windows 8 do not have to do this.

#### I want the original Japanese names for ships/items/etc.
Easiest way is to just delete the text file corrisponding to the things you don't wish to be translated. You are also encouraged to use the [original version of KCV](http://grabacr.net/kancolleviewer) as it has an english UI now.

#### What is the difference between this and the original?
* Horizontal version is unique (for now) to this project. 
* The translation of all equipment, ships, and quests
* Logging features (for now)
* Regional cookie setting.
* Ranking information (for now)

Other than the above, this version is the same with just some minor tweaks to make it fit for an English translation. The plan is to hopefully add some features into the main project for all to enjoy.

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
