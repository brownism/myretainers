# My Retainers

This is a Dalamud plugin for Final Fantasy XIV that uploads retainer data (including or will include ventures, return timers, classes, and stats) to Firebase. Useful for tracking your retainers across all characters on any remote device.

## Features

- Extracts retainer return times directly
- Uploads to Firestore for external access
- Lightweight and automatic (works best with autoretainer)

## Installation (Dev only)

1. Build the project in Visual Studio (Release).
2. Drop your Firestore.json file into the same folder as myretainers.dll
3. Go to `/xlsettings` in-game and add the .dll to your Dev Plugin Locations.
4. Open https://console.firebase.google.com/ and sign in. Obtain a Node.js file from the Firebase Admin SDK and rename it "firebase.json".
5. Drop that .json file into the same folder as the .dll you just built. 
5. Enable it via `/xlplugins`.

## Author

🧠 me and this [bitch](https://github.com/heIIish)
