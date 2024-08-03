using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFacePlayer : MonoBehaviour
{
    private Transform playerCam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        playerCam = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        transform.LookAt(playerCam);
        // reverse it so canvas looks at the player
        transform.forward = -playerCam.forward;
    }
}
