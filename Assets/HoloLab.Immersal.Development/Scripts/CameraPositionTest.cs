using System.Collections;
using System.Collections.Generic;
using HoloLab.Immersal;
using UnityEngine;

public class CameraPositionTest : MonoBehaviour
{
    [SerializeField]
    private ImmersalLocalization immersalLocalization = null;

    // Start is called before the first frame update
    void Start()
    {
        immersalLocalization.OnLocalized += ImmersalLocalization_OnLocalized;
    }

    private void ImmersalLocalization_OnLocalized(ImmersalLocalization.LocalizeInfo info)
    {
        transform.position = info.CameraPose.position;
        transform.rotation = info.CameraPose.rotation;
    }

    // Update is called once per frame
    void Update()
    {
    }
}