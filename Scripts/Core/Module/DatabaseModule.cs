using System.Collections.Generic;
using UnityEngine;
using ProjectMax.Unity;

public class DatabaseModule : MonoBehaviour
{
    public static DatabaseModule ins => Singleton<DatabaseModule>.instance;

    private void Start(){
        Singleton<DatabaseModule>.SetInstance(this, true, Init);
    }

    private void Init(){
        AudioListener.volume = env.mute ? 0f : 1f;
        QualitySettings.vSyncCount = env.vSync ? 1 : 0;
    }

    private void Update(){
        CheckSettings();
    }

    private void CheckSettings(){
        if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)){
            if(Input.GetKeyDown(KeyCode.M)){
                env.mute = !env.mute;
                AudioListener.volume = env.mute ? 0f : 1f;
            }
            if(Input.GetKeyDown(KeyCode.V)){
                env.vSync = !env.vSync;
                QualitySettings.vSyncCount = env.vSync ? 1 : 0;
            }
            if(Input.GetKeyDown(KeyCode.Backspace)){
                env.debugging = !env.debugging;
            }
            if(Input.GetKeyDown(KeyCode.F)){
                game.autoReborn = !game.autoReborn;
            }
            if(Input.GetKeyDown(KeyCode.D)){
                game.dummyMove = !game.dummyMove;
            }
        }
    }

    [SerializeField]
    [Tooltip("環境配置")]
    private Environment _environment = null;
    public static Environment env => ins?._environment;

    [SerializeField]
    [Tooltip("主場景配置")]
    private GameSettings _gameSettings = null;
    public static GameSettings game => ins?._gameSettings;

    [SerializeField]
    [Tooltip("按鍵配置")]
    private Keybindings _keybindings = null;
    public static Keybindings keys => ins?._keybindings;

    [SerializeField]
    [Tooltip("音效配置")]
    private SoundEffects _soundEffects = null;
    public static SoundEffects sounds => ins?._soundEffects;

    [SerializeField]
    private List<RoleConfig> _roleConfigs = null;
    public static List<RoleConfig> roles => ins?._roleConfigs;

    [SerializeField]
    private List<WeaponConfig> _weaponConfigs = null;
    public static List<WeaponConfig> weapons => ins?._weaponConfigs;

    [SerializeField]
    [Tooltip("空白背包項目")]
    private EmptyPackageItems _emptyPackageItems = null;
    public static EmptyPackageItems empty => ins?._emptyPackageItems;

    [System.Serializable]
    public class Environment
    {
        [Tooltip("靜音")]
        public bool mute = false;

        [Tooltip("垂直同步")]
        public bool vSync = true;

        [Tooltip("區域網路連線")]
        public bool onLAN = false;

        [Tooltip("除錯模式")]
        public bool debugging = false;

        /// <summary>自動加入房間</summary>
        public bool autoJoin { get; set; }

        /// <summary>自動開始遊戲</summary>
        public bool autoStart { get; set; }

        [SerializeField]
        [Tooltip("房間玩家數量下限")]
        private byte _playerCountMin = 2;
        public byte playerMin => _playerCountMin;

        [SerializeField]
        [Tooltip("房間玩家數量上限")]
        private byte _playerCountMax = 4;
        public byte playerMax => _playerCountMax;
    }

    [System.Serializable]
    public class GameSettings
    {
        [SerializeField]
        [Tooltip("全域距離單位")]
        private float _globalUnit = 1f;
        public float unit => _globalUnit;

        [SerializeField]
        [Tooltip("玩家隨機生成點")]
        private List<Vector2> _playerSpawnPositions = null;
        public List<Vector2> playerPos => _playerSpawnPositions;

        [SerializeField]
        [Tooltip("玩家隨機生成範圍(X:最小值,Y:最大值)")]
        private Vector2 _playerSpawnRange = new Vector2(-1f, 1f);
        public Vector2 playerRange => _playerSpawnRange;

        [SerializeField]
        [Tooltip("背包項目隨機丟棄範圍(X:最小值,Y:最大值)")]
        private Vector2 _itemDropRange = new Vector2(-0.5f, 0.5f);
        public Vector2 dropRange => _itemDropRange;

        [SerializeField]
        [Tooltip("玩家初始(固定)武器")]
        private WeaponConfig _weaponFixed = null;
        public WeaponConfig weaponFixed => _weaponFixed;

        [Tooltip("允許自動復活(除錯模式)")]
        public bool autoReborn = true;

        [Tooltip("允許假人移動(除錯模式)")]
        public bool dummyMove = false;

        [SerializeField]
        [Tooltip("假人移動速度(除錯模式)")]
        private float _dummySpeed = 1f;
        public float dummySpeed => _dummySpeed;

        [SerializeField]
        [Tooltip("彈匣圖示")]
        private Sprite _spriteMagzine = null;
        public Sprite sMagzine => _spriteMagzine;

        [SerializeField]
        [Tooltip("血瓶圖示")]
        private Sprite _spriteHpJuice = null;
        public Sprite sJuice => _spriteHpJuice;

        [SerializeField]
        [Tooltip("預設相機大小")]
        private float _cameraSize = 5f;
        public float cSize => _cameraSize;

        [SerializeField]
        [Tooltip("預設相機位置")]
        private Vector3 _cameraPosition = new Vector3(0f, 0f, -10f);
        public Vector3 cOffset => _cameraPosition;

        [SerializeField]
        [Tooltip("地圖原點(左下角座標)")]
        private Vector2 _mapOrigin = Vector2.zero;
        public Vector2 mapOrig => _mapOrigin;

        [SerializeField]
        [Tooltip("地圖格大小(X:水平,Y:垂直)")]
        private Vector2 _mapGridSize = new Vector2(1f, 1f);
        public Vector2 mapSize => _mapGridSize;

        [SerializeField]
        [Tooltip("地圖格數量(X:水平,Y:垂直)")]
        private Vector2Int _mapGridCount = new Vector2Int(10, 10);
        public Vector2Int mapCount => _mapGridCount;

        [SerializeField]
        [Tooltip("白天持續時間(秒)")]
        private float _morningTime = 20f;
        public float morningTime => _morningTime;

        [SerializeField]
        [Tooltip("晚上持續時間(秒)")]
        private float _nightTime = 10f;
        public float nightTime => _nightTime;

        [SerializeField]
        [Tooltip("日夜更新頻率(次/秒)")]
        private float _daysRate = 5f;
        public float daysRate => _daysRate;

        [SerializeField]
        [Tooltip("日夜淡入淡出時間(秒)")]
        private float _daysFadeTime = 1f;
        public float fadeTime => _daysFadeTime;

        [SerializeField]
        [Tooltip("日夜遮罩更新頻率(次/秒)")]
        private float _daysFadeRate = 100f;
        public float fadeRate => _daysFadeRate;

        [SerializeField]
        [Tooltip("日夜遮罩縮放係數(X:最小值,Y:最大值)")]
        private Vector2 _daysFadeScale = new Vector2(1f, 2f);
        public Vector2 fadeScale => _daysFadeScale;

        [SerializeField]
        [Tooltip("安全區顯示時間(秒)")]
        private float _safeZoneShowTime = 70f;
        public float safeShow => _safeZoneShowTime;

        [SerializeField]
        [Tooltip("安全區開啟時間(秒)")]
        private float _safeZoneOpenTime = 90f;
        public float safeOpen => _safeZoneOpenTime;

        [SerializeField]
        [Tooltip("安全區關閉時間(秒)")]
        private float _safeZoneCloseTime = 120f;
        public float safeClose => _safeZoneCloseTime;

        [SerializeField]
        [Tooltip("安全區外的傷害數值")]
        private float _safeZoneDamage = 5f;
        public float safeDamage => _safeZoneDamage;

        [SerializeField]
        [Tooltip("安全區外的傷害判定時間(秒)")]
        private float _safeZoneDamageTime = 0.5f;
        public float safeTime => _safeZoneDamageTime;

        [SerializeField]
        [Tooltip("岩漿傷害數值")]
        private float _lavaDamage = 5f;
        public float lavaDamage => _lavaDamage;

        [SerializeField]
        [Tooltip("岩漿傷害判定時間(秒)")]
        private float _lavaDamageTime = 0.5f;
        public float lavaTime => _lavaDamageTime;

        [SerializeField]
        [Tooltip("保留最後攻擊者資料時間(秒)")]
        private float _lastAttackingTime = 5f;
        public float atkTime => _lastAttackingTime;

        [SerializeField]
        [Tooltip("等待復活時間(秒)")]
        private float _rebornWaitingTime = 5f;
        public float reTime => _rebornWaitingTime;

        [SerializeField]
        [Tooltip("遊戲結束判定延時(秒)")]
        private float _gameOverOvertime = 5f;
        public float overtime => _gameOverOvertime;
    }

    [System.Serializable]
    public class Keybindings
    {
        [SerializeField]
        [Tooltip("武器裝彈按鍵")]
        private KeyCode _weaponLoad = KeyCode.R;
        public KeyCode wLoad => _weaponLoad;

        [SerializeField]
        [Tooltip("武器蓄力按鍵")]
        private KeyCode _weaponCharge = KeyCode.Space;
        public KeyCode wCharge => _weaponCharge;

        [SerializeField]
        [Tooltip("武器瞄準按鍵")]
        private KeyCode _weaponAiming = KeyCode.Space;
        public KeyCode wAiming => _weaponAiming;

        [SerializeField]
        [Tooltip("背包裝備格按鍵")]
        private KeyCode _packageGear = KeyCode.F;
        public KeyCode pGear => _packageGear;

        [SerializeField]
        private KeyCode[] _packageGeneric = new KeyCode[]{
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
        };
        public KeyCode[] pGeneric => _packageGeneric;

        [SerializeField]
        [Tooltip("背包項目丟棄按鍵")]
        private KeyCode _packageDrop = KeyCode.Mouse0;
        public KeyCode pDrop => _packageDrop;

        [SerializeField]
        [Tooltip("背包項目撿起按鍵")]
        private KeyCode _packagePick = KeyCode.Space;
        public KeyCode pPick => _packagePick;

        [SerializeField]
        [Tooltip("跳躍按鍵")]
        private KeyCode _rockJump = KeyCode.Space;
        public KeyCode jump => _rockJump;
    }

    [System.Serializable]
    public class SoundEffects
    {
        [SerializeField]
        [Tooltip("安全區顯示音效")]
        private AudioClip _safeShow = null;
        public AudioClip safeShow => _safeShow;
    }

    [System.Serializable]
    public abstract class EmptyItem : IPackageGridItem
    {
        [SerializeField]
        [Tooltip("背包格圖示")]
        private Sprite _gridIcon = null;
        Sprite IPackageGridItem.icon => _gridIcon;

        [SerializeField]
        [Tooltip("背包格顯示資訊")]
        private string _gridInfo = null;
        string IPackageGridItem.info => _gridInfo.ToRichText(c: "red");
    }

    [System.Serializable]
    public class EmptyGearItem : EmptyItem, IPackageGearGridItem
    {
        //
    }

    [System.Serializable]
    public class EmptyGenericItem : EmptyItem, IPackageGenericGridItem
    {
        [SerializeField]
        [Tooltip("玩家移動速度倍率")]
        private float _speedMultiplier = 1f;
        float IPackageGenericGridItem.speedMultiplier => _speedMultiplier;

        [SerializeField]
        [Tooltip("背包格預設切換時間")]
        private float _timeSwitch = 1f;
        float IPackageGenericGridItem.timeSwitch => _timeSwitch;

        void IPackageGenericGridItem.OnSwitchFocus(bool isFocus){
            if(isFocus){
                CameraControl.ResetSize();
            }
        }
    }

    [System.Serializable]
    public class EmptyPackageItems
    {
        [SerializeField]
        [Tooltip("空白裝備格")]
        private EmptyGearItem _gearGrid = null;
        public EmptyGearItem gearItem => _gearGrid;

        [SerializeField]
        [Tooltip("空白通用格")]
        private EmptyGenericItem _genericGrid = null;
        public EmptyGenericItem genericItem => _genericGrid;
    }
}

public static class GlobalExtension
{
    public static float ConvertFromUnit(this float value){
        if(DatabaseModule.game == null){
            Debug.LogWarning($"資料庫參照遺失: {nameof(DatabaseModule.game)}");
            return value;
        }
        return value * DatabaseModule.game.unit;
    }

    public static float ConvertToUnit(this float value){
        if(DatabaseModule.game == null){
            Debug.LogWarning($"資料庫參照遺失: {nameof(DatabaseModule.game)}");
            return value;
        }
        return value / DatabaseModule.game.unit;
    }
}

public static class AudioExtension
{
    public static AudioClip GetAudioClip(this int index){
        switch(index){
            case 0:
                return DatabaseModule.sounds.safeShow;
            default:
                Debug.LogWarning($"資料庫參照遺失: {nameof(DatabaseModule.sounds)}");
                return null;
        }
    }
}

public interface ISoundEffectPlay
{
    AudioSource audio { get; set; }

    void PlayAudio(int audioIndex);
}
