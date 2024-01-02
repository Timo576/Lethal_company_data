// Decompiled with JetBrains decompiler
// Type: CleanPlayerBodyTrigger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using UnityEngine;

#nullable disable
public class CleanPlayerBodyTrigger : MonoBehaviour
{
  private bool enableCleaning = true;

  public void EnableCleaningTrigger(bool enable) => this.enableCleaning = enable;

  private void OnTriggerStay(Collider other)
  {
    if (!this.enableCleaning || !other.CompareTag("Player"))
      return;
    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
    if (!((Object) component != (Object) null))
      return;
    component.RemoveBloodFromBody();
  }
}
