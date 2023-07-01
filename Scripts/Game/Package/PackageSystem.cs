using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectMax.Unity;

[System.Serializable]
public class PackageSystem
{
    private static PackageSystem _ins = null;

    public void Init(){
        if(_ins == null){
            _ins = this;
            buffPackage = new BuffPackage();
            bulletPackage = new BulletPackage();
            _gearGrid.item = DatabaseModule.empty.gearItem;
            foreach(PackageGrid grid in _genericGrids){
                grid.item = DatabaseModule.empty.genericItem;
            }
            WeaponConfig fix = DatabaseModule.game.weaponFixed;
            _genericGrids[0].item = GameLogic.SpawnLocalWeapon(
            fix, rangedCount: fix.ranged.rangedCount);
            SetFocusIndex(0);
        }
    }

    public static void Update(){
        if(PlayerControl.meAlive){
            if(DatabaseModule.env.debugging){
                if(!bulletPackage.HasEnoughCount(10)){
                    bulletPackage.AddCount(20);
                }
            }
            _ins?.CheckGridSwitch();
            _ins?.UpdateInfo();
        }
    }

    [SerializeField]
    private Text _packageInfo = null;

    [SerializeField]
    private Image _switchingBar = null;

    public void UpdateInfo(){
        if(_packageInfo){
            StringBuilder sb = new StringBuilder();
            Ranged ranged = focusGrid.item as Ranged;
            if(ranged){
                if(ranged.loadingTimer.isTiming){
                    sb.AppendLine("彈藥裝填中...".ToRichText(c: "red"));
                }
                sb.AppendLine($"攜帶彈藥: {bulletPackage.count}");
                sb.AppendLine($"武器彈藥: {ranged.rangedCount} / {ranged.config.ranged.rangedCount}");
                if(ranged.rangedCount == 0){
                    sb.AppendLine("按 R 鍵填充彈藥...".ToRichText(c: "orange"));
                }
            }
            else{
                sb.AppendLine($"攜帶彈藥: {bulletPackage.count}");
            }
            sb.AppendLine("按數字鍵切換背包格".ToRichText(c: "cyan", s: _packageInfo.fontSize - 5));
            sb.AppendLine("滑鼠懸停背包格圖示查看資訊".ToRichText(c: "cyan", s: _packageInfo.fontSize - 5));
            sb.AppendLine("點擊當前背包格圖示卸下道具".ToRichText(c: "cyan", s: _packageInfo.fontSize - 5));
            sb.AppendLine("**第一(固定)武器不可卸下**".ToRichText(c: "cyan", s: _packageInfo.fontSize - 5));
            _packageInfo.text = sb.ToString().Trim();
        }
        if(_switchingBar){
            _switchingBar.fillAmount = 1f
            - Time.time.Normalize(_tempSwitchStart, _tempSwitchEnd);
        }
    }

    [SerializeField]
    private PackageGrid _gearGrid = null;

    [SerializeField]
    private List<PackageGrid> _genericGrids = null;

    private static int _focusIndex = 0;

    private static void SetFocusIndex(int index){
        if(index >= 0 && index < _ins._genericGrids.Count){
            _focusIndex = index;
            for(int i = 0; i < _ins._genericGrids.Count; i++){
                PackageGrid grid = _ins._genericGrids[i];
                // Color c = grid.image.color;
                // Color on = new Color(c.r, c.g, c.b, 1f);
                // Color off = new Color(c.r, c.g, c.b, 0.5f);
                // grid.image.color = i == _focusIndex ? on : off;
                Color32 c = grid.background.color;
                Color on = new Color32(255, 255, 128, c.a);
                Color off = new Color32(255, 255, 255, c.a);
                grid.background.color = i == _focusIndex ? on : off;
                IPackageGenericGridItem item = grid.item as IPackageGenericGridItem;
                if(item != null){
                    item.OnSwitchFocus(i == _focusIndex);
                }
            }
        }
    }

    private static PackageGrid focusGrid => _ins._genericGrids[_focusIndex];

    public static IPackageGridItem focusItem => focusGrid.item;

