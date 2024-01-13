// Decompiled with JetBrains decompiler
// Type: GrabbableObject
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

#nullable disable
public abstract class GrabbableObject : NetworkBehaviour
{
  public bool grabbable;
  public bool isHeld;
  public bool isHeldByEnemy;
  public bool deactivated;
  [Space(3f)]
  public Transform parentObject;
  public Vector3 targetFloorPosition;
  public Vector3 startFallingPosition;
  public int floorYRot;
  public float fallTime;
  public bool hasHitGround;
  [Space(5f)]
  public int scrapValue;
  public bool itemUsedUp;
  public PlayerControllerB playerHeldBy;
  public bool isPocketed;
  public bool isBeingUsed;
  public bool isInElevator;
  public bool isInShipRoom;
  public bool isInFactory = true;
  [Space(10f)]
  public float useCooldown;
  public float currentUseCooldown;
  [Space(10f)]
  public Item itemProperties;
  public Battery insertedBattery;
  public string customGrabTooltip;
  [HideInInspector]
  public Rigidbody propBody;
  [HideInInspector]
  public Collider[] propColliders;
  [HideInInspector]
  public Vector3 originalScale;
  public bool wasOwnerLastFrame;
  public MeshRenderer mainObjectRenderer;
  private int isSendingItemRPC;
  public bool scrapPersistedThroughRounds;
  public bool heldByPlayerOnServer;
  [HideInInspector]
  public Transform radarIcon;
  public bool reachedFloorTarget;
  [Space(3f)]
  public bool grabbableToEnemies = true;
  private bool hasBeenHeld;

  public virtual int GetItemDataToSave()
  {
    if (!this.itemProperties.saveItemVariable)
      Debug.LogError((object) ("GetItemDataToSave is being called on " + this.itemProperties.itemName + ", which does not have saveItemVariable set true."));
    return 0;
  }

  public virtual void LoadItemSaveData(int saveData)
  {
    if (this.itemProperties.saveItemVariable)
      return;
    Debug.LogError((object) ("LoadItemSaveData is being called on " + this.itemProperties.itemName + ", which does not have saveItemVariable set true."));
  }

  public virtual void Start()
  {
    this.propColliders = this.gameObject.GetComponentsInChildren<Collider>();
    this.originalScale = this.transform.localScale;
    if (this.itemProperties.itemSpawnsOnGround)
    {
      this.startFallingPosition = this.transform.position;
      if ((UnityEngine.Object) this.transform.parent != (UnityEngine.Object) null)
        this.startFallingPosition = this.transform.parent.InverseTransformPoint(this.startFallingPosition);
      this.FallToGround();
    }
    else
    {
      this.fallTime = 1f;
      this.hasHitGround = true;
      this.reachedFloorTarget = true;
      this.targetFloorPosition = this.transform.localPosition;
    }
    if (this.itemProperties.isScrap)
    {
      this.fallTime = 1f;
      this.hasHitGround = true;
    }
    if (this.itemProperties.isScrap && (UnityEngine.Object) RoundManager.Instance.mapPropsContainer != (UnityEngine.Object) null)
      this.radarIcon = UnityEngine.Object.Instantiate<GameObject>(StartOfRound.Instance.itemRadarIconPrefab, RoundManager.Instance.mapPropsContainer.transform).transform;
    if (!this.itemProperties.isScrap)
      HoarderBugAI.grabbableObjectsInMap.Add(this.gameObject);
    foreach (Renderer componentsInChild in this.gameObject.GetComponentsInChildren<MeshRenderer>())
      componentsInChild.renderingLayerMask = 1U;
    foreach (Renderer componentsInChild in this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
      componentsInChild.renderingLayerMask = 1U;
  }

  public void FallToGround(bool randomizePosition = false)
  {
    this.fallTime = 0.0f;
    RaycastHit hitInfo;
    if (Physics.Raycast(this.transform.position, Vector3.down, out hitInfo, 80f, 268437760, QueryTriggerInteraction.Ignore))
    {
      this.targetFloorPosition = hitInfo.point + this.itemProperties.verticalOffset * Vector3.up;
      if ((UnityEngine.Object) this.transform.parent != (UnityEngine.Object) null)
        this.targetFloorPosition = this.transform.parent.InverseTransformPoint(this.targetFloorPosition);
    }
    else
    {
      Debug.Log((object) ("dropping item did not get raycast : " + this.gameObject.name));
      this.targetFloorPosition = this.transform.localPosition;
    }
    if (!randomizePosition)
      return;
    this.targetFloorPosition += new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0.0f, UnityEngine.Random.Range(-0.5f, 0.5f));
  }

