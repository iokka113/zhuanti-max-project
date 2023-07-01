using UnityEngine;
using UnityEngine.UI;
using ProjectMax.Unity;

public class Tooltip : MonoBehaviour
{
    [SerializeField]
    private Camera _camera = null;

    [SerializeField]
    private RectTransform _canvas = null;

    [SerializeField]
    private RectTransform _tooltip = null;

    [SerializeField]
    private RectTransform _background = null;

    [SerializeField]
    private Text _text = null;

    [SerializeField]
    private int _textSize = 15;

    public static int textSize => _ins._textSize;

    [SerializeField]
    private float _paddingSize = 5f;

    private static Tooltip _ins => Singleton<Tooltip>.instance;

    private void Start(){
        Singleton<Tooltip>.SetInstance(this, init: () => {
            gameObject.SetActive(false);
        });
    }

    private void Update(){
        if(RectTransformUtility.ScreenPointToWorldPointInRectangle(
        _canvas, Input.mousePosition, _camera, out Vector3 point)){
            _tooltip.position = point;
        }
    }

    public static void Enable(string info){
        _ins.gameObject.SetActive(true);
        _ins._text.text = info;
        _ins._text.fontSize = _ins._textSize;
        float x = _ins._text.preferredWidth + _ins._paddingSize * 2f;
        float y = _ins._text.preferredHeight + _ins._paddingSize * 2f;
        _ins._background.sizeDelta = new Vector2(x, y);
    }

    public static void Disable(){
        _ins.gameObject.SetActive(false);
    }
}
