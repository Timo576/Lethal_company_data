// Decompiled with JetBrains decompiler
// Type: BaboonHawkAudioEvents
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class BaboonHawkAudioEvents : MonoBehaviour
{
  public AudioSource audioToPlay;
  public AudioClip[] randomClips;
  public Animator thisAnimator;
  private float timeLastAudioPlayed;
  public ParticleSystem particle;

  public void PlayParticleWithChildren() => this.particle.Play(true);

  public void PlayAudio1RandomClipWithMinSpeedCondition()
  {
    if ((double) Time.realtimeSinceStartup - (double) this.timeLastAudioPlayed < 0.20000000298023224 || (double) Mathf.Abs(this.thisAnimator.GetFloat("VelocityX")) < 0.5 && (double) Mathf.Abs(this.thisAnimator.GetFloat("VelocityZ")) < 0.5)
      return;
    this.timeLastAudioPlayed = Time.realtimeSinceStartup;
    int index = Random.Range(0, this.randomClips.Length);
    this.audioToPlay.spatialize = false;
    this.audioToPlay.PlayOneShot(this.randomClips[index]);
    WalkieTalkie.TransmitOneShotAudio(this.audioToPlay, this.randomClips[index]);
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, 7f, 0.4f, noiseID: 24751);
  }
}
