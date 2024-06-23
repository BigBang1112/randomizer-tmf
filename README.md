# Randomizer TMF (powered by [GBX.NET](https://github.com/BigBang1112/gbx-net))

[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/BigBang1112/randomizer-tmf?include_prereleases&style=for-the-badge)](https://github.com/BigBang1112/randomizer-tmf/releases)
[![GitHub all releases](https://img.shields.io/github/downloads/BigBang1112/randomizer-tmf/total?style=for-the-badge)](https://github.com/BigBang1112/randomizer-tmf/releases)
[![Code Coverage](https://img.shields.io/badge/Code%20Coverage-63%25-yellow?style=for-the-badge)](https://github.com/BigBang1112/randomizer-tmf)

**Randomizer TMF - Random Map Challenge for TMNF/TMUF** is a project (inspired from the Flink's Random Map Challenge) that ports random map picking experience to TMNF and TMUF games.

The project combines features of [TMX](https://tm-exchange.com/), autosave Gbx files, and executable arguments to create flawless and automatic joy of exploration.

### You will never receive a map you finished before!

![Dashboard](Dashboard.jpg "Dashboard")

![Modules](Modules.jpg "Modules")

## [Installation](https://github.com/BigBang1112/randomizer-tmf/wiki/Installation)

## [QnA](https://github.com/BigBang1112/randomizer-tmf/wiki/QnA)

### Most common questions

#### [What is my installation directory?](https://github.com/BigBang1112/randomizer-tmf/wiki/QnA#what-is-my-installation-directory)
#### [Why don't I see the small windows/panels/modules above my game?](https://github.com/BigBang1112/randomizer-tmf/wiki/QnA#why-dont-i-see-the-small-windowspanelsmodules-above-my-game)
#### [Why my game changes its resolution when the map starts?](https://github.com/BigBang1112/randomizer-tmf/wiki/QnA#why-my-game-changes-its-resolution-when-the-map-starts-or-is-switched)
#### [Why is the timer running during map load?](https://github.com/BigBang1112/randomizer-tmf/wiki/QnA#why-is-the-timer-running-during-map-load)

## Features

- Race, Platform, Stunts, Puzzle gamemodes!
- TMNF, TMUF, Nations, Sunrise, Original TMX site randomization
- All kinds of TMX randomization filters supported by the site
  - Map name, author, environment, vehicle, tag, mood, difficulty, routes, leaderboard type, min. AT, max. AT, uploaded before, uploaded after, and many more hidden ones in Config.yml
- Filter Unlimiter maps!
- Custom time limit
- Preview of your autosaves

## Hidden settings

Some settings are hidden in the `Config.yml` file and not available in the UI. You can find the file in the installation directory.

```yml
# {0} is the map name, {1} is the replay score (example: 9'59''59 in Race/Puzzle or 999_9'59''59 in Platform/Stunts), {2} is the player login.
ReplayFileFormat: '{0}_{1}_{2}.Replay.Gbx'
# When replay cannot be accessed due to permissions/corruption, how many times to attempt the parse.
ReplayParseFailRetries: 10
# When replay cannot be accessed due to permissions/corruption, how many milliseconds to wait before next attempt.
ReplayParseFailDelayMs: 50
# If to disable in-depth parse of autosave replays, used for further validation.
DisableAutosaveDetailScan: false
# If to disable automatic skip completely.
DisableAutoSkip: false
# When should automatic skip apply. Options are: AuthorMedal, GoldMedal, SilverMedal, BronzeMedal, Finished
AutoSkipMode: AuthorMedal
# Discord Rich Presence configuration.
DiscordRichPresence:
  # Disable Discord Rich Presence entirely.
  Disable: false
  # Disable map thumbnail in Discord Rich Presence, questionmark icon will be used instead.
  DisableMapThumbnail: false
```

## Download goals

[![GitHub all releases](https://img.shields.io/github/downloads/BigBang1112/randomizer-tmf/total?style=for-the-badge)](https://github.com/BigBang1112/randomizer-tmf/releases)

- ✔️ **50 downloads within 1 week** - Guaranteed support throughout 2023
- ✔️ **100 downloads** - Discord Rich Presence integration
- ✔️ **300 downloads** - TMUF theme **(coming at the end of the year)**
- ✔️ **500 downloads** - Profile management (fresh account randomization) **(coming at the end of the year)**
- ✔️ **2000 downloads** - Automated RMC leaderboards **(coming at the end of the year)**
- **10000 downloads** - UI directly displayed ingame (fullscreen support)
- **99999 downloads** - Randomizer TMTurbo

Gogo!

## Special thanks

To people that rooted the project:

- Flink and Greep (for inventing the challenge)
- Arkady (for inspiring this project with his `Play` feature in [Gbx Map Browser](https://github.com/ArkadySK/GbxMapBrowser))

To all early testers:

- ajsasflaym
- Zai
- pekatour
- LinuxCat
- Poutrel
- Arkes

To all TMX maintainers that make this possible!

And to everyone that still believe in me!
