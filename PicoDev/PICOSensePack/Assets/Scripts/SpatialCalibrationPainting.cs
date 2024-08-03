using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using System;


public class SpatialCalibrationPainting : MonoBehaviour
{
    public GameObject anchorPrefab;

    public GameObject paintingPrefab;
    public GameObject painting1;
    public GameObject painting2;
    public GameObject painting3;

    [SerializeField]
    private float maxDriftDelay = 0.5f;

    private float currDriftDelay = 0f;

    private Dictionary<ulong, Transform> anchorMap = new Dictionary<ulong, Transform>();

    public GameObject colliderCubePrefab;
    public Material orangeMaterial;

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
        // PXR_Manager.SpatialSceneCaptured += SpatialSceneCaptured;
    }

    private void OnDisable()
    {
        PXR_Manager.AnchorEntityLoaded -= AnchorEntityLoaded;
        // PXR_Manager.SpatialSceneCaptured -= SpatialSceneCaptured;
    }

    //private void SpatialSceneCaptured(PxrEventSpatialSceneCaptured result)
    //{
    //    LoadSpaceData();
    //}

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
        if (result.result == PxrResult.SUCCESS && result.count != 0)
        {
            var results = PXR_MixedReality.GetAnchorEntityLoadResults(result.taskId, result.count, out var loadedAnchors);

            if (results == PxrResult.SUCCESS && loadedAnchors.Count > 0)
            {
                var index = 0;
                foreach (var key in loadedAnchors.Keys)
                {
                    
                    // Load an anchor at position
                    GameObject anchorObject = Instantiate(anchorPrefab);

                    PXR_MixedReality.GetAnchorPose(key, out var rotation, out var position);
                    anchorObject.transform.position = position;
                    anchorObject.transform.rotation = rotation;
                    // Now anchor is at correct position in out space

                    Anchor anchor = anchorObject.GetComponent<Anchor>();
                    if (anchor == null)
                        anchorObject.AddComponent<Anchor>();

                    anchorMap.Add(key, anchorObject.transform);

                    PxrResult labelResult = PXR_MixedReality.GetAnchorSceneLabel(key, out var label);
                    if (labelResult == PxrResult.SUCCESS)
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
                            
                            // Windows are labeled as Doors
                            case PxrSceneLabel.Window:
                            case PxrSceneLabel.Door:
                                {
                                    PXR_MixedReality.GetAnchorPlaneBoundaryInfo(key, out var center, out var extent);
                                    //var painting = Instantiate(paintingPrefab);
                                    //painting.transform.SetParent(anchorObject.transform);
                                    //painting.transform.localPosition = Vector3.zero;
                                    //painting.transform.localRotation = Quaternion.identity;
                                    //// painting.transform.Rotate(90, 0, 0);

                                    //painting.transform.localScale = new Vector3(extent.y * 0.1f, extent.y * 0.1f, 0.002f);
                                    index++;
                                    if (index == 1)
                                    {
                                        painting1.SetActive(true);
                                        painting1.transform.SetParent(anchorObject.transform);
                                        painting1.transform.localPosition = Vector3.zero;
                                        painting1.transform.localRotation = Quaternion.identity;
                                        painting1.transform.localScale = new Vector3(extent.y * 0.1f, extent.y * 0.1f, 0.002f);
                                    } else if(index == 2)
                                    {
                                        painting2.SetActive(true);
                                        painting2.transform.SetParent(anchorObject.transform);
                                        painting2.transform.localPosition = Vector3.zero;
                                        painting2.transform.localRotation = Quaternion.identity;
                                        painting2.transform.localScale = new Vector3(extent.y * 0.1f, extent.y * 0.1f, 0.002f);
                                    } else
                                    {
                                        painting3.SetActive(true);
                                        painting3.transform.SetParent(anchorObject.transform);
                                        painting3.transform.localPosition = Vector3.zero;
                                        painting3.transform.localRotation = Quaternion.identity;
                                        painting3.transform.localScale = new Vector3(extent.y * 0.1f, extent.y * 0.1f, 0.002f);
                                    }

                                    //BoxCollider boxCollider = painting.GetComponent<BoxCollider>();

                                    //if (boxCollider != null)
                                    //{
                                        

                                    //    Vector3 localScale = painting.transform.localScale;

                                    //    Vector3 originalSize = new Vector3(4.4f, 10.1f, 0.2f);

                                        
                                    //    boxCollider.size = new Vector3(originalSize.x * localScale.x, originalSize.y * localScale.y, originalSize.z * localScale.z);
                                    //    boxCollider.center = center;

                                    //    // Transform world center to local center in anchorObject's local space
                                    //    Vector3 localCenter = anchorObject.transform.InverseTransformPoint(center);
                                    //    boxCollider.center = localCenter;

                                    //    //var colliderCube = Instantiate(colliderCubePrefab);
                                    //    //colliderCube.transform.SetParent(anchorObject.transform);
                                    //    //colliderCube.transform.localPosition = Vector3.zero;
                                    //    //colliderCube.transform.localRotation = Quaternion.identity;
                                    //    //// colliderCube.transform.localPosition = localCenter;
                                    //    //colliderCube.transform.localScale = boxCollider.size;
                                    //    //colliderCube.GetComponent<Renderer>().material = orangeMaterial;

                                        
                                    //}
                                }
                                break;
                            
                        }
                    }
                }

            }
        }
        
    }

    private void AdjustBoxColliderSize(GameObject painting)
    {
        BoxCollider boxCollider = painting.GetComponent<BoxCollider>();
        if(boxCollider != null)
        {
            Vector3 localScale = painting.transform.localScale;

            Vector3 originalSize = new Vector3(4.4f, 10.1f, 0.2f);

           
            boxCollider.size = new Vector3(originalSize.x * localScale.x, originalSize.y * localScale.y, originalSize.z * localScale.z);
        }
    }

    
}

