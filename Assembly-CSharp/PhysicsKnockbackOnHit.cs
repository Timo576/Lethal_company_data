// Decompiled with JetBrains decompiler
// Type: PhysicsKnockbackOnHit
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using UnityEngine;

#nullable disable
[RequireComponent(typeof (Rigidbody))]
public class PhysicsKnockbackOnHit : MonoBehaviour, IHittable
{
  public AudioClip playSFX;

  public bool Hit(
    int force,
    Vector3 hitDirection,
    PlayerControllerB playerWhoHit = null,
    bool playHitSFX = false)
  {
    if ((Object) this.gameObject.GetComponent<Rigidbody>() == (Object) null)
      return false;
    this.gameObject.GetComponent<Rigidbody>().AddForce(hitDirection * (float) force * 10f, ForceMode.Impulse);
    if ((Object) this.playSFX != (Object) null && (bool) (Object) this.gameObject.GetComponent<AudioSource>())
      this.gameObject.GetComponent<AudioSource>().PlayOneShot(this.playSFX);
    return true;
  }
}
