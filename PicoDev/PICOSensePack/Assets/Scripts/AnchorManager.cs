using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.PXR;
using System;

public class AnchorManager : MonoBehaviour
{
    [SerializeField]
    private InputActionReference rightGrip;

    [SerializeField]
    private GameObject anchorPreview;
    [SerializeField]
    private GameObject anchorPrefab;

    [SerializeField]
    private float maxDriftDelay = 0.5f;

    private float currDriftDelay = 0f;

    private Dictionary<ulong, GameObject> anchorMap = new Dictionary<ulong, GameObject>();

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        rightGrip.action.started += OnRightGripPressed;
        rightGrip.action.canceled += OnRightGripReleased;
        PXR_Manager.AnchorEntityCreated += AnchorEntityCreated;
    }

    private void OnDisable()
    {
        rightGrip.action.started -= OnRightGripPressed;
        rightGrip.action.canceled -= OnRightGripReleased;
        PXR_Manager.AnchorEntityCreated -= AnchorEntityCreated;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        HandleSpatialDrift();
    }

    // called on action.started
    private void OnRightGripPressed(InputAction.CallbackContext callback)
    {
        ShowAnchorPreview();
    }

    // called on action.release
    private void OnRightGripReleased(InputAction.CallbackContext callback)
    {
        CreateAnchor();
    }

    private void ShowAnchorPreview()
    {
        // show anchor
        anchorPreview.SetActive(true);
    }

    private void CreateAnchor()
    {
        // hide anchor
        anchorPreview.SetActive(false);

        // use spatial anchor api to create anchor
        // this will trigger AnchorEntityCreatedEvent
        PXR_MixedReality.CreateAnchorEntity(anchorPreview.transform.position, anchorPreview.transform.rotation, out ulong taskID);
    }

    private void AnchorEntityCreated(PxrEventAnchorEntityCreated result)
    {
        if(result.result == PxrResult.SUCCESS)
        {
            GameObject anchorObject = Instantiate(anchorPrefab);

            PXR_MixedReality.GetAnchorPose(result.anchorHandle, out var rotation, out var position);

            anchorObject.transform.position = position;
            anchorObject.transform.rotation = rotation;

            // keep track of our anchors to handle spatial drift
            anchorMap.Add(result.anchorHandle, anchorObject);
        }
    }

    private void HandleSpatialDrift()
    {
        // if no anchors, don't need to handle spatial
        if (anchorMap.Count == 0)
            return;

        currDriftDelay += Time.deltaTime;
        if(currDriftDelay >= maxDriftDelay)
        {
            currDriftDelay = 0;
            foreach (var handlePair in anchorMap)
            {
                var handle = handlePair.Key;
                var anchorObj = handlePair.Value;

                if (handle == UInt64.MinValue)
                {
                    Debug.LogError("Handle is invalid");
                    continue;
                }

                PXR_MixedReality.GetAnchorPose(handle, out var rotation, out var position);
                anchorObj.transform.position = position;
                anchorObj.transform.rotation = rotation;
            }
        }

        
    }
} 
