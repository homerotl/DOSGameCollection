# DOSGameCollection

This program is a front-end for DOSBox-Staging with an emphasis on preserving many different aspects of the DOS gaming experience era, like access to manuals, box art, original disttribution media and providing easy access to playing the game on a modern computer.

DOSGameCollection is being vibe-developed with Google Gemini Code assist as a C# DotNet 9.0 WinForms project and Visual Studio Code.

## Library directory specification

This program expects games to be arranged in a specific folder structure like so:

```
/Library
    --/[Game Name]
        --/game.cfg
        --/dosbox-staging.conf (DOSBox Staging configuration file)
        --/mapper.cfg (DOSBOX Mapper configuration file)
        --/manual.pdf
        --/game-files (This will be mounted as C)
        --/notes.txt
        --/cheats-and-secrets.txt
        --/disk-images
            --/file-info.txt (CSV file with display names for files, when different)
            --/disk_01.img
            --/disk_02.img
            --/disk_01.png (picture or scan of the media itself)
            --/disk_02.png
        --/isos
            --/file-info.txt
            --/game_cd_1.iso
            --/game_cd_2.iso
            --/game_cd_1.png (picture or scan of the media itself)
        --/media
            --/synopsis.txt
            --/icon.png
            --/background.png

            --/walkthrough
                --/page_01.txt (or pdf)
                --/file-info.txt
            --/box-art
                --/front.png
                --/back.png
            --/captures
                --/capture-info.txt
                --/capture-001.png
            --/videos
                --/file-info.txt
                --/video-001.avi
            --/inserts
                --/file-info.txt
                --/insert_01.pdf
                --/insert_02.png
            --/ost (original soundtrack, .mp3)
                --/midi
                  --/file-info.txt
                  --/track_01.mid
                --/file-info.txt
                --/cover.png
                --/track_01.mp3
        --/other (other files which don't fit in any other category)
          --/file-info.txt 
```

## Game file specification

Each game has a game.cfg, top level configuration file. Here is the structure of that file:

```
# Comments
game.name=Game Name
game.parental.rating=[ESRB ratings]
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

## File Information example

An example of the format for the "file-info.txt" file is as follows:

```
disc_01.img,Installation disk 1
disc_02.img,Installation disk 2
```
