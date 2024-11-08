using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using Unity.Multiplayer.Center.Common;
using System.Collections.Generic;


public class PhotonManager : MonoBehaviourPunCallbacks//���漭������ �����ϴ� �Լ����� ����� �� ����
{
    public static PhotonManager Instance=null;

    //���ӹ��� 1.0
    //������ ���� ������ ������ �ִ� ����鳢���� ��Ī����
    [SerializeField] private const string _version = "1.0";
    //������
    private string _nickname = "Song";

    [Header("UI")]
    [SerializeField] private TMP_InputField _nickNameIF;
    [SerializeField] private TMP_InputField _roomNameIF;

    [Header("Button")]
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _makeRoomButton;

    [Header("Room List")]
    [SerializeField] private GameObject _roomPrefab;
    [SerializeField] private Transform _contentTr;

    //[SerializeField] private Button _btn;

    private void Awake()
    {
        Instance = this;

        //��ư ��Ȱ��
        _loginButton.interactable = false;
        _makeRoomButton.interactable = false;


        //���ӹ��� ����
        PhotonNetwork.GameVersion = _version;
        //������ ����
        PhotonNetwork.NickName = _nickname;

        //������ ���� �ε����� �� Ÿ �����鿡 �ڵ����� ���� �ε������ִ� �ɼ�
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            //���漭���� ���� ��û
            PhotonNetwork.ConnectUsingSettings();
        }

        //��ư �̺�Ʈ ����
        //_btn.onClick.AddListener(() =>
        //{
        //    print("��ưŬ��");
        //    Debug.LogError("��ưŬ�������޽���");
        //});
    }

    private void Start()
    {
        //����� �г��� �ε�
        _nickname = PlayerPrefs.GetString("NICK_NAME", $"User_{Random.Range(0, 1001):0000}");
        _nickNameIF.text = _nickname;
        //��ư �̺�Ʈ ����
        _loginButton.onClick.AddListener(() => OnLoginButtonClick());
        _makeRoomButton.onClick.AddListener( () => OnMakeRoomButtonClick());
    }

    public void SetNickName()
    {
        //�г��� �ʵ尡 ����ִ��� üũ
        if (string.IsNullOrEmpty(_nickNameIF.text))
        {
            _nickname = $"User_{Random.Range(0, 1001):0000}";
            _nickNameIF.text = _nickname;
        }
        else
        {
            _nickname=_nickNameIF.text;
        }

        PhotonNetwork.NickName = _nickname;
        PlayerPrefs.SetString("NICK_NAME", _nickname);
    }


    #region UI �ݹ� �Լ�
    private void OnLoginButtonClick()
    {
        SetNickName();
        PhotonNetwork.JoinRandomRoom();
    }

    private void OnMakeRoomButtonClick()
    {
        SetNickName();
        if (string.IsNullOrEmpty(_roomNameIF.text))
        {
            _roomNameIF.text = $"ROOM_{Random.Range(0, 10000)}";
        }
        var roomOption = new RoomOptions
        {
            MaxPlayers = 20,
            IsOpen = true,
            IsVisible = true,
        };
        //�� ����
        PhotonNetwork.CreateRoom(_roomNameIF.text, roomOption);
    }

    #endregion


    #region �����ݹ��Լ�

    //������ ������ Dictionary ����
    private Dictionary<string, GameObject> roomDict = new Dictionary<string, GameObject>();

    //������ ����Ǹ� ȣ��Ǵ� �ݹ�
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //�κ� ������������ ȣ��Ǵ� �Լ�
        foreach(var room in roomList)
        {
            //print($"{room.Name} : {room.PlayerCount} / {room.MaxPlayers}");

            if(room.RemovedFromList==true)//������ ���� �ǹ� ( �÷��̾� ��Ȳ�� 0/0 �ΰ��)
            {
                //�����
                if(roomDict.TryGetValue(room.Name,out GameObject removedRoom))
                {
                    //������ �ν��Ͻ� ����
                    Destroy(removedRoom);
                    //Dictionary���� Ű�� ����
                    roomDict.Remove(room.Name);
                }
                continue;
            }

            //���� ������ ��, ����� ��� ���� ó��
            //ó�� ������ ��(Dictionary �˻����� �� ����� ���� ���)
            if(roomDict.ContainsKey(room.Name)==false)
            {
                //room prefab ����
                var _room = Instantiate(_roomPrefab, _contentTr);
                //�� �Ӽ� ����
                _room.GetComponent<RoomData>().RoomInfo = room;
                //Dictionary�� ����
                roomDict.Add(room.Name, _room);
            }
            //�˻����� �� ���� ��� �� ������ ����
            else
            {

            }
        }//foreach room

    }//OnRoomListUpdate




    //���漭���� ���ӵǾ��� �� ȣ��Ǵ� �ݹ�(CallBack)
    public override void OnConnectedToMaster()
    {
        _loginButton.interactable = true;
        _makeRoomButton.interactable = true;
        print("�������� �Ϸ�");
        //Lobby ���� ��û
        PhotonNetwork.JoinLobby();
    }

    //�κ� �������� �� ȣ��Ǵ� �ݹ�
    public override void OnJoinedLobby()
    {
        print("�κ� ���� �Ϸ�");
        //������ �濡 ������ ��û
        //PhotonNetwork.JoinRandomRoom();
    }




    //���� ������ �������� �� ȣ��Ǵ� �ݹ�
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print($"������ ���� : {returnCode} : {message}");

        //�� �ɼ� ����
        RoomOptions Ro = new RoomOptions
        {
            MaxPlayers = 20,
            IsOpen = true,
            IsVisible = true
        };
        PhotonNetwork.CreateRoom($"{_nickname}'s Room", Ro);
    }

    //�� ���� �Ϸ� �ݹ�
    public override void OnCreatedRoom()
    {
        print("�� ���� �Ϸ�");
    }

    //�� ���� �Ϸ� �ݹ�
    public override void OnJoinedRoom()
    {
        print($"�� ���� �Ϸ� : {PhotonNetwork.CurrentRoom.Name}");

        //PhotonNetwork.Instantiate("Tank", new Vector3(0, 5.0f, 0), Quaternion.identity, 0);//gropid �̰� �ٸ��� ���� �� ����

        //���������� �̵�ó��
        if(PhotonNetwork.IsMasterClient)//PhotonNetwork.AutomaticallySyncScene = true;�� �������� ������ ������ ���� �ٲٸ� ��ΰ� �ٲ�
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BattleField");
        }

    }

    

    #endregion

}

