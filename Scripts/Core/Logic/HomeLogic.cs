using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectMax.Unity;

public class HomeLogic : MonoBehaviour
{
    [SerializeField]
    private GameObject _menuPanel = null;

    [SerializeField]
    private GameObject _aboutPanel = null;

    private void Start(){
		StartupModule.Init();
        _menuPanel.SetActive(true);
        _aboutPanel.SetActive(false);
    }

    public static void OnHomeStartClick(){
        SceneManager.LoadScene("LoginScene");
    }

    public static void OnHomeExitClick(){
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("結束應用程式".ToRichText(c: "red"));
        sb.AppendLine("確定要退出遊戲??");
        Popup.Create(sb.ToString(), PopupType.Confirm,
		new System.Action[]{ Application.Quit, null });
    }

    public void OnAboutClick(bool open = true){
        _menuPanel.SetActive(!open);
        _aboutPanel.SetActive(open);
    }
}
