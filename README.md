# DiscordRP-tModLoader [Forum Link](https://forums.terraria.org/index.php?threads/discordrp-rich-presence-for-terraria.66146/)
[![Terraria](https://img.shields.io/badge/Terraria-tModLoader-green.svg)](https://forums.terraria.org/index.php?threads/1-3-tmodloader-a-modding-api.23726/) [![tModLoader](https://img.shields.io/badge/tModLoader-v0.11.7.5-brightgreen.svg)](https://github.com/blushiemagic/tModLoader/releases/v0.11.7.5/) [![GitHub release](https://img.shields.io/github/release/PurplefinNeptuna/DiscordRP-tModLoader.svg)](https://github.com/PurplefinNeptuna/DiscordRP-tModLoader/releases/latest)
### Go to [Wiki](https://github.com/PurplefinNeptuna/DiscordRP-tModLoader/wiki) for more information

Discord Rich Presence for Terraria tModLoader Beta

Show your in-game activities to your DIscord status using Discord Rich Presence

Displayed info:
- Player current biome / current encontered boss / current event
- Weapon damage (with type)
- World name and difficulty (Normal/Expert)
- Health, Mana, Defense (or just "Dead")
- Play time


## Development

1. Clone this repo to tmod's source dir

```
$ git clone https://github.com/staticfox/DiscordRP-tModLoader "C:\Users\USERNAME\Documents\My Games\Terraria\ModLoader\Beta\Mod Sources\DiscordRP"
```

2. Download Discord RPC libraries:

    a. Download https://github.com/discord/discord-rpc/releases/download/v3.4.0/discord-rpc-win.zip and place `discord-rpc/win64-dynamic/discord-rpc.dll` in `lib/discord-rpc.dll`. (https://github.com/discord/discord-rpc)

    b. Download `artifacts\net35\DiscordRPC.dll` from https://ci.appveyor.com/project/Lachee/discord-rpc-csharp/build/artifacts and place it in `lib/DiscordRPC.dll`. (https://github.com/Lachee/discord-rpc-csharp)
