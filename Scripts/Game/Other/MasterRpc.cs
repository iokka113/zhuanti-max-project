using UnityEngine;
using Photon.Pun;

public class MasterRpc : NetworkPrefab
{
    public static MasterRpc mine { get; private set; }

    protected override void OnInstantiate(object[] initData){
        if(NetworkModule.IsMyView(photonView)){
            mine = this;
        }
    }

	[PunRPC]
    public void Instantiate(string prefabName, Vector3 pos, Quaternion rot,
	byte group = 0, object[] data = null){
		if(NetworkModule.local.IsMasterClient){
			PhotonNetwork.InstantiateRoomObject(prefabName, pos, rot, group, data);
		}
	}

	[PunRPC]
    public void Destroy(int targetViewID){
		if(NetworkModule.local.IsMasterClient){
			PhotonView target = PhotonView.Find(targetViewID);
			PhotonNetwork.Destroy(target);
		}
	}
}
