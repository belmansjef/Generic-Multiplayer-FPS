using System;
using Photon.Pun;
using UnityEngine;

public class WeaponInventory : MonoBehaviourPun
{
    #region Fields
    
    [SerializeField] private GameObject[] weapons;

    private byte activeWeapon = 0;

    #endregion
    
    #region MonoBehaviour Callbacks

    private void Start()
    {
        photonView.RPC("EquipGun", RpcTarget.All, (byte)0);
    }

    private void Update()
    {
        if(!photonView.IsMine) return;
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            photonView.RPC("EquipGun", RpcTarget.All, (byte)0);   
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
         photonView.RPC("EquipGun", RpcTarget.All, (byte)1);
        }
    }

    #endregion

    #region RPC Methods

    [PunRPC]
    private void EquipGun(byte _gun)
    {
        WeaponController weaponController = weapons[activeWeapon].GetComponent<WeaponController>();

        if (weaponController.IsReloading)
            weaponController.CancelReload();

        for (int i = 0; i < weapons.Length; i++)
        {
            if (i == _gun)
            {
                weapons[i].SetActive(true);
            }
            else
            {
                weapons[i].SetActive(false);
            }
        }
    }

    #endregion
    

}
