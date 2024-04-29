This document provides information on all of Sunberry Village's custom features, added by sophiesalacia and shekurika.

## Contents
* [Features]
  * [Conversation Topics Upon Quest Completion]
  * [Big Animations]
  * [Lighting]
  * [Infinite Time on Special Orders]
  * [Ari's Market Daily Special]
  * [Multi Page Map Strings]
  * [Text Emojis]

## Features
This is a comprehensive list of all features added by Sunberry Village's C# component along with their explanations and examples.

### Conversation Topics Upon Quest Completion
There is no asset to edit for this one! Just add it directly to your dialogue! The syntax is: ===ConversationTopicName=== for the default duration of 1 day, ===ConversationTopicName/[number]=== for a duration of [number] days. Your conversation topic ID can consist of letters, numbers, and underscores. HOWEVER, if you're using $y format, do not put underscores in your CT name because $y syntax requires using underscores for formatting, it will break the formatting of your dialogue if you do.

```js
        {
            "LogName": "Alex CT Example",
            "Action": "EditData",
            "Target": "Characters/Dialogue/Alex",
            "Entries": {
                "fall_Wed_01_01": "Yeah, I know. That's why I have it like this.===Complimented_Alex_Hair===",
                "fall_Wed_01_02": "Yeah right. You're just jealous that I look so good.===Insulted_Alex_Hair===",
            }
        },
```

```js
        {
            "LogName": "Derya CT Example with $y",
            "Action": "EditData",
            "Target": "Characters/Dialogue/DeryaSBV",
            "Entries": {
                "Thu6": "Hey, @.$h#$b#$y 'Do you have any chickens on your farm?_Yes._Oh, hell yeah! Can I come play with them?===DeryaPlayedWithChickens/28===$u_No._Oh, bummer.===DeryaDidNotPlayWithChickens/28===$s' ",
            }
        },
```
