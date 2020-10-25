using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class HealthController : MonoBehaviour, IPunObservable
{
    #region Fields

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    private Manager manager;
    private PhotonView PV;
    
    #endregion
    
    #region MonoBehaviour Callbacks

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            UIManager.instance.UpdateHealthUI(currentHealth);
        }

        manager = GameObject.Find("_GameManager").GetComponent<Manager>();
    }

    #endregion

    #region Pun Callbacks

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // if (stream.IsWriting)
        // {
        //     stream.SendNext(currentHealth);
        // }
        //
        // if (stream.IsReading)
        // {
        //     currentHealth = (float)stream.ReceiveNext();
        // }
    }

    #endregion

    #region Public Methods

    [PunRPC]
    public void TakeDamage(float _damage, int _sourceID)
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0f, maxHealth);

        if (PV.IsMine)
        {
            SoundManager.instance.PlaySound(SoundManager.Sound.HitHurt);
            UIManager.instance.UpdateHealthUI(currentHealth);
            
            if (currentHealth <= 0f)
            {
                Die(_sourceID);
            }
        }
    }

    public void Die(int _sourceID)
    {
        manager.Spawn();
        manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
        
        if (_sourceID >= 0 && _sourceID < 6969)
        {
            manager.ChangeStat_S(_sourceID, 0, 1);
        }
        
        PhotonNetwork.Destroy(gameObject);
    }

    #endregion
    
}
