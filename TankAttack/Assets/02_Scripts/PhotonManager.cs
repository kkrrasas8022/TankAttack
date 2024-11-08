using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using Unity.Multiplayer.Center.Common;
using System.Collections.Generic;


public class PhotonManager : MonoBehaviourPunCallbacks//포톤서버에서 제공하는 함수들을 사용할 수 있음
{
    public static PhotonManager Instance=null;

    //게임버전 1.0
    //포톤은 같은 버전을 가지고 있는 사람들끼리만 매칭해줌
    [SerializeField] private const string _version = "1.0";
    //유저명
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

        //버튼 비활성
        _loginButton.interactable = false;
        _makeRoomButton.interactable = false;


        //게임버전 설정
        PhotonNetwork.GameVersion = _version;
        //유저명 설정
        PhotonNetwork.NickName = _nickname;

        //방장이 씬을 로딩했을 떄 타 유저들에 자동으로 씬을 로딩시켜주는 옵션
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            //포톤서버에 접속 요청
            PhotonNetwork.ConnectUsingSettings();
        }

        //버튼 이벤트 연결
        //_btn.onClick.AddListener(() =>
        //{
        //    print("버튼클릭");
        //    Debug.LogError("버튼클릭에러메시지");
        //});
    }

    private void Start()
    {
        //저장된 닉네임 로드
        _nickname = PlayerPrefs.GetString("NICK_NAME", $"User_{Random.Range(0, 1001):0000}");
        _nickNameIF.text = _nickname;
        //버튼 이벤트 연결
        _loginButton.onClick.AddListener(() => OnLoginButtonClick());
        _makeRoomButton.onClick.AddListener( () => OnMakeRoomButtonClick());
    }

    public void SetNickName()
    {
        //닉네임 필드가 비어있는지 체크
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


    #region UI 콜백 함수
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
        //룸 생성
        PhotonNetwork.CreateRoom(_roomNameIF.text, roomOption);
    }

    #endregion


    #region 포톤콜백함수

    //룸목록을 저장할 Dictionary 선언
    private Dictionary<string, GameObject> roomDict = new Dictionary<string, GameObject>();

    //룸목록이 변경되면 호출되는 콜백
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //로비에 입장했을때만 호출되는 함수
        foreach(var room in roomList)
        {
            //print($"{room.Name} : {room.PlayerCount} / {room.MaxPlayers}");

            if(room.RemovedFromList==true)//삭제된 룸을 의미 ( 플레이어 상황이 0/0 인경우)
            {
                //룸삭제
                if(roomDict.TryGetValue(room.Name,out GameObject removedRoom))
                {
                    //프리팹 인스턴스 삭제
                    Destroy(removedRoom);
                    //Dictionary에서 키를 삭제
                    roomDict.Remove(room.Name);
                }
                continue;
            }

            //새로 생성된 룸, 변경된 경우 로직 처리
            //처음 생성된 룸(Dictionary 검색했을 때 결과가 없을 경우)
            if(roomDict.ContainsKey(room.Name)==false)
            {
                //room prefab 생성
                var _room = Instantiate(_roomPrefab, _contentTr);
                //룸 속성 설정
                _room.GetComponent<RoomData>().RoomInfo = room;
                //Dictionary에 저장
                roomDict.Add(room.Name, _room);
            }
            //검색했을 때 있을 경우 룸 정보를 변경
            else
            {

            }
        }//foreach room

    }//OnRoomListUpdate




    //포톤서버에 접속되었을 떄 호출되는 콜백(CallBack)
    public override void OnConnectedToMaster()
    {
        _loginButton.interactable = true;
        _makeRoomButton.interactable = true;
        print("서버접속 완료");
        //Lobby 접속 요청
        PhotonNetwork.JoinLobby();
    }

    //로비에 입장했을 때 호출되는 콜백
    public override void OnJoinedLobby()
    {
        print("로비 입장 완료");
        //랜덤한 방에 입장을 요청
        //PhotonNetwork.JoinRandomRoom();
    }




    //랜덤 방입장 실패했을 때 호출되는 콜백
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print($"방입장 실패 : {returnCode} : {message}");

        //룸 옵션 설정
        RoomOptions Ro = new RoomOptions
        {
            MaxPlayers = 20,
            IsOpen = true,
            IsVisible = true
        };
        PhotonNetwork.CreateRoom($"{_nickname}'s Room", Ro);
    }

    //룸 생성 완료 콜백
    public override void OnCreatedRoom()
    {
        print("룸 생성 완료");
    }

    //룸 입장 완료 콜백
    public override void OnJoinedRoom()
    {
        print($"방 입장 완료 : {PhotonNetwork.CurrentRoom.Name}");

        //PhotonNetwork.Instantiate("Tank", new Vector3(0, 5.0f, 0), Quaternion.identity, 0);//gropid 이게 다르면 만날 수 없음

        //전투씬으로 이동처리
        if(PhotonNetwork.IsMasterClient)//PhotonNetwork.AutomaticallySyncScene = true;를 설정헀기 떄문에 방장이 씬을 바꾸면 모두가 바뀜
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BattleField");
        }

    }

    

    #endregion

}

