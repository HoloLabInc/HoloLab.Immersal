# HoloLab.Immersal
Immersal localization module for HoloLens.

## Supported versions
- Unity 2019.4.x

## Install
Open `Packages\manifest.json` and add this line in "dependencies".

```
"jp.co.hololab.immersal": "https://github.com/HoloLabInc/HoloLab.Immersal.git?path=Assets/HoloLab.Immersal",
```

## Usage

### Import sample
Open Package Manager in Unity.  
Select HoloLab Immersal Module and press sample import button.

<img width="480" src="https://user-images.githubusercontent.com/4415085/125907168-9aa21b05-edda-497e-83dc-ff9f2254b467.png"></img>

### Settings
Copy "Packages > HoloLab Immersal Module > Settings > LocalizationSettings" to Assets folder.  
Enter your developer token and target map id.  

<img width="320" src="https://user-images.githubusercontent.com/4415085/125907550-8b1ef7a5-d8b2-4c4b-b67d-10230d3dc28f.png"></img>

Assign `LocalizationSettings` to `ImmersalLocalization` in Inspector.

<img width="480" src="https://user-images.githubusercontent.com/4415085/125905945-4a841763-c9b3-4308-a33a-7f588fb314e8.png"></img>

### Place GameObjects
Add GameObjects under the `MapOrign` GameObject. 

<img width="360" src="https://user-images.githubusercontent.com/4415085/125908144-113c98a3-02aa-4160-ba7e-8ff68e8ab25c.png"></img>

### Build
Build the Unity project and deploy to the HoloLens.

## Author
Furuta, Yusuke ([@tarukosu](https://twitter.com/tarukosu))

## License
MIT