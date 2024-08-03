using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;

public class SeeThrough : MonoBehaviour
{
    private void Awake()
    {
        PXR_MixedReality.EnableVideoSeeThrough(true);
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
            PXR_MixedReality.EnableVideoSeeThrough(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
