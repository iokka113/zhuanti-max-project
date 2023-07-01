using UnityEngine;

public class Lava : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider){
        if(collider.CompareTag("Player")){
            PlayerControl player = collider.GetComponent<PlayerControl>();
            if(NetworkModule.IsMyView(player.photonView)){
                player.isInLava = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider){
        if(collider.CompareTag("Player")){
            PlayerControl player = collider.GetComponent<PlayerControl>();
            if(NetworkModule.IsMyView(player.photonView)){
                player.isInLava = false;
            }
        }
    }
}
