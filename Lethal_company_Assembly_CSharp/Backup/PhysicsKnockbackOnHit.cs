// Decompiled with JetBrains decompiler
// Type: PhysicsKnockbackOnHit
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using UnityEngine;

#nullable disable
[RequireComponent(typeof (Rigidbody))]
public class PhysicsKnockbackOnHit : MonoBehaviour, IHittable
{
  public AudioClip playSFX;

  public void Hit(
    int force,
    Vector3 hitDirection,
    PlayerControllerB playerWhoHit = null,
    bool playHitSFX = false)
  {
    if ((Object) this.gameObject.GetComponent<Rigidbody>() == (Object) null)
      return;
    this.gameObject.GetComponent<Rigidbody>().AddForce(hitDirection * (float) force * 10f, ForceMode.Impulse);
    if (!((Object) this.playSFX != (Object) null) || !(bool) (Object) this.gameObject.GetComponent<AudioSource>())
      return;
    this.gameObject.GetComponent<AudioSource>().PlayOneShot(this.playSFX);
  }
}
