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

### 
Copy "Packages > HoloLab Immersal Module > Settings > LocalizationSettings" to Assets folder.  
Enter your developer token and target map id.  
Assign `LocalizationSettings` to `ImmersalLocalization` in Inspector.

![image](https://user-images.githubusercontent.com/4415085/125905945-4a841763-c9b3-4308-a33a-7f588fb314e8.png)

### Build
Build the Unity project and deploy to the HoloLens.

## Author
Furuta, Yusuke ([@tarukosu](https://twitter.com/tarukosu))

## License
MIT