
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LocalResetMulticast : UdonSharpBehaviour
{
    [Space]

    [Header("Reset対象の GameObject を指定します。")]
    public UdonBehaviour resetTargets0 = null;
    public UdonBehaviour resetTargets1 = null;
    public UdonBehaviour resetTargets2 = null;
    public UdonBehaviour resetTargets3 = null;
    public UdonBehaviour resetTargets4 = null;
    public UdonBehaviour resetTargets5 = null;
    public UdonBehaviour resetTargets6 = null;
    public UdonBehaviour resetTargets7 = null;

    [Header("------------------------------------------------------------------")]
    public int coolTime = 300;

    private long timer = 0;
    private readonly System.DateTime EPOCH = System.DateTime.Parse("1970-01-01T00:00:00.00000000Z");

    public void ResetStatus()
    {
        if (_isTimerComplete(coolTime))
        {
            _sendOtherReset();
        }
    }

    private void _sendOtherReset()
    {
        _sendReset(resetTargets0);
        _sendReset(resetTargets1);
        _sendReset(resetTargets2);
        _sendReset(resetTargets3);
        _sendReset(resetTargets4);
        _sendReset(resetTargets5);
        _sendReset(resetTargets6);
        _sendReset(resetTargets7);
    }

    private void _sendReset(UdonBehaviour target)
    {
        // Udon
        if (target != null)
        {
            target.SendCustomEvent(nameof(ResetStatus));
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
