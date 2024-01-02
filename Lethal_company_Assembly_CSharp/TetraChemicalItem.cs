// Decompiled with JetBrains decompiler
// Type: TetraChemicalItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class TetraChemicalItem : GrabbableObject
{
  private PlayerControllerB previousPlayerHeldBy;
  private Coroutine useTZPCoroutine;
  private bool emittingGas;
  private float fuel = 1f;
  public AudioSource localHelmetSFX;
  public AudioSource thisAudioSource;
  public AudioClip twistCanSFX;
  public AudioClip releaseGasSFX;
  public AudioClip holdCanSFX;
  public AudioClip removeCanSFX;
  public AudioClip outOfGasSFX;
  private bool triedUsingWithoutFuel;

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    if (buttonDown)
    {
      this.isBeingUsed = true;
      if ((double) this.fuel <= 0.0)
      {
        if (this.triedUsingWithoutFuel)
          return;
        this.triedUsingWithoutFuel = true;
        this.thisAudioSource.PlayOneShot(this.outOfGasSFX);
        WalkieTalkie.TransmitOneShotAudio(this.thisAudioSource, this.outOfGasSFX);
        this.previousPlayerHeldBy.playerBodyAnimator.SetTrigger("shakeItem");
        return;
      }
      this.previousPlayerHeldBy = this.playerHeldBy;
      this.useTZPCoroutine = this.StartCoroutine(this.UseTZPAnimation());
    }
    else
    {
      this.isBeingUsed = false;
      if (this.triedUsingWithoutFuel)
        this.triedUsingWithoutFuel = false;
      else if (this.useTZPCoroutine != null)
      {
        this.StopCoroutine(this.useTZPCoroutine);
        this.emittingGas = false;
        this.previousPlayerHeldBy.activatingItem = false;
        this.thisAudioSource.Stop();
        this.localHelmetSFX.Stop();
        this.thisAudioSource.PlayOneShot(this.removeCanSFX);
      }
    }
    if (!this.IsOwner)
      return;
    this.previousPlayerHeldBy.activatingItem = buttonDown;
    this.previousPlayerHeldBy.playerBodyAnimator.SetBool("useTZPItem", buttonDown);
  }

  private IEnumerator UseTZPAnimation()
  {
    TetraChemicalItem tetraChemicalItem = this;
    tetraChemicalItem.thisAudioSource.PlayOneShot(tetraChemicalItem.holdCanSFX);
    WalkieTalkie.TransmitOneShotAudio(tetraChemicalItem.previousPlayerHeldBy.itemAudio, tetraChemicalItem.holdCanSFX);
    yield return (object) new WaitForSeconds(0.75f);
    tetraChemicalItem.emittingGas = true;
    HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", true);
    if (tetraChemicalItem.IsOwner)
    {
      tetraChemicalItem.localHelmetSFX.Play();
      tetraChemicalItem.localHelmetSFX.PlayOneShot(tetraChemicalItem.twistCanSFX);
    }
    else
    {
      tetraChemicalItem.thisAudioSource.clip = tetraChemicalItem.releaseGasSFX;
      tetraChemicalItem.thisAudioSource.Play();
      tetraChemicalItem.thisAudioSource.PlayOneShot(tetraChemicalItem.twistCanSFX);
    }
    WalkieTalkie.TransmitOneShotAudio(tetraChemicalItem.previousPlayerHeldBy.itemAudio, tetraChemicalItem.twistCanSFX);
  }

  public override void Update()
  {
    if (this.emittingGas)
    {
      if ((Object) this.previousPlayerHeldBy == (Object) null || !this.isHeld || (double) this.fuel <= 0.0)
      {
        this.emittingGas = false;
        this.thisAudioSource.Stop();
        this.localHelmetSFX.Stop();
        this.RunOutOfFuelServerRpc();
      }
      this.previousPlayerHeldBy.drunknessInertia = Mathf.Clamp(this.previousPlayerHeldBy.drunknessInertia + Time.deltaTime / 1.75f * this.previousPlayerHeldBy.drunknessSpeed, 0.1f, 3f);
      this.previousPlayerHeldBy.increasingDrunknessThisFrame = true;
      this.fuel -= Time.deltaTime / 22f;
    }
    base.Update();
  }

  public override void EquipItem()
  {
    base.EquipItem();
    StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
    if (!((Object) this.playerHeldBy != (Object) null))
      return;
    this.previousPlayerHeldBy = this.playerHeldBy;
  }

  [ServerRpc]
  public void RunOutOfFuelServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1607080184U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1607080184U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.RunOutOfFuelClientRpc();
  }

  [ClientRpc]
  public void RunOutOfFuelClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3625530963U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3625530963U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.itemUsedUp = true;
    this.emittingGas = false;
    this.fuel = 0.0f;
    this.thisAudioSource.Stop();
    this.localHelmetSFX.Stop();
  }

  public override void DiscardItem()
  {
    this.emittingGas = false;
    this.thisAudioSource.Stop();
    this.localHelmetSFX.Stop();
    this.playerHeldBy.playerBodyAnimator.ResetTrigger("shakeItem");
    this.previousPlayerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);
    if ((Object) this.previousPlayerHeldBy != (Object) null)
      this.previousPlayerHeldBy.activatingItem = false;
    base.DiscardItem();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_TetraChemicalItem()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1607080184U, new NetworkManager.RpcReceiveHandler(TetraChemicalItem.__rpc_handler_1607080184)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3625530963U, new NetworkManager.RpcReceiveHandler(TetraChemicalItem.__rpc_handler_3625530963)));
  }

  private static void __rpc_handler_1607080184(
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
      ((TetraChemicalItem) target).RunOutOfFuelServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3625530963(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TetraChemicalItem) target).RunOutOfFuelClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (TetraChemicalItem);
}
