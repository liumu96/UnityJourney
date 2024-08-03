using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;


public class EyeTrackingMR : MonoBehaviour
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
            }
                
            
        } else
        {
            Greenpoint.gameObject.SetActive(false);
        }
    }
}
