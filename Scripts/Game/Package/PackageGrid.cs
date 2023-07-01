using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PointerHandler))]
public class PackageGrid : MonoBehaviour
{
    public Image image = null;
    public Image background = null;

    private IPackageGridItem _item = null;
    public IPackageGridItem item{
        get{
            return _item;
        }
        set{
            _item = value;
            if(_item != null){
                image.sprite = _item.icon;
            }
        }
    }

    private PointerHandler _ph = null;

    private void Start(){
        _ph = GetComponent<PointerHandler>();
        _ph.onPointerExit += (eventData) => {
            Tooltip.Disable();
        };
        _ph.onPointerStay += () => {
            if(item != null){
                Tooltip.Enable(item.info);
                if(Input.GetKeyDown(DatabaseModule.keys.pDrop)){
                    PackageSystem.DropGridItem(this);
                }
            }
        };
    }
}

public interface IPackageGridItem
{
    Sprite icon { get; }

    string info { get; }
}

public interface IPackageGearGridItem : IPackageGridItem
{
    //
}

public interface IPackageGenericGridItem : IPackageGridItem
{
    float speedMultiplier { get; }

    float timeSwitch { get; }

    void OnSwitchFocus(bool isFocus);
}
