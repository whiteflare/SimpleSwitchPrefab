
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LocalResetAreaLeave : UdonSharpBehaviour
{
    [Space]

    [Header("Reset イベントを送信する対象の UdonBehaviour を指定します。")]
    public UdonBehaviour resetTarget = null;

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            _sendOtherReset();
        }
    }

    private void _sendOtherReset()
    {
        // Udon
        if (resetTarget != null)
        {
            resetTarget.SendCustomEvent("ResetStatus");
        }
    }
}
