using UnityEngine;
using UnityEngine.UI;
using ProjectMax.Unity;

public class BloodFade : MonoBehaviour
{
    [SerializeField]
    private Image _image = null;

    private static BloodFade _ins => Singleton<BloodFade>.instance;

    private void Start(){
        Singleton<BloodFade>.SetInstance(this, init: () => {
            _image.color = new Color(
                _image.color.r,
                _image.color.g,
                _image.color.b,
                0f
            );
        });
    }

    public static void Trigger(){
        _ins._image.color = new Color(
            _ins._image.color.r,
            _ins._image.color.g,
            _ins._image.color.b,
            1f
        );
    }

    private void Update(){
        Refresh();
    }

    private void Refresh(){
        if(_image.color.a > 0f){
            _image.color = new Color(
                _image.color.r,
                _image.color.g,
                _image.color.b,
                _image.color.a - Time.deltaTime * 5f
            );
        }
    }
}
