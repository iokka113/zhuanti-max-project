using UnityEngine;

[CreateAssetMenu(fileName = "WeaponConfig")]
public class WeaponConfig : ScriptableObject
{
    public enum AttackType { Untyped, Melee, Ranged }

    [SerializeField]
    [Tooltip("攻擊類型")]
    private AttackType _attackType = AttackType.Untyped;
    public AttackType attackType => _attackType;

    [SerializeField]
    [Tooltip("武器類型")]
    private WeaponType _weaponType = default;
    public WeaponType weaponType => _weaponType;

    [SerializeField]
    [Tooltip("武器預製件")]
    private GameObject _weaponPrefab = null;
    public GameObject weaponPrefab => _weaponPrefab;

    [SerializeField]
    [Tooltip("武器圖示")]
    private Sprite _weaponIcon = null;
    public Sprite weaponIcon => _weaponIcon;

    [SerializeField]
    [Tooltip("武器名稱")]
    private string _weaponName = null;
    public string weaponName => _weaponName;

    [SerializeField]
    [Tooltip("武器說明")]
    [TextArea(10, 50)]
    private string _weaponInfo = null;
    public string weaponInfo => _weaponInfo;

    [SerializeField]
    [Tooltip("通用參數")]
    private GeneralConfig _general = null;
    public GeneralConfig general => _general;

    [SerializeField]
    [Tooltip("近戰參數")]
    private MeleeConfig _melee = null;
    public MeleeConfig melee => _melee;

    [SerializeField]
    [Tooltip("遠程參數")]
    private RangedConfig _ranged = null;
    public RangedConfig ranged => _ranged;

    [SerializeField]
    [Tooltip("蓄力參數")]
    private ChargeConfig _charge = null;
    public ChargeConfig charge => _charge;

    [SerializeField]
    [Tooltip("瞄準參數")]
    private AimingConfig _aiming = null;
    public AimingConfig aiming => _aiming;

    [System.Serializable]
    public class GeneralConfig
    {
        [SerializeField]
        [Tooltip("武器切換速度(秒)")]
        private float _timeSwitch = 1f;
        public float timeSwitch => _timeSwitch;

        [SerializeField]
        [Tooltip("武器攻擊速度(秒)")]
        private float _timeAttack = 1f;
        public float timeAttack => _timeAttack;

        [SerializeField]
        [Tooltip("原始傷害數值")]
        private float _damageOrignal = 10f;
        public float damageOrignal => _damageOrignal;

        [SerializeField]
        [Tooltip("傷害數值倍率(X:最小值,Y:最大值)")]
        private Vector2 _damageMultiplier = Vector2.up;
        public Vector2 damageMultiplier => _damageMultiplier;

        [SerializeField]
        [Tooltip("玩家移動速度倍率")]
        private float _speedMultiplier = 1f;
        public float speedMultiplier => _speedMultiplier;

        [SerializeField]
        [Tooltip("攝影機視野大小")]
        private float _cameraView = 5f;
        public float cameraView => _cameraView;
    }

    [System.Serializable]
    public class MeleeConfig
    {
        public enum AttackType
        {
            Stab, //刺擊
            Slash, //揮砍
            DWield //雙持
        }

        [SerializeField]
        [Tooltip("攻擊類型")]
        private AttackType _attackType = default;
        public AttackType attackType => _attackType;

        [SerializeField]
        [Tooltip("刺擊距離(單位)")]
        private float _stabInUnits = 1f;
        public float stabInUnits => _stabInUnits;

        [SerializeField]
        [Tooltip("揮砍角度(°)")]
        private float _slashAngle = 90f;
        public float slashAngle => _slashAngle;

        [SerializeField]
        [Tooltip("攻擊動畫時間(秒)")]
        private float _timeAnime = 0.5f;
        public float timeAnime => _timeAnime;

        [SerializeField]
        [Tooltip("雙持武器角度(°)")]
        private float _dWieldAngle = 60f;
        public float dWieldAngle => _dWieldAngle;
    }

    [System.Serializable]
    public class RangedConfig
    {
        public enum AttackType
        {
            Single, //單發
            Shot //霰彈
        }

        [SerializeField]
        [Tooltip("攻擊類型")]
        private AttackType _attackType = default;
        public AttackType attackType => _attackType;

