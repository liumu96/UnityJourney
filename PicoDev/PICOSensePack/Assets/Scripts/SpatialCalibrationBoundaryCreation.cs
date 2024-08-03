using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using System;

public class SpatialCalibrationBoundaryCreation : MonoBehaviour
{
    public GameObject anchorPrefab;
    public GameObject sofaPrefab;
    public GameObject tablePrefab;
    public GameObject windowDoorPrefab;
    public GameObject wallPrefab;
    public GameObject floorCeilingPrefab;

    [SerializeField]
    private float maxDriftDelay = 0.5f;

    private float currDriftDelay = 0f;

    private Dictionary<ulong, Transform> anchorMap = new Dictionary<ulong, Transform>();

    private List<Transform> wallAnchors = new List<Transform>();
    private Transform ceilingTransform = null;
    private Transform floorTransform = null;

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
                            // then load the model prefab to fit the space
                            // Sofa & Table & Unknown/Objects
                            // Volume: The anchor is located at the center of the rectangle on the upper surface of the cube with Z axis as up
                            case PxrSceneLabel.Sofa:
                                {
                                    PXR_MixedReality.GetAnchorVolumeInfo(key, out var center, out var extent);
                                    // extent: x-width, y-height, z-depth from the center
                                    var newSofa = Instantiate(sofaPrefab);
                                    // all info is relative to the anchor position
                                    newSofa.transform.SetParent(anchorObject.transform);
                                    newSofa.transform.localPosition = center;
                                    newSofa.transform.localRotation = Quaternion.identity;
                                    newSofa.transform.localScale = extent;
                                }
                                break;
                            case PxrSceneLabel.Table:
                                {
                                    PXR_MixedReality.GetAnchorVolumeInfo(key, out var center, out var extent);
                                    // extent: x-width, y-height, z-depth from the center
                                    var newTable = Instantiate(tablePrefab);
                                    // all info is relative to the anchor position
                                    newTable.transform.SetParent(anchorObject.transform);
                                    newTable.transform.localPosition = center;
                                    newTable.transform.localRotation = Quaternion.identity;
                                    newTable.transform.localScale = extent;
                                }
                                break;
                            // Wall / Window / Door
                            // Plane: Anchor is located in the center of the plane
                            // xaxis - width, yaxis - height, zaxis - normal vector
                            case PxrSceneLabel.Wall:
                                {
                                    PXR_MixedReality.GetAnchorPlaneBoundaryInfo(key, out var center, out var extent);
                                    var wall = Instantiate(wallPrefab);
                                    wall.transform.SetParent(anchorObject.transform);
                                    wall.transform.localPosition = Vector3.zero;
                                    wall.transform.localRotation = Quaternion.identity;
                                    wall.transform.Rotate(90, 0, 0);
                                    // extent - Vector2: x-width, y-depth
                                    // 0.001f because I want a thin wall
                                    // increase wall height to cover any gaps
                                    wall.transform.localScale = new Vector3(extent.x, 0.001f, extent.y * 1.1f);
                                    wallAnchors.Add(wall.transform);
                                }
                                break;
                            // Windows are labeled as Doors
                            case PxrSceneLabel.Window:
                            case PxrSceneLabel.Door:
                                {
                                    PXR_MixedReality.GetAnchorPlaneBoundaryInfo(key, out var center, out var extent);
                                    var windowDoor = Instantiate(windowDoorPrefab);
                                    windowDoor.transform.SetParent(anchorObject.transform);
                                    windowDoor.transform.localPosition = Vector3.zero;
                                    windowDoor.transform.localRotation = Quaternion.identity;
                                    windowDoor.transform.Rotate(90, 0, 0);
                                    // extent - Vector2: x-width, y-depth
                                    // 0.001f because I want a thin wall
                                    // increase wall height to cover any gaps
                                    windowDoor.transform.localScale = new Vector3(extent.x, 0.002f, extent.y);

                                }
                                break;
                            // Not currently supported in the current SDK Version but we can get some info
                            // !PXR_MixedReality.GetAnchorPlanePolygonInfo(ulong anchorHandle, out Vector3[] vertices);
                            case PxrSceneLabel.Floor:
                                {
                                    PXR_MixedReality.GetAnchorPlaneBoundaryInfo(key, out var center, out var extent);
                                    var floorCeiling = Instantiate(floorCeilingPrefab);
                                    floorCeiling.transform.SetParent(anchorObject.transform);
                                    floorCeiling.transform.localPosition = Vector3.zero;
                                    floorCeiling.transform.localRotation = Quaternion.identity;
                                    floorCeiling.transform.Rotate(90, 0, 0);
                                    floorTransform = floorCeiling.transform;
                                    // scale info is missing
                                    // rotation is not align correctly
                                }
                                break;

                            case PxrSceneLabel.Ceiling:
                                {
                                    PXR_MixedReality.GetAnchorPlaneBoundaryInfo(key, out var center, out var extent);
                                    var floorCeiling = Instantiate(floorCeilingPrefab);
                                    floorCeiling.transform.SetParent(anchorObject.transform);
                                    floorCeiling.transform.localPosition = Vector3.zero;
                                    floorCeiling.transform.localRotation = Quaternion.identity;
                                    floorCeiling.transform.Rotate(90, 0, 0);
                                    ceilingTransform = floorCeiling.transform;
                                    // scale info is missing
                                    // rotation is not align correctly
                                }
                                break;
                            
                            // case PxrSceneLabel.Unknown // Objects
                        }
                    }
                }
                ScaleCeilingFloor();
            }
        }
    }

    private void ScaleCeilingFloor()
    {
        Vector2 extent = CalcScaleSides();
        ceilingTransform.localScale = new Vector3(extent.x * 2, 0.001f, extent.y * 2);
        floorTransform.localScale = new Vector3(extent.x * 2, 0.001f, extent.y * 2);
    }

    private Vector2 CalcScaleSides()
    {
        // store distance along with index pairs of each wall anchor
        List<(float distance, int index1, int index2)> distances = new List<(float, int, int)>();

        // Calc all distances between points
        for(int i = 0; i < wallAnchors.Count; i++)
        {
            for(int j = i + 1; j < wallAnchors.Count; j++)
            {
                float distance = Vector3.Distance(wallAnchors[i].position, wallAnchors[j].position);
                distances.Add((distance, i, j));
            }
        }

        // sort the distance
        distances.Sort((x, y) => y.distance.CompareTo(x.distance));

        // the first element is the longest distance between two walls
        var longestPair = distances[0];
        var remainingIndices = new List<int> { 0, 1, 2, 3 };
        remainingIndices.Remove(longestPair.index1);
        remainingIndices.Remove(longestPair.index2);

        float width = Vector3.Distance(wallAnchors[remainingIndices[0]].position, wallAnchors[remainingIndices[1]].position);
        float depth = Vector3.Distance(wallAnchors[longestPair.index1].position, wallAnchors[longestPair.index2].position);

        return new Vector2(width, depth);
    }
}
