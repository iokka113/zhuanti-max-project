using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ProjectMax.Unity;

public class EndLogic : MonoBehaviour
{
    private void Start(){
        StartupModule.Init();
        if(NetworkModule.state == NetworkState.InRoom){
            Init();
        }
        else{
            SceneManager.LoadScene("HomeScene");
        }
    }

    [SerializeField]
    private Text _killText = null;

    private void Init(){
        if(_killText){
            StringBuilder sb = new StringBuilder();
            foreach(Player player in NetworkModule.players){
                string name = player.NickName;
                string role = NetworkModule.GetProp<RoleType>(player, "roleType").GetName();
                int count = NetworkModule.GetProp<int>(player, "killCount");
                sb.AppendLine($"{name}({role}) {count} kill");
            }
            _killText.text = sb.ToString().Trim();
        }
    }

    public static void OnEndExitClick(){
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("結束應用程式".ToRichText(c: "red"));
        sb.AppendLine("確定要退出遊戲??");
        Popup.Create(sb.ToString(), PopupType.Confirm,
		new System.Action[]{ Application.Quit, null });
    }
}