        [SerializeField]
        [Tooltip("彈藥預製件")]
        private GameObject _rangedPrefab = null;
        public GameObject rangedPrefab => _rangedPrefab;

        [SerializeField]
        [Tooltip("彈藥數量(充填一次可擊發次數)")]
        private int _rangedCount = 5;
        public int rangedCount => _rangedCount;

        [SerializeField]
        [Tooltip("彈藥速度(單位/秒)")]
        private float _rangedSpeed = 10f;
        public float rangedSpeed => _rangedSpeed;

        [SerializeField]
        [Tooltip("彈藥射程(單位)")]
        private float _rangeInUnits = 10f;
        public float rangeInUnits => _rangeInUnits;

        [SerializeField]
        [Tooltip("彈道法線偏移(°)")]
        private float _normalOffset = 0f;
        public float normalOffset => _normalOffset;

        [SerializeField]
        [Tooltip("裝彈時間(秒)")]
        private float _timeLoad = 0f;
        public float timeLoad => _timeLoad;

        [SerializeField]
        [Tooltip("裝彈時移動速度倍率")]
        private float _speedMultiplierLoading = 0.5f;
        public float loadingSpeed => _speedMultiplierLoading;

        [SerializeField]
        [Tooltip("霰彈隨機數量(X:最小值,Y:最大值)")]
        private Vector2Int _randomShotCount = new Vector2Int(6, 10);
        public Vector2Int shotCount => _randomShotCount;
    }

    [System.Serializable]
    public class ChargeConfig
    {
        [SerializeField]
        [Tooltip("啟用")]
        private bool _enabled = false;
        public bool enabled => _enabled;

        [SerializeField]
        [Tooltip("蓄力倍率(X:最小值,Y:最大值)")]
        private Vector2 _chargeMultiplier = Vector2.up;
        public Vector2 chargeMultiplier => _chargeMultiplier;

        [SerializeField]
        [Tooltip("每秒蓄力倍率步進")]
        private float _chargeStep = 0.2f;
        public float chargeStep => _chargeStep;
    }

    [System.Serializable]
    public class AimingConfig
    {
        [SerializeField]
        [Tooltip("啟用")]
        private bool _enabled = false;
        public bool enabled => _enabled;

        [SerializeField]
        [Tooltip("瞄準時視野大小")]
        private float _aimingView = 10f;
        public float aimingView => _aimingView;

        [SerializeField]
        [Tooltip("視野移動靈敏度(單位/秒)")]
        private float _sensitivity = 5f;
        public float sensitivity => _sensitivity;
    }
}

public static class WeaponExtension
{
    public static WeaponConfig GetConfig(this WeaponType type){
        foreach(WeaponConfig weaponConfig in DatabaseModule.weapons){
            if(type == weaponConfig.weaponType){
                return weaponConfig;
            }
        }
        Debug.LogWarning($"資料庫參照遺失: {nameof(DatabaseModule.weapons)}");
        return null;
    }

    public static string Text(this WeaponType type){
        switch(type){
            case WeaponType.Dagger:
                return "匕首";
            case WeaponType.Claws:
                return "雙爪";
            case WeaponType.TBatons:
                return "雙拐";
            case WeaponType.Katana:
                return "武士刀";
            case WeaponType.DualBlades:
                return "雙刀";
            case WeaponType.Chain:
                return "鎖鏈";
            case WeaponType.Handgun:
                return "手槍";
            case WeaponType.Catapult:
                return "彈射器";
            case WeaponType.Crossbow:
                return "弩箭";
            case WeaponType.Shotgun:
                return "霰彈槍";
            case WeaponType.Artillery:
                return "火砲";
            default:
                return string.Empty;
        }
    }

    public static string Text(this WeaponConfig.AttackType type){
        switch(type){
            case WeaponConfig.AttackType.Melee:
                return "近戰";
            case WeaponConfig.AttackType.Ranged:
                return "遠程";
            default:
                return string.Empty;
        }
    }
}

public enum WeaponType
{
    Dagger, //匕首
    Claws, //雙爪
    TBatons, //雙拐
    Katana, //武士刀
    DualBlades, //雙刀
    Chain, //鎖鏈
    Handgun, //手槍
    Catapult, //彈射器
    Crossbow, //弩箭
    Shotgun, //霰彈槍
    Artillery, //火砲
}
