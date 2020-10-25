using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    #region Private Constants

    private const string playerNamePrefKey = "PlayerName";

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        string defaultName = string.Empty;
        InputField _inputField = GetComponent<InputField>();

        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }

    #endregion

    #region Public Methods

    public void SetPlayerName(string value)
    {
        PhotonNetwork.NickName = value;
        
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }

    #endregion
}
