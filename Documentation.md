This document provides information on all of Sunberry Village's custom features, added by sophiesalacia and shekurika.

## Contents
* [Features](#features)
  * [Conversation Topics Upon Quest Completion](#conversation-topics-upon-quest-completion)
  * [Big Animations](#big-animations)
  * [Lighting](#lighting)
  * [Infinite Time on Special Orders](#infinite-time-on-special-orders)
  * [Ari's Market Daily Special](#aris-market-daily-speacial)
  * [Multi Page Map Strings](#multi-page-map-strings)
  * [Text Emojis](#text-emojis)

## Features
This is a comprehensive list of all features added by Sunberry Village's C# component along with their explanations and examples.

### Conversation Topics Upon Quest Completion
There is no asset to edit for this one! Just add it directly to your dialogue! Note: Because there's going to be an absolute shit-ton of combinations for daily quests, and an absurd amount of standard quests as well, I wouldn't recommend trying to cover everything. Just pick whatever quests or characters are meaningful to your character and go from there. 

Format: Standard quests have a 7-day CT duration. They're formatted as follows: "QuestComplete_[Name Of Quest]". For example, "QuestComplete_Introductions" indicates that the quest Introductions was completed. Pretty self explanatory. 

Daily quests (the ones that show up on the bulletin board and are randomly generated) have a 1-day CT duration. They're formatted as follows: "QuestComplete_[Quest Type]_[Quest Giver]". Quest Type will always be one of four values: "Delivery", "Fishing", "Gathering", or "SlayMonsters". Quest Giver corresponds to the **internal** name of the NPC that posted the request. See below for various examples.


```js
        {
            "LogName": "Elias Quest CT Example",
            "Action": "EditData",
            "Target": "Characters/Dialogue/EliasSBV",
            "Entries": {
                "QuestComplete_Introductions": "Hey, @.$2#$e#Have you met everyone in town yet?$1#$b#You must be exhausted.$3",
                "QuestComplete_Delivery_Miyoung": "Hey, @! Thanks for grabbin' that stuff for Miyoung earlier.$3#$e#Trips out of town are getting harder for her lately.$2",
                "QuestComplete_Gathering_Clint": "I hear the blacksmith over in Pelican Town has got you gathering ore for him. Any chance you could slip some my way?$0#$e#I was thinking about surprising Maia with it.$10#$b#What?$5"
            }
        },
```

### Big Animations
The asset to target for this one is "SunberryTeam.SBV/Animations. Ask sheku for more information if something is unclear.

Jonghyuk's example:

```js
        {
            "LogName": "Jonghyuk Animation Data Edit",
            "Action": "EditData",
            "Target": "Data/animationDescriptions",
            "Entries": {
                "jh_boxing": "8/8 9 10 11 12/12"
            }
        }, 
        {
            "LogName": "Jonghyuk Animation Asset Edit",
            "Action": "EditData",
            "Target": "SunberryTeam.SBV/Animations",
            "Entries": {
                "jh_boxing": {
                    "npcName": "Jonghyuk",
                    "size": "32, 32"
                }
            }
        },
```

Elias' example:

```js
    {
      "Action": "EditData",
      "Target": "SunberryTeam.SBV/Animations",
      "Entries": {
        "elias_fish": {
          "npcName": "EliasSBV",
          "size": "16, 64",
          "offset": "0, 96"
        }
      }
    }
```

### Lighting

NOTE: This is still a work in progress so it's subject to change. Editing the "SunberryTeam.SBV/Lights" asset will allow you to define lights of custom intensity. Will be further detailed in the future once things are more set in stone.


```js
        {
            "LogName": "Lighting Example",
            "Action": "EditData",
            "Target": "SunberryTeam.SBV/Lights",
            "Entries": {
                "sophie.UniqueLightID": {
                    "Location": "Custom_SBV_SunberryRoad",  // game location to add light in
                    "Position": "10.5, 12.22",              // tile location: decimal values are okay
                    "Intensity": "3.5"                      // intensity of light: decimal values are fine for this too
                }
            }
        }
```

### Infinite Time on Special Orders

TODO

### Ari's Market Daily Special

TODO

### Multi Page Map Strings

TODO

### Text Emojis

TODO
