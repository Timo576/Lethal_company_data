// Decompiled with JetBrains decompiler
// Type: PlayerVoiceIngameSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using Dissonance.Audio.Playback;
using UnityEngine;

#nullable disable
public class PlayerVoiceIngameSettings : MonoBehaviour
{
  public AudioReverbFilter filter;
  public AudioSource voiceAudio;
  public VoicePlayback _playbackComponent;
  public DissonanceComms _dissonanceComms;
  public VoicePlayerState _playerState;
  public bool set2D;
  private bool isEnabled;

  private void Awake() => this.InitializeComponents();

  public void InitializeComponents()
  {
    this._playbackComponent = this.GetComponent<VoicePlayback>();
    this._dissonanceComms = Object.FindObjectOfType<DissonanceComms>();
    this.filter = this.gameObject.GetComponent<AudioReverbFilter>();
    this.voiceAudio = this.gameObject.GetComponent<AudioSource>();
  }

  private void LateUpdate()
  {
    if (!this.isEnabled)
      return;
    if ((Object) this.voiceAudio == (Object) null)
      this.voiceAudio = this.gameObject.GetComponent<AudioSource>();
    if (this.set2D)
      this.voiceAudio.spatialBlend = 0.0f;
    else
      this.voiceAudio.spatialBlend = 1f;
  }

  private void OnEnable()
  {
    this.isEnabled = true;
    if ((Object) this._playbackComponent == (Object) null)
    {
      this.InitializeComponents();
      if ((Object) this._playbackComponent == (Object) null)
        return;
    }
    this._playerState = this._dissonanceComms.FindPlayer(this._playbackComponent.PlayerName);
  }

  public void FindPlayerIfNull()
  {
    if (this._playerState == null)
    {
      if ((Object) this._playbackComponent == (Object) null)
      {
        this.InitializeComponents();
        if ((Object) this._playbackComponent == (Object) null)
          return;
      }
      if (string.IsNullOrEmpty(this._playbackComponent.PlayerName))
        return;
      this._playerState = this._dissonanceComms.FindPlayer(this._playbackComponent.PlayerName);
    }
    this.InitializeComponents();
  }

  private void OnDisable()
  {
    this.isEnabled = false;
    this.voiceAudio = (AudioSource) null;
    this.filter = (AudioReverbFilter) null;
    this._playerState = (VoicePlayerState) null;
  }
}
