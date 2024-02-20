// Decompiled with JetBrains decompiler
// Type: JetpackItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class JetpackItem : GrabbableObject
{
  public Transform backPart;
  public Vector3 backPartRotationOffset;
  public Vector3 backPartPositionOffset;
  private float jetpackPower;
  private bool jetpackActivated;
  private Vector3 forces;
  private bool jetpackActivatedPreviousFrame;
  public GameObject fireEffect;
  public AudioSource jetpackAudio;
  public AudioSource jetpackBeepsAudio;
  public AudioClip startJetpackSFX;
  public AudioClip jetpackSustainSFX;
  public AudioClip jetpackBrokenSFX;
  public AudioClip jetpackWarningBeepSFX;
  public AudioClip jetpackLowBatteriesSFX;
  public ParticleSystem smokeTrailParticle;
  private PlayerControllerB previousPlayerHeldBy;
  private bool jetpackBroken;
  private bool jetpackPlayingWarningBeep;
  private bool jetpackPlayingLowBatteryBeep;
  private float noiseInterval;
  private RaycastHit rayHit;

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    if (buttonDown)
      this.ActivateJetpack();
    else
      this.DeactivateJetpack();
  }

  private void DeactivateJetpack()
  {
    if (this.previousPlayerHeldBy.jetpackControls)
      this.previousPlayerHeldBy.disablingJetpackControls = true;
    this.jetpackActivated = false;
    this.jetpackActivatedPreviousFrame = false;
    this.jetpackPlayingLowBatteryBeep = false;
    this.jetpackPlayingWarningBeep = false;
    this.jetpackBeepsAudio.Stop();
    this.jetpackAudio.Stop();
    this.smokeTrailParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    if (this.jetpackBroken)
      return;
    this.JetpackEffect(false);
  }

  private void ActivateJetpack()
  {
    if (this.jetpackBroken)
    {
      this.jetpackAudio.PlayOneShot(this.jetpackBrokenSFX);
    }
    else
    {
      if (!this.jetpackActivatedPreviousFrame)
      {
        this.playerHeldBy.jetpackTurnCompass.rotation = this.playerHeldBy.transform.rotation;
        this.JetpackEffect(true);
        this.jetpackActivatedPreviousFrame = true;
      }
      this.playerHeldBy.disablingJetpackControls = false;
      this.playerHeldBy.jetpackControls = true;
      this.jetpackActivated = true;
      this.playerHeldBy.syncFullRotation = this.playerHeldBy.transform.eulerAngles;
    }
  }

  private void JetpackEffect(bool enable)
  {
    this.fireEffect.SetActive(enable);
    if (enable)
    {
      if (!this.jetpackActivatedPreviousFrame)
        this.jetpackAudio.PlayOneShot(this.startJetpackSFX);
      this.smokeTrailParticle.Play();
      this.jetpackAudio.clip = this.jetpackSustainSFX;
      this.jetpackAudio.Play();
      Debug.Log((object) string.Format("Is jetpack audio playing?: {0}", (object) this.jetpackAudio.isPlaying));
    }
    else
    {
      this.smokeTrailParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
      this.jetpackAudio.Stop();
    }
    if ((double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.transform.position) >= 10.0)
      return;
    HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
  }

  public override void UseUpBatteries() => this.DeactivateJetpack();

  public override void DiscardItem()
  {
    Debug.Log((object) string.Format("Owner of jetpack?: {0}", (object) this.IsOwner));
    Debug.Log((object) string.Format("Is dead?: {0}", (object) this.playerHeldBy.isPlayerDead));
    if (this.IsOwner && this.playerHeldBy.isPlayerDead && !this.jetpackBroken && this.playerHeldBy.jetpackControls)
      this.ExplodeJetpackServerRpc();
    this.JetpackEffect(false);
    this.DeactivateJetpack();
    this.jetpackPower = 0.0f;
    base.DiscardItem();
  }

  [ServerRpc(RequireOwnership = false)]
  public void ExplodeJetpackServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3663112878U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3663112878U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ExplodeJetpackClientRpc();
  }

  [ClientRpc]
  public void ExplodeJetpackClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2295726646U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2295726646U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.jetpackBroken)
      return;
    this.jetpackBroken = true;
    this.itemUsedUp = true;
    Debug.Log((object) "Spawning explosion");
    Landmine.SpawnExplosion(this.transform.position, true, 5f, 7f);
  }

  public override void EquipItem()
  {
    base.EquipItem();
    this.previousPlayerHeldBy = this.playerHeldBy;
  }

  public override void Update()
  {
    base.Update();
    if ((Object) GameNetworkManager.Instance == (Object) null || (Object) GameNetworkManager.Instance.localPlayerController == (Object) null)
      return;
    this.SetJetpackAudios();
    if ((Object) this.playerHeldBy == (Object) null || !this.IsOwner || (Object) this.playerHeldBy != (Object) GameNetworkManager.Instance.localPlayerController)
      return;
    if (this.jetpackActivated)
    {
      this.jetpackPower = Mathf.Clamp(this.jetpackPower + Time.deltaTime * 10f, 0.0f, 500f);
    }
    else
    {
      this.jetpackPower = Mathf.Clamp(this.jetpackPower - Time.deltaTime * 75f, 0.0f, 1000f);
      if (this.playerHeldBy.thisController.isGrounded)
        this.jetpackPower = 0.0f;
    }
    this.forces = Vector3.Lerp(this.forces, Vector3.ClampMagnitude(this.playerHeldBy.transform.up * this.jetpackPower, 400f), Time.deltaTime);
    if (!this.playerHeldBy.jetpackControls)
      this.forces = Vector3.zero;
    if (!this.playerHeldBy.isPlayerDead && Physics.Raycast(this.playerHeldBy.transform.position, this.forces, out this.rayHit, 25f, StartOfRound.Instance.allPlayersCollideWithMask) && (double) this.forces.magnitude - (double) this.rayHit.distance > 50.0 && (double) this.rayHit.distance < 4.0)
      this.playerHeldBy.KillPlayer(this.forces, causeOfDeath: CauseOfDeath.Gravity);
    this.playerHeldBy.externalForces += this.forces;
  }

  private void SetJetpackAudios()
  {
    if (this.jetpackActivated)
    {
      if ((double) this.noiseInterval >= 0.5)
      {
        this.noiseInterval = 0.0f;
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, 25f, 0.85f, noiseIsInsideClosedShip: this.playerHeldBy.isInHangarShipRoom && StartOfRound.Instance.hangarDoorsClosed, noiseID: 41);
      }
      else
        this.noiseInterval += Time.deltaTime;
      if ((double) this.insertedBattery.charge < 0.15000000596046448)
      {
        if (this.jetpackPlayingLowBatteryBeep)
          return;
        this.jetpackPlayingLowBatteryBeep = true;
        this.jetpackBeepsAudio.clip = this.jetpackLowBatteriesSFX;
        this.jetpackBeepsAudio.Play();
      }
      else if (Physics.CheckSphere(this.transform.position, 6f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
      {
        if (this.jetpackPlayingWarningBeep || this.jetpackPlayingLowBatteryBeep)
          return;
        this.jetpackPlayingWarningBeep = true;
        this.jetpackBeepsAudio.clip = this.jetpackWarningBeepSFX;
        this.jetpackBeepsAudio.Play();
      }
      else
        this.jetpackBeepsAudio.Stop();
    }
    else
    {
      this.jetpackPlayingWarningBeep = false;
      this.jetpackPlayingLowBatteryBeep = false;
      this.jetpackBeepsAudio.Stop();
    }
  }

  public override void LateUpdate()
  {
    base.LateUpdate();
    if (!((Object) this.playerHeldBy != (Object) null) || !this.isHeld)
      return;
    this.backPart.position = this.playerHeldBy.lowerSpine.position;
    this.backPart.rotation = this.playerHeldBy.lowerSpine.rotation;
    this.transform.Rotate(this.backPartRotationOffset);
    this.backPart.position = this.playerHeldBy.lowerSpine.position;
    this.backPart.position += this.playerHeldBy.lowerSpine.rotation * this.backPartPositionOffset;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_JetpackItem()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3663112878U, new NetworkManager.RpcReceiveHandler(JetpackItem.__rpc_handler_3663112878)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2295726646U, new NetworkManager.RpcReceiveHandler(JetpackItem.__rpc_handler_2295726646)));
  }

  private static void __rpc_handler_3663112878(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((JetpackItem) target).ExplodeJetpackServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2295726646(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((JetpackItem) target).ExplodeJetpackClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (JetpackItem);
}
