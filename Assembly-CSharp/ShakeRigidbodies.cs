// Decompiled with JetBrains decompiler
// Type: ShakeRigidbodies
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class ShakeRigidbodies : MonoBehaviour
{
  public Rigidbody[] rigidBodies;
  public float shakeTimer = 8f;
  public float shakeIntensity;
  private bool shaking = true;

  private void Update()
  {
    if ((double) this.shakeTimer > 0.0)
      this.shakeTimer -= Time.deltaTime;
    else
      this.shaking = false;
  }

  private void FixedUpdate()
  {
    if (!this.shaking)
      return;
    for (int index = 0; index < this.rigidBodies.Length; ++index)
      this.rigidBodies[index].AddForce(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * this.shakeIntensity, ForceMode.Force);
  }
}
