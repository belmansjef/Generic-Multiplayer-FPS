using Photon.Pun;
using Photon.Realtime;

using UnityEngine;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Fields

    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined.")]
    [SerializeField] private byte maxPlayersPerRoom = 4;
    
    [Header("Panels: ")]
    [Tooltip("The UI panel to let the user enter name, connect and play")]
    [SerializeField] private GameObject controlPanel = null;

    [Tooltip("The UI label to inform the user that the connection is in progress")]
    [SerializeField] private GameObject progressLabel = null;

    [Tooltip("The UI panel that shows the players in the room and a start button")]
    [SerializeField] private GameObject lobbyPanel = null;

    [Header("UI Elements:")]
    [SerializeField] private Button JoinGameButton;

    private string gameVersion = "0.0.1";
    private Text playerCount;

    #endregion

    #region Monobehaviour Callbacks

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        lobbyPanel.SetActive(false);

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings(); // Connect to photon cloud
            PhotonNetwork.GameVersion = gameVersion; // Set gameversion
        }
        else
        {
            JoinGameButton.interactable = true;
        }
        
        playerCount = lobbyPanel.GetComponentInChildren<Text>();
    }

    #endregion

    #region Public Methods

    public void Connect()
    {
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);
        
        // Join random room if connect to photon cloud
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings(); // Connect to photon cloud
            PhotonNetwork.GameVersion = gameVersion; // Set gameversion
        }
    }

    public void StartMatch()
    {
        PhotonNetwork.LoadLevel(1);
    }

    #endregion

    #region Private Methods

    private void SetPlayerCountText()
    {
        playerCount.text = ($"Connected players: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    #endregion
    
    #region MonoBehaviourPunCallbacks callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Launcher: OnConnectedToMaster() was called by PUN");
        JoinGameButton.interactable = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Launcher: OnJoinRandomFailed() was called by PUN. No room was found. \nCalling: PhotonNetwork.CreateRoom()");

        PhotonNetwork.CreateRoom(null, new RoomOptions{ MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        SetPlayerCountText();
        
        if (!PhotonNetwork.IsMasterClient)
        {
            lobbyPanel.GetComponentInChildren<Button>().interactable = false;
        }

        Debug.Log("Launcher: OnJoinedRoom() was called by PUN. This client is now in a room.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        lobbyPanel.SetActive(false);

        Debug.LogWarningFormat($"Launcher: OnDisconnected() was called by PUN with reason: {cause}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} joined the lobby!");
        playerCount.text = ($"Connected players: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} left the lobby!");
        playerCount.text = ($"Connected players: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    #endregion
}
