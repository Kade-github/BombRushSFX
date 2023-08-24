# Tutorial

Okay so, a folder (on startup for the first time) should be made in **Bomb Rush Cyberfunk_Data/StreamingAssets/Mods/BombRushSFX/SFX/**, this is where you put all your sfx.

## Thunderstore mods

If you're going to depend on BombRushSFX, you can instead put your SFX in **BepinEx/Config/BombRushSFX/SFX**, instead of the mods directory in streamingassets.

I like to organize in folders, but you don't have to.

## Step 1

Get your audio files.

### Supported Formats
- AIF(F)
- IT
- MOD
- MP2
- MP3
- OGG
- S3M
- WAV
- XM
- FLAC

And put them in that folder.

![image](https://github.com/Kade-github/BombRushSFX/assets/26305836/bb0290b7-2dda-4cbb-bfda-49eb02d9fd19)

I'm going to replace red's jumping voice.

## Step 2

Create a new text file, it can be named anything.

Now go into this [file](https://github.com/Kade-github/BombRushSFX/blob/main/IDS.md), and find what collection you want to replace. For voices, it'll be prefixed with "Voice"

Mine would be "VoiceMetalHead"

![image](https://github.com/Kade-github/BombRushSFX/assets/26305836/5deea60b-46de-49e6-bd76-abb878506949)

Which is ID **44**

So that means I would put this in the first line

```
collection: 44
```

Then to tell the game what my audios in the same folder are called, I would create something like this

```
collection: 44
audios: jump1.ogg, jump2.ogg, jump3.ogg, jump4.ogg, jump5.ogg, jump6.ogg, jump7.ogg 
```

## Step 3

Finally, we need to find the type of audio the collection will contain, and that would be our voice jumps.

Hop into the ID file again, and find VoiceJump-

which is id **489**

and add the final line

```
collection: 44
audios: jump1.ogg, jump2.ogg, jump3.ogg, jump4.ogg, jump5.ogg, jump6.ogg, jump7.ogg 
id: 489
```

Then you're done!

If all works it should look like this

![image](https://i.imgur.com/tYfHhUf.png)

Launch the game, and you should have your custom SFX.

(Oh btw, you can have more than one txt file for sounds in the same folder, and you can use the same collection twice. It'll just add a new audio type to that collection)

And in the txt files, if a line starts with a **#** it is a comment.

Like so (it will be ignored)

```
# 44 is the id of VoiceMetalHead in the code
collection: 44
audios: jump1.ogg, jump2.ogg, jump3.ogg, jump4.ogg, jump5.ogg, jump6.ogg, jump7.ogg 
# 489 is VoiceJump
id: 489
```

## Important Note

If you replace VoiceMetalHead (or any collection really) as a collection, you will remove all base game sounds in that collection. So they wont play in game.
