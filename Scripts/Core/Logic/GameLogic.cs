using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ProjectMax.Unity;

public class GameLogic : MonoBehaviour
{
    [SerializeField]
    private GameObject _mainPanel = null;

    [SerializeField]
    private Text _infoText = null;

    [SerializeField]
    private Text _daysText = null;

    [SerializeField]
    private Text _rebornText = null;

    [SerializeField]
    private GameObject _rebornPanel = null;

    [SerializeField]
    private PackageSystem _packageSystem = null;

    private static GameLogic _ins => Singleton<GameLogic>.instance;

    private void Start(){
        StartupModule.Init();
        if(NetworkModule.state == NetworkState.InRoom){
            Singleton<GameLogic>.SetInstance(this);
            Init();
        }
        else{
            SceneManager.LoadScene("HomeScene");
        }
    }

    private void Init(){
        MapZone.Init();
        SpawnMasterRpc();
        SpawnLocalPlayer(false);
        _packageSystem.Init();
        _rebornPanel.SetActive(false);
        NetworkModule.onRoomPropertiesUpdate += (propertiesThatChanged) => {
            UpdateDays();
            if(NetworkModule.GetRoomProp<bool>("gameOver")){
                // if(DatabaseModule.env.debugging){
                //     return;
                // }
                NetworkModule.ResetCallbacks();
                SceneManager.LoadScene("EndScene");
            }
        };
        if(NetworkModule.local.IsMasterClient){
            SpawnRoomPickable();
            if(DatabaseModule.env.debugging){
                if(NetworkModule.players.Count == 1){
                    SpawnLocalPlayer(true, DatabaseModule.env.playerMax - 1);
                }
            }
        }
        NetworkModule.SetProp(NetworkModule.local, "gameInit", true);
    }

    private void Update(){
        PrintInfo();
        PackageSystem.Update();
        _mainPanel.SetActive(PlayerControl.meAlive);
        if(NetworkModule.local.IsMasterClient){
            SetDays();
            CheckOver();
        }
        StringBuilder sb = new StringBuilder();
        float time = _rebornWaiting + DatabaseModule.game.reTime - Time.time;
        if(_rebornText){
            sb.AppendLine($"等待玩家復活");
            sb.AppendLine($"剩餘 {time:00} 秒".ToRichText(s: _rebornText.fontSize - 20));
            _rebornText.text = sb.ToString().Trim();
        }
    }

    private static string _pink = "#f88192ff";

    private void PrintInfo(){
        if(_infoText){
            StringBuilder sb = new StringBuilder();
            string role = NetworkModule.GetProp<RoleType>(NetworkModule.local, "roleType").GetName();
            sb.AppendLine($"玩家: {NetworkModule.local.NickName}({role})".ToRichText(c: "yellow"));
            string vsync = DatabaseModule.env.vSync ? "啟用".ToRichText(c: "lime") : "禁用".ToRichText(c: _pink);
            string mute = DatabaseModule.env.mute ? "啟用".ToRichText(c: "lime") : "禁用".ToRichText(c: _pink);
            sb.AppendLine($"靜音模式: {mute} [Alt+M]");
            sb.AppendLine($"垂直同步: {vsync} [Alt+V]");
            sb.AppendLine($"網路延遲: {NetworkModule.ping} ms");
            if(DatabaseModule.env.debugging){
                sb.AppendLine("已啟用除錯模式... [Alt+Backspace]".ToRichText(c: "yellow"));
                sb.AppendLine("> 攜帶彈藥數量過少時自動補充".ToRichText(c: "orange"));
                sb.AppendLine("> 渲染所有可視化武器碰撞框".ToRichText(c: "orange"));
                string reborn = DatabaseModule.game.autoReborn ?
                "補滿".ToRichText(c: "lime") : "死亡".ToRichText(c: _pink);
                sb.AppendLine($"> 當血量歸零時: {reborn} [Alt+F]".ToRichText(c: "orange"));
                if(NetworkModule.players.Count == 1){
                    sb.AppendLine("已啟用單人模式...".ToRichText(c: "yellow"));
                    string move = DatabaseModule.game.dummyMove ?
                    "允許移動".ToRichText(c: "lime") : "停止移動".ToRichText(c: _pink);
                    sb.AppendLine($"> 假人移動模式: {move} [Alt+D]".ToRichText(c: "orange"));
                }
            }
            _infoText.text = sb.ToString();
        }
    }

    public static Vector3 GetPlayerSpawnPos(){
        float min = DatabaseModule.game.playerRange.x;
        float max = DatabaseModule.game.playerRange.y;
        Vector2 range = new Vector2(Random.Range(min, max), Random.Range(min, max));
        Vector2 pos = DatabaseModule.game.playerPos[Random.Range(0, DatabaseModule.game.playerPos.Count)];
        return pos + range;
    }

    private static void SpawnMasterRpc(){
        NetworkModule.Instantiate("Prefabs/MasterRpc", Vector3.zero, Quaternion.identity);
    }

    /// <param name = "count">isDummy 為非時 count 不可用</param>
    private static void SpawnLocalPlayer(bool isDummy, int count = 1){
        count = isDummy ? count : 1;
        while(count > 0){
            object[] data = new object[]{ isDummy };
            NetworkModule.Instantiate("Prefabs/Player",
            GetPlayerSpawnPos(), Quaternion.identity, data: data);
            count--;
        }
    }

