using UnityEngine;

[CreateAssetMenu(fileName = "RoleConfig")]
public class RoleConfig : ScriptableObject
{
    [SerializeField]
    [Tooltip("角色類型")]
    private RoleType _roleType = default;
    public RoleType roleType => _roleType;

    [SerializeField]
    [Tooltip("角色預製件")]
    private GameObject _rolePrefab = null;
    public GameObject rolePrefab => _rolePrefab;

    [SerializeField]
    [Tooltip("角色圖示")]
    private Sprite _roleIcon = null;
    public Sprite roleIcon => _roleIcon;

    [SerializeField]
    [Tooltip("角色名稱")]
    private string _roleName = null;
    public string roleName => _roleName;

    [SerializeField]
    [Tooltip("角色說明")]
    [TextArea(10, 50)]
    private string _roleInfo = null;
    public string roleInfo => _roleInfo;
}

public static class RoleExtension
{
    public static string GetName(this RoleType type){
        foreach(RoleConfig roleConfig in DatabaseModule.roles){
            if(type == roleConfig.roleType){
                return roleConfig.roleName;
            }
        }
        Debug.LogWarning($"資料庫參照遺失: {nameof(DatabaseModule.roles)}");
        return string.Empty;
    }
}

public enum RoleType
{
    Nameless, //無名
    Ryuujin, //龍人
}
