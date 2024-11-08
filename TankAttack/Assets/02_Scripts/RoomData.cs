using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomData : MonoBehaviour
{
    [SerializeField] private TMP_Text roomTxt;

    private RoomInfo _roomInfo;

    public RoomInfo RoomInfo
    {
        get { return _roomInfo; }
        set 
        {
            _roomInfo = value;
            roomTxt.text = $"{_roomInfo.Name} : {_roomInfo.PlayerCount} / {_roomInfo.MaxPlayers}";
            //버튼 이벤트 연결
            GetComponent<Button>().onClick.AddListener(() =>
            {
                PhotonNetwork.JoinRoom(_roomInfo.Name);
                PhotonManager.Instance.SetNickName();
            });
        }
    }

    private void Awake()
    {
        roomTxt = GetComponentInChildren<TMP_Text>();

    }
}
