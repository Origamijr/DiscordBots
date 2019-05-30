# Discord-Bots

This is a collection of C# discord bots created using the discord.net API. There is no general theme among the bot's. They simply contain things that I think are interesting, and that I thought deserved a testing medium besides the console. All bots are tested on my discord server. Add me on discord (Alumina#4305) if you are interested/want in.

All bots should contain a Ping.cs module for the most primitive of commands, but the contents of Ping.cs may differ. 

## DelBot

This is the main bot that will contain most of the experimental modules. It is built to function within my own server. Commands can be triggered by bots.

### Functionality
* Message Queueing
  * Functionality to queue messages to hopefully avoid rate limit issues, but still work in progress.
* Database Management
  * storing strings into Json files using Json.net API. 
  * Database utility extends to other modules
* RPG Minigame
  * Users can create profiles with stats
  * Basic battle simulations against other user's stored profiles. Work in progress
* Natural Language Processing
  * Ability to create PCFGs and run sample simulations from the start state
  
## MarxBot

MarxBot is built to test out the even more experimental modules that I didn't feel fit into DelBot's functionality. It mainly acts as a companion bot to DelBot to test out bot-bot interraction. Commands can be triggered by bots.

### Functionality
* Math
  * Recursive factorial by calling command on itself
  * Quick square root just cuz I really like that hack.

## TestBot

This bot was really made as a demonstration to a friend on how to create a discord bot, but I decided to use it to experiment with "commandless" functionality. Deployed across several servers I'm in as more of a joke than anything else. Commands cannot be triggered by other bots.

### Functionality
* Randomness
  * Randomly responds to user messages to remind people that it exists.
  * Able to roll a custom set of dice

## NablaBot

This bot is built in order to try out the Discord.py API. At first, early in development, it will mimic the functionality of DelBot, but it will eventually diverge to experiment with more NLP and ML type things.

### Functionality
* Message Queueing
  * Similar to Del, but in python
* Anime Analysis
  * WIP for class project. Most of source code will not be made available for academic integrity purposes.
