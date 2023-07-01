using System.Text;
using System.Collections.Generic;
using UnityEngine;
using ProjectMax.CSharp;
using ProjectMax.Unity;

[RequireComponent(typeof(CircleCollider2D))]
public class PickableObject : NetworkPrefab, IShowHintText
{
    [SerializeField]
    private SpriteRenderer _icon = null;

    private PickableType _pickableType = default;
    public PickableType pickableType => _pickableType;

    private WeaponType _weaponType = default;
    public WeaponType weaponType => _weaponType;

    private int _rangedCount = 0;
    public int rangedCount => _rangedCount;

    public static List<PickableObject> all { get; private set; }
    = new List<PickableObject>();

    public Vector2Int mapGridIndex { get; private set; }
    = -Vector2Int.one;

    protected override void OnInstantiate(object[] initData){
        if(initData != null){
            all.Add(this);
            mapGridIndex = MapZone.Locate(transform.position);
            initData.TryGetValue(0, out _pickableType);
            initData.TryGetValue(1, out _weaponType);
            initData.TryGetValue(2, out _rangedCount);
            if(_pickableType == PickableType.Weapon){
                _icon.sprite = _weaponType.GetConfig().weaponIcon;
            }
            if(_pickableType == PickableType.Magzine){
                _icon.sprite = DatabaseModule.game.sMagzine;
            }
            if(_pickableType == PickableType.HpJuice){
                _icon.sprite = DatabaseModule.game.sJuice;
            }
        }
    }

    private bool _myPlayerEntered = false;

    private void OnTriggerEnter2D(Collider2D collider){
        if(collider.GetComponent<PlayerControl>() == PlayerControl.mine){
            if(PlayerControl.meAlive){
                _myPlayerEntered = true;
                HintText.Add(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider){
        if(collider.GetComponent<PlayerControl>() == PlayerControl.mine){
            if(PlayerControl.meAlive){
                _myPlayerEntered = false;
                HintText.Remove(this);
            }
        }
    }

    private void Update(){
        if(PlayerControl.meAlive && _myPlayerEntered){
            if(Input.GetKeyDown(DatabaseModule.keys.pPick)){
                PackageSystem.PickGridItem(this);
            }
        }
    }

    private void OnDestroy(){
        HintText.Remove(this);
        all.Remove(this);
    }

    string IShowHintText.GetHintText(){
        StringBuilder sb = new StringBuilder();
        if(_pickableType == PickableType.Weapon){
            if(PackageSystem.focusIsEmpty){
                sb.AppendLine("按空白鍵撿取武器");
                sb.AppendLine("或切換至其他背包格(數字鍵)以撿取武器".ToRichText(s: HintText.fontSize - 10));
            }
            else{
                if(PackageSystem.focusIsFixed){
                    sb.AppendLine("第一(固定)武器不可卸下");
                    sb.AppendLine("切換至空白背包格(數字鍵)以撿取武器".ToRichText(s: HintText.fontSize - 10));
                }
                else{
                    sb.AppendLine("按空白鍵撿取武器");
                    sb.AppendLine("背包格不為空(將卸下當前道具)".ToRichText(s: HintText.fontSize - 10));
                    sb.AppendLine("切換至空白背包格(數字鍵)以撿取武器".ToRichText(s: HintText.fontSize - 10));
                    sb.AppendLine("或點擊當前背包格圖示卸下道具".ToRichText(s: HintText.fontSize - 10));
                }
            }
        }
        if(_pickableType == PickableType.Magzine){
            sb.AppendLine("按空白鍵撿取彈匣");
            sb.AppendLine("將隨機補充 6~10 發彈藥數量".ToRichText(s: HintText.fontSize - 5));
        }
        if(_pickableType == PickableType.HpJuice){
            sb.AppendLine("按空白鍵飲用能量果汁");
            sb.AppendLine("將補充 20 點 HP".ToRichText(s: HintText.fontSize - 5));
        }
        return sb.ToString().Trim();
    }
}

public enum PickableType
{
    None,
    Weapon,
    Magzine,
    HpJuice
}
