
using UdonSharp;
using UnityEngine;
using VRC.Udon;

public class LocalToggleOnOff : UdonSharpBehaviour
{
    [Space]

    [Header("OFF から ON に切り替える GameObject を指定します。")]
    public GameObject[] setActive = { };

    [Header("ON から OFF に切り替える GameObject を指定します。")]
    public GameObject[] setDeactive = { };

    [Header("初期状態を指定します。")]
    public bool initialValue = false;

    [Header("------------------------------------------------------------------")]
    public Animator statusAnimator;
    public AudioSource sound;
    public UdonBehaviour resetTarget = null;
    public int coolTime = 300;

    private bool value = false;
    private bool initialized = false;
    private long timer = 0;
    private readonly System.DateTime EPOCH = System.DateTime.Parse("1970-01-01T00:00:00.00000000Z");

    public void Start()
    {
        _init();
        _updateState();
    }

    public override void Interact()
    {
        if (_isTimerComplete(coolTime))
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
        // OFF -> ON
        if (setActive != null)
        {
            foreach (var go in setActive)
            {
                if (go != null)
                {
                    go.SetActive(v);
                }
            }
        }
        // ON -> OFF
        if (setDeactive != null)
        {
            foreach (var go in setDeactive)
            {
                if (go != null)
                {
                    go.SetActive(!v);
                }
            }
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
