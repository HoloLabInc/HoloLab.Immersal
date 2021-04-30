using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SDKRequestBase
{
    public string token;
}

[Serializable]
public class SDKResultBase
{
    public string token;
}

[Serializable]
public class SDKMapId
{
    public int id;
}

[Serializable]
public class SDKLocalizeRequest : SDKRequestBase
{
    public double fx;    // camera intrinsics focal length x
    public double fy;    // camera intrinsics focal length y
    public double ox;    // camera intrinsics principal point x
    public double oy;    // camera intrinsics principal point y
    public string b64;   // Base64-encoded PNG image, 8-bit grayscale or 24-bit RGB
    public SDKMapId[] mapIds;  // list of maps to localize against
}

[Serializable]
public class SDKLocalizeResult : SDKResultBase
{
    public bool success;
    public int map;     // ID of the map if localization was successful
    public float px;    // x position within the map
    public float py;    // y position within the map
    public float pz;    // z position within the map
    public float r00;   // rotation matrix row 0, col 0
    public float r01;   // rotation matrix row 0, col 1
    public float r02;   // rotation matrix row 0, col 2
    public float r10;   // rotation matrix row 1, col 0
    public float r11;   // rotation matrix row 1, col 1
    public float r12;   // rotation matrix row 1, col 2
    public float r20;   // rotation matrix row 2, col 0
    public float r21;   // rotation matrix row 2, col 1
    public float r22;   // rotation matrix row 2, col 2
}