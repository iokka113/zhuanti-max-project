using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ProjectMax.CSharp;
using ProjectMax.Unity;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PhotonTransformView))]
public class PlayerControl : NetworkPrefab, ISoundEffectPlay
{
    public static List<PlayerControl> all => _all;
    private static List<PlayerControl> _all = new List<PlayerControl>();

    public static PlayerControl mine { get; private set; }

    public bool isDummy { get; private set; } = true;

    private Rigidbody2D _rb = null;

    [SerializeField]
    private Text _nameText = null;

    protected override void OnInstantiate(object[] initData){
        if(initData != null){
            if(initData.TryGetValue(0, out bool dummy)){
                isDummy = dummy;
            }
        }
        _rb = GetComponent<Rigidbody2D>();
        _rb.Sleep();
        if(isDummy){
            if(_nameText){
                _nameText.text = $"(假人){StringLib.n}DebugDummy".ToRichText(c: "orange");
            }
        }
        else{
            _all.Add(this);
            if(NetworkModule.IsMyView(photonView)){
                mine = this;
                _rb.WakeUp();
            }
            if(_nameText){
                string name = photonView.Controller.NickName;
                string role = NetworkModule.GetProp<RoleType>(
                photonView.Controller, "roleType").GetName();
                _nameText.text = $"({role}){StringLib.n}{name}".ToRichText(
                c: NetworkModule.IsMyView(photonView) ? "green" : "red");
            }
        }
        audio = GetComponent<AudioSource>();
    }

    [SerializeField]
    private Image _hpBar = null;

    public float hp { get; private set; } = 100f;

    public float hpMax { get; private set; } = 100f;

    public static bool meAlive => mine ? mine.hp > 0f : false;

    /// <summary>同步更新血量(對所有客戶端調用)</summary>
    [PunRPC]
    public void UpdateHp(float newHp, float newHpMax){
        hpMax = newHpMax > 0f ? newHpMax : hpMax;
        hp = Mathf.Clamp(newHp, 0f, hpMax);
        if(_hpBar){
            _hpBar.fillAmount = hp.Normalize(0f, hpMax);
        }
    }

    private float _lastAttackTime = 0f;
    private int _lastAttackerID = -1;

    /// <summary>請求更改血量(受到攻擊或回血)</summary>
    /// <param name = "attackerID">發動攻擊的玩家ID, 補血或場景傷害(岩漿.安全區)則傳入 -1</param>
    /// <remarks>
    /// 僅對單一客戶端(玩家控制者)調用此方法<br/>
    /// **對所有客戶端調用將會導致血量被重複修改**<br/>
    /// </remarks>
    [PunRPC]
    public void ChangeHp(float add, int attackerID){
        if(NetworkModule.IsMyView(photonView)){
            float newHp = Mathf.Clamp(hp + add, 0f, hpMax);
            if(add < 0f){
                if(attackerID != -1){
                    _lastAttackerID = attackerID;
                }
                if(newHp > 0f){
                    if(this == mine){
                        BloodFade.Trigger();
                    }
                }
                else{
                    if(DatabaseModule.env.debugging && DatabaseModule.game.autoReborn){
                        newHp = hpMax;
                    }
                    else{
                        Die(attackerID);
                    }
                }
            }
            photonView.RPC("UpdateHp", RpcTarget.All, newHp, hpMax);
        }
    }

    private bool _dieLock = false;

    public void Die(int attackerID){
        if(NetworkModule.IsMyView(photonView)){
            if(!_dieLock){
                _dieLock = true;
                AddKillCount(attackerID);
                photonView.RPC("Active", RpcTarget.All, false);
                if(this == mine){
                    Weapon weapon = PackageSystem.focusItem as Weapon;
                    if(weapon){
                        weapon.photonView.RPC("UpdateSprite", RpcTarget.All, false);
                    }
                    if(NetworkModule.GetRoomProp<bool>("safeShow")){
                        Lens.Enable(transform.position);
                    }
                }
                NetworkModule.Instantiate("Prefabs/Grave",
                transform.position, Quaternion.identity, true);
            }
        }
    }

    private void AddKillCount(int attackerID){
        if(NetworkModule.IsMyView(photonView)){
            if(attackerID == -1 && _lastAttackerID != -1){
                attackerID = _lastAttackerID;
            }
            Player killer = NetworkModule.FindPlayer(attackerID);
            if(killer != null){
                string myName = photonView.Controller.NickName;
                string myRole = NetworkModule.GetProp<RoleType>(
                photonView.Controller, "roleType").GetName();
                string kName = killer.NickName;
                string kRole = NetworkModule.GetProp<RoleType>(killer, "roleType").GetName();
                Debug.Log($"{myName}({myRole}) killed by {kName}({kRole})");
                int count = NetworkModule.GetProp<int>(killer, "killCount");
                NetworkModule.SetProp(killer, "killCount", count + 1);
            }
        }
    }

