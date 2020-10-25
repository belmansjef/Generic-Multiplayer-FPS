using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInfo
{
    public int actor;
    public short kills;
    public short deaths;

    public PlayerInfo(int _actor, short _kills, short _deaths)
    {
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }
}

public enum GameState
{ 
    Waiting = 0,
    Starting = 1,
    Playing = 2,
    Ending = 3,
}

public class Manager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region Fields

    public int mainmenu = 0;
    public int killsToWin = 5;

    public string playerPrefabString;
    public GameObject playerPrefab;
    public GameObject mapCamera;
    
    public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
    public int myID;

    private int winnerID;
    
    private bool isPlayerAdded;

    private GameState state = GameState.Waiting;

    #endregion

    #region EventCodes

    public enum EventCodes : byte
    {
        NewPlayer,
        UpdatePlayers,
        ChangeStat
    }

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        mapCamera.SetActive(false);
        
        ValidateConnection();
        NewPlayer_S();

        if (PhotonNetwork.IsMasterClient)
        {
            isPlayerAdded = true;
            Spawn();
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #endregion

    #region Photon

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code >= 200) return; // Photon reserved events

        EventCodes e = (EventCodes) photonEvent.Code;
        object[] o = (object[]) photonEvent.CustomData;

        switch (e)
        {
            case EventCodes.NewPlayer:
                NewPlayer_R(o);
                break;
            
            case EventCodes.UpdatePlayers:
                UpdatePlayers_R(o);
                break;
            
            case EventCodes.ChangeStat:
                ChangeStat_R(o);
                break;
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(mainmenu);
    }

    #endregion
    
    #region Methods

    public void Spawn()
    {
        Transform spawn = SpawnpointController.instance.GetRandomSpawnpoint();

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Instantiate(playerPrefabString, spawn.position, spawn.rotation);
        }
        else
        {
            GameObject newPlayer = Instantiate(playerPrefab, spawn.position, spawn.rotation);
        }
    }

    private void RefreshMyStats()
    {
        if (playerInfo.Count > myID) // If we're already in the game
        {
            UIManager.instance.UpdateKills(playerInfo[myID].kills);
            UIManager.instance.UpdateDeaths(playerInfo[myID].deaths);
        }
        else // Else we're new
        {
            UIManager.instance.UpdateKills(0);
            UIManager.instance.UpdateDeaths(0);
        }
    }
    
    private void ValidateConnection()
    {
        if(PhotonNetwork.IsConnected) return;
        SceneManager.LoadScene(mainmenu);
    }

    private void StateCheck()
    {
        if (state == GameState.Ending)
        {
            EndGame();
        }
    }

    private void ScoreCheck()
    {
        bool detectwin = false;

        foreach (PlayerInfo a in playerInfo)
        {
            if (a.kills >= killsToWin)
            {
                detectwin = true;
                winnerID = a.actor;
                break;
            }
        }

        if (detectwin)
        {
            if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                UpdatePlayers_S((int)GameState.Ending, winnerID, playerInfo);
            }
        }
    }

    private void EndGame()
    {
        state = GameState.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        
        mapCamera.SetActive(true);
        UIManager.instance.SetWinText(PhotonNetwork.CurrentRoom.GetPlayer(winnerID).NickName);
        
        Invoke(nameof(EndMatch), 5f);
    }

    private void EndMatch()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }
    
    #endregion

    #region Events

    public void NewPlayer_S()
    {
        // Create player package
        object[] package = new object[3];

        // Assign variables
        package[0] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[1] = (short) 0;
        package[2] = (short) 0;

        // Send package to master
        PhotonNetwork.RaiseEvent(
            (byte) EventCodes.NewPlayer,
            package,
            new RaiseEventOptions{ Receivers = ReceiverGroup.MasterClient},
            new SendOptions{ Reliability = true }
        );
    }

    public void NewPlayer_R(object[] data)
    {
        PlayerInfo p = new PlayerInfo(
            (int)data[0],
            (short) data[1],
            (short)data[2]
            );

        playerInfo.Add(p);
        
        // TODO: Add player sync for player username
        
        UpdatePlayers_S((int)state, winnerID, playerInfo);
    }

    public void UpdatePlayers_S(int state, int winner, List<PlayerInfo> info)
    {
        object[] package = new object[info.Count + 2];

        package[0] = state;
        package[1] = winner;
        for (int i = 0; i < info.Count; i++)
        {
            object[] piece = new object[3];

            piece[0] = info[i].actor;
            piece[1] = info[i].kills;
            piece[2] = info[i].deaths;

            package[i + 2] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdatePlayers,
            package,
            new RaiseEventOptions{ Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void UpdatePlayers_R(object[] data)
    {
        state = (GameState) data[0];
        winnerID = (int) data[1];
        
        if (playerInfo.Count < data.Length - 1)
        {
            // TODO: Add player sync for player username
        }
        
        playerInfo = new List<PlayerInfo>();

        for (int i = 2; i < data.Length; i++)
        {
            object[] extract = (object[]) data[i];
            
            PlayerInfo p = new PlayerInfo(
                (int) extract[0],
                (short) extract[1],
                (short) extract[2]
            );
            
            playerInfo.Add(p);

            if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor)
            {
                myID = i - 2;

                // If we've been waiting to be added to the game then spawn us in
                if (!isPlayerAdded)
                {
                    isPlayerAdded = true;
                    Spawn();
                }
            }
        }
        
        StateCheck();
    }

    public void ChangeStat_S(int actor, byte stat, byte value)
    {
        object[] package = { actor, stat, value };

        PhotonNetwork.RaiseEvent(
            (byte) EventCodes.ChangeStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void ChangeStat_R(object[] data)
    {
        int actor = (int)data[0];
        byte stat = (byte)data[1];
        byte value = (byte)data[2];

        for (int i = 0; i < playerInfo.Count; i++)
        {
            if (playerInfo[i].actor == actor)
            {
                switch (stat)
                {
                    case 0: // Kills
                        playerInfo[i].kills += value;
                        Debug.Log($"Player {PhotonNetwork.CurrentRoom.GetPlayer(actor).NickName}: Kills = {playerInfo[i].kills}");
                        break;
                    case 1: // Deaths
                        playerInfo[i].deaths += value;
                        Debug.Log($"Player {PhotonNetwork.CurrentRoom.GetPlayer(actor).NickName}: Deaths = {playerInfo[i].deaths}");
                        break;
                    default:
                        Debug.Log("Something went wrong changing stats");
                        break;
                }
                
                if(i == myID) RefreshMyStats();
                break;
            }
        }
        
        ScoreCheck();
    }
    
    #endregion
}
