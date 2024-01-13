// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningWhipSpell
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningWhipSpell : LightningSpellScript
  {
    [Header("Whip")]
    [Tooltip("Attach the whip to what object")]
    public GameObject AttachTo;
    [Tooltip("Rotate the whip with this object")]
    public GameObject RotateWith;
    [Tooltip("Whip handle")]
    public GameObject WhipHandle;
    [Tooltip("Whip start")]
    public GameObject WhipStart;
    [Tooltip("Whip spring")]
    public GameObject WhipSpring;
    [Tooltip("Whip crack audio source")]
    public AudioSource WhipCrackAudioSource;
    [HideInInspector]
    public Action<Vector3> CollisionCallback;

    private IEnumerator WhipForward()
    {
      LightningWhipSpell lightningWhipSpell = this;
      for (int index = 0; index < lightningWhipSpell.WhipStart.transform.childCount; ++index)
      {
        Rigidbody component = lightningWhipSpell.WhipStart.transform.GetChild(index).gameObject.GetComponent<Rigidbody>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          component.drag = 0.0f;
          component.velocity = Vector3.zero;
          component.angularVelocity = Vector3.zero;
        }
      }
      lightningWhipSpell.WhipSpring.SetActive(true);
      Vector3 position = lightningWhipSpell.WhipStart.GetComponent<Rigidbody>().position;
      Vector3 whipPositionForwards;
      RaycastHit hitInfo;
      Vector3 vector3;
      if (Physics.Raycast(position, lightningWhipSpell.Direction, out hitInfo, lightningWhipSpell.MaxDistance, (int) lightningWhipSpell.CollisionMask))
      {
        Vector3 normalized = (hitInfo.point - position).normalized;
        whipPositionForwards = position + normalized * lightningWhipSpell.MaxDistance;
        vector3 = position - normalized * 25f;
      }
      else
      {
        whipPositionForwards = position + lightningWhipSpell.Direction * lightningWhipSpell.MaxDistance;
        vector3 = position - lightningWhipSpell.Direction * 25f;
      }
      lightningWhipSpell.WhipSpring.GetComponent<Rigidbody>().position = vector3;
      yield return (object) new WaitForSecondsLightning(0.25f);
      lightningWhipSpell.WhipSpring.GetComponent<Rigidbody>().position = whipPositionForwards;
      yield return (object) new WaitForSecondsLightning(0.1f);
      if ((UnityEngine.Object) lightningWhipSpell.WhipCrackAudioSource != (UnityEngine.Object) null)
        lightningWhipSpell.WhipCrackAudioSource.Play();
      yield return (object) new WaitForSecondsLightning(0.1f);
      if ((UnityEngine.Object) lightningWhipSpell.CollisionParticleSystem != (UnityEngine.Object) null)
        lightningWhipSpell.CollisionParticleSystem.Play();
      lightningWhipSpell.ApplyCollisionForce(lightningWhipSpell.SpellEnd.transform.position);
      lightningWhipSpell.WhipSpring.SetActive(false);
      if (lightningWhipSpell.CollisionCallback != null)
        lightningWhipSpell.CollisionCallback(lightningWhipSpell.SpellEnd.transform.position);
      yield return (object) new WaitForSecondsLightning(0.1f);
      for (int index = 0; index < lightningWhipSpell.WhipStart.transform.childCount; ++index)
      {
        Rigidbody component = lightningWhipSpell.WhipStart.transform.GetChild(index).gameObject.GetComponent<Rigidbody>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          component.velocity = Vector3.zero;
          component.angularVelocity = Vector3.zero;
          component.drag = 0.5f;
        }
      }
    }

    protected override void Start()
    {
      base.Start();
      this.WhipSpring.SetActive(false);
      this.WhipHandle.SetActive(false);
    }

    protected override void Update()
    {
      base.Update();
      this.gameObject.transform.position = this.AttachTo.transform.position;
      this.gameObject.transform.rotation = this.RotateWith.transform.rotation;
    }

    protected override void OnCastSpell() => this.StartCoroutine(this.WhipForward());

    protected override void OnStopSpell()
    {
    }

    protected override void OnActivated()
    {
      base.OnActivated();
      this.WhipHandle.SetActive(true);
    }

    protected override void OnDeactivated()
    {
      base.OnDeactivated();
      this.WhipHandle.SetActive(false);
    }
  }
}
