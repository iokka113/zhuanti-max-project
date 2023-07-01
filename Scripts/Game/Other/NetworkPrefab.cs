using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public abstract class NetworkPrefab : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    protected abstract void OnInstantiate(object[] initData);

    public void OnPhotonInstantiate(PhotonMessageInfo info){
        object[] initData = info.photonView.InstantiationData;
        OnInstantiate(initData);
    }
}
