using Photon.Pun;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    private Transform _tr;
    [SerializeField] private float _turnSpeed = 50f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _tr = GetComponent<Transform>();//�׳� this.transform���� �ص� ������ ����

        this.enabled = _tr.root.GetComponent<PhotonView>().IsMine;
    }

    // Update is called once per frame
    void Update()
    {
        //����ī�޶󿡼� Ray�� ����
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.green);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 8))//1<<8 -> �������� 8��Ʈ �ű�ٴ� �ǹ�(2^0->2^8)
        {
            //�ͷ� �������� hit�� ������ǥ -> ������ǥ�� ��ȯ
            Vector3 pos = _tr.InverseTransformPoint(hit.point);
            //Atan2 : �� ��ǥ���� ������ ��� , Atan(pos.x/posz)
            float angle = Mathf.Atan2(pos.x, pos.z) * Mathf.Rad2Deg;

            //�ͷ�ȸ��
            _tr.Rotate(Vector3.up * angle * Time.deltaTime * _turnSpeed);

        }
    }
}
