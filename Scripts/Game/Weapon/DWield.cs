using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PhotonTransformView))]
public class DWield : MonoBehaviour
{
    private Melee _melee = null;

    private Quaternion _origRot = Quaternion.identity;

    public void Init(Melee melee, float angle){
        _melee = melee;
        transform.localRotation = _origRot = Quaternion.Euler(0f, 0f, angle);
    }

    public void ResetRotation(){
        transform.localRotation = _origRot;
    }

    private void OnTriggerEnter2D(Collider2D collider){
        _melee?.OnTriggerEnter2D(collider);
    }
}
