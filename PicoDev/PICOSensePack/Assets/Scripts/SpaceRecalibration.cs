using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.InputSystem;

public class SpaceRecalibration : MonoBehaviour
{
    public InputActionReference rightTrigger;

    private void Awake()
    {
        rightTrigger.action.started += OnRightTrigger;
    }

    private void OnDestroy()
    {
        rightTrigger.action.started -= OnRightTrigger;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRightTrigger(InputAction.CallbackContext callback)
    {
        PXR_MixedReality.StartSpatialSceneCapture(out var taskId);
    }
}
