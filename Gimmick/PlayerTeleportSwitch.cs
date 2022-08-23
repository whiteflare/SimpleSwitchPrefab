
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerTeleportSwitch : UdonSharpBehaviour
{
    [Space]

    [Header("テレポート先の Transform を指定します。")]
    public Transform target;

    [Header("操作を許可する人を指定します。")]
    [Tooltip("全員に許可")]
    public bool permissionEveryone = true;
    [Tooltip("マスターに許可")]
    public bool permissionMaster = true;
    [Tooltip("インスタンスを立てた人に許可")]
    public bool permissionInstanceOwner = true;

    [Header("------------------------------------------------------------------")]
    public AudioSource sound;
    public int coolTime = 300;

    private long timer = 0;
    private readonly System.DateTime EPOCH = System.DateTime.Parse("1970-01-01T00:00:00.00000000Z");

    public override void Interact()
    {
        if (_isTimerComplete(coolTime) && _canInteract() && _teleport())
        {
            if (sound != null)
            {
                sound.Play();
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

    private bool _teleport()
    {
        if (target != null && target.gameObject.activeInHierarchy)
        {
            if (Networking.LocalPlayer != null)
            {
                Networking.LocalPlayer.TeleportTo(target.position, target.rotation, VRC_SceneDescriptor.SpawnOrientation.Default, false);
            }
            return true;
        }
        return false;
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
