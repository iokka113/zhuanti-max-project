using UnityEngine;
using ProjectMax.Unity;

public class Lens : MonoBehaviour
{
    private static Lens _ins => Singleton<Lens>.instance;

    private void Start(){
        Singleton<Lens>.SetInstance(this);
        gameObject.SetActive(false);
    }

    private void Update(){
        CameraControl.size += Input.mouseScrollDelta.y * 0.1f;
        Vector3 dir = TransLib.axisDir;
        IPackageGenericGridItem empty = DatabaseModule.empty.genericItem;
        float dis = empty.speedMultiplier.ConvertFromUnit();
        CameraControl.focusDefault.position += dir * dis * Time.deltaTime;
    }

    public static void Enable(Vector3 playerPos){
        _ins.gameObject.SetActive(true);
        CameraControl.focusDefault.position = playerPos;
        CameraControl.focus = CameraControl.focusDefault;
    }

    public static void Disable(){
        CameraControl.focus = null;
        _ins.gameObject.SetActive(false);
    }
}
