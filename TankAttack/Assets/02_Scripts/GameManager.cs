using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine.UI;
using TMPro;


public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _sendButton;

    [SerializeField] private TMP_Text _connectInfoText;
    [SerializeField] private TMP_Text _messageListText;
    [SerializeField] private TMP_Text _playerListText;

    [SerializeField] private TMP_InputField _messageIF;
    private void Awake()
    {
        Instance = this;
        PhotonNetwork.IsMessageQueueRunning = false;
    }
    IEnumerator Start()//Start함수는 코루틴으로 호출 가능(Awake는 불가능)
    {
        _exitButton.onClick.AddListener(() => OnExitButtonClick());
        _sendButton.onClick.AddListener(() => OnSendButtonClick());
        _messageIF.onEndEdit.AddListener((inputMessage) =>
        {
            string msg = $"<color=#00ff00>[{PhotonNetwork.NickName}]</color> {inputMessage}";
            SendMessageByRPC(msg);
            _messageIF.text = "";
        });
        yield return new WaitForSeconds(0.2f);
        CreateTank();
        yield return new WaitForSeconds(0.2f);
        PhotonNetwork.IsMessageQueueRunning = true;
        DisplayConnectInfo();
        DisplayPlayerList();
    }

    private void CreateTank()
    {
        Vector3 pos = new Vector3(Random.Range(-100.0f, 100.0f), 5.0f, Random.Range(-100.0f, 100.0f));
        PhotonNetwork.Instantiate("Tank",pos,Quaternion.identity);
    }

    private void DisplayConnectInfo()
    {
        int currnetPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        int maxPlayer = PhotonNetwork.CurrentRoom.MaxPlayers;
        string roomName = PhotonNetwork.CurrentRoom.Name;

        string connectStr = $"{roomName}(<color=#00ff00>{currnetPlayer}</color>/<color=#ff0000>{maxPlayer}</color>)";
        _connectInfoText.text = connectStr;
    }

    private void DisplayPlayerList()
    {
        string playerList = "";

        foreach(var player in PhotonNetwork.PlayerList)
        {
            string _color = player.IsMasterClient ? "#ff0000" : "#00ff00";
            playerList += $"<color={_color}>{player.NickName}</color>\n";
        }
        _playerListText.text = playerList;
    }

    public void SendMessageByRPC(string msg)
    {
        photonView.RPC(nameof(DisPlayMessage), RpcTarget.AllBufferedViaServer, msg);
    }

    private void OnSendButtonClick()
    {
        string msg = $"<color=#00ff00>[{PhotonNetwork.NickName}]</color> {_messageIF.text}";
        SendMessageByRPC(msg);
    }

    private void OnExitButtonClick()
    {
        //포톤에서 방에서 나갈려면 방안에서 했던 모든것을 초기화 해야한다

        //룸을 Exit 요청
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        print("로비로 돌아옴");
    }

    //플레이어가 방에 들어오면 호출되는 함수
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        DisplayConnectInfo();
        DisplayPlayerList();  
        string msg = $"<color=#00ff00>[{newPlayer.NickName}]</color>님이 입장하셨습니다";
        DisPlayMessage(msg);
    }

    //플레이어가 방에서 나가면 호출되는 함수
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        DisplayConnectInfo();
        DisplayPlayerList();
        string msg = $"<color=#ff0000>[{otherPlayer.NickName}]</color>님이 퇴장하셨습니다";
        DisPlayMessage(msg);
    }

    [PunRPC]
    private void DisPlayMessage(string msg)
    {
        _messageListText.text += $"{msg}\n";
    }

}