    [SerializeField]
    private Transform _head = null;

    [SerializeField]
    private SpriteRenderer _headImg = null;

    [SerializeField]
    private SpriteRenderer _bodyImg = null;

    private UnityTimer _lavaTimer = new UnityTimer();
    private bool _isInLava = false;
    public bool isInLava{
        set{
            _isInLava = value;
            _lavaTimer.Reset();
        }
    }

    private UnityTimer _dummyTimer = new UnityTimer();
    private Vector3 _dummyDir = new Vector3();

    private void Update(){
        if(NetworkModule.IsMyView(photonView)){
            if(isDummy){
                if(DatabaseModule.game.dummyMove){
                    _dummyTimer.Timing(true, Random.Range(5f, 10f), onTimerStart: () => {
                        _dummyDir = TransLib.RandomDir2D;
                    });
                    Move();
                }
            }
            if(this == mine && hp > 0f){
                Move();
            }
            if(Time.time > _lastAttackTime + DatabaseModule.game.atkTime){
                _lastAttackerID = -1;
            }
            CheckReborn();
        }
    }

    private void Move(){
        float flipX = 0f;
        if(isDummy){
            float dis = DatabaseModule.game.dummySpeed.ConvertFromUnit();
            transform.position += _dummyDir * dis * Time.deltaTime;
            flipX = _dummyDir.x;
        }
        else{
            Vector3 dir = TransLib.axisDir;
            float dis = PackageSystem.GetPlayerSpeedMultiplier().ConvertFromUnit();
            // _rb.MovePosition(transform.position + dir * dis * Time.deltaTime);
            transform.position += dir * dis * Time.deltaTime;
            // float x = TransLib.CursorToWorldPoint().x - transform.position.x;
            // _head.LookAtCursorFlip2D();
            // _body.FlipScaleAxis(TransLib.Axis.X, x);
            Vector2 rotDir = TransLib.cursorPos
            - transform.position.WorldToScreen(CameraControl.main, true);
            float angle = TransLib.CrossingToPositiveX(rotDir.Vector2ToAngle());
            _head.rotation = Quaternion.Euler(0f, 0f, angle);
            flipX = TransLib.cursorPos.ScreenToWorld(CameraControl.main, true).x
            - transform.position.x;
        }
        photonView.RPC("FlipSprite", RpcTarget.All, flipX < 0f);
        _lavaTimer.Timing(_isInLava, DatabaseModule.game.lavaTime, onTimerStart: () => {
            photonView.RPC("ChangeHp", mine.photonView.Controller,
            -DatabaseModule.game.lavaDamage, -1);
        });
    }

    [PunRPC]
    public void FlipSprite(bool flip){
        _headImg.flipX = flip;
        _bodyImg.flipX = flip;
    }

    [SerializeField]
    private List<GameObject> _activeList;

    [SerializeField]
    private Transform _weaponSpawn = null;
    public Transform weaponSpawn => _weaponSpawn;

    [PunRPC]
    public void Active(bool value){
        foreach(GameObject obj in _activeList){
            obj.SetActive(value);
        }
    }

    private UnityTimer _rebornTimer = new UnityTimer();

    private void CheckReborn(){
        _rebornTimer.Timing(_dieLock && !NetworkModule.GetRoomProp<bool>("safeShow"),
        DatabaseModule.game.reTime, onTimesUp: Reborn, onTimerStart: () => {
            if(this == mine){
                GameLogic.RebornPanel(true, Time.time);
            }
        });
    }

    private void Reborn(){
        _dieLock = false;
        _lastAttackerID = -1;
        transform.position = GameLogic.GetPlayerSpawnPos();
        photonView.RPC("Active", RpcTarget.All, true);
        photonView.RPC("UpdateHp", RpcTarget.All, hpMax, hpMax);
        if(this == mine){
            GameLogic.RebornPanel(false);
            Weapon weapon = PackageSystem.focusItem as Weapon;
            if(weapon){
                weapon.photonView.RPC("UpdateSprite", RpcTarget.All, true);
            }
        }
    }

    public void Jump(RockJump toJump){
        transform.position = toJump.transform.position;
    }

    public new AudioSource audio { get; set; }

    [PunRPC]
    public void PlayAudio(int audioIndex){
        AudioClip a = audioIndex.GetAudioClip();
        if(a){
            audio.PlayOneShot(a);
        }
    }
}
