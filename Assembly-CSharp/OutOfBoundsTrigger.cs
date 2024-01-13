// Decompiled with JetBrains decompiler
// Type: OutOfBoundsTrigger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using UnityEngine;

#nullable disable
public class OutOfBoundsTrigger : MonoBehaviour
{
  private StartOfRound playersManager;
  public bool disableWhenRoundStarts;

  private void Start() => this.playersManager = Object.FindObjectOfType<StartOfRound>();

  private void OnTriggerEnter(Collider other)
  {
    if (this.disableWhenRoundStarts && !this.playersManager.inShipPhase)
      return;
    if (other.tag.StartsWith("PlayerRagdoll"))
    {
      DeadBodyInfo componentInParent = other.GetComponentInParent<DeadBodyInfo>();
      if (!((Object) componentInParent != (Object) null))
        return;
      ++componentInParent.timesOutOfBounds;
      if (componentInParent.timesOutOfBounds > 2)
        componentInParent.SetBodyPartsKinematic();
      else
        componentInParent.ResetRagdollPosition();
    }
    else
    {
      if (!(other.tag == "Player"))
        return;
      PlayerControllerB component = other.GetComponent<PlayerControllerB>();
      if ((Object) GameNetworkManager.Instance.localPlayerController != (Object) component)
        return;
      component.ResetFallGravity();
      if (!((Object) component != (Object) null))
        return;
      if (!this.playersManager.shipDoorsEnabled)
        this.playersManager.ForcePlayerIntoShip();
      else if (component.isInsideFactory)
        component.TeleportPlayer(RoundManager.FindMainEntrancePosition(true));
      else if (component.isInHangarShipRoom)
        component.TeleportPlayer(this.playersManager.playerSpawnPositions[0].position);
      else
        component.TeleportPlayer(this.playersManager.outsideShipSpawnPosition.position);
    }
  }
}
