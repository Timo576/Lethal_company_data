﻿// Decompiled with JetBrains decompiler
// Type: CompanyMonsterCollisionDetect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class CompanyMonsterCollisionDetect : MonoBehaviour
{
  public int monsterAnimationID;

  private void OnTriggerEnter(Collider other)
  {
    if ((Object) NetworkManager.Singleton == (Object) null || !other.CompareTag("Player"))
      return;
    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
    if (!((Object) component != (Object) null) || component.isPlayerDead || !component.IsOwner)
      return;
    Object.FindObjectOfType<DepositItemsDesk>().CollisionDetect(this.monsterAnimationID);
  }
}
