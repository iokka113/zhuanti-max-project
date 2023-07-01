using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ProjectMax.Unity;

public class Melee : Weapon
{
    [SerializeField]
    private DWield[] _dWields = null;

    [SerializeField]
    private BoxCollider2D[] _colliders = null;

    [SerializeField]
    private LineRenderer[] _colliRens = null;

    [SerializeField]
    private Material _material = null;

    protected override void OnInstantiate(object[] initData){
        base.OnInstantiate(initData);
        if(_dWields.Length == 2){
            if(_config.melee.attackType == WeaponConfig.MeleeConfig.AttackType.DWield){
                _dWields[0].Init(this, _config.melee.dWieldAngle / 2f);
                _dWields[1].Init(this, -_config.melee.dWieldAngle / 2f);
            }
        }
        foreach(BoxCollider2D colli in _colliders){
            colli.enabled = false;
            colli.isTrigger = true;
        }
        foreach(LineRenderer colliRen in _colliRens){
            colliRen.enabled = false;
        }
    }

    protected override void Update(){
        base.Update();
        if(NetworkModule.IsMyView(photonView)){
            if(_config.attackType == WeaponConfig.AttackType.Melee){
                AttackAnime();
            }
        }
    }

    private UnityTimer _atkCdTimer = new UnityTimer();

    protected override void CheckAttack(){
        if(_config.attackType == WeaponConfig.AttackType.Melee){
            _atkCdTimer.Timing(Input.GetKeyDown(KeyCode.Mouse0)
            && _config.general.timeAttack >= _config.melee.timeAnime,
            _config.general.timeAttack, onTimerStart: Attack);
        }
    }

    private float _damage = 0f;

    private Vector3 _atkDir = Vector3.zero;

    private bool _dWieldSwitch = false;

    private bool _attacking = false;

    protected override void Attack(){
        _attacking = true;
        _allowSpriteMove = false;
        _arrivedAnimeTarget = false;
        foreach(BoxCollider2D collider in _colliders){
            collider.enabled = true;
        }
        _damage = _config.general.damageOrignal *
        Random.Range(_config.general.damageMultiplier.x, _config.general.damageMultiplier.y);
        _atkDir = TransLib.cursorPos.ScreenToWorld(CameraControl.main, true) - transform.position;
        _atkDir.Normalize();
        _hasHit.Clear();
        _stabedDis = 0f;
        _slashedAngle = 0f;
        _dWieldSwitch = !_dWieldSwitch;
    }

    private void StopAttack(){
        _attacking = false;
        _allowSpriteMove = true;
        transform.localPosition = _origPos;
        transform.localRotation = _origRot;
        foreach(DWield dWield in _dWields){
            dWield.ResetRotation();
        }
        foreach(BoxCollider2D collider in _colliders){
            collider.enabled = false;
        }
        foreach(LineRenderer colliRen in _colliRens){
            colliRen.enabled = false;
        }
    }

    /// <summary>是否抵達動畫目標(擊出或收回武器)</summary>
    /// <remarks>
    /// 未抵達目標擊出武器<br/>
    /// 抵達目標時收回武器<br/>
    /// 刺擊: 是否突刺到最前距離<br/>
    /// 揮砍: 是否揮出到最大角度<br/>
    /// </remarks>
    private bool _arrivedAnimeTarget = false;

    private void AttackAnime(){
        if(_attacking){
            _allowSpriteMove = false;
            if(DatabaseModule.env.debugging){
                for(int i = 0; i < _colliders.Length; i++){
                    _colliRens[i].enabled = true;
                    _colliders[i].DrawCollider2D(_colliRens[i], _material, Color.white);
                }
            }
            if(_config.melee.attackType == WeaponConfig.MeleeConfig.AttackType.Slash){
                Slash(_config.melee.slashAngle, transform);
            }
            if(_config.melee.attackType == WeaponConfig.MeleeConfig.AttackType.DWield){
                if(_dWieldSwitch){
                    Slash(_config.melee.slashAngle, _dWields[0].transform);
                }
                else{
                    Slash(-_config.melee.slashAngle, _dWields[1].transform);
                }
            }
            if(_config.melee.attackType == WeaponConfig.MeleeConfig.AttackType.Stab){
                Stab();
            }
        }
    }

    /// <summary>已經揮出的無向角度值</summary>
    private float _slashedAngle = 0f;

    private void Slash(float angle, Transform rotated){
        //要揮出的無向最大角度值
        float unsignAngle = Mathf.Abs(angle);
        //計算揮出到最大角度需要多少時間
        float timeToTarget = _config.melee.timeAnime / 2f;
        //計算每幀揮出的有向角度值
        bool stepSign = !(_atkDir.x >= 0 ^ angle >= 0);
        float stepAngle = unsignAngle / timeToTarget * Time.deltaTime;
        float step = stepSign ? +stepAngle : -stepAngle;
        //開始動作
        if(!_arrivedAnimeTarget){
            rotated.rotation *= Quaternion.Euler(0f, 0f, step);
            _slashedAngle += stepAngle;
            if(_slashedAngle > unsignAngle){
                _arrivedAnimeTarget = true;
            }
        }
        else{
            rotated.rotation *= Quaternion.Euler(0f, 0f, -step);
            _slashedAngle -= stepAngle;
            if(_slashedAngle < 0f){
                StopAttack();
            }
        }
    }

    private float _stabedDis = 0f;

    private void Stab(){
        float dis = _config.melee.stabInUnits.ConvertFromUnit();
        float timeToTarget = _config.melee.timeAnime / 2f;
        float step = dis / timeToTarget * Time.deltaTime;
        if(!_arrivedAnimeTarget){
            transform.position += _atkDir * step;
            _stabedDis += step;
            if(_stabedDis > dis){
                _arrivedAnimeTarget = true;
            }
        }
        else{
            transform.position -= _atkDir * step;
            _stabedDis -= step;
            if(_stabedDis < 0f){
                StopAttack();
            }
        }
    }

    private List<PhotonView> _hasHit = new List<PhotonView>();

    public void OnTriggerEnter2D(Collider2D collider){
        if(NetworkModule.IsMyView(photonView)){
            if(_config.attackType == WeaponConfig.AttackType.Melee){
                if(collider.CompareTag("Player")){
                    PhotonView hit = collider.GetComponent<PlayerControl>()?.photonView;
                    if(hit != null && hit != _playerView && !_hasHit.Contains(hit) && !_arrivedAnimeTarget){
                        hit.RPC("ChangeHp", hit.Controller, -_damage, _playerView.Controller.ActorNumber);
                        _hasHit.Add(hit);
                    }
                }
            }
        }
    }

    protected override void CheckSpecial(){
        return;
    }
}
