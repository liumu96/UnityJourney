using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EyeTrackingManager : MonoBehaviour
{
    public Transform origin;
    public GameObject greenPoint;
    public GameObject anchorPreview;

    public GameObject SpotLight;

    private Vector3 combineEyeGazeVector;
    private Vector3 combineEyeGazeOriginOffset;
    private Vector3 combineEyeGazeOrigin;
    private Matrix4x4 headPoseMatrix;
    private Matrix4x4 originPoseMatrix;

    private Vector3 combineEyeGazeVectorInWorldSpace;
    private Vector3 combineEyeGazeOriginInWorldSpace;

    private RaycastHit hitInfo;

    EyeTrackingStartInfo startInfo;
    EyeTrackingMode eyeTrackingMode;

    public static bool startEyeTracking = false;

    public GameObject testCube;
    public Material validMaterial;
    public Material invalidMaterial;
    public Material activeMaterial;
    public Material orangeMaterial;


    

    // Start is called before the first frame update
    void Start()
    {
        combineEyeGazeOriginOffset = Vector3.zero;
        combineEyeGazeVector = Vector3.zero;
        combineEyeGazeOrigin = Vector3.zero;
        originPoseMatrix = origin.localToWorldMatrix;

        // Request the eye tracking service for the current app
        PXR_MotionTracking.WantEyeTrackingService();

        eyeTrackingMode = EyeTrackingMode.PXR_ETM_BOTH;

        bool supported = false;
        int supportedModesCount = 0;
        PXR_MotionTracking.GetEyeTrackingSupported(ref supported, ref supportedModesCount, ref eyeTrackingMode);

        if(supported)
        {
            Debug.Log("Support eye-tracking.");
            testCube.GetComponent<Renderer>().material = validMaterial;

            startInfo = new EyeTrackingStartInfo();

            startInfo.needCalibration = 1;
            startInfo.mode = eyeTrackingMode;
            PXR_MotionTracking.StartEyeTracking(ref startInfo);
        } else
        {
            Debug.LogError("Not supported eye-tracking");
            testCube.GetComponent<Renderer>().material = invalidMaterial;
        }
    }

    // Update is called once per frame
    void Update()
    {
        testCube.GetComponent<Renderer>().material = activeMaterial;
        // greenPoint.GetComponent<Renderer>().material = activeMaterial;

        PXR_EyeTracking.GetHeadPosMatrix(out headPoseMatrix);
        PXR_EyeTracking.GetCombineEyeGazeVector(out combineEyeGazeVector);
        PXR_EyeTracking.GetCombineEyeGazePoint(out combineEyeGazeOrigin);

        // Translate Eye Gaze point and vector to world space
        combineEyeGazeOrigin += combineEyeGazeOriginOffset;
        combineEyeGazeOriginInWorldSpace = originPoseMatrix.MultiplyPoint(headPoseMatrix.MultiplyPoint(combineEyeGazeOrigin));
        combineEyeGazeVectorInWorldSpace = originPoseMatrix.MultiplyVector(headPoseMatrix.MultiplyVector(combineEyeGazeVector));

        SpotLight.transform.position = combineEyeGazeOriginInWorldSpace;
        SpotLight.transform.rotation = Quaternion.LookRotation(combineEyeGazeVectorInWorldSpace, Vector3.up);

        GazeTargetControl(combineEyeGazeOriginInWorldSpace, combineEyeGazeVectorInWorldSpace);
    }

    void GazeTargetControl(Vector3 origin, Vector3 vector)
    {
        greenPoint.GetComponent<Renderer>().material = orangeMaterial;
        if (Physics.SphereCast(origin, 0.005f, vector, out hitInfo))
        {
            greenPoint.GetComponent<Renderer>().material = validMaterial;
            if (hitInfo.collider.transform.tag.Equals("Painting"))
            {
               //  testCube.GetComponent<Renderer>().material = orangeMaterial;
                greenPoint.GetComponent<Renderer>().material = orangeMaterial;
                greenPoint.transform.position = hitInfo.point;

                // greenPoint.SetActive(true);
                // greenPoint.transform.position = hitInfo.point;
            } else
            {
                // greenPoint.SetActive(false);
            }
        }
        else
        {
            // greenPoint.SetActive(false);
        }
    }

    public static void StartEyeTracking()
    {
        startEyeTracking = true;
    }
}
