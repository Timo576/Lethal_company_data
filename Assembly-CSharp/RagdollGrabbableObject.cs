// Decompiled with JetBrains decompiler
// Type: RagdollGrabbableObject
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class RagdollGrabbableObject : GrabbableObject
{
  public NetworkVariable<int> bodyID = new NetworkVariable<int>();
  public DeadBodyInfo ragdoll;
  private bool foundRagdollObject;
  private bool bodySetToHold;
  public bool testBody;
  private bool setBodyInElevator;
  private PlayerControllerB previousPlayerHeldBy;
  private bool hasBeenPlaced;
  public bool heldByEnemy;
  private bool heldByEnemyThisFrame;

  public override void Start()
  {
    base.Start();
    if (HoarderBugAI.grabbableObjectsInMap != null && !HoarderBugAI.grabbableObjectsInMap.Contains(this.gameObject))
      HoarderBugAI.grabbableObjectsInMap.Add(this.gameObject);
    if (!((UnityEngine.Object) this.radarIcon != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.radarIcon.gameObject);
  }

  public override void EquipItem()
  {
    base.EquipItem();
    this.previousPlayerHeldBy = this.playerHeldBy;
    this.hasBeenPlaced = false;
  }

  public override void OnPlaceObject()
  {
    base.OnPlaceObject();
    this.hasBeenPlaced = true;
  }

  public override void OnDestroy()
  {
    base.OnDestroy();
    if (!this.foundRagdollObject || !((UnityEngine.Object) this.ragdoll != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.ragdoll.gameObject);
  }

  public override void Update()
  {
    base.Update();
    if (NetworkManager.Singleton.ShutdownInProgress || this.bodyID.Value == -1)
      return;
    if (!this.foundRagdollObject)
    {
      if (this.testBody)
      {
        DeadBodyInfo[] objectsOfType = UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>();
        for (int index = 0; index < objectsOfType.Length; ++index)
        {
          if (objectsOfType[index].playerObjectId == 0)
          {
            this.ragdoll = objectsOfType[index];
            break;
          }
        }
        this.ragdoll.grabBodyObject = (GrabbableObject) this;
        this.parentObject = this.ragdoll.bodyParts[5].transform;
        this.transform.SetParent(this.ragdoll.bodyParts[5].transform);
        this.foundRagdollObject = true;
      }
      else
      {
        if (!((UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[this.bodyID.Value].deadBody != (UnityEngine.Object) null))
          return;
        this.ragdoll = StartOfRound.Instance.allPlayerScripts[this.bodyID.Value].deadBody;
        this.ragdoll.grabBodyObject = (GrabbableObject) this;
        this.parentObject = this.ragdoll.bodyParts[5].transform;
        this.transform.SetParent(this.ragdoll.bodyParts[5].transform);
        this.foundRagdollObject = true;
      }
    }
    if ((UnityEngine.Object) this.ragdoll == (UnityEngine.Object) null)
      return;
    if (this.isHeld || this.heldByEnemy || this.hasBeenPlaced)
    {
      if (this.hasBeenPlaced)
      {
        this.ragdoll.matchPositionExactly = false;
        this.ragdoll.attachedLimb.isKinematic = false;
        this.ragdoll.speedMultiplier = 45f;
        this.ragdoll.maxVelocity = 0.75f;
      }
      if (this.bodySetToHold)
        return;
      if (this.heldByEnemy)
        this.heldByEnemyThisFrame = true;
      else
        this.ragdoll.bodyBleedingHeavily = false;
      this.grabbableToEnemies = false;
      this.bodySetToHold = true;
      this.ragdoll.gameObject.SetActive(true);
      this.ragdoll.SetBodyPartsKinematic(false);
      this.ragdoll.attachedTo = this.transform;
      this.ragdoll.attachedLimb = this.ragdoll.bodyParts[5];
      this.ragdoll.matchPositionExactly = true;
      this.ragdoll.lerpBeforeMatchingPosition = true;
      this.SetRagdollParentToMatchHoldingPlayer();
    }
    else
    {
      if (!this.bodySetToHold)
        return;
      this.bodySetToHold = false;
      this.grabbableToEnemies = true;
      this.ragdoll.attachedTo = (Transform) null;
      this.parentObject = this.ragdoll.bodyParts[5].transform;
      this.transform.SetParent(this.ragdoll.bodyParts[5].transform);
      this.ragdoll.attachedLimb = (Rigidbody) null;
      this.ragdoll.matchPositionExactly = false;
      this.ragdoll.lerpBeforeMatchingPosition = false;
      this.SetRagdollParentToMatchHoldingPlayer();
      this.heldByEnemyThisFrame = false;
    }
  }

  public override void GrabItemFromEnemy(EnemyAI enemy)
  {
    base.GrabItemFromEnemy(enemy);
    this.heldByEnemy = true;
  }

  public override void DiscardItemFromEnemy()
  {
    base.DiscardItemFromEnemy();
    this.heldByEnemy = false;
  }

  private void SetRagdollParentToMatchHoldingPlayer()
  {
    if (this.heldByEnemyThisFrame || !((UnityEngine.Object) this.previousPlayerHeldBy != (UnityEngine.Object) null))
      return;
    if (this.previousPlayerHeldBy.isInElevator && !this.setBodyInElevator)
    {
      this.setBodyInElevator = true;
      this.ragdoll.transform.SetParent(StartOfRound.Instance.elevatorTransform);
    }
    else
    {
      if (this.previousPlayerHeldBy.isInElevator || !this.setBodyInElevator)
        return;
      this.setBodyInElevator = false;
      this.ragdoll.transform.SetParent((Transform) null);
    }
  }

  protected override void __initializeVariables()
  {
    if (this.bodyID == null)
      throw new Exception("RagdollGrabbableObject.bodyID cannot be null. All NetworkVariableBase instances must be initialized.");
    this.bodyID.Initialize((NetworkBehaviour) this);
    this.__nameNetworkVariable((NetworkVariableBase) this.bodyID, "bodyID");
    this.NetworkVariableFields.Add((NetworkVariableBase) this.bodyID);
    base.__initializeVariables();
  }

  protected internal override string __getTypeName() => nameof (RagdollGrabbableObject);
}
