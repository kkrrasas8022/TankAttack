#pragma warning disable CS0108//해당 워닝을 나타나게 하지 않는다는 전처리기

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
    private AudioSource audio; //이미 시스템 내에 있는 변수 이름을 사용해서 모호함 워닝이 떴을때 1.맨 위에 전처리기 #pragma warning disable
                               //2. 변수 생성할때 new 키워드 붙이기

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


    private List<Renderer> renderers = new List<Renderer>();//탱크 모델의 모든 mesh들을 가져오는 리스트

    /*
     => : gose to
    (파라메터) => 문장을 실행
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

        //닉네임 설정
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
            //Fire();//싱크가 안맞을 수 있음
            //로컬에서도 사용하고 싶다면 RpcTarget.Other을 사용
            //네트워크 전송속도가 다른것을 고려하여 서버에게 대신 해달라고 하는 방법 RpcTarget.AllViaServer
            //RpcTarget.Buffered 쌓여있는 것들을 로비에 뒤늦게 들어온 유저에게 쏴주는 것
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
                    string msg = $"<color=#00ff00>[{_pv.Owner.NickName}]</color>님은"
                    + $"<color=#ff0000>[{shooter.NickName}]</color>에게 피격당했습니다";
                    GameManager.Instance.SendMessageByRPC(msg);
                }
                TankDestory();
            }
        }
    }

    private void TankDestory()
    {
        //탱크 비활성화
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
