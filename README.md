# WoW WTF Sync

## Introduction

WoW WTF Sync is a Windows application for managing your World of Warcraft WTF folder. Ultimately, its purpose is to make it easier for a user to maintain multiple accounts and computers when playing WoW.

This app's purpose is mostly for the ability to maintain multiple accounts, but will also be for multiple computers, via cloud sync (unfortunately, Battle.net does not provide for this).

This app will focus primarily on WoW Classic Era, but will be expanded to Classic Progression, and possibly Retail.

## How it works

The data combining feature will utilize a background Lua script to properly handle combining Lua files. A Lua module will be created for each supported addon.

When combining, all characters and accounts that are being pushed to must be logged out, otherwise it will revert when logout occurs. The character being pushed from does not need to be logged out.

## Current features

- Combine data across accounts for the following addons:
  - Altoholic
  - Auctionator (scan data)
  - Bagnon
  - NovaWorldBuffs
  - TitanGold

## Planned features

- Combine other addons' data across accounts.
- Configure sharing/copying of global settings for addons across accounts.
- Configure sharing/copying of character-specific settings for addons across accounts.
- Cloud sync of WTF data.
- Cloud sync of WTF settings.

## Screenshots

![alt text](https://github.com/bjthompson805/WowWtfSync/blob/master/Resources/screenshot-1.png?raw=true)
![alt text](https://github.com/bjthompson805/WowWtfSync/blob/master/Resources/screenshot-2.png?raw=true)
![alt text](https://github.com/bjthompson805/WowWtfSync/blob/master/Resources/screenshot-3.png?raw=true)
![alt text](https://github.com/bjthompson805/WowWtfSync/blob/master/Resources/screenshot-4.png?raw=true)

## Developer practices

- Development must make sure that user data does not get corrupted as a result of a bug or updates to an addon, so make sure to make assurances with regard to data integrity, possible race conditions, etc.
