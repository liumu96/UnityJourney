using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.InputSystem;

public class SeeThroughInteraction : MonoBehaviour
{
    public InputActionReference leftTrigger;
    public InputActionReference rightTrigger;

    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    public GameObject blueBoxPrefab;
    public GameObject redBoxPrefab;

    private void Awake()
    {
        PXR_MixedReality.EnableVideoSeeThrough(true);
        leftTrigger.action.started += OnLeftTrigger;
        rightTrigger.action.started += OnRightTrigger;
    }

    private void OnDestroy()
    {
        leftTrigger.action.started -= OnLeftTrigger;
        rightTrigger.action.started -= OnRightTrigger;
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

    private void OnLeftTrigger(InputAction.CallbackContext callback)
    {
        GameObject newLeftObj = Instantiate(redBoxPrefab, leftSpawnPoint.position, Quaternion.identity);
    }

    private void OnRightTrigger(InputAction.CallbackContext callback)
    {
        GameObject newRightObj = Instantiate(blueBoxPrefab, rightSpawnPoint.position, Quaternion.identity);
    }
}