  public void EnablePhysics(bool enable)
  {
    for (int index = 0; index < this.propColliders.Length; ++index)
    {
      if (!((UnityEngine.Object) this.propColliders[index] == (UnityEngine.Object) null) && !this.propColliders[index].gameObject.CompareTag("InteractTrigger") && !this.propColliders[index].gameObject.CompareTag("DoNotSet"))
        this.propColliders[index].enabled = enable;
    }
  }

  public virtual void InspectItem()
  {
    if (!this.IsOwner || !((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null) || !this.itemProperties.canBeInspected)
      return;
    this.playerHeldBy.IsInspectingItem = !this.playerHeldBy.IsInspectingItem;
    HUDManager.Instance.SetNearDepthOfFieldEnabled(!this.playerHeldBy.IsInspectingItem);
  }

  public virtual void InteractItem()
  {
  }

  public void GrabItemOnClient()
  {
    if (!this.IsOwner)
    {
      Debug.LogError((object) "GrabItemOnClient was called but player was not the owner.");
    }
    else
    {
      this.SetControlTipsForItem();
      this.GrabItem();
      if (!this.itemProperties.syncGrabFunction)
        return;
      ++this.isSendingItemRPC;
      this.GrabServerRpc();
    }
  }

  public virtual void SetControlTipsForItem()
  {
    HUDManager.Instance.ChangeControlTipMultiple(this.itemProperties.toolTips, true, this.itemProperties);
  }

  public virtual void GrabItem()
  {
  }

  public void UseItemOnClient(bool buttonDown = true)
  {
    if (!this.IsOwner)
    {
      Debug.Log((object) "Can't use item; not owner");
    }
    else
    {
      if (this.RequireCooldown() || !this.UseItemBatteries())
        return;
      if (this.itemProperties.syncUseFunction)
      {
        ++this.isSendingItemRPC;
        this.ActivateItemServerRpc(this.isBeingUsed, buttonDown);
      }
      this.ItemActivate(this.isBeingUsed, buttonDown);
    }
  }

  public bool UseItemBatteries()
  {
    if (this.itemProperties.requiresBattery && (this.insertedBattery == null || this.insertedBattery.empty))
      return false;
    if (this.itemProperties.itemIsTrigger)
    {
      this.insertedBattery.charge = Mathf.Clamp(this.insertedBattery.charge - this.itemProperties.batteryUsage, 0.0f, 1f);
      if ((double) this.insertedBattery.charge <= 0.0)
        this.insertedBattery.empty = true;
      this.isBeingUsed = false;
    }
    else if (this.itemProperties.automaticallySetUsingPower)
      this.isBeingUsed = !this.isBeingUsed;
    return true;
  }

  public virtual void ItemActivate(bool used, bool buttonDown = true)
  {
  }

  public void ItemInteractLeftRightOnClient(bool right)
  {
    if (!this.IsOwner)
    {
      Debug.Log((object) "InteractLeftRight was called but player was not the owner.");
    }
    else
    {
      if (this.RequireCooldown() || !this.UseItemBatteries())
        return;
      this.ItemInteractLeftRight(right);
      if (!this.itemProperties.syncInteractLRFunction)
        return;
      ++this.isSendingItemRPC;
      this.InteractLeftRightServerRpc(right);
    }
  }

  public virtual void ItemInteractLeftRight(bool right)
  {
  }

  public virtual void UseUpBatteries() => this.isBeingUsed = false;

  public virtual void GrabItemFromEnemy(EnemyAI enemy)
  {
  }

  public virtual void DiscardItemFromEnemy()
  {
  }

  public virtual void ChargeBatteries()
  {
  }

  public virtual void DestroyObjectInHand(PlayerControllerB playerHolding)
  {
    this.grabbable = false;
    this.grabbableToEnemies = false;
    this.deactivated = true;
    if ((UnityEngine.Object) playerHolding != (UnityEngine.Object) null)
      playerHolding.activatingItem = false;
    if ((UnityEngine.Object) this.radarIcon != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.radarIcon.gameObject);
    foreach (UnityEngine.Object componentsInChild in this.gameObject.GetComponentsInChildren<MeshRenderer>())
      UnityEngine.Object.Destroy(componentsInChild);
    foreach (UnityEngine.Object componentsInChild in this.gameObject.GetComponentsInChildren<Collider>())
      UnityEngine.Object.Destroy(componentsInChild);
    if (!this.IsOwner || !this.isHeld || this.isPocketed || !((UnityEngine.Object) playerHolding != (UnityEngine.Object) null) || !((UnityEngine.Object) this.playerHeldBy == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController))
      return;
    this.playerHeldBy.DiscardHeldObject();
  }

  public virtual void EquipItem()
  {
    if (this.IsOwner)
    {
      HUDManager.Instance.ClearControlTips();
      this.SetControlTipsForItem();
    }
    this.EnableItemMeshes(true);
    this.isPocketed = false;
    if (this.hasBeenHeld)
      return;
    this.hasBeenHeld = true;
    if (this.isInShipRoom || StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap)
      return;
    RoundManager.Instance.valueOfFoundScrapItems += this.scrapValue;
  }

  public virtual void PocketItem()
  {
    if (this.IsOwner && (UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null)
      this.playerHeldBy.IsInspectingItem = false;
    this.isPocketed = true;
    this.EnableItemMeshes(false);
    this.gameObject.GetComponent<AudioSource>().PlayOneShot(this.itemProperties.pocketSFX, 1f);
  }

  public void DiscardItemOnClient()
  {
    if (!this.IsOwner)
      return;
    this.DiscardItem();
    HUDManager.Instance.ClearControlTips();
    this.SyncBatteryServerRpc((int) ((double) this.insertedBattery.charge * 100.0));
    if (!this.itemProperties.syncDiscardFunction)
      return;
    ++this.isSendingItemRPC;
    this.DiscardItemServerRpc();
  }

  [ServerRpc]
  public void SyncBatteryServerRpc(int charge)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3484508350U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, charge);
      this.__endSendServerRpc(ref bufferWriter, 3484508350U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SyncBatteryClientRpc(charge);
  }

  [ClientRpc]
  public void SyncBatteryClientRpc(int charge)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2670202430U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, charge);
      this.__endSendClientRpc(ref bufferWriter, 2670202430U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    float chargeNumber = (float) charge / 100f;
    this.insertedBattery = new Battery((double) chargeNumber <= 0.0, chargeNumber);
    this.ChargeBatteries();
  }

