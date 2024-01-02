// Decompiled with JetBrains decompiler
// Type: BridgeTrigger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
public class BridgeTrigger : NetworkBehaviour
{
  public float bridgeDurability = 1f;
  private PlayerControllerB playerOnBridge;
  private List<PlayerControllerB> playersOnBridge = new List<PlayerControllerB>();
  public AudioSource bridgeAudioSource;
  public AudioClip[] bridgeCreakSFX;
  public AudioClip bridgeFallSFX;
  public Animator bridgeAnimator;
  private bool hasBridgeFallen;
  public Transform bridgePhysicsPartsContainer;
  private bool giantOnBridge;
  private bool giantOnBridgeLastFrame;
  public Collider[] fallenBridgeColliders;

  private void OnEnable()
  {
    StartOfRound.Instance.playerTeleportedEvent.AddListener(new UnityAction<PlayerControllerB>(this.RemovePlayerFromBridge));
  }

  private void OnDisable()
  {
    StartOfRound.Instance.playerTeleportedEvent.RemoveListener(new UnityAction<PlayerControllerB>(this.RemovePlayerFromBridge));
  }

  private void Update()
  {
    if (this.hasBridgeFallen)
      return;
    if (this.giantOnBridge)
      this.bridgeDurability -= Time.deltaTime / 6f;
    if (this.playersOnBridge.Count > 0)
    {
      this.bridgeDurability = Mathf.Clamp(this.bridgeDurability - Time.deltaTime * (0.02f * (float) (this.playersOnBridge.Count * this.playersOnBridge.Count)), 0.0f, 1f);
      for (int index = 0; index < this.playersOnBridge.Count; ++index)
      {
        if ((double) this.playersOnBridge[index].carryWeight > 1.1000000238418579)
          this.bridgeDurability -= Time.deltaTime * (0.04f * this.playersOnBridge[index].carryWeight);
      }
    }
    else if ((double) this.bridgeDurability < 1.0 && !this.giantOnBridge)
      this.bridgeDurability = Mathf.Clamp(this.bridgeDurability + Time.deltaTime * 0.2f, 0.0f, 1f);
    if (this.IsServer && (double) this.bridgeDurability <= 0.0 && !this.hasBridgeFallen)
    {
      this.hasBridgeFallen = true;
      this.BridgeFallServerRpc();
      Debug.Log((object) "Bridge collapsed! On server");
    }
    this.bridgeAnimator.SetFloat("durability", Mathf.Clamp(Mathf.Abs(this.bridgeDurability - 1f), 0.0f, 1f));
  }

  private void LateUpdate()
  {
    if (!this.giantOnBridge)
      return;
    if (this.giantOnBridgeLastFrame)
    {
      this.giantOnBridge = false;
      this.giantOnBridgeLastFrame = false;
    }
    else
      this.giantOnBridgeLastFrame = true;
  }

  [ServerRpc]
  public void BridgeFallServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2883846656U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2883846656U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.BridgeFallClientRpc();
  }

  [ClientRpc]
  public void BridgeFallClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(123213822U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 123213822U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) "Bridge collapsed! On client");
    this.hasBridgeFallen = true;
    this.bridgeAnimator.SetTrigger("Fall");
    this.EnableFallenBridgeColliders();
    this.bridgeAudioSource.PlayOneShot(this.bridgeFallSFX);
    float num = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.bridgeAudioSource.transform.position);
    if ((double) num < 30.0)
    {
      HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
      Debug.Log((object) "Shaking screen!!!");
    }
    else
    {
      if ((double) num >= 50.0)
        return;
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
    }
  }

  private void EnableFallenBridgeColliders()
  {
    for (int index = 0; index < this.fallenBridgeColliders.Length; ++index)
      this.fallenBridgeColliders[index].enabled = true;
  }

  private void OnTriggerStay(Collider other)
  {
    if (other.gameObject.CompareTag("Player"))
    {
      this.playerOnBridge = other.gameObject.GetComponent<PlayerControllerB>();
      if (!((Object) this.playerOnBridge != (Object) null) || this.playersOnBridge.Contains(this.playerOnBridge))
        return;
      this.playersOnBridge.Add(this.playerOnBridge);
      if (Random.Range(this.playersOnBridge.Count * 25, 100) <= 60)
        return;
      RoundManager.PlayRandomClip(this.bridgeAudioSource, this.bridgeCreakSFX);
    }
    else
    {
      if (!other.gameObject.CompareTag("Enemy"))
        return;
      EnemyAICollisionDetect component = other.gameObject.GetComponent<EnemyAICollisionDetect>();
      if (!((Object) component != (Object) null) || !(component.mainScript.enemyType.enemyName == "ForestGiant"))
        return;
      this.giantOnBridge = true;
      this.giantOnBridgeLastFrame = false;
    }
  }

  public void RemovePlayerFromBridge(PlayerControllerB playerOnBridge)
  {
    if (!((Object) playerOnBridge != (Object) null) || !this.playersOnBridge.Contains(playerOnBridge))
      return;
    this.playersOnBridge.Remove(playerOnBridge);
  }

  private void OnTriggerExit(Collider other)
  {
    if (!other.gameObject.CompareTag("Player"))
      return;
    this.playerOnBridge = other.gameObject.GetComponent<PlayerControllerB>();
    this.RemovePlayerFromBridge(this.playerOnBridge);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_BridgeTrigger()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2883846656U, new NetworkManager.RpcReceiveHandler(BridgeTrigger.__rpc_handler_2883846656)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(123213822U, new NetworkManager.RpcReceiveHandler(BridgeTrigger.__rpc_handler_123213822)));
  }

  private static void __rpc_handler_2883846656(
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
      ((BridgeTrigger) target).BridgeFallServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_123213822(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BridgeTrigger) target).BridgeFallClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (BridgeTrigger);
}
