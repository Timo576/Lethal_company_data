// Decompiled with JetBrains decompiler
// Type: GiftBoxItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class GiftBoxItem : GrabbableObject
{
  private GameObject objectInPresent;
  public ParticleSystem PoofParticle;
  public AudioSource presentAudio;
  public AudioClip openGiftAudio;
  private PlayerControllerB previousPlayerHeldBy;
  private bool hasUsedGift;
  private int objectInPresentValue;

  public override void Start()
  {
    base.Start();
    System.Random randomSeed = new System.Random((int) this.targetFloorPosition.x + (int) this.targetFloorPosition.y);
    if (!this.IsServer)
      return;
    List<int> weights = new List<int>(RoundManager.Instance.currentLevel.spawnableScrap.Count);
    for (int index = 0; index < RoundManager.Instance.currentLevel.spawnableScrap.Count; ++index)
    {
      if (RoundManager.Instance.currentLevel.spawnableScrap[index].spawnableItem.itemId == 152767)
        weights.Add(0);
      else
        weights.Add(RoundManager.Instance.currentLevel.spawnableScrap[index].rarity);
    }
    Item spawnableItem = RoundManager.Instance.currentLevel.spawnableScrap[RoundManager.Instance.GetRandomWeightedIndexList(weights, randomSeed)].spawnableItem;
    this.objectInPresent = spawnableItem.spawnPrefab;
    this.objectInPresentValue = (int) ((double) randomSeed.Next(spawnableItem.minValue + 25, spawnableItem.maxValue + 35) * (double) RoundManager.Instance.scrapValueMultiplier);
  }

  public override void EquipItem()
  {
    base.EquipItem();
    this.previousPlayerHeldBy = this.playerHeldBy;
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    if ((UnityEngine.Object) this.playerHeldBy == (UnityEngine.Object) null || this.hasUsedGift)
      return;
    this.hasUsedGift = true;
    this.playerHeldBy.activatingItem = true;
    this.OpenGiftBoxServerRpc();
  }

  public override void PocketItem()
  {
    base.PocketItem();
    this.playerHeldBy.activatingItem = false;
  }

  [ServerRpc(RequireOwnership = false)]
  public void OpenGiftBoxServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2878544999U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2878544999U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    GameObject gameObject = (GameObject) null;
    int num = 0;
    Vector3 vector3 = Vector3.zero;
    if ((UnityEngine.Object) this.objectInPresent == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "Error: There is no object in gift box!");
    }
    else
    {
      Transform parent = (!((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null) || !this.playerHeldBy.isInElevator) && !StartOfRound.Instance.inShipPhase || !((UnityEngine.Object) RoundManager.Instance.spawnedScrapContainer != (UnityEngine.Object) null) ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.spawnedScrapContainer;
      vector3 = this.transform.position + Vector3.up * 0.25f;
      gameObject = UnityEngine.Object.Instantiate<GameObject>(this.objectInPresent, vector3, Quaternion.identity, parent);
      GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
      component.startFallingPosition = vector3;
      this.StartCoroutine(this.SetObjectToHitGroundSFX(component));
      component.targetFloorPosition = component.GetItemFloorPosition(this.transform.position);
      if ((UnityEngine.Object) this.previousPlayerHeldBy != (UnityEngine.Object) null && this.previousPlayerHeldBy.isInHangarShipRoom)
        this.previousPlayerHeldBy.SetItemInElevator(true, true, component);
      num = this.objectInPresentValue;
      component.SetScrapValue(num);
      component.NetworkObject.Spawn();
    }
    if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      this.OpenGiftBoxClientRpc((NetworkObjectReference) gameObject.GetComponent<NetworkObject>(), num, vector3);
    this.OpenGiftBoxNoPresentClientRpc();
  }

  private IEnumerator SetObjectToHitGroundSFX(GrabbableObject gObject)
  {
    yield return (object) new WaitForEndOfFrame();
    Debug.Log((object) ("Setting " + gObject.itemProperties.itemName + " hit ground to false"));
    gObject.reachedFloorTarget = false;
    gObject.hasHitGround = false;
    gObject.fallTime = 0.0f;
  }

  [ClientRpc]
  public void OpenGiftBoxNoPresentClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3328558740U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3328558740U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.PoofParticle.Play();
    this.presentAudio.PlayOneShot(this.openGiftAudio);
    WalkieTalkie.TransmitOneShotAudio(this.presentAudio, this.openGiftAudio);
    RoundManager.Instance.PlayAudibleNoise(this.presentAudio.transform.position, 8f, noiseIsInsideClosedShip: this.isInShipRoom && StartOfRound.Instance.hangarDoorsClosed);
    if (!((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null))
      return;
    this.playerHeldBy.activatingItem = false;
    this.DestroyObjectInHand(this.playerHeldBy);
  }

  [ClientRpc]
  public void OpenGiftBoxClientRpc(
    NetworkObjectReference netObjectRef,
    int presentValue,
    Vector3 startFallingPos)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1252354594U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in netObjectRef, new FastBufferWriter.ForNetworkSerializable());
      BytePacker.WriteValueBitPacked(bufferWriter, presentValue);
      bufferWriter.WriteValueSafe(in startFallingPos);
      this.__endSendClientRpc(ref bufferWriter, 1252354594U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.PoofParticle.Play();
    this.presentAudio.PlayOneShot(this.openGiftAudio);
    WalkieTalkie.TransmitOneShotAudio(this.presentAudio, this.openGiftAudio);
    RoundManager.Instance.PlayAudibleNoise(this.presentAudio.transform.position, 8f, noiseIsInsideClosedShip: this.isInShipRoom && StartOfRound.Instance.hangarDoorsClosed);
    if ((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null)
    {
      this.playerHeldBy.activatingItem = false;
      this.DestroyObjectInHand(this.playerHeldBy);
    }
    if (this.IsServer)
      return;
    this.StartCoroutine(this.waitForGiftPresentToSpawnOnClient(netObjectRef, presentValue, startFallingPos));
  }

  private IEnumerator waitForGiftPresentToSpawnOnClient(
    NetworkObjectReference netObjectRef,
    int presentValue,
    Vector3 startFallingPos)
  {
    GiftBoxItem giftBoxItem = this;
    NetworkObject netObject = (NetworkObject) null;
    float startTime = Time.realtimeSinceStartup;
    while ((double) Time.realtimeSinceStartup - (double) startTime < 8.0 && !netObjectRef.TryGet(out netObject))
      yield return (object) new WaitForSeconds(0.03f);
    if ((UnityEngine.Object) netObject == (UnityEngine.Object) null)
    {
      Debug.Log((object) "No network object found");
    }
    else
    {
      yield return (object) new WaitForEndOfFrame();
      GrabbableObject component = netObject.GetComponent<GrabbableObject>();
      RoundManager.Instance.totalScrapValueInLevel -= (float) giftBoxItem.scrapValue;
      RoundManager.Instance.totalScrapValueInLevel += (float) component.scrapValue;
      component.SetScrapValue(presentValue);
      component.startFallingPosition = startFallingPos;
      component.fallTime = 0.0f;
      component.hasHitGround = false;
      component.reachedFloorTarget = false;
      if ((UnityEngine.Object) giftBoxItem.previousPlayerHeldBy != (UnityEngine.Object) null && giftBoxItem.previousPlayerHeldBy.isInHangarShipRoom)
        giftBoxItem.previousPlayerHeldBy.SetItemInElevator(true, true, component);
    }
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_GiftBoxItem()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2878544999U, new NetworkManager.RpcReceiveHandler(GiftBoxItem.__rpc_handler_2878544999)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3328558740U, new NetworkManager.RpcReceiveHandler(GiftBoxItem.__rpc_handler_3328558740)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1252354594U, new NetworkManager.RpcReceiveHandler(GiftBoxItem.__rpc_handler_1252354594)));
  }

  private static void __rpc_handler_2878544999(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GiftBoxItem) target).OpenGiftBoxServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3328558740(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GiftBoxItem) target).OpenGiftBoxNoPresentClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1252354594(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference netObjectRef;
    reader.ReadValueSafe<NetworkObjectReference>(out netObjectRef, new FastBufferWriter.ForNetworkSerializable());
    int presentValue;
    ByteUnpacker.ReadValueBitPacked(reader, out presentValue);
    Vector3 startFallingPos;
    reader.ReadValueSafe(out startFallingPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GiftBoxItem) target).OpenGiftBoxClientRpc(netObjectRef, presentValue, startFallingPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (GiftBoxItem);
}
