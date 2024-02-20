// Decompiled with JetBrains decompiler
// Type: KeyItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class KeyItem : GrabbableObject
{
  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    RaycastHit hitInfo;
    if ((Object) this.playerHeldBy == (Object) null || !this.IsOwner || !Physics.Raycast(new Ray(this.playerHeldBy.gameplayCamera.transform.position, this.playerHeldBy.gameplayCamera.transform.forward), out hitInfo, 3f, 2816))
      return;
    DoorLock component = hitInfo.transform.GetComponent<DoorLock>();
    if (!((Object) component != (Object) null) || !component.isLocked || component.isPickingLock)
      return;
    component.UnlockDoorSyncWithServer();
    this.playerHeldBy.DespawnHeldObject();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (KeyItem);
}
