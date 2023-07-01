using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ProjectMax.CSharp;
using ProjectMax.Unity;

public class LoginLogic : MonoBehaviour
{
	#region ref

    [SerializeField]
    private GameObject _loadingPanel = null;

    [SerializeField]
    private GameObject _reloadPanel = null;

	[SerializeField]
	private Text _playerIDText = null;

    [SerializeField]
    private GameObject _roomPanel = null;

    [SerializeField]
    private GameObject _roomJoinPanel = null;

	[SerializeField]
	private Text _roomListText = null;

	[SerializeField]
	private Text _roomNameText = null;

	[SerializeField]
	private Text _playerNameText = null;

    [SerializeField]
    private GameObject _roomReadyPanel = null;

	[SerializeField]
	private Text _playerListText = null;

	[SerializeField]
	private Button _roomStartButton = null;

	[SerializeField]
	private Toggle _autoStartToggle = null;

    [SerializeField]
    private GameObject _rolePanel = null;

    [SerializeField]
    private GameObject _roleSelectPanel = null;

    [SerializeField]
    private Text _roleListText = null;

    [SerializeField]
    private GameObject _rolePlayerPanel = null;

    [SerializeField]
    private Text _roleWaitText = null;

    private int _roleSelectIndex = 0;

	#endregion

    private void Start(){
        StartupModule.Init();
		if(DatabaseModule.ins == null){
			SceneManager.LoadScene("HomeScene");
		}
		else{
			InitScene();
			SubscribeEvents();
			CheckNetworkState();
		}
    }

	private void InitScene(){
        _loadingPanel.SetActive(false);
		_reloadPanel.SetActive(false);
		_roomPanel.SetActive(false);
		_roomJoinPanel.SetActive(false);
		_roomReadyPanel.SetActive(false);
		_roomStartButton.interactable = false;
		_rolePanel.SetActive(false);
		_roleSelectPanel.SetActive(false);
		_rolePlayerPanel.SetActive(false);
		_autoStartToggle.isOn = DatabaseModule.env.autoStart;
	}

	private void SubscribeEvents(){
		NetworkModule.onConnected += () => {
			_loadingPanel.SetActive(false);
			CheckNetworkState();
		};
		NetworkModule.onDisconnected += (cause) => {
			_loadingPanel.SetActive(false);
			NetworkModule.ResetCallbacks();
			SceneManager.LoadScene("HomeScene");
		};
		NetworkModule.onJoinedLobby += () => {
			_loadingPanel.SetActive(false);
			_roomPanel.SetActive(true);
			_roomJoinPanel.SetActive(true);
			if(DatabaseModule.env.autoJoin){
				OnJoinRandomRoomClick();
			}
		};
		NetworkModule.onLeftLobby += () => {
			_loadingPanel.SetActive(false);
			_roomPanel.SetActive(false);
		};
		NetworkModule.onCreatedRoom += () => {
			_loadingPanel.SetActive(false);
		};
		NetworkModule.onCreateRoomFailed += (returnCode, message) => {
			_loadingPanel.SetActive(false);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("創建房間失敗");
			sb.AppendLine("cause:");
			sb.AppendLine(message);
			Popup.Create(sb.ToString());
		};
		NetworkModule.onJoinedRoom += () => {
			DatabaseModule.env.autoStart = false;
			_autoStartToggle.isOn = DatabaseModule.env.autoStart;
			_loadingPanel.SetActive(false);
			_roomJoinPanel.SetActive(false);
			_roomReadyPanel.SetActive(true);
			UpdatePlayerList();
		};
		NetworkModule.onJoinRoomFailed += (returnCode, message) => {
			_loadingPanel.SetActive(false);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("加入房間失敗");
			sb.AppendLine("cause:");
			sb.AppendLine(message);
			Popup.Create(sb.ToString());
		};
		NetworkModule.onJoinRandomFailed += (returnCode, message) => {
			_loadingPanel.SetActive(false);
			if(DatabaseModule.env.autoJoin){
				OnCreateRoomClick();
			}
			else{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("隨機加入房間失敗");
				sb.AppendLine("請手動加入房間");
				sb.AppendLine("cause:");
				sb.AppendLine(message);
				Popup.Create(sb.ToString());
			}
		};
		NetworkModule.onLeftRoom += () => {
			_isLeaving = false;
			_loadingPanel.SetActive(false);
			DatabaseModule.env.autoJoin = false;
			DatabaseModule.env.autoStart = false;
			_autoStartToggle.isOn = DatabaseModule.env.autoStart;
			if(NetworkModule.state == NetworkState.InLobby){
				_roomPanel.SetActive(true);
				_roomJoinPanel.SetActive(true);
			}
			else{
				_roomPanel.SetActive(false);
				NetworkModule.JoinLobby();
			}
		};
		NetworkModule.onRoomListUpdate += (roomList) => {
			UpdateRoomList(roomList);
		};
		NetworkModule.onRoomPropertiesUpdate +=
		(propertiesThatChanged) => {
			if(!_isLeaving){
				SwitchRolePanel(NetworkModule.GetRoomProp<bool>("roomStart"));
				CheckGameStart();
			}
		};
		NetworkModule.onPlayerEnteredRoom += (newPlayer) => {
			if(!_isLeaving){
				UpdatePlayerList();
				CheckRoomReady();
			}
		};
		NetworkModule.onPlayerLeftRoom += (otherPlayer) => {
			if(!_isLeaving){
				UpdatePlayerList();
				CheckRoomReady();
				JudgeDissolution();
			}
		};
		NetworkModule.onPlayerPropertiesUpdate +=
		(targetPlayer, changedProps) => {
			if(!_isLeaving){
				UpdatePlayerList();
				CheckRoomReady();
				UpdatePlayerRoleList();
				CheckGameReady();
			}
		};
	}

