// Decompiled with JetBrains decompiler
// Type: PlayAudioAnimationEvent
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Events;

#nullable disable
public class PlayAudioAnimationEvent : MonoBehaviour
{
  public AudioSource audioToPlay;
  public AudioSource audioToPlayB;
  public AudioClip audioClip;
  public AudioClip audioClip2;
  public AudioClip audioClip3;
  public AudioClip[] randomClips;
  public AudioClip[] randomClips2;
  public bool randomizePitch;
  public ParticleSystem particle;
  public UnityEvent onAnimationEventCalled;
  public GameObject destroyObject;
  private float timeAtStart;
  public bool playAudibleNoise;

  private void Start() => this.timeAtStart = Time.timeSinceLevelLoad;

  public void PlayAudio1()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.timeAtStart < 2.0)
      return;
    this.audioToPlay.clip = this.audioClip;
    this.audioToPlay.Play();
    WalkieTalkie.TransmitOneShotAudio(this.audioToPlay, this.audioClip);
    if (!this.playAudibleNoise)
      return;
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, noiseLoudness: 0.65f, noiseID: 546);
  }

  public void PlayAudio1RandomClip()
  {
    int index = Random.Range(0, this.randomClips.Length);
    if ((Object) this.randomClips[index] == (Object) null)
      return;
    this.audioToPlay.spatialize = false;
    this.audioToPlay.PlayOneShot(this.randomClips[index]);
    WalkieTalkie.TransmitOneShotAudio(this.audioToPlay, this.randomClips[index]);
    if (!this.playAudibleNoise)
      return;
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, noiseLoudness: 0.65f, noiseID: 546);
  }

  public void PlayAudio2RandomClip()
  {
    int index = Random.Range(0, this.randomClips2.Length);
    if ((Object) this.randomClips2[index] == (Object) null)
      return;
    this.audioToPlayB.PlayOneShot(this.randomClips2[index]);
    Debug.Log((object) "Playing random clip 2");
    WalkieTalkie.TransmitOneShotAudio(this.audioToPlayB, this.randomClips2[index]);
    if (!this.playAudibleNoise)
      return;
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, noiseLoudness: 0.65f, noiseID: 546);
  }

  public void PlayAudioB1()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.timeAtStart < 2.0)
      return;
    this.audioToPlayB.clip = this.audioClip;
    this.audioToPlayB.Play();
    WalkieTalkie.TransmitOneShotAudio(this.audioToPlayB, this.audioClip);
    if (!this.playAudibleNoise)
      return;
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, noiseLoudness: 0.65f, noiseID: 546);
  }

  public void PlayParticle() => this.particle.Play();

  public void PlayParticleWithChildren() => this.particle.Play(true);

  public void StopParticle() => this.particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);

  public void PlayAudio1Oneshot()
  {
    if (this.TooEarlySinceInitializing())
      return;
    if (this.randomizePitch)
      this.audioToPlay.pitch = Random.Range(0.8f, 1.4f);
    this.audioToPlay.PlayOneShot(this.audioClip);
    WalkieTalkie.TransmitOneShotAudio(this.audioToPlay, this.audioClip);
    if (!this.playAudibleNoise)
      return;
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, noiseLoudness: 0.65f, noiseID: 546);
  }

  public void PlayAudio2()
  {
    this.audioToPlay.clip = this.audioClip2;
    this.audioToPlay.Play();
  }

  public void PlayAudioB2()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.timeAtStart < 2.0)
      return;
    this.audioToPlayB.clip = this.audioClip2;
    this.audioToPlayB.Play();
  }

  public void PlayAudio2Oneshot()
  {
    if (this.TooEarlySinceInitializing())
      return;
    if (this.randomizePitch)
      this.audioToPlay.pitch = Random.Range(0.6f, 1.4f);
    this.audioToPlay.PlayOneShot(this.audioClip2);
    WalkieTalkie.TransmitOneShotAudio(this.audioToPlay, this.audioClip2);
    if (!this.playAudibleNoise)
      return;
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, noiseLoudness: 0.65f, noiseID: 546);
  }

  public void PlayAudio3Oneshot()
  {
    if (this.TooEarlySinceInitializing())
      return;
    if (this.randomizePitch)
      this.audioToPlay.pitch = Random.Range(0.6f, 1.2f);
    this.audioToPlay.PlayOneShot(this.audioClip3);
    WalkieTalkie.TransmitOneShotAudio(this.audioToPlay, this.audioClip3);
    if (!this.playAudibleNoise)
      return;
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, noiseLoudness: 0.65f, noiseID: 546);
  }

  public void StopAudio() => this.audioToPlay.Stop();

  public void PauseAudio() => this.audioToPlay.Pause();

  public void PlayAudio1DefaultClip() => this.audioToPlay.Play();

  public void OnAnimationEvent() => this.onAnimationEventCalled.Invoke();

  private bool TooEarlySinceInitializing()
  {
    return (double) Time.timeSinceLevelLoad - (double) this.timeAtStart < 2.0;
  }

  public void DestroyObject() => Object.Destroy((Object) this.destroyObject);
}
