// Decompiled with JetBrains decompiler
// Type: WhoopieCushionTrigger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class WhoopieCushionTrigger : MonoBehaviour
{
  public WhoopieCushionItem itemScript;

  private void OnTriggerEnter(Collider other)
  {
    if (this.itemScript.isHeld || !other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy"))
      return;
    Debug.Log((object) "Collided with whoopie cushion");
    this.itemScript.FartWithDebounce();
  }
}
