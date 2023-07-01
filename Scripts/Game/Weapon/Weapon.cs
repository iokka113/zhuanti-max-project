using System.Text;
using UnityEngine;
using Photon.Pun;
using ProjectMax.CSharp;
using ProjectMax.Unity;

[RequireComponent(typeof(PhotonTransformView))]
public abstract class Weapon : NetworkPrefab, IPackageGenericGridItem
{
    #region interface

    Sprite IPackageGridItem.icon => config.weaponIcon;

    string IPackageGridItem.info{
        get{
            StringBuilder sb = new StringBuilder();
            string name = $"{config.weaponName}({config.weaponType.Text()}) [{config.attackType.Text()}]";
            sb.AppendLine(name.ToRichText(c: "blue"));
            sb.AppendLine(config.weaponInfo);
            return sb.ToString().Trim();
        }
    }

    public virtual float speedMultiplier => config.general.speedMultiplier;

    float IPackageGenericGridItem.timeSwitch => config.general.timeSwitch;

    void IPackageGenericGridItem.OnSwitchFocus(bool isFocus){
        photonView.RPC("UpdateSprite", RpcTarget.All, isFocus);
        packageLocking = !isFocus;
        if(isFocus){
            CameraControl.size = _config.general.cameraView;
        }
    }

    #endregion

    protected PhotonView _playerView { get; private set; }

    protected Vector3 _origPos { get; private set; }
    protected Quaternion _origRot { get; private set; }

    protected override void OnInstantiate(object[] initData){
        if(initData != null){
            if(initData.TryGetValue(0, out int id)){
                _playerView = NetworkModule.FindView(id);
                transform.SetParent(_playerView.transform);
                _origPos = transform.localPosition;
                _origRot = transform.localRotation;
            }
        }
    }

    [SerializeField]
    protected WeaponConfig _config = null;
    public WeaponConfig config => _config;

    [SerializeField]
    private SpriteRenderer[] _renderers = null;

    public bool packageLocking { get; set; }

    protected bool usable =>
    NetworkModule.IsMyView(photonView) &&
    NetworkModule.IsMyView(_playerView) &&
    PackageSystem.switchTimer.isTiming == false &&
    packageLocking == false && PlayerControl.meAlive;

    protected virtual void Update(){
        if(usable){
            CheckSprite();
            CheckAttack();
            CheckSpecial();
            CheckCharge();
            CheckAiming();
        }
    }

    protected bool _allowSpriteMove = true;

    private void CheckSprite(){
        if(_allowSpriteMove){
            transform.LookAtCursorFlip2D(CameraControl.main);
            // Vector2 rotDir = Input.mousePosition - transform.position.WorldToScreenPoint();
            // float angle = TransLib.CrossingToPositiveX(rotDir.Vector2ToAngle());
            // transform.rotation = Quaternion.Euler(0f, 0f, angle);
            // float x = TransLib.CursorToWorldPoint().x - transform.position.x;
            // photonView.RPC("FlipSprite", RpcTarget.All, x < 0f);
        }
    }

    // [PunRPC]
    // public void FlipSprite(bool flip){
    //     if(_renderers != null){
    //         foreach(SpriteRenderer renderer in _renderers){
    //             renderer.flipX = flip;
    //         }
    //     }
    // }

    [PunRPC]
    public void UpdateSprite(bool enabled){
        if(_renderers != null){
            foreach(SpriteRenderer renderer in _renderers){
                renderer.enabled = enabled;
            }
        }
    }

    protected abstract void Attack();

    protected abstract void CheckAttack();

    protected float _specialBufferLeft = 0f;
    protected float _specialBufferRight = 0f;

    protected abstract void CheckSpecial();

    protected float _chargeMutiple = 0f;

    private void CheckCharge(){
        if(_config.charge.enabled){
            if(Input.GetKey(DatabaseModule.keys.wCharge)
            && _chargeMutiple < _config.charge.chargeMultiplier.y){
                _chargeMutiple += _config.charge.chargeStep * Time.deltaTime;
            }
            _chargeMutiple = Mathf.Clamp(_chargeMutiple,
            _config.charge.chargeMultiplier.x, _config.charge.chargeMultiplier.y);
            // Debug.Log(_chargeMutiple);
        }
    }

    private void CheckAiming(){
        if(_config.aiming.enabled){
            if(Input.GetKeyDown(DatabaseModule.keys.wAiming)){
                Scope.Enable(_config.aiming.sensitivity);
            }
            if(Input.GetKeyUp(DatabaseModule.keys.wAiming)){
                Scope.Disable();
            }
        }
    }
}
