using UnityEngine;
using UnityEngine.UI;
using ProjectMax.Unity;

[RequireComponent(typeof(Image))]
public class Scope : MonoBehaviour
{
    private static Scope _ins => Singleton<Scope>.instance;

    private Image _image = null;

    private void Start(){
        Singleton<Scope>.SetInstance(this, init: () => {
            _image = GetComponent<Image>();
            _image.enabled = false;
        });
    }

    private float _speed = 0f;

    private Vector3 _lastMousePos = Vector3.zero;

    private void Update(){
        Vector3 dir = (Input.mousePosition - _lastMousePos).normalized;
        float dis = _speed.ConvertFromUnit() * Time.deltaTime;
        CameraControl.focusDefault.position += dir * dis;
        _lastMousePos = Input.mousePosition;
    }

    public static void Enable(float moveSpeed){
        _ins._speed = moveSpeed;
        _ins._lastMousePos = Input.mousePosition;
        _ins._image.enabled = true;
        CameraControl.focusDefault.position = TransLib.cursorPos.ScreenToWorld(CameraControl.main, true);
        CameraControl.focus = CameraControl.focusDefault;
    }

    public static void Disable(){
        _ins._image.enabled = false;
        CameraControl.focus = null;
    }
}
