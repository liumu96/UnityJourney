using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using System;

public class SpaceCalibrateMR : MonoBehaviour
{

    [SerializeField]
    private float maxDriftDelay = 0.5f;

    private float currDriftDelay = 0f;

    private Dictionary<ulong, Transform> anchorMap = new Dictionary<ulong, Transform>();

    public GameObject painting1;
    public GameObject painting2;
    public GameObject painting3;

    //public GameObject testCube;
    //public Material greenMat;
    //public Material blueMat;


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
            foreach (var handlePair in anchorMap)
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
            // PxrSpatialSceneDataTypeFlags.Ceiling,
            // PxrSpatialSceneDataTypeFlags.Floor,
            PxrSpatialSceneDataTypeFlags.Door,
            // PxrSpatialSceneDataTypeFlags.Object,
            PxrSpatialSceneDataTypeFlags.Opening,
            PxrSpatialSceneDataTypeFlags.Unknown,
            // PxrSpatialSceneDataTypeFlags.Wall,
            PxrSpatialSceneDataTypeFlags.Window
        };

        // This will trigger AnchorEntityLoaded Event
        PXR_MixedReality.LoadAnchorEntityBySceneFilter(flags, out var taskID);
    }

    private void AnchorEntityLoaded(PxrEventAnchorEntityLoaded result)
    {
        if (result.result == PxrResult.SUCCESS && result.count != 0)
        {
            var results = PXR_MixedReality.GetAnchorEntityLoadResults(result.taskId, result.count, out var loadedAnchors);

            if (results == PxrResult.SUCCESS && loadedAnchors.Count > 0)
            {
                var index = 0;
                foreach (var key in loadedAnchors.Keys)
                {
                    PxrResult labelResult = PXR_MixedReality.GetAnchorSceneLabel(key, out var label);
                    PXR_MixedReality.GetAnchorPose(key, out var rotation, out var position);
                    if (labelResult == PxrResult.SUCCESS)
                    {
                        switch (label)
                        {
                            // Windows are labeled as Doors
                            case PxrSceneLabel.Window:
                            case PxrSceneLabel.Door:
                                {
                                    index++;
                                    PXR_MixedReality.GetAnchorPlaneBoundaryInfo(key, out var center, out var extent);

                                    var painting = index % 3 == 1 ? Instantiate(painting1) : index % 3 == 2 ? Instantiate(painting2) : Instantiate(painting3);
                                    painting.SetActive(true);
                                    painting.transform.position = position;
                                    painting.transform.rotation = rotation;
                                    Canvas canvas = painting.GetComponentInChildren<Canvas>();
                                    if(canvas != null)
                                    {
                                        //testCube.GetComponent<Renderer>().material = greenMat;
                                        RectTransform rectTransform = canvas.GetComponent<RectTransform>();
                                        if(rectTransform != null)
                                        {
                                            //testCube.GetComponent<Renderer>().material = blueMat;
                                            Vector2 paintingOriginalSize = rectTransform.sizeDelta;

                                            float scaleX = extent.x / paintingOriginalSize.x;
                                            float scaleY = extent.y / paintingOriginalSize.y;

                                            float scale = Math.Max(scaleX, scaleY);
                                            painting.transform.localScale = new Vector3(scale, scale, 0.002f);
                                        }
                                    }
                                   
                                    
                                }
                                break;
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
