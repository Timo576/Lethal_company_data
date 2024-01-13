// Decompiled with JetBrains decompiler
// Type: HighAndLowAltitudeAudio
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class HighAndLowAltitudeAudio : MonoBehaviour
{
  public AudioSource HighAudio;
  public AudioSource LowAudio;
  public float maxAltitude;
  public float minAltitude;

  private void Update()
  {
    if ((Object) GameNetworkManager.Instance.localPlayerController == (Object) null || (Object) NetworkManager.Singleton == (Object) null)
      return;
    if (GameNetworkManager.Instance.localPlayerController.isInsideFactory)
    {
      this.HighAudio.volume = 0.0f;
      this.LowAudio.volume = 0.0f;
    }
    else if (!GameNetworkManager.Instance.localPlayerController.isPlayerDead)
    {
      this.SetAudioVolumeBasedOnAltitude(GameNetworkManager.Instance.localPlayerController.transform.position.y);
    }
    else
    {
      if (!((Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != (Object) null))
        return;
      this.SetAudioVolumeBasedOnAltitude(GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.transform.position.y);
    }
  }

  private void SetAudioVolumeBasedOnAltitude(float playerHeight)
  {
    this.HighAudio.volume = Mathf.Clamp((playerHeight - this.minAltitude) / this.maxAltitude, 0.0f, 1f);
    this.LowAudio.volume = Mathf.Abs(this.HighAudio.volume - 1f);
  }
}
