# Contacts [![Build Status](https://travis-ci.org/discord-csharp/Contacts.svg?branch=dev)](https://travis-ci.org/discord-csharp/Contacts)

Contacts is a Discord Bot that is actively developed and maintained by the C# Discord Server. Its aim is to make it easier to moderate the Discord Chat and to provide a small toolkit for answering questions about C# and .Net.

## Features

The roadmap of Contacts changed a lot since the first kickoff. A lot of people contributed good ideas which made its way into the bot. For now, the following modules made its way into the Contacts Bot:

**Moderation**

- Mass-deleting of messages
- Muting a person for N:N:N amount of time
- Logging leaves and joins as well as used commands.


**Toolkit**

- MSDN Lookup
- .Net source lookup
- Discord User whois
- Unflipping tables

## Installation
 - Navigate to Contacts/ContactsBot/Configs/ directory
 - Modify two configuration files in each directory under ContactsBot/Config/ (PostgreSQLConfiguration and BotConfiguration) and provide necessary informations and configuration.
 
 Note: default.json is for release build and dev.json is for debug build. (Visual Studios Solution file is configured to debug build with DEV define flag.)
 
 Note: Add your bot token to "Token" line in default.json under BotConfiguration directory, for more information how, read below.
 
 Note: For Logging Guild ID and LoggingChannelID, go to DiscordApp website and navigate to the logging channel you have control over and you will notice 2 sets of numbers in the URL that are seperated by a forward slash seperator "/". First number is the Guild ID and the Second number is the channel ID. https://discordapp.com/channels/ **##################/##################**
 
 - Once configured both PostgreSQLConfiguration and BotConfiguration, open the solution in Visual Studios 2017 RC
 - Open Package Manager Console under View/Other Windows/Package Manager Console and execute "Update-Database Initial" to add default schema and tables to configured PostgreSQL database.
 - Now run your bot and see if it works. If not, please add issue.
 
## Contributing

There are two major paths for contributing to Contacts: You can either supply the maintainers with ideas or simply add new modules and features on your own and PR them. 

**Contributing an idea**

If you want to contribute an Idea, just create an issue with a description of the feature you think the bot needs. If you think that your idea might need some thoughts before it is submitted, offering it in the #contacts channel might be a better way of doing it.

**Contributing a feature**

If you have implemented a feature, just PR it with a brief description of what you did. If you want to make sure that your feature makes it´s way into the bot, you should follow the steps written in *Contributing an idea* first. Please follow these four simple steps for contributing:

1. Please make sure that your contribution fits the overall codestyle of the project.
2. Please close the according issue in your PR. If there is no issue, the PR should describe the added feature.
3. ???
4. _Profit!_
