using UnityEngine;
using UnityEngine.UI;
using ProjectMax.Unity;

public class DaysFade : MonoBehaviour
{
    [SerializeField]
    private Image _image = null;

    [SerializeField]
    private RectTransform _mask = null;

    private static DaysFade _ins => Singleton<DaysFade>.instance;

    private void Start(){
        Singleton<DaysFade>.SetInstance(this);
    }

    private void Update(){
        Refresh();
    }

    public static void Trigger(){
        if(!_ins._isOn){
            _ins._isOn = true;
        }
    }

    private bool _isOn = false;

    private UnityTimer _timer = new UnityTimer();

	/// <summary>0f = Noon, 1f = Midnight</summary>
    private float _value = 0f;

    private bool _dir = true;

    private void Refresh(){
        _timer.Timing(_isOn, 1f / DatabaseModule.game.fadeRate, onTimesUp: () => {
            float step = 1f / DatabaseModule.game.fadeRate / DatabaseModule.game.fadeTime;
            _value = _dir ? _value + step : _value - step;
            _value = Mathf.Clamp(_value, 0f, 1f);
            if(_value == 0f || _value == 1f){
                // Debug.Log(_dir ? "midnight" : "noon");
                _isOn = false;
                _dir =  !_dir;
            }
            Display();
        });
    }

    private void Display(){
        if(_image){
            _value = Mathf.Clamp(_value, 0f, 1f);
            float inverse = 1f - _value;
            _image.color = new Color(inverse, inverse, inverse, _value);
            float scale = inverse.Denormalize(DatabaseModule.game.fadeScale.x, DatabaseModule.game.fadeScale.y);
            _mask.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
