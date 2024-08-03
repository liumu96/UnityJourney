using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.InputSystem;

public class ShootingCtrl : MonoBehaviour
{
    public Transform leftShootPoint;
    public Transform rightShootPoint;
    public GameObject bulletPrefab;

    [SerializeField]
    private InputActionReference leftTrigger;
    [SerializeField]
    private InputActionReference rightTrigger;
    [SerializeField]
    private float bulletSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {

        leftTrigger.action.started += OnLeftTrigger;
        rightTrigger.action.started += OnRightTrigger;
    }

    private void OnDisable()
    {
        leftTrigger.action.started -= OnLeftTrigger;
        rightTrigger.action.started -= OnRightTrigger;
    }

    private void OnLeftTrigger(InputAction.CallbackContext callback)
    {
        LeftShoot();
    }

    private void OnRightTrigger(InputAction.CallbackContext callback)
    {
        RightShoot();
    }


    private void LeftShoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, leftShootPoint.position, leftShootPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(bullet.transform.forward * bulletSpeed, ForceMode.Impulse);
    }

    private void RightShoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, rightShootPoint.position, rightShootPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(bullet.transform.forward * (bulletSpeed / 2), ForceMode.Impulse);
    }
}
