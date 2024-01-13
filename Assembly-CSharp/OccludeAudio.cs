// Decompiled with JetBrains decompiler
// Type: OccludeAudio
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
[RequireComponent(typeof (AudioSource))]
public class OccludeAudio : MonoBehaviour
{
  private AudioLowPassFilter lowPassFilter;
  private AudioReverbFilter reverbFilter;
  public bool useReverb;
  private bool occluded;
  private AudioSource thisAudio;
  private float checkInterval;
  public bool overridingLowPass;
  public float lowPassOverride = 20000f;
  public bool debugLog;

  private void Start()
  {
    this.lowPassFilter = this.gameObject.GetComponent<AudioLowPassFilter>();
    if ((Object) this.lowPassFilter == (Object) null)
    {
      this.lowPassFilter = this.gameObject.AddComponent<AudioLowPassFilter>();
      this.lowPassFilter.cutoffFrequency = 20000f;
    }
    if (this.useReverb)
    {
      this.reverbFilter = this.gameObject.GetComponent<AudioReverbFilter>();
      if ((Object) this.reverbFilter == (Object) null)
        this.reverbFilter = this.gameObject.AddComponent<AudioReverbFilter>();
      this.reverbFilter.reverbPreset = AudioReverbPreset.Hallway;
      this.reverbFilter.reverbPreset = AudioReverbPreset.User;
      this.reverbFilter.dryLevel = -1f;
      this.reverbFilter.decayTime = 0.8f;
      this.reverbFilter.room = -2300f;
    }
    this.thisAudio = this.gameObject.GetComponent<AudioSource>();
    this.occluded = (Object) StartOfRound.Instance != (Object) null && Physics.Linecast(this.transform.position, StartOfRound.Instance.audioListener.transform.position, 256, QueryTriggerInteraction.Ignore);
    this.checkInterval = Random.Range(0.0f, 0.4f);
  }

  private void Update()
  {
    if (this.thisAudio.isVirtual)
      return;
    if (this.useReverb && (Object) GameNetworkManager.Instance != (Object) null && (Object) GameNetworkManager.Instance.localPlayerController != (Object) null)
    {
      if (GameNetworkManager.Instance.localPlayerController.isInsideFactory || GameNetworkManager.Instance.localPlayerController.isPlayerDead && (Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != (Object) null && GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.isInsideFactory)
      {
        this.reverbFilter.dryLevel = Mathf.Lerp(this.reverbFilter.dryLevel, Mathf.Clamp((float) -(3.4000000953674316 * ((double) Vector3.Distance(StartOfRound.Instance.audioListener.transform.position, this.transform.position) / ((double) this.thisAudio.maxDistance / 5.0))), -300f, -1f), Time.deltaTime * 8f);
        this.reverbFilter.enabled = true;
      }
      else
        this.reverbFilter.enabled = false;
    }
    this.lowPassFilter.cutoffFrequency = this.overridingLowPass ? this.lowPassOverride : (!this.occluded ? Mathf.Lerp(this.lowPassFilter.cutoffFrequency, 10000f, Time.deltaTime * 8f) : Mathf.Lerp(this.lowPassFilter.cutoffFrequency, Mathf.Clamp((float) (2500.0 / ((double) Vector3.Distance(StartOfRound.Instance.audioListener.transform.position, this.transform.position) / ((double) this.thisAudio.maxDistance / 2.0))), 900f, 4000f), Time.deltaTime * 8f));
    if ((double) this.checkInterval >= 0.5)
    {
      this.checkInterval = 0.0f;
      if (Physics.Linecast(this.transform.position, StartOfRound.Instance.audioListener.transform.position, out RaycastHit _, 256, QueryTriggerInteraction.Ignore))
      {
        int num = this.debugLog ? 1 : 0;
        this.occluded = true;
      }
      else
        this.occluded = false;
    }
    else
      this.checkInterval += Time.deltaTime;
  }
}
