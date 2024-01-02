// Decompiled with JetBrains decompiler
// Type: HauntedMaskItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class HauntedMaskItem : GrabbableObject, IVisibleThreat
{
  private bool maskOn;
  private bool attaching;
  private bool clampedToHead;
  private float lastIntervalCheck;
  private float attachTimer = 5f;
  private bool finishedAttaching;
  public AudioSource maskAudio;
  public AudioClip maskAttachAudio;
  public AudioClip maskAttachAudioLocal;
  public Animator maskAnimator;
  public MeshRenderer maskEyesFilled;
  public GameObject headMaskPrefab;
  public Transform currentHeadMask;
  public Vector3 headPositionOffset;
  public Vector3 headRotationOffset;
  private PlayerControllerB previousPlayerHeldBy;
  public EnemyType mimicEnemy;
  private bool holdingLastFrame;
  public bool maskIsHaunted = true;
  public int maskTypeId;

  ThreatType IVisibleThreat.type => ThreatType.Item;

  int IVisibleThreat.GetInterestLevel() => 0;

  int IVisibleThreat.GetThreatLevel(Vector3 seenByPosition) => this.isHeld ? 3 : 1;

  Transform IVisibleThreat.GetThreatLookTransform() => this.transform;

  Transform IVisibleThreat.GetThreatTransform() => this.transform;

  Vector3 IVisibleThreat.GetThreatVelocity() => Vector3.zero;

  float IVisibleThreat.GetVisibility() => 1f;

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    if (this.attaching || this.finishedAttaching || (UnityEngine.Object) this.playerHeldBy == (UnityEngine.Object) null || !this.IsOwner)
      return;
    this.playerHeldBy.playerBodyAnimator.SetBool("HoldMask", buttonDown);
    Debug.Log((object) "attaching: {attaching}; finishedAttaching: {finishedAttaching}");
    Debug.Log((object) string.Format("Setting maskOn {0}", (object) buttonDown));
    this.maskOn = buttonDown;
    this.playerHeldBy.activatingItem = buttonDown;
  }

  public override void EquipItem()
  {
    base.EquipItem();
    this.lastIntervalCheck = Time.realtimeSinceStartup + 10f;
    this.previousPlayerHeldBy = this.playerHeldBy;
    this.holdingLastFrame = true;
  }

  public override void DiscardItem()
  {
    base.DiscardItem();
    if ((UnityEngine.Object) this.currentHeadMask != (UnityEngine.Object) null)
    {
      Debug.Log((object) "Discard item called; not going through since headmask is not null");
    }
    else
    {
      Debug.Log((object) string.Format("Discard item called; headmask null: {0}", (object) ((UnityEngine.Object) this.currentHeadMask == (UnityEngine.Object) null)));
      this.previousPlayerHeldBy.activatingItem = false;
      this.maskOn = false;
      this.CancelAttachToPlayerOnLocalClient();
    }
  }

  public override void PocketItem()
  {
    base.PocketItem();
    if ((UnityEngine.Object) this.currentHeadMask != (UnityEngine.Object) null)
    {
      Debug.Log((object) "Discard item called; not going through since headmask is not null");
    }
    else
    {
      Debug.Log((object) string.Format("Discard item called; headmask null: {0}", (object) ((UnityEngine.Object) this.currentHeadMask == (UnityEngine.Object) null)));
      this.maskOn = false;
      this.playerHeldBy.activatingItem = false;
      this.CancelAttachToPlayerOnLocalClient();
    }
  }

  private void CancelAttachToPlayerOnLocalClient()
  {
    this.attachTimer = 8f;
    this.attaching = false;
    this.maskAnimator.SetBool("attaching", false);
    this.finishedAttaching = false;
    if ((UnityEngine.Object) this.currentHeadMask != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.currentHeadMask.gameObject);
    if (this.holdingLastFrame)
      this.holdingLastFrame = false;
    try
    {
      if ((UnityEngine.Object) this.previousPlayerHeldBy.currentVoiceChatAudioSource == (UnityEngine.Object) null)
        StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
      if (!((UnityEngine.Object) this.previousPlayerHeldBy.currentVoiceChatAudioSource != (UnityEngine.Object) null))
        return;
      this.previousPlayerHeldBy.currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>().lowpassResonanceQ = 1f;
      OccludeAudio component = this.previousPlayerHeldBy.currentVoiceChatAudioSource.GetComponent<OccludeAudio>();
      component.overridingLowPass = false;
      component.lowPassOverride = 20000f;
      this.previousPlayerHeldBy.voiceMuffledByEnemy = false;
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Caught exception while attempting to unmuffle player voice from mask item: {0}", (object) ex));
    }
  }

  public void BeginAttachment()
  {
    if (!this.IsOwner)
      return;
    this.AttachToPlayerOnLocalClient();
    this.AttachServerRpc();
  }

  [ServerRpc]
  public void AttachServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2665559382U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2665559382U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.AttachClientRpc();
  }

  [ClientRpc]
  public void AttachClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2055165511U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2055165511U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.AttachToPlayerOnLocalClient();
  }

  private void AttachToPlayerOnLocalClient()
  {
    this.attaching = true;
    this.maskAnimator.SetBool("attaching", true);
    this.maskEyesFilled.enabled = true;
    try
    {
      if ((UnityEngine.Object) this.previousPlayerHeldBy.currentVoiceChatAudioSource == (UnityEngine.Object) null)
        StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
      if ((UnityEngine.Object) this.previousPlayerHeldBy.currentVoiceChatAudioSource != (UnityEngine.Object) null)
      {
        this.previousPlayerHeldBy.currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>().lowpassResonanceQ = 3f;
        OccludeAudio component = this.previousPlayerHeldBy.currentVoiceChatAudioSource.GetComponent<OccludeAudio>();
        component.overridingLowPass = true;
        component.lowPassOverride = 300f;
        this.previousPlayerHeldBy.voiceMuffledByEnemy = true;
      }
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Caught exception while attempting to muffle player voice from mask item: {0}", (object) ex));
    }
    if (this.IsOwner)
    {
      HUDManager.Instance.UIAudio.PlayOneShot(this.maskAttachAudioLocal, 1f);
    }
    else
    {
      this.maskAudio.PlayOneShot(this.maskAttachAudio, 1f);
      WalkieTalkie.TransmitOneShotAudio(this.maskAudio, this.maskAttachAudio);
    }
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, 8f, 0.6f, noiseIsInsideClosedShip: this.isInShipRoom && StartOfRound.Instance.hangarDoorsClosed);
  }

  public void MaskClampToHeadAnimationEvent()
  {
    Debug.Log((object) "Mask clamp animation event called");
    if (!this.attaching || (UnityEngine.Object) this.previousPlayerHeldBy == (UnityEngine.Object) null)
      return;
    Debug.Log((object) "Creating currentHeadMask");
    this.currentHeadMask = UnityEngine.Object.Instantiate<GameObject>(this.headMaskPrefab, (Transform) null).transform;
    this.PositionHeadMaskWithOffset();
    this.previousPlayerHeldBy.playerBodyAnimator.SetBool("HoldMask", false);
    Debug.Log((object) string.Format("Destroying object in hand; headmask null: {0}", (object) ((UnityEngine.Object) this.currentHeadMask == (UnityEngine.Object) null)));
    this.DestroyObjectInHand(this.previousPlayerHeldBy);
    this.clampedToHead = true;
  }

  private void FinishAttaching()
  {
    if (!this.IsOwner || this.finishedAttaching)
      return;
    this.finishedAttaching = true;
    if (!this.previousPlayerHeldBy.AllowPlayerDeath())
    {
      Debug.Log((object) "Player could not die so the mask did not spawn a mimic");
      this.CancelAttachToPlayerOnLocalClient();
    }
    else
    {
      bool isInsideFactory = this.previousPlayerHeldBy.isInsideFactory;
      Vector3 position = this.previousPlayerHeldBy.transform.position;
      this.previousPlayerHeldBy.KillPlayer(Vector3.zero, causeOfDeath: CauseOfDeath.Suffocation, deathAnimation: this.maskTypeId);
      this.CreateMimicServerRpc(isInsideFactory, position);
    }
  }

  [ServerRpc]
  public void CreateMimicServerRpc(bool inFactory, Vector3 playerPositionAtDeath)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1065539967U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in inFactory, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe(in playerPositionAtDeath);
      this.__endSendServerRpc(ref bufferWriter, 1065539967U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if ((UnityEngine.Object) this.previousPlayerHeldBy == (UnityEngine.Object) null)
      Debug.LogError((object) "Previousplayerheldby is null so the mask mimic could not be spawned");
    Debug.Log((object) "Server creating mimic from mask");
    Vector3 navMeshPosition = RoundManager.Instance.GetNavMeshPosition(playerPositionAtDeath, sampleRadius: 10f);
    if (RoundManager.Instance.GotNavMeshPositionResult)
    {
      if ((UnityEngine.Object) this.mimicEnemy == (UnityEngine.Object) null)
      {
        Debug.Log((object) "No mimic enemy set for mask");
      }
      else
      {
        NetworkObjectReference netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(navMeshPosition, this.previousPlayerHeldBy.transform.eulerAngles.y, -1, this.mimicEnemy);
        NetworkObject networkObject;
        if (netObjectRef.TryGet(out networkObject))
        {
          Debug.Log((object) "Got network object for mask enemy");
          MaskedPlayerEnemy component = networkObject.GetComponent<MaskedPlayerEnemy>();
          component.SetSuit(this.previousPlayerHeldBy.currentSuitID);
          component.mimickingPlayer = this.previousPlayerHeldBy;
          component.SetEnemyOutside(!inFactory);
          component.SetVisibilityOfMaskedEnemy();
          component.SetMaskType(this.maskTypeId);
          this.previousPlayerHeldBy.redirectToEnemy = (EnemyAI) component;
          if ((UnityEngine.Object) this.previousPlayerHeldBy.deadBody != (UnityEngine.Object) null)
            this.previousPlayerHeldBy.deadBody.DeactivateBody(false);
        }
        this.CreateMimicClientRpc(netObjectRef, inFactory);
      }
    }
    else
      Debug.Log((object) "No nav mesh found; no mimic could be created");
  }

  [ClientRpc]
  public void CreateMimicClientRpc(NetworkObjectReference netObjectRef, bool inFactory)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3721656136U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in netObjectRef, new FastBufferWriter.ForNetworkSerializable());
      bufferWriter.WriteValueSafe<bool>(in inFactory, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 3721656136U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsServer)
      return;
    this.StartCoroutine(this.waitForMimicEnemySpawn(netObjectRef, inFactory));
  }

  private IEnumerator waitForMimicEnemySpawn(NetworkObjectReference netObjectRef, bool inFactory)
  {
    NetworkObject netObject = (NetworkObject) null;
    float startTime = Time.realtimeSinceStartup;
    yield return (object) new WaitUntil((Func<bool>) (() => (double) Time.realtimeSinceStartup - (double) startTime > 20.0 || netObjectRef.TryGet(out netObject)));
    if ((UnityEngine.Object) this.previousPlayerHeldBy.deadBody == (UnityEngine.Object) null)
    {
      startTime = Time.realtimeSinceStartup;
      yield return (object) new WaitUntil((Func<bool>) (() => (double) Time.realtimeSinceStartup - (double) startTime > 20.0 || (UnityEngine.Object) this.previousPlayerHeldBy.deadBody != (UnityEngine.Object) null));
    }
    if (!((UnityEngine.Object) this.previousPlayerHeldBy.deadBody == (UnityEngine.Object) null))
    {
      this.previousPlayerHeldBy.deadBody.DeactivateBody(false);
      if ((UnityEngine.Object) netObject != (UnityEngine.Object) null)
      {
        Debug.Log((object) "Got network object for mask enemy client");
        MaskedPlayerEnemy component = netObject.GetComponent<MaskedPlayerEnemy>();
        component.mimickingPlayer = this.previousPlayerHeldBy;
        component.SetSuit(this.previousPlayerHeldBy.currentSuitID);
        component.SetEnemyOutside(!inFactory);
        component.SetVisibilityOfMaskedEnemy();
        component.SetMaskType(this.maskTypeId);
        this.previousPlayerHeldBy.redirectToEnemy = (EnemyAI) component;
      }
    }
  }

  public override void Update()
  {
    base.Update();
    if (!this.maskIsHaunted || !this.IsOwner || (UnityEngine.Object) this.previousPlayerHeldBy == (UnityEngine.Object) null || !this.maskOn || !this.holdingLastFrame || this.finishedAttaching)
      return;
    if (!this.attaching)
    {
      if (StartOfRound.Instance.shipIsLeaving || StartOfRound.Instance.inShipPhase && (UnityEngine.Object) StartOfRound.Instance.testRoom == (UnityEngine.Object) null || (double) Time.realtimeSinceStartup <= (double) this.lastIntervalCheck)
        return;
      this.lastIntervalCheck = Time.realtimeSinceStartup + 5f;
      if (UnityEngine.Random.Range(0, 100) >= 65)
        return;
      Debug.Log((object) "Got 15% chance");
      this.BeginAttachment();
    }
    else
    {
      this.attachTimer -= Time.deltaTime;
      if ((double) this.attachTimer > 0.0)
        return;
      this.FinishAttaching();
    }
  }

  public override void OnDestroy()
  {
    base.OnDestroy();
    if (!((UnityEngine.Object) this.currentHeadMask != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.currentHeadMask.gameObject);
  }

  public override void LateUpdate()
  {
    base.LateUpdate();
    if ((UnityEngine.Object) this.previousPlayerHeldBy == (UnityEngine.Object) null || !this.clampedToHead || !((UnityEngine.Object) this.currentHeadMask != (UnityEngine.Object) null))
      return;
    if (this.previousPlayerHeldBy.isPlayerDead)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.currentHeadMask.gameObject);
    else
      this.PositionHeadMaskWithOffset();
  }

  private void PositionHeadMaskWithOffset()
  {
    if (this.IsOwner)
    {
      this.currentHeadMask.rotation = this.previousPlayerHeldBy.gameplayCamera.transform.rotation;
      this.currentHeadMask.Rotate(this.headRotationOffset);
      this.currentHeadMask.position = this.previousPlayerHeldBy.gameplayCamera.transform.position;
      this.currentHeadMask.position += this.previousPlayerHeldBy.gameplayCamera.transform.rotation * this.headPositionOffset;
    }
    else
    {
      this.currentHeadMask.rotation = this.previousPlayerHeldBy.playerGlobalHead.rotation;
      this.currentHeadMask.Rotate(this.headRotationOffset);
      this.currentHeadMask.position = this.previousPlayerHeldBy.playerGlobalHead.position;
      this.currentHeadMask.position += this.previousPlayerHeldBy.playerGlobalHead.rotation * (this.headPositionOffset + Vector3.up * 0.25f);
    }
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_HauntedMaskItem()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2665559382U, new NetworkManager.RpcReceiveHandler(HauntedMaskItem.__rpc_handler_2665559382)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2055165511U, new NetworkManager.RpcReceiveHandler(HauntedMaskItem.__rpc_handler_2055165511)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1065539967U, new NetworkManager.RpcReceiveHandler(HauntedMaskItem.__rpc_handler_1065539967)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3721656136U, new NetworkManager.RpcReceiveHandler(HauntedMaskItem.__rpc_handler_3721656136)));
  }

  private static void __rpc_handler_2665559382(
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
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((HauntedMaskItem) target).AttachServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2055165511(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HauntedMaskItem) target).AttachClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1065539967(
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
      bool inFactory;
      reader.ReadValueSafe<bool>(out inFactory, new FastBufferWriter.ForPrimitives());
      Vector3 playerPositionAtDeath;
      reader.ReadValueSafe(out playerPositionAtDeath);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((HauntedMaskItem) target).CreateMimicServerRpc(inFactory, playerPositionAtDeath);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3721656136(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference netObjectRef;
    reader.ReadValueSafe<NetworkObjectReference>(out netObjectRef, new FastBufferWriter.ForNetworkSerializable());
    bool inFactory;
    reader.ReadValueSafe<bool>(out inFactory, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HauntedMaskItem) target).CreateMimicClientRpc(netObjectRef, inFactory);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (HauntedMaskItem);
}
