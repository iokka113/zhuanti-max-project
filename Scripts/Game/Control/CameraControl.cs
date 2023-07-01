using UnityEngine;
using ProjectMax.Unity;

public class CameraControl : MonoBehaviour
{
    private static CameraControl _ins => Singleton<CameraControl>.instance;

    private static Camera _main = null;
    public static Camera main => _main ? _main : Camera.main;

    public static Transform focus { get; set; }
    public static Transform focusDefault { get; private set; }

    public static float size{
        get => main.orthographicSize;
        set => main.orthographicSize = Mathf.Clamp(value, 1f, 10f);
    }

    private void Start(){
        Singleton<CameraControl>.SetInstance(this, init: Init);
    }

    private void Init(){
        _main = Camera.main;
        focusDefault = new GameObject("FocusTarget").transform;
    }

    private void LateUpdate(){
        if(focus){
            main.transform.position = DatabaseModule.game.cOffset
            + focus.position;
            return;
        }
        if(PlayerControl.mine){
            main.transform.position = DatabaseModule.game.cOffset
            + PlayerControl.mine.transform.position;
        }
    }

    public static void ResetSize(){
        size = DatabaseModule.game.cSize;
    }
}
