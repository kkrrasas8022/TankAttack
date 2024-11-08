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
    IEnumerator Start()//Start�Լ��� �ڷ�ƾ���� ȣ�� ����(Awake�� �Ұ���)
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
        //���濡�� �濡�� �������� ��ȿ��� �ߴ� ������ �ʱ�ȭ �ؾ��Ѵ�

        //���� Exit ��û
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        print("�κ�� ���ƿ�");
    }

    //�÷��̾ �濡 ������ ȣ��Ǵ� �Լ�
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        DisplayConnectInfo();
        DisplayPlayerList();  
        string msg = $"<color=#00ff00>[{newPlayer.NickName}]</color>���� �����ϼ̽��ϴ�";
        DisPlayMessage(msg);
    }

    //�÷��̾ �濡�� ������ ȣ��Ǵ� �Լ�
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        DisplayConnectInfo();
        DisplayPlayerList();
        string msg = $"<color=#ff0000>[{otherPlayer.NickName}]</color>���� �����ϼ̽��ϴ�";
        DisPlayMessage(msg);
    }

    [PunRPC]
    private void DisPlayMessage(string msg)
    {
        _messageListText.text += $"{msg}\n";
    }

}
