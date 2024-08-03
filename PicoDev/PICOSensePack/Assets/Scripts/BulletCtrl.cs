using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 5f;

    private void OnEnable()
    {
        Destroy(gameObject, lifeTime);
    }

}
