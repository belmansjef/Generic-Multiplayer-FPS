using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    
    #region Serialized Private Fields

    [SerializeField] private GameObject playerCanvas;

    #endregion

    #region Private Fields

    private PhotonView PV;

    #endregion
    
    #region MonoBehaviour Callbacks

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            CreateController();
            CreateUI();
            if (!instance)
            {
                instance = this;
            }
        }
    }

    #endregion

    #region Public Methods

    public void CreateController()
    {
        GameObject playerGo = PhotonNetwork.Instantiate("PlayerController", SpawnpointController.instance.GetRandomSpawnpoint().position + Vector3.up * 4f, Quaternion.identity);
    }

    #endregion
    
    #region Private Methods

    private void CreateUI()
    {
        Instantiate(playerCanvas, Vector3.zero, Quaternion.identity);
    }

    #endregion
}
