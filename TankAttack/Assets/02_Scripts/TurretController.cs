using Photon.Pun;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    private Transform _tr;
    [SerializeField] private float _turnSpeed = 50f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _tr = GetComponent<Transform>();//그냥 this.transform으로 해도 별차이 없다

        this.enabled = _tr.root.GetComponent<PhotonView>().IsMine;
    }

    // Update is called once per frame
    void Update()
    {
        //메인카메라에서 Ray를 생성
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.green);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 8))//1<<8 -> 왼쪽으로 8비트 옮긴다는 의미(2^0->2^8)
        {
            //터렛 기준으로 hit를 월드좌표 -> 로컬좌표로 변환
            Vector3 pos = _tr.InverseTransformPoint(hit.point);
            //Atan2 : 두 좌표간의 각도를 계산 , Atan(pos.x/posz)
            float angle = Mathf.Atan2(pos.x, pos.z) * Mathf.Rad2Deg;

            //터렛회전
            _tr.Rotate(Vector3.up * angle * Time.deltaTime * _turnSpeed);

        }
    }
}
