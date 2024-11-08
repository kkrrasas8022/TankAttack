#pragma warning disable CS0108//�ش� ������ ��Ÿ���� ���� �ʴ´ٴ� ��ó����

using System;
using UnityEngine;
using Photon.Pun;
using Unity.Cinemachine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Photon.Realtime;

public class TankController : MonoBehaviour
{


    private Transform _tr;
    private Rigidbody _rb;
    private PhotonView _pv;
    private CinemachineCamera cinemachineCamera;
    private AudioSource audio; //�̹� �ý��� ���� �ִ� ���� �̸��� ����ؼ� ��ȣ�� ������ ������ 1.�� ���� ��ó���� #pragma warning disable
                               //2. ���� �����Ҷ� new Ű���� ���̱�

    [SerializeField] private float _moveSpeed = 10.0f;
    [SerializeField] private float _turnSpeed = 100.0f;
    [SerializeField] private GameObject _cannonPrefab;
    [SerializeField] private Transform _firePos;
    [SerializeField] private AudioClip _fireSfx;
    [SerializeField] private Image _hpBar;

    private TMP_Text nickNameText;

    private float v => Input.GetAxis("Vertical");
    private float h => Input.GetAxis("Horizontal");
    private bool isFire => Input.GetMouseButtonDown(0);

    private int _initHp = 100;
    private int _curHp = 100;
    private bool _isDie = false;


    private List<Renderer> renderers = new List<Renderer>();//��ũ ���� ��� mesh���� �������� ����Ʈ

    /*
     => : gose to
    (�Ķ����) => ������ ����
     */


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _tr=GetComponent<Transform>();
        _rb=GetComponent<Rigidbody>();
        audio = GetComponent<AudioSource>();
        _pv= GetComponent<PhotonView>();
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        nickNameText = transform.Find("Canvas/Panel/Text_NickName").GetComponent<TMP_Text>();
        GetComponentsInChildren<Renderer>(renderers);


        _rb.isKinematic = !_pv.IsMine;
        if(_pv.IsMine)
        {
            cinemachineCamera.Target.TrackingTarget = _tr;
        }

        //�г��� ����
        nickNameText.text = _pv.Owner.NickName;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (_pv.IsMine == false)
            return;
        if (_isDie)
            return;
        _tr.Translate(Vector3.forward * Time.deltaTime * v * _moveSpeed);
        _tr.Rotate(Vector3.up * Time.deltaTime * h * _turnSpeed);

        if(isFire && !EventSystem.current.IsPointerOverGameObject())
        {
            //Fire();//��ũ�� �ȸ��� �� ����
            //���ÿ����� ����ϰ� �ʹٸ� RpcTarget.Other�� ���
            //��Ʈ��ũ ���ۼӵ��� �ٸ����� ����Ͽ� �������� ��� �ش޶�� �ϴ� ��� RpcTarget.AllViaServer
            //RpcTarget.Buffered �׿��ִ� �͵��� �κ� �ڴʰ� ���� �������� ���ִ� ��
            _pv.RPC(nameof(Fire), RpcTarget.AllViaServer,_pv.Owner.ActorNumber);
        }
    }

    [PunRPC]
    private void Fire(int shooterId)
    {
        var _cannon = Instantiate(_cannonPrefab, _firePos.position, _firePos.rotation);
        _cannon.GetComponent<Cannon>().shooterId = shooterId;
        audio?.PlayOneShot(_fireSfx, 0.8f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isDie) return;
        if (collision.collider.CompareTag("CANNON"))
        {
            //ActorNum=>nickName
            int actorNumber = collision.gameObject.GetComponent<Cannon>().shooterId;
            Player shooter = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

            _curHp -= 20;
            _hpBar.fillAmount = (float)_curHp / (float)_initHp;
            if(_curHp<=0)
            {
                if (_pv.IsMine)
                {
                    string msg = $"<color=#00ff00>[{_pv.Owner.NickName}]</color>����"
                    + $"<color=#ff0000>[{shooter.NickName}]</color>���� �ǰݴ��߽��ϴ�";
                    GameManager.Instance.SendMessageByRPC(msg);
                }
                TankDestory();
            }
        }
    }

    private void TankDestory()
    {
        //��ũ ��Ȱ��ȭ
        SetVisibleTank(false);
        Invoke(nameof(RespawnTank), 3);
    }

    private void RespawnTank()
    {
        _curHp = _initHp;
        _hpBar.fillAmount = 1.0f;

        SetVisibleTank(true);
    }

    
    private void SetVisibleTank(bool isVisible)
    {
        _isDie = !isVisible;
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].enabled = isVisible;
        }
        _tr.GetComponentInChildren<Canvas>().enabled = isVisible;
    }


}