	private void CheckNetworkState(){
		switch(NetworkModule.state){
			case NetworkState.IsDisconnected:
				NetworkModule.Connect(() => {
					_loadingPanel.SetActive(true);
				});
				return;
			case NetworkState.IsConnected:
				NetworkModule.JoinLobby(() => {
					_loadingPanel.SetActive(true);
				});
				return;
			case NetworkState.InLobby:
				return;
			case NetworkState.InRoom:
				return;
		}
	}

	private void Update(){
		_playerIDText.text = NetworkModule.local.NickName.IsNullOrEmpty() ?
		string.Empty : $"玩家: {NetworkModule.local.NickName}";
	}

	private void BackHome(){
		NetworkModule.Disconnect(() => {
			_loadingPanel.SetActive(true);
		});
	}

	private bool _isLeaving = false;

	private void LeaveRoom(){
		NetworkModule.LeaveRoom(false, () => {
			_isLeaving = true;
			_loadingPanel.SetActive(true);
			_roomPanel.SetActive(false);
			_roomJoinPanel.SetActive(false);
			_roomReadyPanel.SetActive(false);
			_rolePanel.SetActive(false);
			_roleSelectPanel.SetActive(false);
			_rolePlayerPanel.SetActive(false);
		});
	}

	private void UpdateRoomList(List<RoomInfo> roomList){
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("房間列表".ToRichText(s: _roomListText.fontSize + 5));
		sb.AppendLine(
		$"單一房間人數限制: 最少 {DatabaseModule.env.playerMin} 人, 最多 {DatabaseModule.env.playerMax} 人");
		int count = 0;
		foreach(RoomInfo room in roomList){
			if(room.PlayerCount > 0){
				string closed = " [房間已鎖定]".ToRichText(c: "red");
				string state = room.IsOpen ? string.Empty : closed;
				sb.AppendLine($"房間名稱: {room.Name} 當前人數: {room.PlayerCount}{state}");
				count++;
			}
		}
		if(count == 0){
			sb.AppendLine("現在還沒有任何房間 請建立新房間");
		}
		_roomListText.text = sb.ToString();
	}

