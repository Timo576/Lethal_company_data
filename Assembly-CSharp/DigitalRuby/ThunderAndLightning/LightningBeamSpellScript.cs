﻿// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningBeamSpellScript
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningBeamSpellScript : LightningSpellScript
  {
    [Header("Beam")]
    [Tooltip("The lightning path script creating the beam of lightning")]
    public LightningBoltPathScriptBase LightningPathScript;
    [Tooltip("Give the end point some randomization")]
    public float EndPointRandomization = 1.5f;
    [HideInInspector]
    public Action<RaycastHit> CollisionCallback;

    private void CheckCollision()
    {
      RaycastHit hitInfo;
      if (Physics.Raycast(this.SpellStart.transform.position, this.Direction, out hitInfo, this.MaxDistance, (int) this.CollisionMask))
      {
        this.SpellEnd.transform.position = hitInfo.point;
        this.SpellEnd.transform.position += UnityEngine.Random.insideUnitSphere * this.EndPointRandomization;
        this.PlayCollisionSound(this.SpellEnd.transform.position);
        if ((UnityEngine.Object) this.CollisionParticleSystem != (UnityEngine.Object) null)
        {
          this.CollisionParticleSystem.transform.position = hitInfo.point;
          this.CollisionParticleSystem.Play();
        }
        this.ApplyCollisionForce(hitInfo.point);
        if (this.CollisionCallback == null)
          return;
        this.CollisionCallback(hitInfo);
      }
      else
      {
        if ((UnityEngine.Object) this.CollisionParticleSystem != (UnityEngine.Object) null)
          this.CollisionParticleSystem.Stop();
        this.SpellEnd.transform.position = this.SpellStart.transform.position + this.Direction * this.MaxDistance;
        this.SpellEnd.transform.position += UnityEngine.Random.insideUnitSphere * this.EndPointRandomization;
      }
    }

    protected override void Start()
    {
      base.Start();
      this.LightningPathScript.ManualMode = true;
    }

    protected override void LateUpdate()
    {
      base.LateUpdate();
      if (!this.Casting)
        return;
      this.CheckCollision();
    }

    protected override void OnCastSpell() => this.LightningPathScript.ManualMode = false;

    protected override void OnStopSpell() => this.LightningPathScript.ManualMode = true;
  }
}
