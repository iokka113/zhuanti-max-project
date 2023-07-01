using UnityEngine;
using Photon.Pun;
using ProjectMax.CSharp;
using ProjectMax.Unity;

public class Ranged : Weapon
{
    public override float speedMultiplier{
        get{
            if(loadingTimer.isTiming){
                return config.ranged.loadingSpeed;
            }
            return config.general.speedMultiplier;
        }
    }

    private NonRepeating _shotRandomCount = null;

    protected override void OnInstantiate(object[] initData){
        base.OnInstantiate(initData);
        if(initData != null){
            if(initData.TryGetValue(1, out int rangedCount)){
                this.rangedCount = rangedCount;
            }
        }
        if(_config.ranged.attackType == WeaponConfig.RangedConfig.AttackType.Shot){
            if(NetworkModule.IsMyView(_playerView)){
                int count = _config.ranged.shotCount.y - _config.ranged.shotCount.x + 1;
                _shotRandomCount = new NonRepeating(_config.ranged.shotCount.x, count);
            }
        }
    }

    protected override void Update(){
        base.Update();
        if(usable){
            CheckLoad();
        }
    }

    private UnityTimer _atkCdTimer = new UnityTimer();

    protected override void CheckAttack(){
        if(_config.attackType == WeaponConfig.AttackType.Ranged){
            _atkCdTimer.Timing(Input.GetKeyDown(KeyCode.Mouse0)
            && rangedCount > 0 && loadingTimer.isTiming == false,
            _specialHandgun ? 0.1f : _config.general.timeAttack, onTimerStart: Attack);
        }
    }

    [SerializeField]
    private Transform _bulletSpawn = null;

    protected override void Attack(){
        rangedCount--;
        string path = $"Weapons/{_config.ranged.rangedPrefab.name}";
        Vector3 dir = TransLib.cursorPos.ScreenToWorld(CameraControl.main, true) - _bulletSpawn.position;
        if(_config.ranged.attackType == WeaponConfig.RangedConfig.AttackType.Shot){
            Shoot(path, dir, _shotRandomCount.next);
        }
        if(_config.ranged.attackType == WeaponConfig.RangedConfig.AttackType.Single){
            Shoot(path, dir, 1);
        }
    }

    private void Shoot(string path, Vector3 dir, int count){
        while(count > 0){
            count--;
            Vector3 newDir = TransLib.RandomOffset2D(dir, _config.ranged.normalOffset).normalized;
            object[] data = new object[]{
                _playerView.ViewID,
                transform.localScale,
                newDir,
                _config.ranged.rangedSpeed,
                _config.ranged.rangeInUnits,
                _config.general.damageOrignal *
                Random.Range(_config.general.damageMultiplier.x, _config.general.damageMultiplier.y),
                _specialHandgun
            };
            NetworkModule.Instantiate(path, _bulletSpawn.position, _bulletSpawn.rotation, true, data: data);
        }
    }

    private int _rangedCount = 0;
    public int rangedCount{
        get{
            return _rangedCount;
        }
        private set{
            photonView.RPC("UpdateRangedCount", RpcTarget.All, value);
        }
    }

    [PunRPC]
    public void UpdateRangedCount(int value){
        _rangedCount = value;
    }

    private int NeedLoadCount{
        get{
            int min = PackageSystem.bulletPackage.count;
            int max = _config.ranged.rangedCount - rangedCount;
            return PackageSystem.bulletPackage.HasEnoughCount(max) ? max : min;
        }
    }

    public UnityTimer loadingTimer { get; private set; } = new UnityTimer();

    private void CheckLoad(){
        if(_config.attackType == WeaponConfig.AttackType.Ranged){
            loadingTimer.Timing(Input.GetKeyDown(DatabaseModule.keys.wLoad)
            && NeedLoadCount > 0, _config.ranged.timeLoad, onTimesUp: Load);
        }
    }

    private void Load(){
        int count = NeedLoadCount;
        rangedCount += count;
        PackageSystem.bulletPackage.CostCount(count);
    }

    private bool _specialHandgun = false;

    protected override void CheckSpecial(){
        if(Input.GetKey(KeyCode.Mouse0)){
            _specialBufferLeft += Time.deltaTime;
        }
        if(Input.GetKeyUp(KeyCode.Mouse0)){
            _specialBufferLeft = 0f;
        }
        if(Input.GetKey(KeyCode.Mouse1)){
            _specialBufferRight += Time.deltaTime;
        }
        if(Input.GetKeyUp(KeyCode.Mouse1)){
            _specialBufferRight = 0f;
        }
        if(_config.weaponType == WeaponType.Handgun){
            if(Input.GetKeyDown(KeyCode.Mouse1)){
                _specialHandgun = !_specialHandgun;
                Debug.Log("set handgun special " + _specialHandgun);
            }
        }
    }
}
