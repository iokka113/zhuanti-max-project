using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectMax.Unity;

public class HintText : MonoBehaviour
{
    private static HintText _ins => Singleton<HintText>.instance;

    private void Start(){
        if(_text){
            _text.text = string.Empty;
        }
        Singleton<HintText>.SetInstance(this);
    }

    [SerializeField]
    private Text _text = null;

    public static int fontSize => _ins._text.fontSize;

    private static List<IShowHintText> _show = new List<IShowHintText>();

    public static void Add(IShowHintText show){
        _show.Add(show);
        _ins?.Refresh();
    }

    public static void Remove(IShowHintText show){
        _show.Remove(show);
        _ins?.Refresh();
    }

    public void Refresh(){
        if(_text){
            foreach(IShowHintText obj in _show){
                if(obj != null){
                    _text.text = obj.GetHintText();
                    return;
                }
            }
            _text.text = string.Empty;
        }
    }
}

public interface IShowHintText
{
    string GetHintText();
}
