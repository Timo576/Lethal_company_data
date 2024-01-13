// Decompiled with JetBrains decompiler
// Type: BoomboxItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

#nullable disable
public class BoomboxItem : GrabbableObject
{
  public AudioSource boomboxAudio;
  public AudioClip[] musicAudios;
  public AudioClip[] stopAudios;
  public System.Random musicRandomizer;
  private StartOfRound playersManager;
  private RoundManager roundManager;
  public bool isPlayingMusic;
  private float noiseInterval;
  private int timesPlayedWithoutTurningOff;

  public override void Start()
  {
    base.Start();
    this.playersManager = UnityEngine.Object.FindObjectOfType<StartOfRound>();
    this.roundManager = UnityEngine.Object.FindObjectOfType<RoundManager>();
    this.musicRandomizer = new System.Random(this.playersManager.randomMapSeed - 10);
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    this.StartMusic(used);
  }

  private void StartMusic(bool startMusic, bool pitchDown = false)
  {
    if (startMusic)
    {
      this.boomboxAudio.clip = this.musicAudios[this.musicRandomizer.Next(0, this.musicAudios.Length)];
      this.boomboxAudio.pitch = 1f;
      this.boomboxAudio.Play();
    }
    else if (this.isPlayingMusic)
    {
      if (pitchDown)
      {
        this.StartCoroutine(this.musicPitchDown());
      }
      else
      {
        this.boomboxAudio.Stop();
        this.boomboxAudio.PlayOneShot(this.stopAudios[UnityEngine.Random.Range(0, this.stopAudios.Length)]);
      }
      this.timesPlayedWithoutTurningOff = 0;
    }
    this.isBeingUsed = startMusic;
    this.isPlayingMusic = startMusic;
  }

  private IEnumerator musicPitchDown()
  {
    for (int i = 0; i < 30; ++i)
    {
      yield return (object) null;
      this.boomboxAudio.pitch -= 0.033f;
      if ((double) this.boomboxAudio.pitch <= 0.0)
        break;
    }
    this.boomboxAudio.Stop();
    this.boomboxAudio.PlayOneShot(this.stopAudios[UnityEngine.Random.Range(0, this.stopAudios.Length)]);
  }

  public override void UseUpBatteries()
  {
    base.UseUpBatteries();
    this.StartMusic(false, true);
  }

  public override void PocketItem()
  {
    base.PocketItem();
    this.StartMusic(false);
  }

  public override void Update()
  {
    base.Update();
    if (!this.isPlayingMusic)
      return;
    if ((double) this.noiseInterval <= 0.0)
    {
      this.noiseInterval = 1f;
      ++this.timesPlayedWithoutTurningOff;
      this.roundManager.PlayAudibleNoise(this.transform.position, 16f, 0.9f, this.timesPlayedWithoutTurningOff, noiseID: 5);
    }
    else
      this.noiseInterval -= Time.deltaTime;
    if ((double) this.insertedBattery.charge >= 0.05000000074505806)
      return;
    this.boomboxAudio.pitch = (float) (1.0 - (0.05000000074505806 - (double) this.insertedBattery.charge) * 4.0);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (BoomboxItem);
}
