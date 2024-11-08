using System;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField] private CannonDataSO cannonDataSo;

    //Shooter ID
    //[NonSerialized]//퍼블릭이지만 인스펙터에 노출시키고싶지 않을떄 사용
    [HideInInspector]//퍼블릭이지만 인스펙터에 노출시키고싶지 않을떄 사용
    public int shooterId;
    

    //[SerializeField] private float force = 1500.0f;
    //[SerializeField] private GameObject expEffect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * cannonDataSo.force);
        Destroy(this.gameObject, 10.0f);
    }

    private void OnCollisionEnter()
    {
        var obj = Instantiate(cannonDataSo.expEffect, transform.position,Quaternion.identity);
        Destroy(obj,3.0f);
        Destroy(gameObject);
    }
}
