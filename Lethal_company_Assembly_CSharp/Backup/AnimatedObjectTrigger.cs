// Decompiled with JetBrains decompiler
// Type: AnimatedObjectTrigger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class AnimatedObjectTrigger : NetworkBehaviour
{
  public Animator triggerAnimator;
  public Animator triggerAnimatorB;
  public bool isBool = true;
  public string animationString;
  public bool boolValue;
  public bool setInitialState;
  public bool initialBoolState;
  [Space(5f)]
  public AudioSource thisAudioSource;
  public AudioClip[] boolFalseAudios;
  public AudioClip[] boolTrueAudios;
  public AudioClip[] secondaryAudios;
  [Space(4f)]
  public AudioClip playWhileTrue;
  public bool resetAudioWhenFalse;
  public bool makeAudibleNoise;
  public float noiseLoudness = 0.7f;
  [Space(3f)]
  public ParticleSystem playParticle;
  [Space(4f)]
  private StartOfRound playersManager;
  private bool localPlayerTriggered;
  public BooleanEvent onTriggerBool;
  [Space(5f)]
  public bool playAudiosInSequence;
  private int timesTriggered;
  public bool triggerByChance;
  public float chancePercent = 5f;
  private bool hasInitializedRandomSeed;
  public System.Random triggerRandom;
  private float audioTime;

  public void Start()
  {
    if (!this.setInitialState)
      return;
    this.boolValue = this.initialBoolState;
    this.triggerAnimator.SetBool(this.animationString, this.boolValue);
    if (!((UnityEngine.Object) this.triggerAnimatorB != (UnityEngine.Object) null))
      return;
    this.triggerAnimatorB.SetBool("on", this.boolValue);
  }

  public void TriggerAnimation(PlayerControllerB playerWhoTriggered)
  {
    if (this.triggerByChance)
    {
      this.InitializeRandomSeed();
      if ((double) this.triggerRandom.Next(100) >= (double) this.chancePercent)
        return;
    }
    if (this.isBool)
    {
      Debug.Log((object) string.Format("Triggering animated object trigger bool: setting to {0}", (object) !this.boolValue));
      this.boolValue = !this.boolValue;
      if ((UnityEngine.Object) this.triggerAnimator != (UnityEngine.Object) null)
        this.triggerAnimator.SetBool(this.animationString, this.boolValue);
      if ((UnityEngine.Object) this.triggerAnimatorB != (UnityEngine.Object) null)
        this.triggerAnimatorB.SetBool("on", this.boolValue);
    }
    else if ((UnityEngine.Object) this.triggerAnimator != (UnityEngine.Object) null)
      this.triggerAnimator.SetTrigger(this.animationString);
    this.SetParticleBasedOnBoolean();
    this.PlayAudio(this.boolValue);
    this.localPlayerTriggered = true;
    if (this.isBool)
    {
      this.onTriggerBool.Invoke(this.boolValue);
      this.UpdateAnimServerRpc(this.boolValue, playerWhoTriggered: (int) playerWhoTriggered.playerClientId);
    }
    else
      this.UpdateAnimTriggerServerRpc();
  }

  public void TriggerAnimationNonPlayer(
    bool playSecondaryAudios = false,
    bool overrideBool = false,
    bool setBoolFalse = false)
  {
    if (overrideBool & setBoolFalse && !this.boolValue)
      return;
    if (this.triggerByChance)
    {
      this.InitializeRandomSeed();
      if ((double) this.triggerRandom.Next(100) >= (double) this.chancePercent)
        return;
    }
    if (this.isBool)
    {
      this.boolValue = !this.boolValue;
      this.triggerAnimator.SetBool(this.animationString, this.boolValue);
    }
    else
      this.triggerAnimator.SetTrigger(this.animationString);
    this.SetParticleBasedOnBoolean();
    this.PlayAudio(this.boolValue, playSecondaryAudios);
    this.localPlayerTriggered = true;
    if (this.isBool)
      this.UpdateAnimServerRpc(this.boolValue, playSecondaryAudios);
    else
      this.UpdateAnimTriggerServerRpc();
  }

  public void InitializeRandomSeed()
  {
    if (this.hasInitializedRandomSeed)
      return;
    this.hasInitializedRandomSeed = true;
    this.triggerRandom = new System.Random(this.playersManager.randomMapSeed);
  }

  [ServerRpc(RequireOwnership = false)]
  private void UpdateAnimServerRpc(bool setBool, bool playSecondaryAudios = false, int playerWhoTriggered = -1)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1461767556U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in setBool, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in playSecondaryAudios, new FastBufferWriter.ForPrimitives());
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoTriggered);
      this.__endSendServerRpc(ref bufferWriter, 1461767556U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.UpdateAnimClientRpc(setBool, playSecondaryAudios, playerWhoTriggered);
  }

  [ClientRpc]
  private void UpdateAnimClientRpc(bool setBool, bool playSecondaryAudios = false, int playerWhoTriggered = -1)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(848048148U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in setBool, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in playSecondaryAudios, new FastBufferWriter.ForPrimitives());
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoTriggered);
      this.__endSendClientRpc(ref bufferWriter, 848048148U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || playerWhoTriggered != -1 && (int) GameNetworkManager.Instance.localPlayerController.playerClientId == playerWhoTriggered)
      return;
    if (this.isBool)
    {
      if ((UnityEngine.Object) this.triggerAnimatorB != (UnityEngine.Object) null)
        this.triggerAnimatorB.SetBool("on", setBool);
      this.boolValue = setBool;
      if ((UnityEngine.Object) this.triggerAnimator != (UnityEngine.Object) null)
        this.triggerAnimator.SetBool(this.animationString, setBool);
      this.onTriggerBool.Invoke(this.boolValue);
    }
    else
      this.triggerAnimator.SetTrigger(this.animationString);
    this.SetParticleBasedOnBoolean();
    this.PlayAudio(setBool, playSecondaryAudios);
  }

  public void SetBoolOnClientOnly(bool setTo)
  {
    if (this.isBool)
    {
      this.boolValue = setTo;
      if ((UnityEngine.Object) this.triggerAnimator != (UnityEngine.Object) null)
        this.triggerAnimator.SetBool(this.animationString, this.boolValue);
      this.SetParticleBasedOnBoolean();
    }
    this.PlayAudio(this.boolValue);
  }

  public void SetBoolOnClientOnlyInverted(bool setTo)
  {
    if (this.isBool)
    {
      this.boolValue = !setTo;
      if ((UnityEngine.Object) this.triggerAnimator != (UnityEngine.Object) null)
        this.triggerAnimator.SetBool(this.animationString, this.boolValue);
      this.SetParticleBasedOnBoolean();
    }
    this.PlayAudio(this.boolValue);
  }

  private void SetParticleBasedOnBoolean()
  {
    if ((UnityEngine.Object) this.playParticle == (UnityEngine.Object) null)
      return;
    if (this.boolValue)
      this.playParticle.Play(true);
    else
      this.playParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
  }

  [ServerRpc(RequireOwnership = false)]
  private void UpdateAnimTriggerServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2219526317U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2219526317U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.UpdateAnimTriggerClientRpc();
  }

  [ClientRpc]
  private void UpdateAnimTriggerClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1023577379U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1023577379U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.onTriggerBool.Invoke(false);
    if (this.localPlayerTriggered)
    {
      this.localPlayerTriggered = false;
    }
    else
    {
      if ((UnityEngine.Object) this.triggerAnimator != (UnityEngine.Object) null)
        this.triggerAnimator.SetTrigger(this.animationString);
      this.PlayAudio(false);
    }
  }

  private void PlayAudio(bool boolVal, bool playSecondaryAudios = false)
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || (UnityEngine.Object) this.thisAudioSource == (UnityEngine.Object) null)
      return;
    Debug.Log((object) string.Format("bool val: {0}", (object) boolVal));
    if ((UnityEngine.Object) this.playWhileTrue != (UnityEngine.Object) null)
    {
      this.thisAudioSource.clip = this.playWhileTrue;
      if (boolVal)
      {
        this.thisAudioSource.Play();
        if (!this.resetAudioWhenFalse)
          this.thisAudioSource.time = this.audioTime;
      }
      else
      {
        this.audioTime = this.thisAudioSource.time;
        this.thisAudioSource.Stop();
      }
    }
    AudioClip clip = (AudioClip) null;
    if (playSecondaryAudios)
      clip = this.secondaryAudios[UnityEngine.Random.Range(0, this.secondaryAudios.Length)];
    else if (boolVal && this.boolTrueAudios.Length != 0)
      clip = this.boolTrueAudios[UnityEngine.Random.Range(0, this.boolTrueAudios.Length)];
    else if (this.boolFalseAudios.Length != 0)
    {
      if (this.playAudiosInSequence)
      {
        if (this.timesTriggered >= this.boolFalseAudios.Length)
          return;
        clip = this.boolFalseAudios[this.timesTriggered];
      }
      else
        clip = this.boolFalseAudios[UnityEngine.Random.Range(0, this.boolFalseAudios.Length)];
    }
    if ((UnityEngine.Object) clip == (UnityEngine.Object) null)
      return;
    this.thisAudioSource.PlayOneShot(clip, 1f);
    WalkieTalkie.TransmitOneShotAudio(this.thisAudioSource, clip);
    if (!this.makeAudibleNoise)
      return;
    RoundManager.Instance.PlayAudibleNoise(this.thisAudioSource.transform.position, 18f, this.noiseLoudness, noiseIsInsideClosedShip: StartOfRound.Instance.hangarDoorsClosed, noiseID: 400);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_AnimatedObjectTrigger()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1461767556U, new NetworkManager.RpcReceiveHandler(AnimatedObjectTrigger.__rpc_handler_1461767556)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(848048148U, new NetworkManager.RpcReceiveHandler(AnimatedObjectTrigger.__rpc_handler_848048148)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2219526317U, new NetworkManager.RpcReceiveHandler(AnimatedObjectTrigger.__rpc_handler_2219526317)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1023577379U, new NetworkManager.RpcReceiveHandler(AnimatedObjectTrigger.__rpc_handler_1023577379)));
  }

  private static void __rpc_handler_1461767556(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool setBool;
    reader.ReadValueSafe<bool>(out setBool, new FastBufferWriter.ForPrimitives());
    bool playSecondaryAudios;
    reader.ReadValueSafe<bool>(out playSecondaryAudios, new FastBufferWriter.ForPrimitives());
    int playerWhoTriggered;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((AnimatedObjectTrigger) target).UpdateAnimServerRpc(setBool, playSecondaryAudios, playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_848048148(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool setBool;
    reader.ReadValueSafe<bool>(out setBool, new FastBufferWriter.ForPrimitives());
    bool playSecondaryAudios;
    reader.ReadValueSafe<bool>(out playSecondaryAudios, new FastBufferWriter.ForPrimitives());
    int playerWhoTriggered;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((AnimatedObjectTrigger) target).UpdateAnimClientRpc(setBool, playSecondaryAudios, playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2219526317(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((AnimatedObjectTrigger) target).UpdateAnimTriggerServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1023577379(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((AnimatedObjectTrigger) target).UpdateAnimTriggerClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (AnimatedObjectTrigger);
}