    public static bool focusIsFixed => _focusIndex == 0;

    public static bool focusIsEmpty => focusGrid.item == DatabaseModule.empty.genericItem;

    public static float GetPlayerSpeedMultiplier(){
        IPackageGenericGridItem item = focusGrid.item as IPackageGenericGridItem;
        return item == null ? 1f : item.speedMultiplier;
    }

    private int _tempSwitchIndex;
    private float _tempSwitchStart;
    private float _tempSwitchEnd;

    private UnityTimer _switchTimer = new UnityTimer();
    public static UnityTimer switchTimer => _ins._switchTimer;

    private void CheckGridSwitch(){
        for(int i = 0; i < _genericGrids.Count; i++){
            IPackageGenericGridItem item = focusGrid.item as IPackageGenericGridItem;
            _switchTimer.Timing(_focusIndex != i && Input.GetKeyDown(DatabaseModule.keys.pGeneric[i]),
            item == null ? 0f : item.timeSwitch,
            onTimerStart: () => {
                _tempSwitchIndex = i;
                _tempSwitchStart = Time.time;
                _tempSwitchEnd = Time.time + (item == null ? 0f : item.timeSwitch);
                // Debug.Log(_tempSwitchStart.ToString() + " : " + _tempSwitchEnd.ToString());
                Weapon weapon = focusGrid.item as Weapon;
                if(weapon){
                    weapon.packageLocking = true;
                }
            },
            onTimesUp: () => {
                SetFocusIndex(_tempSwitchIndex);
            });
        }
    }

    public static void PickGridItem(PickableObject pickable){
        if(pickable.pickableType == PickableType.Magzine){
            bulletPackage.AddCount(Random.Range(6, 11));
        }
        if(pickable.pickableType == PickableType.HpJuice){
            PlayerControl.mine.photonView.RPC("ChangeHp", NetworkModule.local, 20f, -1);
        }
        if(pickable.pickableType == PickableType.Weapon){
            if(!switchTimer.isTiming){
                if(!focusIsEmpty){
                    if(!DropGridItem(focusGrid)){
                        return;
                    }
                }
                focusGrid.item = GameLogic.SpawnLocalWeapon(
                pickable.weaponType.GetConfig(), rangedCount: pickable.rangedCount);
                SetFocusIndex(_focusIndex);
            }
        }
        HintText.Remove(pickable);
        NetworkModule.Destroy(pickable.photonView, true);
    }

    public static bool DropGridItem(PackageGrid grid){
        if(!switchTimer.isTiming){
            if(focusGrid == grid && !focusIsFixed){
                Weapon weapon = grid.item as Weapon;
                Ranged ranged = weapon as Ranged;
                if(weapon){
                    object[] data = new object[]{
                        PickableType.Weapon,
                        weapon.config.weaponType,
                        ranged ? ranged.rangedCount : 0
                    };
                    NetworkModule.Instantiate("Prefabs/Pickable",
                    PlayerControl.mine.transform.position + GetDropRange(),
                    Quaternion.identity, true, data: data);
                    NetworkModule.Destroy(weapon.photonView);
                    grid.item = DatabaseModule.empty.genericItem;
                    SetFocusIndex(_focusIndex);
                    return true;
                }
            }
        }
        return false;
    }

    private static Vector3 GetDropRange(){
        float min = DatabaseModule.game.dropRange.x;
        float max = DatabaseModule.game.dropRange.y;
        return new Vector3(Random.Range(min, max), Random.Range(min, max));
    }

    public static BulletPackage bulletPackage { get; private set; }

    public class BulletPackage
    {
        public int count { get; private set; }

        public void AddCount(int add){
            count += add;
        }

        public void CostCount(int cost){
            if(HasEnoughCount(cost)){
                count -= cost;
            }
        }

        public bool HasEnoughCount(int required){
            return count >= required;
        }
    }

    public static BuffPackage buffPackage { get; private set; }

    public class BuffPackage
    {
        //
    }
}

public enum BuffType
{
    Hp,
    Attack,
    Speed,
    View
}
