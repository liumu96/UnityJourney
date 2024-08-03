using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using System;

public class SpatialCalibration : MonoBehaviour
{
    public GameObject anchorPrefab;
    public GameObject sofaPrefab;
    public GameObject tablePrefab;
    public GameObject windowPrefab;
    public GameObject wallPrefab;

    [SerializeField]
    private float maxDriftDelay = 0.5f;

    private float currDriftDelay = 0f;

    private Dictionary<ulong, Transform> anchorMap = new Dictionary<ulong, Transform>();

    private void Awake()
    {
        PXR_MixedReality.EnableVideoSeeThrough(true);
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
            PXR_MixedReality.EnableVideoSeeThrough(true);
    }

    private void OnEnable()
    {
        PXR_Manager.AnchorEntityLoaded += AnchorEntityLoaded;
    }

    private void OnDisable()
    {
        PXR_Manager.AnchorEntityLoaded -= AnchorEntityLoaded;
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadSpaceData();
    }



    private void FixedUpdate()
    {
        HandleSpatialDrift();
    }

    private void HandleSpatialDrift()
    {
        // if no anchors, we don't need to handle drift
        if (anchorMap.Count == 0)
            return;

        currDriftDelay += Time.deltaTime;
        if (currDriftDelay >= maxDriftDelay)
        {
            currDriftDelay = 0f;
            foreach(var handlePair in anchorMap)
            {
                var handle = handlePair.Key;
                var anchorTransform = handlePair.Value;

                if (handle == UInt64.MinValue)
                {
                    Debug.LogError("Handle is invalid");
                    continue;
                }

                PXR_MixedReality.GetAnchorPose(handle, out var rotation, out var position);
                anchorTransform.position = position;
                anchorTransform.rotation = rotation;
            }
        }
    }

    private void LoadSpaceData()
    {
        // what type of flags are we looking for
        // Load all
        PxrSpatialSceneDataTypeFlags[] flags =
        {
            PxrSpatialSceneDataTypeFlags.Ceiling,
            PxrSpatialSceneDataTypeFlags.Floor,
            PxrSpatialSceneDataTypeFlags.Door,
            PxrSpatialSceneDataTypeFlags.Object,
            PxrSpatialSceneDataTypeFlags.Opening,
            PxrSpatialSceneDataTypeFlags.Unknown,
            PxrSpatialSceneDataTypeFlags.Wall,
            PxrSpatialSceneDataTypeFlags.Window
        };

        // This will trigger AnchorEntityLoaded Event
        PXR_MixedReality.LoadAnchorEntityBySceneFilter(flags, out var taskID);
    }

    private void AnchorEntityLoaded(PxrEventAnchorEntityLoaded result)
    {
        if(result.result == PxrResult.SUCCESS && result.count != 0)
        {
            var results = PXR_MixedReality.GetAnchorEntityLoadResults(result.taskId, result.count, out var loadedAnchors);

            if(results == PxrResult.SUCCESS && loadedAnchors.Count > 0)
            {
                foreach(var key in loadedAnchors.Keys)
                {
                    // Load an anchor at position
                    GameObject anchorObject = Instantiate(anchorPrefab);

                    PXR_MixedReality.GetAnchorPose(key, out var rotation, out var position);
                    anchorObject.transform.position = position;
                    anchorObject.transform.rotation = rotation;
                    // Now anchor is at correct position in out space

                    Anchor anchor = anchorObject.GetComponent<Anchor>();
                    if(anchor == null)
                        anchorObject.AddComponent<Anchor>();

                    anchorMap.Add(key, anchorObject.transform);
                   
                    PxrResult labelResult = PXR_MixedReality.GetAnchorSceneLabel(key, out var label);
                    if(labelResult == PxrResult.SUCCESS)
                    {
                        // what labels
                        // system space calibration furniture names -> SDK SceneData Labels
                        // Couch            -> Sofa
                        // Desk             -> Table
                        // Door/Windows     -> Doors
                        // Objects/Unknows  -> Unknowns
                        // Floors           -> Floors
                        // Ceiling          -> Ceiling
                        // Walls            -> Walls
                        anchor.UpdateLabel(label.ToString());
                        switch (label)
                        {
                            // then load the model prefab to fit the space
                        }
                    }
                }
            }
        }
    }
}
