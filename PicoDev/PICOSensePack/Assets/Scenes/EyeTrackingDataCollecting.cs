using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using System.IO;
using System;

[System.Serializable]
public class GazeData
{
    public Vector3 position;
    public float timestamp;

    public GazeData(Vector3 pos, float time)
    {
        position = pos;
        timestamp = time;
    }
}

[System.Serializable]
public class GazeDataListWrapper
{
    public List<GazeData> gazeData;
}


public class EyeTrackingDataCollecting : MonoBehaviour
{
    public Transform Origin;
    public Transform Greenpoint;
    public GameObject SpotLight;

    private Vector3 combineEyeGazeVector;
    private Vector3 combineEyeGazeOriginOffset;
    private Vector3 combineEyeGazeOrigin;
    private Matrix4x4 headPoseMatrix;
    private Matrix4x4 originPoseMatrix;

    private Vector3 combineEyeGazeVectorInWorldSpace;
    private Vector3 combineEyeGazeOriginInWorldSpace;

    private RaycastHit hitinfo;

    private List<GazeData> gazeDataList = new List<GazeData> ();

    //public GameObject testCube;
    //public Material redMat;
    //public Material blueMat;
    //public Material greenMat;

    // Start is called before the first frame update
    void Start()
    {
        combineEyeGazeOriginOffset = Vector3.zero;
        combineEyeGazeVector = Vector3.zero;
        combineEyeGazeOrigin = Vector3.zero;
        originPoseMatrix = Origin.localToWorldMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        PXR_EyeTracking.GetHeadPosMatrix(out headPoseMatrix);
        PXR_EyeTracking.GetCombineEyeGazeVector(out combineEyeGazeVector);
        PXR_EyeTracking.GetCombineEyeGazePoint(out combineEyeGazeOrigin);
        //Translate Eye Gaze point and vector to world space
        combineEyeGazeOrigin += combineEyeGazeOriginOffset;
        combineEyeGazeOriginInWorldSpace = originPoseMatrix.MultiplyPoint(headPoseMatrix.MultiplyPoint(combineEyeGazeOrigin));
        combineEyeGazeVectorInWorldSpace = originPoseMatrix.MultiplyVector(headPoseMatrix.MultiplyVector(combineEyeGazeVector));

        SpotLight.transform.position = combineEyeGazeOriginInWorldSpace;
        SpotLight.transform.rotation = Quaternion.LookRotation(combineEyeGazeVectorInWorldSpace, Vector3.up);

        GazeTargetControl(combineEyeGazeOriginInWorldSpace, combineEyeGazeVectorInWorldSpace);
    }

    void GazeTargetControl(Vector3 origin, Vector3 vector)
    {
        if (Physics.SphereCast(origin, 0.0005f, vector, out hitinfo))
        {
            if (hitinfo.collider.transform.tag.Equals("Painting"))
            {
                Greenpoint.gameObject.SetActive(true);
                Greenpoint.position = hitinfo.point;

                // record gaze data
                float timestamp = Time.time;
                GazeData data = new GazeData(hitinfo.point, timestamp);
                gazeDataList.Add(data);
                
                //if (gazeDataList == null || gazeDataList.Count == 0)
                //{
                //    testCube.GetComponent<Renderer>().material = redMat;
                //    Debug.LogWarning("No gaze data to save.");
                //    return;
                //} else
                //{
                //    testCube.GetComponent<Renderer>().material = greenMat;
                //}
            }


        }
        else
        {
            Greenpoint.gameObject.SetActive(false);
        }
    }

    void SaveDataToFile()
    {
        if (gazeDataList == null || gazeDataList.Count == 0)
        {
            //testCube.GetComponent<Renderer>().material = redMat;
            Debug.LogWarning("No gaze data to save.");
            return;
        }
        //testCube.GetComponent<Renderer>().material = blueMat;

        string dateString = DateTime.Now.ToString("yyyyMMdd");
        string fileName = $"Gaze_{dateString}.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        //string path = Path.Combine(Application.persistentDataPath, "Gaze.json");
        GazeDataListWrapper wrapper = new GazeDataListWrapper { gazeData = gazeDataList };
        string jsonData = JsonUtility.ToJson(wrapper, true); // Pretty print

        File.WriteAllText(path, jsonData);
        Debug.Log($"Data saved to {path}");
    }

    private void OnApplicationQuit()
    {
        SaveDataToFile();
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            SaveDataToFile();
        }
    }
}
