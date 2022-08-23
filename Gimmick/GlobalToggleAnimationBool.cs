
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GlobalToggleAnimationBool : UdonSharpBehaviour
{
    [Space]

    [Header("値を設定する対象の Animator を指定します。")]
    public Animator animator = null;

    [Header("設定先の変数名を指定します。")]
    public string parameterName = "";

    [Header("初期状態を指定します。")]
    public bool initialValue = false;

    [Header("操作を許可する人を指定します。")]
    [Tooltip("全員に許可")]
    public bool permissionEveryone = true;
    [Tooltip("マスターに許可")]
    public bool permissionMaster = true;
    [Tooltip("インスタンスを立てた人に許可")]
    public bool permissionInstanceOwner = true;

    [Header("------------------------------------------------------------------")]
    public Animator statusAnimator;
    public AudioSource sound;
    public UdonBehaviour resetTarget = null;
    public int coolTime = 300;

    [UdonSynced(UdonSyncMode.None)]
    private bool value = false;

    private bool valueLocal = false;
    private bool initialized = false;
    private long timer = 0;
    private readonly System.DateTime EPOCH = System.DateTime.Parse("1970-01-01T00:00:00.00000000Z");

    public void Start()
    {
        _init();
        _updateState();
    }

    public override void OnDeserialization()
    {
        initialized = true;

        if (valueLocal != value)
        {
            // ローカル状態の同期
            _updateState();
            if (value)
            {
                _sendOtherReset();
            }
            valueLocal = value;
        }
    }

    public override void Interact()
    {
        if (_isTimerComplete(coolTime) && _canInteract())
        {
            ToggleStatus();
            if (sound != null)
            {
                sound.Play();
            }
        }
    }

    public void ToggleStatus()
    {
        _init();

        // トグル
        value = !value;
        // ローカル状態の同期
        _updateState();
        if (value)
        {
            _sendOtherReset();
        }

        // 同期
        valueLocal = value;
        if (Networking.LocalPlayer != null)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                RequestSerialization();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ToggleStatus));
            }
        }
    }

    public void ResetStatus()
    {
        _init();

        // リセット
        value = initialValue;
        // ローカル状態の同期
        _updateState();
        if (value)
        {
            _sendOtherReset();
        }

        // 同期
        valueLocal = value;
        if (Networking.LocalPlayer != null)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                RequestSerialization();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ResetStatus));
            }
        }
    }

    private bool _canInteract()
    {
        if (Networking.LocalPlayer != null)
        {
            if (permissionEveryone)
            {
                return true;
            }
            if (permissionMaster && Networking.LocalPlayer.isMaster)
            {
                return true;
            }
            if (permissionInstanceOwner && Networking.LocalPlayer.isInstanceOwner)
            {
                return true;
            }
            return false;
        }
        else
        {
            // 未接続時は許可
            return true;
        }
    }

    private void _init()
    {
        if (initialized)
        {
            return;
        }
        initialized = true;
        value = initialValue;
    }

    private void _updateState()
    {
        var v = value;
        // Animator
        if (animator != null && !string.IsNullOrWhiteSpace(parameterName))
        {
            animator.SetBool(parameterName, v);
        }
        // Status
        if (statusAnimator != null)
        {
            statusAnimator.SetInteger("lamp1", v ? 2 : 1);
            statusAnimator.SetInteger("lamp2", 0);
            statusAnimator.SetInteger("lamp3", 0);
            statusAnimator.SetInteger("lamp4", 0);
        }
    }

    private void _sendOtherReset()
    {
        // Udon
        if (resetTarget != null)
        {
            resetTarget.SendCustomEvent(nameof(ResetStatus));
        }
    }

    private long _getNowUnixEpochTime()
    {
        var span = System.DateTime.UtcNow - EPOCH;
        long time = span.Days;
        time = time * 24 + span.Hours;
        time = time * 60 + span.Minutes;
        time = time * 60 + span.Seconds;
        time = time * 1000 + span.Milliseconds;
        return time;
    }

    private bool _isTimerComplete(int span)
    {
        long now = _getNowUnixEpochTime();
        if (now - this.timer < span)
        {
            return false; // 一定時間未満
        }
        else
        {
            this.timer = now;
            return true;
        }
    }
}
