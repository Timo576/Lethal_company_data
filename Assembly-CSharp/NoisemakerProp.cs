// Decompiled with JetBrains decompiler
// Type: NoisemakerProp
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class NoisemakerProp : GrabbableObject
{
  public AudioSource noiseAudio;
  public AudioSource noiseAudioFar;
  [Space(3f)]
  public AudioClip[] noiseSFX;
  public AudioClip[] noiseSFXFar;
  [Space(3f)]
  public float noiseRange;
  public float maxLoudness;
  public float minLoudness;
  public float minPitch;
  public float maxPitch;
  private System.Random noisemakerRandom;
  public Animator triggerAnimator;

  public override void Start()
  {
    base.Start();
    this.noisemakerRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 85);
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    int index = this.noisemakerRandom.Next(0, this.noiseSFX.Length);
    float num1 = (float) this.noisemakerRandom.Next((int) ((double) this.minLoudness * 100.0), (int) ((double) this.maxLoudness * 100.0)) / 100f;
    float num2 = (float) this.noisemakerRandom.Next((int) ((double) this.minPitch * 100.0), (int) ((double) this.maxPitch * 100.0)) / 100f;
    this.noiseAudio.pitch = num2;
    this.noiseAudio.PlayOneShot(this.noiseSFX[index], num1);
    if ((UnityEngine.Object) this.noiseAudioFar != (UnityEngine.Object) null)
    {
      this.noiseAudioFar.pitch = num2;
      this.noiseAudioFar.PlayOneShot(this.noiseSFXFar[index], num1);
    }
    if ((UnityEngine.Object) this.triggerAnimator != (UnityEngine.Object) null)
      this.triggerAnimator.SetTrigger("playAnim");
    WalkieTalkie.TransmitOneShotAudio(this.noiseAudio, this.noiseSFX[index], num1);
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, this.noiseRange, num1, noiseIsInsideClosedShip: this.isInElevator && StartOfRound.Instance.hangarDoorsClosed);
    if ((double) this.minLoudness < 0.60000002384185791 || !((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null))
      return;
    this.playerHeldBy.timeSinceMakingLoudNoise = 0.0f;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (NoisemakerProp);
}
