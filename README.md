# My Retainers

This is a Dalamud plugin for Final Fantasy XIV that uploads retainer venture return timers to Firebase. Useful for tracking your retainers across characters or devices.

## Features

- Extracts retainer return times directly
- Uploads to Firestore for external access
- Lightweight and automatic (works best with autoretainer)

## Installation (Dev only)

1. Build the project in Visual Studio (Release).
2. Drop your Firestore.json file into the same folder as myretainers.dll
3. Go to `/xlsettings` in-game and add the .dll to your Dev Plugin Locations.
4. You will need a .json file from Firestore to make this work. Whereever the data goes after it automatically goes to Firestore is up to you. 
5. Enable it via `/xlplugins`.

## Author

🧠 me and this [bitch](https://github.com/heIIish)
