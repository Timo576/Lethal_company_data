// Decompiled with JetBrains decompiler
// Type: AnimatedItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class AnimatedItem : GrabbableObject
{
  public string grabItemBoolString;
  public string dropItemTriggerString;
  public bool makeAnimationWhenDropping;
  public Animator itemAnimator;
  public AudioSource itemAudio;
  public AudioClip grabAudio;
  public AudioClip dropAudio;
  public bool loopGrabAudio;
  public bool loopDropAudio;
  [Range(0.0f, 100f)]
  public int chanceToTriggerAnimation = 100;
  public int chanceToTriggerAlternateMesh;
  public Mesh alternateMesh;
  private Mesh normalMesh;
  private System.Random itemRandomChance;
  public float noiseRange;
  public float noiseLoudness;
  private int timesPlayedInOneSpot;
  private float makeNoiseInterval;
  private Vector3 lastPosition;
  public AudioLowPassFilter itemAudioLowPassFilter;
  private bool wasInPocket;

  public override void Start()
  {
    base.Start();
    this.itemRandomChance = new System.Random(StartOfRound.Instance.randomMapSeed + StartOfRound.Instance.currentLevelID + this.itemProperties.itemId);
    if (this.chanceToTriggerAlternateMesh <= 0)
      return;
    this.normalMesh = this.gameObject.GetComponent<MeshFilter>().mesh;
  }

  public override void EquipItem()
  {
    base.EquipItem();
    if ((UnityEngine.Object) this.itemAudioLowPassFilter != (UnityEngine.Object) null)
      this.itemAudioLowPassFilter.cutoffFrequency = 20000f;
    this.itemAudio.volume = 1f;
    if (this.chanceToTriggerAlternateMesh > 0)
    {
      if (this.itemRandomChance.Next(0, 100) < this.chanceToTriggerAlternateMesh)
      {
        this.gameObject.GetComponent<MeshFilter>().mesh = this.alternateMesh;
        this.itemAudio.Stop();
        return;
      }
      this.gameObject.GetComponent<MeshFilter>().mesh = this.normalMesh;
    }
    if (!this.wasInPocket)
    {
      if (this.itemRandomChance.Next(0, 100) > this.chanceToTriggerAnimation)
      {
        this.itemAudio.Stop();
        return;
      }
    }
    else
      this.wasInPocket = false;
    if ((UnityEngine.Object) this.itemAnimator != (UnityEngine.Object) null)
      this.itemAnimator.SetBool(this.grabItemBoolString, true);
    if (!((UnityEngine.Object) this.itemAudio != (UnityEngine.Object) null))
      return;
    this.itemAudio.clip = this.grabAudio;
    this.itemAudio.loop = this.loopGrabAudio;
    this.itemAudio.Play();
  }

  public override void DiscardItem()
  {
    base.DiscardItem();
    if ((UnityEngine.Object) this.itemAnimator != (UnityEngine.Object) null)
      this.itemAnimator.SetBool(this.grabItemBoolString, false);
    if (this.chanceToTriggerAlternateMesh > 0)
      this.gameObject.GetComponent<MeshFilter>().mesh = this.normalMesh;
    if (!this.makeAnimationWhenDropping)
      this.itemAudio.Stop();
    else if (this.itemRandomChance.Next(0, 100) < this.chanceToTriggerAnimation)
    {
      this.itemAudio.Stop();
    }
    else
    {
      if ((UnityEngine.Object) this.itemAnimator != (UnityEngine.Object) null)
        this.itemAnimator.SetTrigger(this.dropItemTriggerString);
      if (!((UnityEngine.Object) this.itemAudio != (UnityEngine.Object) null))
        return;
      this.itemAudio.loop = this.loopDropAudio;
      this.itemAudio.clip = this.dropAudio;
      this.itemAudio.Play();
      if ((UnityEngine.Object) this.itemAudioLowPassFilter != (UnityEngine.Object) null)
        this.itemAudioLowPassFilter.cutoffFrequency = 20000f;
      this.itemAudio.volume = 1f;
    }
  }

  public override void PocketItem()
  {
    base.PocketItem();
    this.wasInPocket = true;
    if (!((UnityEngine.Object) this.itemAudio != (UnityEngine.Object) null))
      return;
    if ((UnityEngine.Object) this.itemAudioLowPassFilter != (UnityEngine.Object) null)
      this.itemAudioLowPassFilter.cutoffFrequency = 1700f;
    this.itemAudio.volume = 0.5f;
  }

  public override void Update()
  {
    base.Update();
    if ((UnityEngine.Object) this.itemAudio == (UnityEngine.Object) null || !this.itemAudio.isPlaying)
      return;
    if ((double) this.makeNoiseInterval <= 0.0)
    {
      this.makeNoiseInterval = 0.75f;
      if ((double) Vector3.Distance(this.lastPosition, this.transform.position) < 4.0)
        ++this.timesPlayedInOneSpot;
      else
        this.timesPlayedInOneSpot = 0;
      if (this.isPocketed)
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, this.noiseRange / 2f, this.noiseLoudness / 2f, this.timesPlayedInOneSpot, this.isInElevator && StartOfRound.Instance.hangarDoorsClosed);
      else
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, this.noiseRange, this.noiseLoudness, this.timesPlayedInOneSpot, this.isInElevator && StartOfRound.Instance.hangarDoorsClosed);
    }
    else
      this.makeNoiseInterval -= Time.deltaTime;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (AnimatedItem);
}
