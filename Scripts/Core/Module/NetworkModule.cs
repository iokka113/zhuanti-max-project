using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using ProjectMax.CSharp;
using ProjectMax.Unity;

public class NetworkModule : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private void Start(){
        Singleton<NetworkModule>.SetInstance(this, true);
    }

    #region ref

	public static int ping => PhotonNetwork.GetPing();
    public static Room room => PhotonNetwork.CurrentRoom;
    public static Player local => PhotonNetwork.LocalPlayer;
    public static Player master => room?.GetPlayer(room.MasterClientId);
    public static Dictionary<int, Player>.ValueCollection players => room?.Players.Values;

    public static NetworkState state{
        get{
			if(PhotonNetwork.InRoom){
				return NetworkState.InRoom;
			}
			if(PhotonNetwork.InLobby){
				return NetworkState.InLobby;
			}
			if(PhotonNetwork.IsConnected){
				return NetworkState.IsConnected;
			}
			return NetworkState.IsDisconnected;
        }
    }

    #endregion

    #region connect

	public static void Connect(Action onConnect = null){
		onConnect?.Invoke();
		Debug.Log("connecting server");
		PhotonNetwork.ConnectUsingSettings();
	}

    public static void Disconnect(Action onDisconnect = null){
		onDisconnect?.Invoke();
		Debug.Log("disconnecting server");
		PhotonNetwork.Disconnect();
	}

	public static void JoinLobby(Action onJoinLobby = null){
		onJoinLobby?.Invoke();
		Debug.Log("joining lobby");
		PhotonNetwork.JoinLobby();
	}

	public static void CreateRoom(string roomName, RoomOptions roomOptions = null,
	Action onCreateRoom = null){
        onCreateRoom?.Invoke();
		Debug.Log("creating room");
		PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
	}

	public static void JoinRoom(string roomName, Action onJoinRoom = null){
		onJoinRoom?.Invoke();
		Debug.Log("joining room");
		PhotonNetwork.JoinRoom(roomName);
	}

	public static void JoinRandomRoom(Action onJoinRandomRoom = null){
		onJoinRandomRoom?.Invoke();
		Debug.Log("joining random room");
		PhotonNetwork.JoinRandomRoom();
	}

	public static void LeaveRoom(bool becomeInactive = true, Action onLeaveRoom = null){
        onLeaveRoom?.Invoke();
        // ResetLocalProps();
		Debug.Log("leaving room");
		PhotonNetwork.LeaveRoom(becomeInactive);
	}

    #endregion

	#region callback

    public static Action onConnected;

	public override void OnConnectedToMaster(){
		Debug.Log("connected server");
		onConnected?.Invoke();
	}

    public static Action<DisconnectCause> onDisconnected;

	public override void OnDisconnected(DisconnectCause cause){
		Debug.Log("disconnected server");
		Debug.Log(cause.ToString());
		onDisconnected?.Invoke(cause);
	}

    public static Action onJoinedLobby;

	public override void OnJoinedLobby(){
		Debug.Log("joined lobby");
		onJoinedLobby?.Invoke();
	}

    public static Action onLeftLobby;

	public override void OnLeftLobby(){
		Debug.Log("left lobby");
		onLeftLobby?.Invoke();
	}

    public static Action onCreatedRoom;

	public override void OnCreatedRoom(){
		Debug.Log("created room");
		onCreatedRoom?.Invoke();
	}

    public static Action<short, string> onCreateRoomFailed;

	public override void OnCreateRoomFailed(short returnCode, string message){
		Debug.Log("create room failed");
		Debug.Log($"{returnCode}: {message}");
		onCreateRoomFailed?.Invoke(returnCode, message);
	}

    public static Action onJoinedRoom;

	public override void OnJoinedRoom(){
		Debug.Log("joined room");
		onJoinedRoom?.Invoke();
	}

    public static Action<short, string> onJoinRoomFailed;

	public override void OnJoinRoomFailed(short returnCode, string message){
		Debug.Log("join room failed");
		Debug.Log($"{returnCode}: {message}");
		onJoinRoomFailed?.Invoke(returnCode, message);
	}

    public static Action<short, string> onJoinRandomFailed;

	public override void OnJoinRandomFailed(short returnCode, string message){
		Debug.Log("join random failed");
		Debug.Log($"{returnCode}: {message}");
		onJoinRandomFailed?.Invoke(returnCode, message);
	}

    public static Action onLeftRoom;

	public override void OnLeftRoom(){
		Debug.Log("left room");
		onLeftRoom?.Invoke();
	}

    public static Action<List<RoomInfo>> onRoomListUpdate;

    public override void OnRoomListUpdate(List<RoomInfo> roomList){
    	Debug.Log("room list updated");
		onRoomListUpdate?.Invoke(roomList);
    }

    public static Action<Hashtable> onRoomPropertiesUpdate;

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged){
    	// Debug.Log("room properties updated");
		onRoomPropertiesUpdate?.Invoke(propertiesThatChanged);
    }

    public static Action<Player> onPlayerEnteredRoom;

    public override void OnPlayerEnteredRoom(Player newPlayer){
    	Debug.Log("player entered room");
		onPlayerEnteredRoom?.Invoke(newPlayer);
    }

    public static Action<Player> onPlayerLeftRoom;

    public override void OnPlayerLeftRoom(Player otherPlayer){
    	Debug.Log("player left room");
		onPlayerLeftRoom?.Invoke(otherPlayer);
    }

    public static Action<Player, Hashtable> onPlayerPropertiesUpdate;

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps){
    	// Debug.Log("player properties updated");
		onPlayerPropertiesUpdate?.Invoke(targetPlayer, changedProps);
    }

    public static Action<EventData> onEventReceived;

	public void OnEvent(EventData photonEvent){
    	// Debug.Log("pun event received");
		onEventReceived?.Invoke(photonEvent);
    }

    #endregion

    #region sync

    public static void SetName(string nickName){
		local.NickName = nickName;
        Debug.Log($"set NickName {nickName}");
    }

    public static void ResetName(){
		SetName(string.Empty);
    }

    public static T GetProp<T>(Player player, object key){
        if(player.CustomProperties.TryGetValue(key, out object value)){
            return (T)value;
        }
        return default(T);
    }

	public static bool SetProp(Player player, object key, object value){
		Hashtable table = new Hashtable();
		table.Add(key, value);
		return player.SetCustomProperties(table);
	}

	public static bool ResetProps(Player player){
		Hashtable table = new Hashtable();
		foreach(object key in player.CustomProperties.Keys){
			table.Add(key, null);
		}
		return player.SetCustomProperties(table);
	}

    public static T GetRoomProp<T>(object key){
        if(room.CustomProperties.TryGetValue(key, out object value)){
            return (T)value;
        }
        return default(T);
    }

	public static bool SetRoomProp(object key, object value){
		Hashtable table = new Hashtable();
		table.Add(key, value);
		return room.SetCustomProperties(table);
	}

	public static bool ResetRoomProps(){
		Hashtable table = new Hashtable();
		foreach(object key in room.CustomProperties.Keys){
			table.Add(key, null);
		}
		return room.SetCustomProperties(table);
	}

	public static void RaiseEvent(byte eventCode, object data = null,
	ReceiverGroup receivers = ReceiverGroup.All){
		if(eventCode < 0 || eventCode > 199){
			Debug.LogWarning("eventCode overflow");
			return;
		}
		RaiseEventOptions options = new RaiseEventOptions{ Receivers = receivers };
		PhotonNetwork.RaiseEvent(eventCode, ObjectLib.Serialize(data),
		options, SendOptions.SendReliable);
	}

    public static GameObject Instantiate(string prefabName, Vector3 pos, Quaternion rot,
	bool inRoom = false, byte group = 0, object[] data = null){
		if(inRoom){
			MasterRpc.mine?.photonView.RPC("Instantiate",
			RpcTarget.MasterClient, prefabName, pos, rot, group, data);
			return null;
		}
		else{
			return PhotonNetwork.Instantiate(prefabName, pos, rot, group, data);
		}
	}

    public static void Destroy(PhotonView targetView, bool inRoom = false){
		if(inRoom){
			MasterRpc.mine?.photonView.RPC("Destroy",
			RpcTarget.MasterClient, targetView.ViewID);
		}
		else{
			PhotonNetwork.Destroy(targetView);
		}
	}

	public static Player FindPlayer(int playerID){
		return room?.GetPlayer(playerID);
	}

	public static PhotonView FindView(int viewID){
		return PhotonView.Find(viewID);
	}

	public static bool IsMyView(PhotonView view){
		return view && view.IsMine;
	}

    #endregion

	public static void ResetCallbacks(){
    	Debug.Log("reset all callbacks");
		onConnected = null;
		onDisconnected = null;
		onJoinedLobby = null;
		onLeftLobby = null;
		onCreatedRoom = null;
		onCreateRoomFailed = null;
		onJoinedRoom = null;
		onJoinRoomFailed = null;
		onJoinRandomFailed = null;
		onLeftRoom = null;
		onRoomListUpdate = null;
		onRoomPropertiesUpdate = null;
		onPlayerEnteredRoom = null;
		onPlayerLeftRoom = null;
		onPlayerPropertiesUpdate = null;
		onEventReceived = null;
	}

	public static void CloseRoom(){
		// room.IsVisible = false;
		room.IsOpen = false;
		Debug.Log($"room closed");
	}
}

public enum NetworkState
{
    InRoom,
    InLobby,
    IsConnected,
    IsDisconnected
}