  public virtual void DiscardItem()
  {
    if (this.IsOwner)
    {
      HUDManager.Instance.ClearControlTips();
      if ((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null)
      {
        this.playerHeldBy.IsInspectingItem = false;
        this.playerHeldBy.activatingItem = false;
      }
    }
    this.playerHeldBy = (PlayerControllerB) null;
  }

  public virtual void LateUpdate()
  {
    if ((UnityEngine.Object) this.parentObject != (UnityEngine.Object) null)
    {
      this.transform.rotation = this.parentObject.rotation;
      this.transform.Rotate(this.itemProperties.rotationOffset);
      this.transform.position = this.parentObject.position;
      this.transform.position += this.parentObject.rotation * this.itemProperties.positionOffset;
    }
    if (!((UnityEngine.Object) this.radarIcon != (UnityEngine.Object) null))
      return;
    this.radarIcon.position = this.transform.position;
  }

  public virtual void FallWithCurve()
  {
    float num = this.startFallingPosition.y - this.targetFloorPosition.y;
    if (this.floorYRot == -1)
      this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(this.itemProperties.restingRotation.x, this.transform.eulerAngles.y, this.itemProperties.restingRotation.z), Mathf.Clamp(14f * Time.deltaTime / num, 0.0f, 1f));
    else
      this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(this.itemProperties.restingRotation.x, (float) (this.floorYRot + this.itemProperties.floorYOffset) + 90f, this.itemProperties.restingRotation.z), Mathf.Clamp(14f * Time.deltaTime / num, 0.0f, 1f));
    if ((double) num > 5.0)
      this.transform.localPosition = Vector3.Lerp(this.startFallingPosition, this.targetFloorPosition, StartOfRound.Instance.objectFallToGroundCurveNoBounce.Evaluate(this.fallTime));
    else
      this.transform.localPosition = Vector3.Lerp(this.startFallingPosition, this.targetFloorPosition, StartOfRound.Instance.objectFallToGroundCurve.Evaluate(this.fallTime));
    this.fallTime += Mathf.Abs(Time.deltaTime * 6f / num);
  }

  public virtual void OnPlaceObject()
  {
  }

  public virtual void OnBroughtToShip()
  {
    if (!((UnityEngine.Object) this.radarIcon != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.radarIcon.gameObject);
  }

  public virtual void Update()
  {
    if ((double) this.currentUseCooldown >= 0.0)
      this.currentUseCooldown -= Time.deltaTime;
    if (this.IsOwner)
    {
      if (this.isBeingUsed && this.itemProperties.requiresBattery)
      {
        if ((double) this.insertedBattery.charge > 0.0)
        {
          if (!this.itemProperties.itemIsTrigger)
            this.insertedBattery.charge -= Time.deltaTime / this.itemProperties.batteryUsage;
        }
        else if (!this.insertedBattery.empty)
        {
          this.insertedBattery.empty = true;
          if (this.isBeingUsed)
          {
            this.isBeingUsed = false;
            this.UseUpBatteries();
            ++this.isSendingItemRPC;
            this.UseUpItemBatteriesServerRpc();
          }
        }
      }
      if (!this.wasOwnerLastFrame)
        this.wasOwnerLastFrame = true;
    }
    else if (this.wasOwnerLastFrame)
      this.wasOwnerLastFrame = false;
    if (this.isHeld || !((UnityEngine.Object) this.parentObject == (UnityEngine.Object) null))
      return;
    if ((double) this.fallTime < 1.0)
    {
      this.reachedFloorTarget = false;
      this.FallWithCurve();
      if ((double) this.transform.localPosition.y - (double) this.targetFloorPosition.y >= 0.10000000149011612 || this.hasHitGround)
        return;
      this.PlayDropSFX();
      this.OnHitGround();
    }
    else
    {
      if (!this.reachedFloorTarget)
      {
        this.reachedFloorTarget = true;
        if (this.floorYRot == -1)
          this.transform.rotation = Quaternion.Euler(this.itemProperties.restingRotation.x, this.transform.eulerAngles.y, this.itemProperties.restingRotation.z);
        else
          this.transform.rotation = Quaternion.Euler(this.itemProperties.restingRotation.x, (float) (this.floorYRot + this.itemProperties.floorYOffset) + 90f, this.itemProperties.restingRotation.z);
      }
      this.transform.localPosition = this.targetFloorPosition;
    }
  }

  public virtual void OnHitGround()
  {
  }

  private void PlayDropSFX()
  {
    if ((UnityEngine.Object) this.itemProperties.dropSFX != (UnityEngine.Object) null)
    {
      this.gameObject.GetComponent<AudioSource>().PlayOneShot(this.itemProperties.dropSFX);
      if (this.IsOwner)
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, 8f, noiseIsInsideClosedShip: this.isInElevator && StartOfRound.Instance.hangarDoorsClosed, noiseID: 941);
    }
    this.hasHitGround = true;
  }

  public void SetScrapValue(int setValueTo)
  {
    this.scrapValue = setValueTo;
    ScanNodeProperties componentInChildren = this.gameObject.GetComponentInChildren<ScanNodeProperties>();
    if ((UnityEngine.Object) componentInChildren == (UnityEngine.Object) null)
    {
      Debug.LogError((object) ("Scan node is missing for item!: " + this.gameObject.name));
    }
    else
    {
      componentInChildren.subText = string.Format("Value: ${0}", (object) setValueTo);
      componentInChildren.scrapValue = setValueTo;
    }
  }

  public bool RequireCooldown()
  {
    if ((double) this.useCooldown <= 0.0 || this.itemProperties.holdButtonUse && this.isBeingUsed)
      return false;
    if ((double) this.currentUseCooldown > 0.0)
      return true;
    this.currentUseCooldown = this.useCooldown;
    return false;
  }

  [ServerRpc(RequireOwnership = false)]
  private void InteractLeftRightServerRpc(bool right)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1469591241U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in right, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 1469591241U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.InteractLeftRightClientRpc(right);
  }

  [ClientRpc]
  private void InteractLeftRightClientRpc(bool right)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3081511085U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in right, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 3081511085U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.ItemInteractLeftRight(right);
  }

  [ServerRpc(RequireOwnership = false)]
  private void GrabServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2618697776U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2618697776U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.GrabClientRpc();
  }

  [ClientRpc]
  private void GrabClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1334815929U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1334815929U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.GrabItem();
  }

  [ServerRpc(RequireOwnership = false)]
  private void ActivateItemServerRpc(bool onOff, bool buttonDown)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4280509730U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in onOff, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in buttonDown, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 4280509730U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ActivateItemClientRpc(onOff, buttonDown);
  }

  [ClientRpc]
  private void ActivateItemClientRpc(bool onOff, bool buttonDown)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1761213193U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in onOff, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in buttonDown, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1761213193U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.isBeingUsed = onOff;
    this.ItemActivate(onOff, buttonDown);
  }

  [ServerRpc(RequireOwnership = false)]
  private void DiscardItemServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1974688543U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1974688543U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.DiscardItemClientRpc();
  }

  [ClientRpc]
  private void DiscardItemClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(335835173U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 335835173U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.DiscardItem();
  }

  [ServerRpc(RequireOwnership = false)]
  private void UseUpItemBatteriesServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2025123357U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2025123357U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.UseUpItemBatteriesClientRpc();
  }

  [ClientRpc]
  private void UseUpItemBatteriesClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(738171084U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 738171084U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.UseUpBatteries();
  }

  [ServerRpc(RequireOwnership = false)]
  private void EquipItemServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(947748389U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 947748389U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.EquipItemClientRpc();
  }

  [ClientRpc]
  private void EquipItemClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1898191537U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1898191537U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.EquipItem();
  }

  [ServerRpc(RequireOwnership = false)]
  private void PocketItemServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(101807903U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 101807903U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PocketItemClientRpc();
  }

  [ClientRpc]
  private void PocketItemClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3399384424U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3399384424U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.PocketItem();
  }

  public void ChangeOwnershipOfProp(ulong clientId)
  {
    this.ChangeOwnershipOfPropServerRpc(clientId);
  }

  [ServerRpc(RequireOwnership = false)]
  private void ChangeOwnershipOfPropServerRpc(ulong NewOwner)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1391130874U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, NewOwner);
      this.__endSendServerRpc(ref bufferWriter, 1391130874U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    try
    {
      this.gameObject.GetComponent<NetworkRigidbodyModifiable>().kinematicOnOwner = true;
      this.transform.SetParent(this.playerHeldBy.localItemHolder, true);
      this.gameObject.GetComponent<ClientNetworkTransform>().InLocalSpace = true;
      this.transform.localPosition = Vector3.zero;
      this.transform.localEulerAngles = Vector3.zero;
      this.playerHeldBy.grabSetParentServer = false;
      this.gameObject.GetComponent<NetworkObject>().ChangeOwnership(NewOwner);
    }
    catch (Exception ex)
    {
      Debug.Log((object) string.Format("Failed to transfer ownership of prop to client: {0}", (object) ex));
    }
  }

  public void EnableItemMeshes(bool enable)
  {
    MeshRenderer[] componentsInChildren1 = this.gameObject.GetComponentsInChildren<MeshRenderer>();
    for (int index = 0; index < componentsInChildren1.Length; ++index)
    {
      if (!componentsInChildren1[index].gameObject.CompareTag("DoNotSet") && !componentsInChildren1[index].gameObject.CompareTag("InteractTrigger"))
        componentsInChildren1[index].enabled = enable;
    }
    SkinnedMeshRenderer[] componentsInChildren2 = this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
    for (int index = 0; index < componentsInChildren2.Length; ++index)
    {
      componentsInChildren2[index].enabled = enable;
      Debug.Log((object) ("DISABLING/ENABLING SKINNEDMESH: " + componentsInChildren2[index].gameObject.name));
    }
  }

  public Vector3 GetItemFloorPosition(Vector3 startPosition = default (Vector3))
  {
    if (startPosition == Vector3.zero)
      startPosition = this.transform.position;
    RaycastHit hitInfo;
    return Physics.Raycast(startPosition, -Vector3.up, out hitInfo, 80f, 268437761, QueryTriggerInteraction.Ignore) ? hitInfo.point + Vector3.up * 0.04f + this.itemProperties.verticalOffset * Vector3.up : startPosition;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_GrabbableObject()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3484508350U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_3484508350)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2670202430U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_2670202430)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1469591241U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_1469591241)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3081511085U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_3081511085)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2618697776U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_2618697776)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1334815929U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_1334815929)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4280509730U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_4280509730)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1761213193U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_1761213193)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1974688543U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_1974688543)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(335835173U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_335835173)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2025123357U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_2025123357)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(738171084U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_738171084)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(947748389U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_947748389)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1898191537U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_1898191537)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(101807903U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_101807903)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3399384424U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_3399384424)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1391130874U, new NetworkManager.RpcReceiveHandler(GrabbableObject.__rpc_handler_1391130874)));
  }

  private static void __rpc_handler_3484508350(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      int charge;
      ByteUnpacker.ReadValueBitPacked(reader, out charge);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((GrabbableObject) target).SyncBatteryServerRpc(charge);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2670202430(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int charge;
    ByteUnpacker.ReadValueBitPacked(reader, out charge);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GrabbableObject) target).SyncBatteryClientRpc(charge);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1469591241(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool right;
    reader.ReadValueSafe<bool>(out right, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GrabbableObject) target).InteractLeftRightServerRpc(right);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3081511085(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool right;
    reader.ReadValueSafe<bool>(out right, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GrabbableObject) target).InteractLeftRightClientRpc(right);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2618697776(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GrabbableObject) target).GrabServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1334815929(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GrabbableObject) target).GrabClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4280509730(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool onOff;
    reader.ReadValueSafe<bool>(out onOff, new FastBufferWriter.ForPrimitives());
    bool buttonDown;
    reader.ReadValueSafe<bool>(out buttonDown, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GrabbableObject) target).ActivateItemServerRpc(onOff, buttonDown);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1761213193(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool onOff;
    reader.ReadValueSafe<bool>(out onOff, new FastBufferWriter.ForPrimitives());
    bool buttonDown;
    reader.ReadValueSafe<bool>(out buttonDown, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GrabbableObject) target).ActivateItemClientRpc(onOff, buttonDown);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1974688543(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GrabbableObject) target).DiscardItemServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_335835173(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GrabbableObject) target).DiscardItemClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2025123357(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GrabbableObject) target).UseUpItemBatteriesServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_738171084(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GrabbableObject) target).UseUpItemBatteriesClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_947748389(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GrabbableObject) target).EquipItemServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1898191537(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GrabbableObject) target).EquipItemClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_101807903(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GrabbableObject) target).PocketItemServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3399384424(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GrabbableObject) target).PocketItemClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1391130874(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ulong NewOwner;
    ByteUnpacker.ReadValueBitPacked(reader, out NewOwner);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GrabbableObject) target).ChangeOwnershipOfPropServerRpc(NewOwner);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (GrabbableObject);
}
