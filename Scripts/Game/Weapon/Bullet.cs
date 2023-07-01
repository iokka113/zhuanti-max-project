using UnityEngine;
using Photon.Pun;
using ProjectMax.CSharp;
using ProjectMax.Unity;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PhotonTransformView))]
public class Bullet : NetworkPrefab
{
    private PhotonView _playerView = null;
    private Rigidbody2D _rb = null;
    private Vector3 _origPos = Vector3.zero;
    private Vector3 _dir = Vector3.zero;
    private float _speed = 0f;
    private float _range = 0f;
    private float _damage = 0f;

    private bool _handgunSpecial = false;
    private UnityTimer _handgunTimer = new UnityTimer();

    protected override void OnInstantiate(object[] initData){
        _rb = GetComponent<Rigidbody2D>();
        _rb.Sleep();
        _origPos = transform.position;
        if(initData != null){
            if(initData.TryGetValue(0, out int id)){
                _playerView = NetworkModule.FindView(id);
            }
            if(initData.TryGetValue(1, out Vector3 scale)){
                transform.localScale = scale;
            }
            initData.TryGetValue(2, out _dir);
            initData.TryGetValue(3, out _speed);
            initData.TryGetValue(4, out _range);
            initData.TryGetValue(5, out _damage);
            initData.TryGetValue(6, out _handgunSpecial);
            float angle = TransLib.Vector2ToAngle(_dir).CrossingToPositiveX();
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider){
        if(NetworkModule.local.IsMasterClient){
            if(collider.CompareTag("Player")){
                PhotonView hit = collider.GetComponent<PlayerControl>()?.photonView;
                if(hit != null && hit != _playerView){
                    hit.RPC("ChangeHp", hit.Controller, -_damage, _playerView.Controller.ActorNumber);
                    NetworkModule.Destroy(photonView);
                }
            }
            if(collider.gameObject.layer == LayerMask.NameToLayer("Wall")){
                NetworkModule.Destroy(photonView);
            }
        }
    }

    private void Update(){
        if(NetworkModule.local.IsMasterClient){
            if(_rb.IsSleeping()){
                _rb.WakeUp();
            }
            if(!_handgunStaying){
                Move();
            }
            _handgunTimer.Timing(_handgunStaying, 5f, onTimesUp: () => {
                NetworkModule.Destroy(photonView);
            });
        }
        else{
            if(!_rb.IsSleeping()){
                _rb.Sleep();
            }
        }
    }

    private bool _handgunStaying = false;

    private void Move(){
        float dis = _speed.ConvertFromUnit() * Time.deltaTime;
        // _rb.MovePosition(transform.position + _dir * dis);
        transform.position +=  _dir * dis;
        float d = Vector3.Distance(transform.position, _origPos).ConvertToUnit();
        if(d > _range){
            if(_handgunSpecial){
                _handgunStaying = true;
            }
            else{
                NetworkModule.Destroy(photonView);
            }
        }
    }
}
