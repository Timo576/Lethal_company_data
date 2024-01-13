// Decompiled with JetBrains decompiler
// Type: PlaceableObjectsSurface
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class PlaceableObjectsSurface : NetworkBehaviour
{
  public NetworkObject parentTo;
  public Collider placeableBounds;
  public InteractTrigger triggerScript;
  private float checkHoverTipInterval;

  private void Update()
  {
    if (!((Object) GameNetworkManager.Instance != (Object) null) || !((Object) GameNetworkManager.Instance.localPlayerController != (Object) null))
      return;
    this.triggerScript.interactable = GameNetworkManager.Instance.localPlayerController.isHoldingObject;
  }

  public void PlaceObject(PlayerControllerB playerWhoTriggered)
  {
    if (!playerWhoTriggered.isHoldingObject || !((Object) playerWhoTriggered.currentlyHeldObjectServer != (Object) null))
      return;
    Debug.Log((object) "Placing object in storage");
    Vector3 vector3 = this.itemPlacementPosition(playerWhoTriggered.gameplayCamera.transform, playerWhoTriggered.currentlyHeldObjectServer);
    if (vector3 == Vector3.zero)
      return;
    if ((Object) this.parentTo != (Object) null)
      vector3 = this.parentTo.transform.InverseTransformPoint(vector3);
    playerWhoTriggered.DiscardHeldObject(true, this.parentTo, vector3, false);
    Debug.Log((object) "discard held object called from placeobject");
  }

  private Vector3 itemPlacementPosition(Transform gameplayCamera, GrabbableObject heldObject)
  {
    RaycastHit hitInfo;
    if (!Physics.Raycast(gameplayCamera.position, gameplayCamera.forward, out hitInfo, 7f, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
      return Vector3.zero;
    return this.placeableBounds.bounds.Contains(hitInfo.point) ? hitInfo.point + Vector3.up * heldObject.itemProperties.verticalOffset : this.placeableBounds.ClosestPoint(hitInfo.point);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (PlaceableObjectsSurface);
}
