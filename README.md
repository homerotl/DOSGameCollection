# DOSGameCollection

This program is a front-end for DOSBox-Staging with an emphasis on preserving many different aspects of the DOS gaming experience era, like access to manuals, box art, original disttribution media and providing easy access to playing the game on a modern computer.

DOSGameCollection is being vibe-developed with Google Gemini Code assist as a C# DotNet 9.0 WinForms project and Visual Studio Code.

## Library directory specification

This program expects games to be arranged in a specific folder structure like so:

```
/Library
    --/[Game Name]
        --/game.cfg
        --/dosbox-staging.conf
        --/manual.pdf
        --/game-files <- This will be mounted as C:
        --/disc-images
            --/disc-info.txt
            --/disc_01.img
            --/disc_02.img
            --/disc_01.png
            --/disc_02.png
        --/isos
            --/disc-info.txt
            --/game_cd_1.iso
            --/game_cd_2.iso
            --/game_cd_1.png (picture or scan of the media itself)
        --/media
            --/synopsis.txt
            --/box-art
                --/front.png
                --/back.png
                --/art_01.png
            --/captures
                --/capture-info.txt
                --/capture_001.png
            --/videos
                --/video-info.txt
                --/video_001.mpg
            --/inserts
                --/insert-info.txt
                --/insert_01.pdf
                --/insert_02.pdf
            --/main.png
            --/icon.png
            --/background.png
            --/walkthrough
                --/page_01.txt (or pdf)
                --/walkthrough-info.txt
            --/cheats_and_secrets
            --notes.txt
            --/ost
                --/midi
                  --/track-info.txt
                  --/track_01.mid
                --/track-info.txt
                --/cover.png
                --/track_01.mp3
```

## Game file specification

Each game has a game.cfg, top level configuration file. Here is the structure of that file:

```
# Comments
game.name=Game Name
game.rating=[NR,...]
game.publisher=Publisher
game.developer=Developer
game.release.year=1978
game.genre=
game.type=[Full,Demo,Shareware,Cracked]
favorite=false

[isos]
iso_file_name.iso

[commands]
CD GAMES\FOLDER
EXECUTABLE.EXE

[setup-commands]
CD GAMES\FOLDER
SETUP.EXE
```

Both specifications are more of good wishes than reality. At this stage not all the properties and files are used, but this is the blueprint for future development.

## Disk images disc-info.txt file

An example of the format for the disc-info.txt text file is as follows:

```
disc_01.img,Installation disk 1
disc_02.img,Installation disk 2
```
## Open Source Project Dependencies 

* [LibVLCSharp.Winforms](https://www.nuget.org/packages/LibVLCSharp.WinForms/)