	private void UpdatePlayerList(){
		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"當前房間: {NetworkModule.room.Name}"
		.ToRichText(s: _playerListText.fontSize + 5));
		sb.AppendLine(
		$"單一房間人數限制: 最少 {DatabaseModule.env.playerMin} 人, 最多 {DatabaseModule.env.playerMax} 人");
		sb.AppendLine("所有玩家準備完成後 房主可開始遊戲");
		int count = 0;
		string g = " [OK]".ToRichText(c: "green");
		string r = " [準備中]".ToRichText(c: "red");
		string b = " [本機]".ToRichText(c: "blue");
		string p = " [房主]".ToRichText(c: "purple");
		foreach(Player player in NetworkModule.players){
			count++;
			bool state = NetworkModule.GetProp<bool>(player, "roomReady");
			string ready = state ? g : r;
			string isLocal = player.IsLocal ? b : string.Empty;
			string isMaster = player.IsMasterClient ? p : string.Empty;
			sb.AppendLine($"玩家 {count}: {player.NickName}{ready}{isMaster}{isLocal}");
		}
		_playerListText.text = sb.ToString();
	}

	private void CheckRoomReady(){
		if(NetworkModule.local.IsMasterClient){
			int readyCount = 0;
			foreach(Player player in NetworkModule.players){
				if(NetworkModule.GetProp<bool>(player, "roomReady")){
					readyCount++;
				}
			}
			bool allReady = readyCount == NetworkModule.players.Count;
			bool playerLimit = readyCount >= DatabaseModule.env.playerMin;
			if(DatabaseModule.env.debugging){
				playerLimit = true;
			}
			_roomStartButton.interactable =  playerLimit && allReady;
			if(DatabaseModule.env.autoStart && !NetworkModule.GetRoomProp<bool>("roomStart")){
				if(_roomStartButton.interactable){
					OnRoomStartClick();
				}
			}
		}
		else{
			_roomStartButton.interactable = false;
		}
	}

	private void SwitchRolePanel(bool open){
		if(open){
			_roomPanel.SetActive(false);
			_rolePanel.SetActive(true);
			_roleSelectPanel.SetActive(true);
			_rolePlayerPanel.SetActive(false);
			ShowSelectedRole();
		}
		else{
			_roomPanel.SetActive(true);
			_rolePanel.SetActive(false);
			_roleSelectPanel.SetActive(false);
			_rolePlayerPanel.SetActive(false);
		}
	}

	private void ShowSelectedRole(){
		StringBuilder sb = new StringBuilder();
		string name = DatabaseModule.roles[_roleSelectIndex].roleName;
		string info = DatabaseModule.roles[_roleSelectIndex].roleInfo;
		sb.AppendLine($"{name}".ToRichText(s: _playerListText.fontSize + 5));
		sb.AppendLine(info);
		_roleListText.text = sb.ToString().Trim();
	}

	private void UpdatePlayerRoleList(){
		string roomName = NetworkModule.room.Name;
		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"當前房間: {roomName}".ToRichText(s: _playerListText.fontSize + 5));
		sb.AppendLine("所有玩家準備完成後 立即開始遊戲");
		int count = 0;
		foreach(Player player in NetworkModule.players){
			count++;
			bool select = NetworkModule.GetProp<bool>(player, "roleSelect");
			RoleType type = NetworkModule.GetProp<RoleType>(player, "roleType");
			string roleName = $" [{type.GetName()}]".ToRichText(c: "green");
			string isChoosing = " [準備中]".ToRichText(c: "red");
			string isLocal = " [本機]".ToRichText(c: "blue");
			sb.AppendLine($"玩家 {count}: {player.NickName}" +
			(select ? roleName : isChoosing) +
			(player.IsLocal ? isLocal : string.Empty));
		}
		_roleWaitText.text = sb.ToString();
	}

	private void JudgeDissolution(){
		if(NetworkModule.GetRoomProp<bool>("roomStart")){
			LeaveRoom();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("房間已解散".ToRichText(c: "red"));
			sb.AppendLine("遊戲中途有玩家離開房間");
			sb.AppendLine("請檢查網路連線狀況");
			Popup.Create(sb.ToString());
		}
	}

	private void CheckGameReady(){
		if(NetworkModule.local.IsMasterClient){
			int readyCount = 0;
			foreach(Player player in NetworkModule.players){
				if(NetworkModule.GetProp<bool>(player, "roleSelect")){
					readyCount++;
				}
			}
			if(readyCount == NetworkModule.players.Count){
				NetworkModule.SetRoomProp("gameStart", true);
			}
		}
	}

	private void CheckGameStart(){
		if(NetworkModule.GetRoomProp<bool>("gameStart")){
			NetworkModule.ResetCallbacks();
			Debug.Log("game start");
			SceneManager.LoadScene("GameScene");
		}
	}

	#region button

	public void OnBackHomeClick(){
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("返回主畫面".ToRichText(c: "red"));
        sb.AppendLine("確定返回主畫面??");
        sb.AppendLine("將與伺服器斷開連線");
        Popup.Create(sb.ToString(), PopupType.Confirm,
		new System.Action[]{ BackHome, null });
	}

	public void OnCreateRoomClick(){
		string roomName = _roomNameText.text;
		string playerName = _playerNameText.text;
		if(roomName.IsNullOrEmpty() || playerName.IsNullOrEmpty()){
			if(DatabaseModule.env.autoJoin){
				if(roomName.IsNullOrEmpty()){
					roomName = StringLib.RandomString(6);
				}
				if(playerName.IsNullOrEmpty()){
					playerName = StringLib.RandomString(12);
				}
			}
			else{
				Debug.Log("room name or player name is empty");
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("房間或玩家名稱為空");
				sb.AppendLine("請輸入房間和玩家名稱");
				Popup.Create(sb.ToString());
				return;
			}
		}
		NetworkModule.SetName(playerName);
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = DatabaseModule.env.playerMax;
		roomOptions.PlayerTtl = 0;
		NetworkModule.CreateRoom(roomName, roomOptions, () => {
			_loadingPanel.SetActive(true);
		});
	}

	public void OnJoinRoomClick(){
		string roomName = _roomNameText.text;
		string playerName = _playerNameText.text;
		if(roomName.IsNullOrEmpty() || playerName.IsNullOrEmpty()){
			Debug.Log("room name or player name is empty");
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("房間或玩家名稱為空");
			sb.AppendLine("請輸入房間和玩家名稱");
			Popup.Create(sb.ToString());
			return;
		}
		NetworkModule.SetName(playerName);
		NetworkModule.JoinRoom(_roomNameText.text, () => {
			_loadingPanel.SetActive(true);
		});
	}

	public void OnJoinRandomRoomClick(){
		DatabaseModule.env.autoJoin = true;
		string playerName = _playerNameText.text;
		if(playerName.IsNullOrEmpty()){
			playerName = StringLib.RandomString(12);
		}
		NetworkModule.SetName(playerName);
		NetworkModule.JoinRandomRoom(() => {
			_loadingPanel.SetActive(true);
		});
	}

	public void OnLeaveRoomClick(){
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("離開房間".ToRichText(c: "red"));
        sb.AppendLine("確定離開房間??");
        Popup.Create(sb.ToString(), PopupType.Confirm,
		new System.Action[]{ LeaveRoom, null });
	}

    public void OnAutoStartValueChanged(bool value){
        DatabaseModule.env.autoStart = value;
        if(DatabaseModule.env.autoStart){
            OnRoomReadyClick();
			CheckRoomReady();
        }
    }

	public void OnRoomReadyClick(){
		bool roomReady = NetworkModule.GetProp<bool>(NetworkModule.local, "roomReady");
		roomReady = DatabaseModule.env.autoStart ? true : !roomReady;
		NetworkModule.SetProp(NetworkModule.local, "roomReady", roomReady);
	}

	public void OnRoomStartClick(){
		NetworkModule.SetRoomProp("roomStart", true);
		NetworkModule.CloseRoom();
	}

	public void OnRoleSelectClick(bool next){
        int temp = _roleSelectIndex;
		temp = next ? temp + 1 : temp - 1;
        if(temp > DatabaseModule.roles.Count - 1){
            _roleSelectIndex = 0;
        }
        else if(temp < 0){
            _roleSelectIndex = DatabaseModule.roles.Count - 1;
        }
        else{
            _roleSelectIndex = temp;
        }
		ShowSelectedRole();
	}

	public void OnRoleOkClick(){
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("角色確認".ToRichText(c: "red"));
        sb.AppendLine("確定選擇此腳色??");
        sb.AppendLine("確認後無法更改");
        Popup.Create(sb.ToString(), PopupType.Confirm,
		new System.Action[]{() => {
		RoleType type = DatabaseModule.roles[_roleSelectIndex].roleType;
		NetworkModule.SetProp(NetworkModule.local, "roleType", type);
		NetworkModule.SetProp(NetworkModule.local, "roleSelect", true);
		_roleSelectPanel.SetActive(false);
		_rolePlayerPanel.SetActive(true);
		}, null });
	}

	#endregion
}
