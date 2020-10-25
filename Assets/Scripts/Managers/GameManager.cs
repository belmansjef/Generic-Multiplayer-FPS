using System;
using Photon.Pun;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager instance;
    
    #region Public fields

    public delegate void OnPlayerKilledCallback(int playerID, int sourceID);
    public OnPlayerKilledCallback OnPlayerKilled;

    #endregion

    #region Private Fields

    private PhotonView PV;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        if (!instance) { instance = this; }
        
    }

    #endregion
}
