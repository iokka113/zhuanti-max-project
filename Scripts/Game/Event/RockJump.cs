using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RockJump : MonoBehaviour, IShowHintText
{
    [SerializeField]
    private RockJump _nextJumpPoint = null;

    private bool _myPlayerEntered = false;

    private void OnTriggerEnter2D(Collider2D collider){
        if(collider.GetComponent<PlayerControl>() == PlayerControl.mine){
            if(PlayerControl.meAlive){
                _myPlayerEntered = true;
                HintText.Add(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider){
        if(collider.GetComponent<PlayerControl>() == PlayerControl.mine){
            if(PlayerControl.meAlive){
                _myPlayerEntered = false;
                HintText.Remove(this);
            }
        }
    }

    private void Start(){
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Update(){
        if(PlayerControl.meAlive && _myPlayerEntered){
            if(Input.GetKeyDown(DatabaseModule.keys.jump)){
                PlayerControl.mine.Jump(_nextJumpPoint);
                HintText.Remove(this);
            }
        }
    }

    string IShowHintText.GetHintText(){
        return "按空白鍵跳到另一個岩石上";
    }
}
