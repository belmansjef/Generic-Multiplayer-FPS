using Photon.Pun;
using UnityEngine;

public class Deathzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.root.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, 500f, 6969);
        }
    }
}
