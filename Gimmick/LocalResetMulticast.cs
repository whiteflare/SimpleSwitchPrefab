
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LocalResetMulticast : UdonSharpBehaviour
{
    [Space]

    [Header("Reset対象の GameObject を指定します。")]
    public UdonBehaviour[] resetTargets = { };

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
        if (resetTargets != null)
        {
            foreach(var r in resetTargets)
            {
                _sendReset(r);
            }
        }
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
