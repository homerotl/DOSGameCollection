# DOSGameCollection

This program is a front-end for DOSBox-Staging with an emphasis on game collecting and preservation.

The game library is maintained in a folder structure with the following definition

---
/Library
    --/[Game Name]
        --/game.cfg
        --/dosbox-staging.conf
        --/manual.pdf
        --/game-files <- This will be mounted as C:
        --/disk-images
            --/properties.txt
            --/disk_01.img
            --/disk_02.img
            --/disc_scan_01.png
            --/disc_scan_02.png
            --/checksums.md5 (MD5 checksums of the disk images)
        --/isos
            --/game_cd_1.iso
            --/game_cd_2.iso
            --/game_cd_1.pmg (picture or scan of the media itself)
            --/checksums.md5 (MD5 checksums of the disk images)
        --/media
            --/main.png
            --/icon.png
            --/synopsis.txt
            --/background.png
            --/walkthrough
                --/page_01.txt (pdf)
            --/cheats_and_secrets
            --notes
            --/ost
                --/index.csv
                --/cover_art.png
                --/Track 01.mp3
            --/box-art
                --/front.png
                --/back.png
                --/art_01.png
            --/inserts
                --/insert_01_front.png
                --/insert_01_back.png
            --/captures
                --/capture_001.png
            --/videos
                --/video_001.mpg
--

Each game has a game.cfg, top level configuration file. Here is the structure of that file:

---
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
---