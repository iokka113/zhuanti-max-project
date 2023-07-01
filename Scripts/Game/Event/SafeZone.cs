using UnityEngine;
using UnityEngine.UI;
using ProjectMax.Unity;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class SafeZone : MonoBehaviour, ISoundEffectPlay
{
    private BoxCollider2D _colli = null;
    private LineRenderer _ren = null;

    [SerializeField]
    private Material _material = null;

    private void Start(){
        audio = GetComponent<AudioSource>();
        _colli = GetComponent<BoxCollider2D>();
        _ren = GetComponent<LineRenderer>();
        _ren.enabled = false;
        _arrowImage.enabled = false;
    }

    private bool _playerEnter = false;

    private void OnTriggerEnter2D(Collider2D collider){
        if(collider.gameObject == PlayerControl.mine.gameObject){
            _playerEnter = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider){
        if(collider.gameObject == PlayerControl.mine.gameObject){
            _playerEnter = false;
        }
    }

    [SerializeField]
    private Image _arrowImage = null;

    [SerializeField]
    private RectTransform _canvasRect = null;

    private UnityTimer _damageTimer = new UnityTimer();

    [SerializeField]
    private Color _lineColor = Color.red;

    [SerializeField]
    private float _lineWidth = 0.1f;

    private bool _showAudioLock = false;

    private void Update(){
        if(NetworkModule.room != null){
            if(NetworkModule.GetRoomProp<bool>("safeClose")){
                _ren.enabled = false;
                _colli.enabled = false;
                _arrowImage.enabled = false;
                _damageTimer.Timing(true, DatabaseModule.game.safeTime, onTimesUp: () => {
                    PlayerControl.mine.photonView.RPC("ChangeHp", Photon.Pun.RpcTarget.All,
                    -DatabaseModule.game.safeDamage, -1);
                });
            }
            else if(NetworkModule.GetRoomProp<bool>("safeOpen")){
                _ren.enabled = true;
                _colli.enabled = true;
                _colli.DrawCollider2D(_ren, _material, _lineColor, _lineWidth);
                if(_playerEnter){
                    _arrowImage.enabled = false;
                }
                else{
                    _arrowImage.enabled = true;
                    UpdateArrow(transform.position, _arrowImage, _canvasRect);
                }
                _damageTimer.Timing(!_playerEnter, DatabaseModule.game.safeTime, onTimesUp: () => {
                    PlayerControl.mine.photonView.RPC("ChangeHp", Photon.Pun.RpcTarget.All,
                    -DatabaseModule.game.safeDamage, -1);
                });
            }
            else if(NetworkModule.GetRoomProp<bool>("safeShow")){
                if(!_showAudioLock){
                    _showAudioLock = true;
                    PlayAudio(0);
                }
                _ren.enabled = true;
                _colli.enabled = true;
                _colli.DrawCollider2D(_ren, _material, _lineColor, _lineWidth);
                if(_playerEnter){
                    _arrowImage.enabled = false;
                }
                else{
                    _arrowImage.enabled = true;
                    UpdateArrow(transform.position, _arrowImage, _canvasRect);
                }
            }
            else{
                _ren.enabled = false;
                _colli.enabled = false;
                _arrowImage.enabled = false;
            }
        }
    }

    private static void UpdateArrow(Vector3 targetPos, Image arrow, RectTransform canvas){
        // arrow.enabled = !targetPos.IsVisableInCamera(CameraControl.main, true);
        if(targetPos.IsVisableInCamera(CameraControl.main, true)){
            arrow.enabled = false;
            return;
        }
        Vector2 uiPos = targetPos.WorldToUI(canvas, CameraControl.main);
        // float w = canvas.rect.width / 2f;
        // float h = canvas.rect.height / 2f;
        float w = canvas.rect.width / 2.5f;
        float h = canvas.rect.height / 2.5f;
        uiPos = new Vector2(Mathf.Clamp(uiPos.x, -w, w), Mathf.Clamp(uiPos.y, -h, h));
        arrow.rectTransform.localPosition = uiPos;
        arrow.rectTransform.localEulerAngles = new Vector3(0f, 0f, uiPos.Vector2ToAngle());
    }

    public new AudioSource audio { get; set; }

    public void PlayAudio(int audioIndex){
        AudioClip a = audioIndex.GetAudioClip();
        if(a){
            audio.PlayOneShot(a);
        }
    }
}
