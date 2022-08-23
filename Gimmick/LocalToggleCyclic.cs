
using UdonSharp;
using UnityEngine;

public class LocalToggleCyclic : UdonSharpBehaviour
{
    [Space]

    [Header("順にON/OFFを切り替える GameObject を指定します。")]
    public GameObject[] sequence = { };

    [Header("初期状態を指定します。")]
    public int initialValue = 0;

    [Header("------------------------------------------------------------------")]
    public Animator statusAnimator;
    public AudioSource sound;
    public int coolTime = 300;

    private int value = 0;
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
        value++;
        // ローカル状態の同期
        _updateState();
    }

    public void ResetStatus()
    {
        _init();

        // リセット
        value = initialValue;
        // ローカル状態の同期
        _updateState();
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
        var count = sequence.Length;
        if (count <= value)
        {
            value = 0;
        }
        var v = value;
        for (int i = 0; i < count; i++)
        {
            var go = sequence[i];
            if (go != null)
            {
                go.SetActive(value == i);
            }
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
