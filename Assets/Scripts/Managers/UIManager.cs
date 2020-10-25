using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    #region Fields
    
    public static UIManager instance;
    public bool isPaused;
    
    [Header("UI Objects:")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text killFeedText;
    [SerializeField] private Text killsText;
    [SerializeField] private Text deathsText;
    [SerializeField] private Text winText;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private GameObject crosshairGo;
    [SerializeField] private GameObject hitmarkerGo;

    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;

    private float hitmarkerTime;
    private bool doShowCrosshair = false;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    private void Start()
    {
        menuPanel.SetActive(false);
        winText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Time.time > hitmarkerTime)
        {
            hitmarkerGo.SetActive(false);
        }

        if (Input.GetButtonDown("Fire2") && doShowCrosshair)
        {
            crosshairGo.SetActive(false);
        }

        if (Input.GetButtonUp("Fire2") && doShowCrosshair)
        {
            crosshairGo.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    #endregion

    #region Public Methods
    
    public void UpdateHealthUI(float _value)
    {
        healthText.text = _value.ToString();
    }

    public void UpdateAmmoUI(float _inMag, float _magSize)
    {
        ammoText.text = $"{_inMag}/{_magSize}";
    }

    public void ShowHitmarker(float _time)
    {
        hitmarkerTime = Time.time + _time;
        hitmarkerGo.SetActive(true);
    }

    public void EnableCrosshair()
    {
        doShowCrosshair = true;
        crosshairGo.SetActive(true);
    }

    public void DisableCrosshair()
    {
        doShowCrosshair = false;
        crosshairGo.SetActive(false);
    }
    
    public void UpdateKillFeed(int _playerID, int _sourceID)
    {
        if (_sourceID == 6969)
        {
            killFeedText.text = $"{PhotonView.Find(_playerID).Controller.NickName} fell into the void";
        }
        else
        {
            killFeedText.text = $"{PhotonView.Find(_sourceID).Controller.NickName} killed {PhotonView.Find(_playerID).Controller.NickName}";
        }
    }

    public void UpdateKills(int _value)
    {
        killsText.text = $"Kills: {_value.ToString("00")}";
    }

    public void UpdateDeaths(int _value)
    {
        deathsText.text = $"Deaths: {_value.ToString("00")}";
    }

    public void SetWinText(string _winner)
    {
        crosshairGo.SetActive(false);
        winText.gameObject.SetActive(true);
        winText.text = $"{_winner} won the match. He gud, all others succ";
    }
    
    public void SensitivityChanged()
    {
        PlayerController.i.SetSensitivity(sensitivitySlider.value);
    }

    public void SoundChanged()
    {
        SoundManager.instance.masterMixer.SetFloat("MasterVolume", soundSlider.value);
    }

    public void ExitMatch()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    #endregion

    #region Private Methods

    private void ToggleMenu()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            menuPanel.SetActive(true);
            crosshairGo.SetActive(false);
        }
        else if (!isPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            menuPanel.SetActive(false);
            crosshairGo.SetActive(false);
        }
    }

    #endregion
}
