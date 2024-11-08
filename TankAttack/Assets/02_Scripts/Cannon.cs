using System;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField] private CannonDataSO cannonDataSo;

    //Shooter ID
    //[NonSerialized]//�ۺ������� �ν����Ϳ� �����Ű����� ������ ���
    [HideInInspector]//�ۺ������� �ν����Ϳ� �����Ű����� ������ ���
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
