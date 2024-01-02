// Decompiled with JetBrains decompiler
// Type: SteamValveHazard
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class SteamValveHazard : NetworkBehaviour
{
  public float valveCrackTime;
  public float valveBurstTime;
  private bool valveHasBurst;
  private bool valveHasCracked;
  private bool valveHasBeenRepaired;
  public InteractTrigger triggerScript;
  [Header("Fog")]
  public Animator fogAnimator;
  public Animator valveAnimator;
  public float fogSizeMultiplier;
  public float currentFogSize;
  [Header("Other Effects")]
  public ParticleSystem valveSteamParticle;
  public AudioClip[] pipeFlowingSFX;
  public AudioClip valveTwistSFX;
  public AudioClip valveBurstSFX;
  public AudioClip valveCrackSFX;
  public AudioClip steamBlowSFX;
  public AudioSource valveAudio;

  private void Start()
  {
    this.valveAudio.pitch = Random.Range(0.85f, 1.1f);
    this.valveAudio.clip = this.pipeFlowingSFX[Random.Range(0, this.pipeFlowingSFX.Length)];
    this.valveAudio.Play();
  }

  private void Update()
  {
    if (StartOfRound.Instance.allPlayersDead || (Object) NetworkManager.Singleton == (Object) null || !GameNetworkManager.Instance.gameHasStarted)
      return;
    if (this.valveHasBeenRepaired)
    {
      this.currentFogSize = Mathf.Clamp(this.currentFogSize - Time.deltaTime / 4f, 0.01f, 1f * this.fogSizeMultiplier);
      this.fogAnimator.SetFloat("time", this.currentFogSize);
    }
    else if (!this.valveHasCracked && (double) this.valveCrackTime > 0.0 && (double) TimeOfDay.Instance.normalizedTimeOfDay > (double) this.valveCrackTime)
    {
      this.valveHasCracked = true;
      this.CrackValve();
    }
    else if (!this.valveHasBurst && (double) this.valveBurstTime > 0.0 && (double) TimeOfDay.Instance.normalizedTimeOfDay > (double) this.valveBurstTime)
    {
      this.valveHasBurst = true;
      this.BurstValve();
    }
    else
    {
      if (!this.valveHasBurst)
        return;
      this.currentFogSize = Mathf.Clamp(this.currentFogSize + Time.deltaTime / 12f, 0.0f, 1f * this.fogSizeMultiplier);
      this.fogAnimator.SetFloat("time", this.currentFogSize);
      if ((double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.valveAudio.transform.position) >= 10.0)
        return;
      HUDManager.Instance.increaseHelmetCondensation = true;
      HUDManager.Instance.DisplayStatusEffect("VISIBILITY LOW!\n\nSteam leak detected in area");
    }
  }

  private void CrackValve()
  {
    this.valveAudio.PlayOneShot(this.valveCrackSFX);
    WalkieTalkie.TransmitOneShotAudio(this.valveAudio, this.valveCrackSFX);
    this.valveSteamParticle.main.loop = false;
    this.valveSteamParticle.Play();
  }

  private void BurstValve()
  {
    this.valveSteamParticle.main.loop = true;
    this.valveSteamParticle.Play();
    this.valveAudio.clip = this.steamBlowSFX;
    this.valveAudio.Play();
    this.valveAudio.PlayOneShot(this.valveBurstSFX);
    WalkieTalkie.TransmitOneShotAudio(this.valveAudio, this.valveBurstSFX);
    this.triggerScript.interactable = true;
  }

  private void FixValveLocalClient()
  {
    if (!this.valveHasBurst || this.valveHasBeenRepaired)
      return;
    this.valveSteamParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    this.valveAudio.clip = this.pipeFlowingSFX[Random.Range(0, this.pipeFlowingSFX.Length)];
    this.valveAudio.Play();
    this.valveAudio.PlayOneShot(this.valveTwistSFX, 1f);
    WalkieTalkie.TransmitOneShotAudio(this.valveAudio, this.valveTwistSFX);
    this.valveAnimator.SetTrigger("TwistValve");
    this.valveHasBeenRepaired = true;
    this.triggerScript.interactable = false;
  }

  public void FixValve()
  {
    this.FixValveLocalClient();
    if (this.IsServer)
      this.FixValveClientRpc();
    else
      this.FixValveServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void FixValveServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2205874137U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2205874137U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.FixValveClientRpc();
  }

  [ClientRpc]
  public void FixValveClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3330239287U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3330239287U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.FixValveLocalClient();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_SteamValveHazard()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2205874137U, new NetworkManager.RpcReceiveHandler(SteamValveHazard.__rpc_handler_2205874137)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3330239287U, new NetworkManager.RpcReceiveHandler(SteamValveHazard.__rpc_handler_3330239287)));
  }

  private static void __rpc_handler_2205874137(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((SteamValveHazard) target).FixValveServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3330239287(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SteamValveHazard) target).FixValveClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (SteamValveHazard);
}
