﻿// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningLightsabreScript
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningLightsabreScript : LightningBoltPrefabScript
  {
    [Header("Lightsabre Properties")]
    [Tooltip("Height of the blade")]
    public float BladeHeight = 19f;
    [Tooltip("How long it takes to turn the lightsabre on and off")]
    public float ActivationTime = 0.5f;
    [Tooltip("Sound to play when the lightsabre turns on")]
    public AudioSource StartSound;
    [Tooltip("Sound to play when the lightsabre turns off")]
    public AudioSource StopSound;
    [Tooltip("Sound to play when the lightsabre stays on")]
    public AudioSource ConstantSound;
    private int state;
    private Vector3 bladeStart;
    private Vector3 bladeDir;
    private float bladeTime;
    private float bladeIntensity;

    protected override void Start() => base.Start();

    protected override void Update()
    {
      if (this.state == 2 || this.state == 3)
      {
        this.bladeTime += LightningBoltScript.DeltaTime;
        float num = Mathf.Lerp(0.01f, 1f, this.bladeTime / this.ActivationTime);
        this.Destination.transform.position = this.bladeStart + this.bladeDir * num * this.BladeHeight;
        this.GlowIntensity = this.bladeIntensity * (this.state == 3 ? num : 1f - num);
        if ((double) this.bladeTime >= (double) this.ActivationTime)
        {
          this.GlowIntensity = this.bladeIntensity;
          this.bladeTime = 0.0f;
          if (this.state == 2)
          {
            this.ManualMode = true;
            this.state = 0;
          }
          else
            this.state = 1;
        }
      }
      base.Update();
    }

    public bool TurnOn(bool value)
    {
      if (this.state == 2 || this.state == 3 || this.state == 1 & value || this.state == 0 && !value)
        return false;
      this.bladeStart = this.Destination.transform.position;
      this.ManualMode = false;
      this.bladeIntensity = this.GlowIntensity;
      if (value)
      {
        this.bladeDir = this.Camera.orthographic ? this.transform.up : this.transform.forward;
        this.state = 3;
        this.StartSound.Play();
        this.StopSound.Stop();
        this.ConstantSound.Play();
      }
      else
      {
        this.bladeDir = -(this.Camera.orthographic ? this.transform.up : this.transform.forward);
        this.state = 2;
        this.StartSound.Stop();
        this.StopSound.Play();
        this.ConstantSound.Stop();
      }
      return true;
    }

    public void TurnOnGUI(bool value) => this.TurnOn(value);
  }
}
