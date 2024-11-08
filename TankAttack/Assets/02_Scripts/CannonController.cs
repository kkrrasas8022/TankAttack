using Photon.Pun;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    private Transform _tr;
    private float r => Input.GetAxis("Mouse ScrollWheel")*-1;
    [SerializeField] private float _speed = 10.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _tr= transform;
        this.enabled = _tr.root.GetComponent<PhotonView>().IsMine;
    }

    // Update is called once per frame
    void Update()
    {
        _tr.Rotate(Vector3.right * Time.deltaTime * r * _speed);
    }
}
