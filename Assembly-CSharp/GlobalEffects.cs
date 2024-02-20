// Decompiled with JetBrains decompiler
// Type: GlobalEffects
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class GlobalEffects : NetworkBehaviour
{
  private StartOfRound playersManager;
  public bool ownedByPlayer;

  public static GlobalEffects Instance { get; private set; }

  private void Awake()
  {
    if (!this.ownedByPlayer)
    {
      if ((Object) GlobalEffects.Instance == (Object) null)
      {
        GlobalEffects.Instance = this;
      }
      else
      {
        Object.Destroy((Object) this.gameObject);
        return;
      }
    }
    this.playersManager = Object.FindObjectOfType<StartOfRound>();
  }

  public void PlayAnimAndAudioServer(ServerAnimAndAudio serverAnimAndAudio)
  {
    this.playersManager.allPlayerObjects[this.playersManager.thisClientPlayerId].GetComponentInChildren<GlobalEffects>().PlayAnimAndAudioServerFromSenderObject(serverAnimAndAudio);
  }

  public void PlayAnimAndAudioServerFromSenderObject(ServerAnimAndAudio serverAnimAndAudio)
  {
    this.PlayAnimAndAudioServerRpc(serverAnimAndAudio);
  }

  [ServerRpc(RequireOwnership = false)]
  private void PlayAnimAndAudioServerRpc(ServerAnimAndAudio serverAnimAndAudio)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2259057361U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<ServerAnimAndAudio>(in serverAnimAndAudio, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendServerRpc(ref bufferWriter, 2259057361U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayAnimAndAudioClientRpc(serverAnimAndAudio);
  }

  [ClientRpc]
  private void PlayAnimAndAudioClientRpc(ServerAnimAndAudio serverAnimAndAudio)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2993461149U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<ServerAnimAndAudio>(in serverAnimAndAudio, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendClientRpc(ref bufferWriter, 2993461149U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    NetworkObject networkObject1;
    if (serverAnimAndAudio.animatorObj.TryGet(out networkObject1))
      networkObject1.GetComponent<Animator>().SetTrigger(serverAnimAndAudio.animationString);
    NetworkObject networkObject2;
    if (!serverAnimAndAudio.audioObj.TryGet(out networkObject2))
      return;
    networkObject2.GetComponent<AudioSource>().PlayOneShot(networkObject2.GetComponent<AudioSource>().clip);
  }

  public void PlayAnimationServer(ServerAnimation serverAnimation)
  {
    this.playersManager.allPlayerObjects[this.playersManager.thisClientPlayerId].GetComponentInChildren<GlobalEffects>().PlayAnimationServerFromSenderObject(serverAnimation);
  }

  public void PlayAnimationServerFromSenderObject(ServerAnimation serverAnimation)
  {
    this.PlayAnimationServerRpc(serverAnimation);
  }

  [ServerRpc(RequireOwnership = false)]
  private void PlayAnimationServerRpc(ServerAnimation serverAnimation)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2698736096U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<ServerAnimation>(in serverAnimation, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendServerRpc(ref bufferWriter, 2698736096U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayAnimationClientRpc(serverAnimation);
  }

  [ClientRpc]
  private void PlayAnimationClientRpc(ServerAnimation serverAnimation)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(780678780U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<ServerAnimation>(in serverAnimation, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendClientRpc(ref bufferWriter, 780678780U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    NetworkObject networkObject;
    if (serverAnimation.animatorObj.TryGet(out networkObject))
    {
      if (serverAnimation.isTrigger)
        networkObject.GetComponent<Animator>().SetTrigger(serverAnimation.animationString);
      else
        networkObject.GetComponent<Animator>().SetBool(serverAnimation.animationString, serverAnimation.setTrue);
    }
    else
      Debug.LogWarning((object) ("Was not able to retrieve NetworkObject from NetworkObjectReference; string " + serverAnimation.animationString));
  }

  public void PlayAudioServer(ServerAudio serverAudio)
  {
    this.playersManager.allPlayerObjects[this.playersManager.thisClientPlayerId].GetComponentInChildren<GlobalEffects>().PlayAudioServerFromSenderObject(serverAudio);
  }

  public void PlayAudioServerFromSenderObject(ServerAudio serverAudio)
  {
    this.PlayAudioServerRpc(serverAudio);
  }

  [ServerRpc(RequireOwnership = false)]
  private void PlayAudioServerRpc(ServerAudio serverAudio)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1842858504U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<ServerAudio>(in serverAudio, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendServerRpc(ref bufferWriter, 1842858504U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayAudioClientRpc(serverAudio);
  }

  [ClientRpc]
  private void PlayAudioClientRpc(ServerAudio serverAudio)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(182727243U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<ServerAudio>(in serverAudio, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendClientRpc(ref bufferWriter, 182727243U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    NetworkObject networkObject;
    if (serverAudio.audioObj.TryGet(out networkObject))
    {
      AudioSource component = networkObject.gameObject.GetComponent<AudioSource>();
      if (serverAudio.oneshot)
      {
        component.PlayOneShot(component.clip, 1f);
      }
      else
      {
        component.loop = serverAudio.looped;
        component.Play();
      }
    }
    else
      Debug.LogWarning((object) "Was not able to retrieve NetworkObject from NetworkObjectReference; audio");
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_GlobalEffects()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2259057361U, new NetworkManager.RpcReceiveHandler(GlobalEffects.__rpc_handler_2259057361)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2993461149U, new NetworkManager.RpcReceiveHandler(GlobalEffects.__rpc_handler_2993461149)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2698736096U, new NetworkManager.RpcReceiveHandler(GlobalEffects.__rpc_handler_2698736096)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(780678780U, new NetworkManager.RpcReceiveHandler(GlobalEffects.__rpc_handler_780678780)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1842858504U, new NetworkManager.RpcReceiveHandler(GlobalEffects.__rpc_handler_1842858504)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(182727243U, new NetworkManager.RpcReceiveHandler(GlobalEffects.__rpc_handler_182727243)));
  }

  private static void __rpc_handler_2259057361(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ServerAnimAndAudio serverAnimAndAudio;
    reader.ReadValueSafe<ServerAnimAndAudio>(out serverAnimAndAudio, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GlobalEffects) target).PlayAnimAndAudioServerRpc(serverAnimAndAudio);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2993461149(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ServerAnimAndAudio serverAnimAndAudio;
    reader.ReadValueSafe<ServerAnimAndAudio>(out serverAnimAndAudio, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GlobalEffects) target).PlayAnimAndAudioClientRpc(serverAnimAndAudio);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2698736096(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ServerAnimation serverAnimation;
    reader.ReadValueSafe<ServerAnimation>(out serverAnimation, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GlobalEffects) target).PlayAnimationServerRpc(serverAnimation);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_780678780(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ServerAnimation serverAnimation;
    reader.ReadValueSafe<ServerAnimation>(out serverAnimation, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GlobalEffects) target).PlayAnimationClientRpc(serverAnimation);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1842858504(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ServerAudio serverAudio;
    reader.ReadValueSafe<ServerAudio>(out serverAudio, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((GlobalEffects) target).PlayAudioServerRpc(serverAudio);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_182727243(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ServerAudio serverAudio;
    reader.ReadValueSafe<ServerAudio>(out serverAudio, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((GlobalEffects) target).PlayAudioClientRpc(serverAudio);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (GlobalEffects);
}