    public static Weapon SpawnLocalWeapon(WeaponConfig config, int rangedCount = 0){
        string path = $"Weapons/{config.weaponPrefab.name}";
        Vector3 pos = PlayerControl.mine.weaponSpawn.position;
        Quaternion rot = PlayerControl.mine.weaponSpawn.rotation;
        object[] data = new object[]{ PlayerControl.mine.photonView.ViewID, rangedCount };
        GameObject obj = NetworkModule.Instantiate(path, pos, rot, data: data);
        return obj.GetComponent<Weapon>();
    }

    private static void SpawnRoomPickable(){
        int idx = 0;
        foreach(Vector2 pivot in MapZone.pivots){
            idx = idx >= DatabaseModule.weapons.Count ? 0 : idx;
            SpawnRoomPickableSub(pivot, PickableType.Weapon, DatabaseModule.weapons[idx]);
            SpawnRoomPickableSub(pivot, PickableType.Magzine);
            SpawnRoomPickableSub(pivot, PickableType.HpJuice);
            idx++;
        }
    }

    private static void SpawnRoomPickableSub(Vector2 pivot, PickableType type, WeaponConfig config = null){
        object[] data = new object[]{
            type,
            config?.weaponType,
            config?.ranged.rangedCount
        };
        float mapX = pivot.x + Random.Range(0f, DatabaseModule.game.mapSize.x);
        float mapY = pivot.y + Random.Range(0f, DatabaseModule.game.mapSize.y);
        NetworkModule.Instantiate("Prefabs/Pickable",
        new Vector2(mapX, mapY), Quaternion.identity, true, data: data);
    }

    private UnityTimer _daysTimer = new UnityTimer();

    private void SetDays(){
        _daysTimer.Timing(true, 1f / DatabaseModule.game.daysRate, onTimesUp: () => {
            float time = NetworkModule.GetRoomProp<float>("daysTime");
            time += 1f / DatabaseModule.game.daysRate;
            float all = DatabaseModule.game.morningTime + DatabaseModule.game.nightTime;
            bool isNight = time % all - DatabaseModule.game.morningTime > 0f ? true : false;
            NetworkModule.SetRoomProp("daysTime", time);
            NetworkModule.SetRoomProp("isNight", isNight);
            NetworkModule.SetRoomProp("safeShow", time > DatabaseModule.game.safeShow);
            NetworkModule.SetRoomProp("safeOpen", time > DatabaseModule.game.safeOpen);
            NetworkModule.SetRoomProp("safeClose", time > DatabaseModule.game.safeClose);
        });
    }

    private bool _isNight = false;

    private void UpdateDays(){
        bool isNight = NetworkModule.GetRoomProp<bool>("isNight");
        if(_isNight != isNight){
            _isNight = isNight;
            DaysFade.Trigger();
        }
        float time = NetworkModule.GetRoomProp<float>("daysTime");
        float all = DatabaseModule.game.morningTime + DatabaseModule.game.nightTime;
        int date = (int)(time / all) + 1;
        StringBuilder sb = new StringBuilder();
        if(_isNight){
            int cd = Mathf.RoundToInt(all - time % all);
            sb.AppendLine($"第{date}天 黑夜");
            sb.AppendLine($"{(cd / 60).ToString("D2")}:{(cd % 60).ToString("D2")}");
            _daysText.text = sb.ToString().Trim().ToRichText(c: "white");
        }
        else{
            int cd = Mathf.RoundToInt(DatabaseModule.game.morningTime - time % all);
            sb.AppendLine($"第{date}天 白晝");
            sb.AppendLine($"{(cd / 60).ToString("D2")}:{(cd % 60).ToString("D2")}");
            _daysText.text = sb.ToString().Trim().ToRichText(c: "black");
        }
    }

    private bool _overLock = false;

    private void CheckOver(){
        float overTime = NetworkModule.GetRoomProp<float>("overTime");
        if(overTime == 0f){
            bool clientsInit = NetworkModule.GetProp<bool>(NetworkModule.local, "gameInit");
            foreach(Player player in NetworkModule.players){
                clientsInit &= NetworkModule.GetProp<bool>(player, "gameInit");
            }
            if(clientsInit && NetworkModule.GetRoomProp<bool>("safeOpen")){
                int aliveCount = 0;
                foreach(PlayerControl player in PlayerControl.all){
                    if(player != null && player.hp > 0f){
                        aliveCount++;
                    }
                }
                if(aliveCount <= 1){
                    NetworkModule.SetRoomProp("overTime", Time.time);
                }
            }
        }
        else{
            if(Time.time > overTime + DatabaseModule.game.overtime && !_overLock){
                _overLock = true;
                Debug.Log("game over");
                NetworkModule.SetRoomProp("gameOver", true);
            }
        }
    }

    private float _rebornWaiting = 0f;

    public static void RebornPanel(bool enabled, float startWaitingTime = 0f){
        if(_ins._rebornText){
            _ins._rebornPanel.SetActive(enabled);
            _ins._rebornWaiting = startWaitingTime;
        }
    }

    public static void OnBuffClick(int buffType){
        Debug.Log(buffType);
    }
}
