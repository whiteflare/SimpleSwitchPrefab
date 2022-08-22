
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GlobalToggleAnimationInt : UdonSharpBehaviour
{
    [Space]

    [Header("値を設定する対象の Animator を指定します。")]
    public Animator animator = null;

    [Header("設定先の変数名を指定します。")]
    public string parameterName = "";

    [Header("状態数を指定します。ここで指定した値 - 1 までが Animator に設定されます。")]
    public int stateCount = 1;

    [Header("初期状態を指定します。")]
    public int initialValue = 0;

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
    public int coolTime = 500;

    [UdonSynced(UdonSyncMode.None)]
    private int value = 0;

    private int valueLocal = 0;
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
        value++;
        // ローカル状態の同期
        _updateState();

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
        var count = stateCount;
        if (count <= value)
        {
            value = 0;
        }
        var v = value;
        // Animator
        if (animator != null && !string.IsNullOrWhiteSpace(parameterName))
        {
            animator.SetInteger(parameterName, value);
        }
        // Status
        if (statusAnimator != null)
        {
            // 4以下と5以上で表示方法を変える
            v++;
            if (count <= 4)
            {
                statusAnimator.SetInteger("lamp1", count < 1 ? 0 : v == 1 ? 2 : 1);
                statusAnimator.SetInteger("lamp2", count < 2 ? 0 : v == 2 ? 2 : 1);
                statusAnimator.SetInteger("lamp3", count < 3 ? 0 : v == 3 ? 2 : 1);
                statusAnimator.SetInteger("lamp4", count < 4 ? 0 : v == 4 ? 2 : 1);
            }
            else
            {
                statusAnimator.SetInteger("lamp1", (v & 1) != 0 ? 2 : 1);
                statusAnimator.SetInteger("lamp2", (v & 2) != 0 ? 2 : 1);
                statusAnimator.SetInteger("lamp3", (v & 4) != 0 ? 2 : 1);
                statusAnimator.SetInteger("lamp4", (v & 8) != 0 ? 2 : 1);
            }
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
