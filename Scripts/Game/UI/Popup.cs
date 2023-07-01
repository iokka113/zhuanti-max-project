using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    [SerializeField]
    private Text _text = null;

    [SerializeField]
    private GameObject[] _buttonSet = null;

    private System.Action[] _eventArray = null;

    public static void Create(
        string text,
        PopupType type = PopupType.Notice,
        System.Action[] onEachButtonClick = null){
            GameObject obj = Resources.Load("Prefabs/Popup") as GameObject;
            Popup popup = GameObject.Instantiate(obj).GetComponent<Popup>();
            popup._text.text = text;
            popup._eventArray = onEachButtonClick;
            foreach(GameObject b in popup._buttonSet){
                b.SetActive(false);
            }
            popup._buttonSet[(int)type]?.SetActive(true);
    }

    public void OnButtonClick(int buttonIndex){
        _eventArray?[buttonIndex]?.Invoke();
        GameObject.Destroy(this.gameObject);
    }
}

public enum PopupType
{
    Notice,
    Confirm
}
