// Decompiled with JetBrains decompiler
// Type: Turret
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class Turret : NetworkBehaviour, IHittable
{
  [Header("Effects")]
  public AudioSource mainAudio;
  [Header("Effects")]
  public AudioSource bulletCollisionAudio;
  [Header("Effects")]
  public AudioSource farAudio;
  public AudioClip firingSFX;
  public AudioClip chargingSFX;
  public AudioClip detectPlayerSFX;
  public AudioClip firingFarSFX;
  public AudioClip bulletsHitWallSFX;
  public AudioClip turretActivate;
  public AudioClip turretDeactivate;
  public ParticleSystem bulletParticles;
  public Animator turretAnimator;
  [Header("Variables")]
  public bool turretActive = true;
  [Space(5f)]
  public TurretMode turretMode;
  private TurretMode turretModeLastFrame;
  public Transform turretRod;
  public float targetRotation;
  public float rotationSpeed = 20f;
  public Transform turnTowardsObjectCompass;
  public Transform forwardFacingPos;
  public Transform aimPoint;
  public Transform centerPoint;
  public PlayerControllerB targetPlayerWithRotation;
  public Transform targetTransform;
  private bool targetingDeadPlayer;
  public float rotationRange = 45f;
  public float currentRotation;
  public bool rotatingOnInterval = true;
  private bool rotatingRight;
  private float switchRotationTimer;
  private bool hasLineOfSight;
  private float lostLOSTimer;
  private bool wasTargetingPlayerLastFrame;
  private RaycastHit hit;
  private int wallAndPlayerMask = 2824;
  private float turretInterval;
  private string previousHitLog;
  private bool rotatingSmoothly = true;
  private Ray shootRay;
  private Coroutine fadeBulletAudioCoroutine;
  public Transform tempTransform;
  private bool rotatingClockwise;
  private float berserkTimer;
  public AudioSource berserkAudio;
  private bool enteringBerserkMode;

  private void Start()
  {
    this.rotationRange = Mathf.Abs(this.rotationRange);
    this.rotationSpeed = 28f;
  }

  private IEnumerator FadeBulletAudio()
  {
    float initialVolume = this.bulletCollisionAudio.volume;
    for (int i = 0; i <= 30; ++i)
    {
      yield return (object) new WaitForSeconds(0.012f);
      this.bulletCollisionAudio.volume = Mathf.Lerp(initialVolume, 0.0f, (float) i / 30f);
    }
    this.bulletCollisionAudio.Stop();
  }

  private void Update()
  {
    if (!this.turretActive)
    {
      this.wasTargetingPlayerLastFrame = false;
      this.turretMode = TurretMode.Detection;
      this.targetPlayerWithRotation = (PlayerControllerB) null;
    }
    else
    {
      if ((Object) this.targetPlayerWithRotation != (Object) null)
      {
        if (!this.wasTargetingPlayerLastFrame)
        {
          this.wasTargetingPlayerLastFrame = true;
          if (this.turretMode == TurretMode.Detection)
            this.turretMode = TurretMode.Charging;
        }
        this.SetTargetToPlayerBody();
        this.TurnTowardsTargetIfHasLOS();
      }
      else if (this.wasTargetingPlayerLastFrame)
      {
        this.wasTargetingPlayerLastFrame = false;
        this.turretMode = TurretMode.Detection;
      }
      switch (this.turretMode)
      {
        case TurretMode.Detection:
          if (this.turretModeLastFrame != TurretMode.Detection)
          {
            this.turretModeLastFrame = TurretMode.Detection;
            this.rotatingClockwise = false;
            this.mainAudio.Stop();
            this.farAudio.Stop();
            this.berserkAudio.Stop();
            if (this.fadeBulletAudioCoroutine != null)
              this.StopCoroutine(this.fadeBulletAudioCoroutine);
            this.fadeBulletAudioCoroutine = this.StartCoroutine(this.FadeBulletAudio());
            this.bulletParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            this.rotationSpeed = 28f;
            this.rotatingSmoothly = true;
            this.turretAnimator.SetInteger("TurretMode", 0);
            this.turretInterval = Random.Range(0.0f, 0.15f);
          }
          if (this.IsServer)
          {
            if ((double) this.switchRotationTimer >= 7.0)
            {
              this.switchRotationTimer = 0.0f;
              bool setRotateRight = !this.rotatingRight;
              this.SwitchRotationClientRpc(setRotateRight);
              this.SwitchRotationOnInterval(setRotateRight);
            }
            else
              this.switchRotationTimer += Time.deltaTime;
            if ((double) this.turretInterval >= 0.25)
            {
              this.turretInterval = 0.0f;
              PlayerControllerB playerControllerB = this.CheckForPlayersInLineOfSight(1.35f, true);
              if ((Object) playerControllerB != (Object) null && !playerControllerB.isPlayerDead)
              {
                this.targetPlayerWithRotation = playerControllerB;
                this.SwitchTurretMode(1);
                this.SwitchTargetedPlayerClientRpc((int) playerControllerB.playerClientId, true);
                break;
              }
              break;
            }
            this.turretInterval += Time.deltaTime;
            break;
          }
          break;
        case TurretMode.Charging:
          if (this.turretModeLastFrame != TurretMode.Charging)
          {
            this.turretModeLastFrame = TurretMode.Charging;
            this.rotatingClockwise = false;
            this.mainAudio.PlayOneShot(this.detectPlayerSFX);
            this.berserkAudio.Stop();
            WalkieTalkie.TransmitOneShotAudio(this.mainAudio, this.detectPlayerSFX);
            this.rotationSpeed = 95f;
            this.rotatingSmoothly = false;
            this.lostLOSTimer = 0.0f;
            this.turretAnimator.SetInteger("TurretMode", 1);
          }
          if (this.IsServer)
          {
            if ((double) this.turretInterval >= 1.5)
            {
              this.turretInterval = 0.0f;
              Debug.Log((object) "Charging timer is up, setting to firing mode");
              if (!this.hasLineOfSight)
              {
                Debug.Log((object) "hasLineOfSight is false");
                this.targetPlayerWithRotation = (PlayerControllerB) null;
                this.RemoveTargetedPlayerClientRpc();
                break;
              }
              this.SwitchTurretMode(2);
              this.SetToModeClientRpc(2);
              break;
            }
            this.turretInterval += Time.deltaTime;
            break;
          }
          break;
        case TurretMode.Firing:
          if (this.turretModeLastFrame != TurretMode.Firing)
          {
            this.turretModeLastFrame = TurretMode.Firing;
            this.berserkAudio.Stop();
            this.mainAudio.clip = this.firingSFX;
            this.mainAudio.Play();
            this.farAudio.clip = this.firingFarSFX;
            this.farAudio.Play();
            this.bulletParticles.Play(true);
            this.bulletCollisionAudio.Play();
            if (this.fadeBulletAudioCoroutine != null)
              this.StopCoroutine(this.fadeBulletAudioCoroutine);
            this.bulletCollisionAudio.volume = 1f;
            this.rotatingSmoothly = false;
            this.lostLOSTimer = 0.0f;
            this.turretAnimator.SetInteger("TurretMode", 2);
          }
          if ((double) this.turretInterval >= 0.20999999344348907)
          {
            this.turretInterval = 0.0f;
            if ((Object) this.CheckForPlayersInLineOfSight(3f) == (Object) GameNetworkManager.Instance.localPlayerController)
            {
              if (GameNetworkManager.Instance.localPlayerController.health > 50)
                GameNetworkManager.Instance.localPlayerController.DamagePlayer(50, causeOfDeath: CauseOfDeath.Gunshots);
              else
                GameNetworkManager.Instance.localPlayerController.KillPlayer(this.aimPoint.forward * 40f, causeOfDeath: CauseOfDeath.Gunshots);
            }
            this.shootRay = new Ray(this.aimPoint.position, this.aimPoint.forward);
            if (Physics.Raycast(this.shootRay, out this.hit, 30f, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
            {
              this.bulletCollisionAudio.transform.position = this.shootRay.GetPoint(this.hit.distance - 0.5f);
              break;
            }
            break;
          }
          this.turretInterval += Time.deltaTime;
          break;
        case TurretMode.Berserk:
          if (this.turretModeLastFrame != TurretMode.Berserk)
          {
            this.turretModeLastFrame = TurretMode.Berserk;
            this.turretAnimator.SetInteger("TurretMode", 1);
            this.berserkTimer = 1.3f;
            this.berserkAudio.Play();
            this.rotationSpeed = 77f;
            this.enteringBerserkMode = true;
            this.rotatingSmoothly = true;
            this.lostLOSTimer = 0.0f;
            this.wasTargetingPlayerLastFrame = false;
            this.targetPlayerWithRotation = (PlayerControllerB) null;
          }
          if (this.enteringBerserkMode)
          {
            this.berserkTimer -= Time.deltaTime;
            if ((double) this.berserkTimer <= 0.0)
            {
              this.enteringBerserkMode = false;
              this.rotatingClockwise = true;
              this.berserkTimer = 9f;
              this.turretAnimator.SetInteger("TurretMode", 2);
              this.mainAudio.clip = this.firingSFX;
              this.mainAudio.Play();
              this.farAudio.clip = this.firingFarSFX;
              this.farAudio.Play();
              this.bulletParticles.Play(true);
              this.bulletCollisionAudio.Play();
              if (this.fadeBulletAudioCoroutine != null)
                this.StopCoroutine(this.fadeBulletAudioCoroutine);
              this.bulletCollisionAudio.volume = 1f;
              break;
            }
            break;
          }
          if ((double) this.turretInterval >= 0.20999999344348907)
          {
            this.turretInterval = 0.0f;
            if ((Object) this.CheckForPlayersInLineOfSight(3f) == (Object) GameNetworkManager.Instance.localPlayerController)
            {
              if (GameNetworkManager.Instance.localPlayerController.health > 50)
                GameNetworkManager.Instance.localPlayerController.DamagePlayer(50, causeOfDeath: CauseOfDeath.Gunshots);
              else
                GameNetworkManager.Instance.localPlayerController.KillPlayer(this.aimPoint.forward * 40f, causeOfDeath: CauseOfDeath.Gunshots);
            }
            this.shootRay = new Ray(this.aimPoint.position, this.aimPoint.forward);
            if (Physics.Raycast(this.shootRay, out this.hit, 30f, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
              this.bulletCollisionAudio.transform.position = this.shootRay.GetPoint(this.hit.distance - 0.5f);
          }
          else
            this.turretInterval += Time.deltaTime;
          if (this.IsServer)
          {
            this.berserkTimer -= Time.deltaTime;
            if ((double) this.berserkTimer <= 0.0)
            {
              this.SwitchTurretMode(0);
              this.SetToModeClientRpc(0);
              break;
            }
            break;
          }
          break;
      }
      if (this.rotatingClockwise)
      {
        this.turnTowardsObjectCompass.localEulerAngles = new Vector3(-180f, this.turretRod.localEulerAngles.y - Time.deltaTime * 20f, 180f);
        this.turretRod.rotation = Quaternion.RotateTowards(this.turretRod.rotation, this.turnTowardsObjectCompass.rotation, this.rotationSpeed * Time.deltaTime);
      }
      else
      {
        if (this.rotatingSmoothly)
          this.turnTowardsObjectCompass.localEulerAngles = new Vector3(-180f, Mathf.Clamp(this.targetRotation, -this.rotationRange, this.rotationRange), 180f);
        this.turretRod.rotation = Quaternion.RotateTowards(this.turretRod.rotation, this.turnTowardsObjectCompass.rotation, this.rotationSpeed * Time.deltaTime);
      }
    }
  }

  private void SetTargetToPlayerBody()
  {
    if (this.targetPlayerWithRotation.isPlayerDead)
    {
      if (!this.targetingDeadPlayer)
        this.targetingDeadPlayer = true;
      if (!((Object) this.targetPlayerWithRotation.deadBody != (Object) null))
        return;
      this.targetTransform = this.targetPlayerWithRotation.deadBody.bodyParts[5].transform;
    }
    else
    {
      this.targetingDeadPlayer = false;
      this.targetTransform = this.targetPlayerWithRotation.gameplayCamera.transform;
    }
  }

  private void TurnTowardsTargetIfHasLOS()
  {
    bool flag = true;
    if (this.targetingDeadPlayer || (double) Vector3.Angle(this.targetTransform.position - this.centerPoint.position, this.forwardFacingPos.forward) > (double) this.rotationRange)
      flag = false;
    if (Physics.Linecast(this.aimPoint.position, this.targetTransform.position, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
      flag = false;
    if (flag)
    {
      this.hasLineOfSight = true;
      this.lostLOSTimer = 0.0f;
      this.tempTransform.position = this.targetTransform.position;
      this.tempTransform.position -= Vector3.up * 0.15f;
      this.turnTowardsObjectCompass.LookAt(this.tempTransform);
    }
    else
    {
      if (this.hasLineOfSight)
      {
        this.hasLineOfSight = false;
        this.lostLOSTimer = 0.0f;
      }
      if (!this.IsServer)
        return;
      this.lostLOSTimer += Time.deltaTime;
      if ((double) this.lostLOSTimer < 2.0)
        return;
      this.lostLOSTimer = 0.0f;
      Debug.Log((object) "Turret: LOS timer ended on server. checking for new player target");
      PlayerControllerB playerControllerB = this.CheckForPlayersInLineOfSight();
      if ((Object) playerControllerB != (Object) null)
      {
        this.targetPlayerWithRotation = playerControllerB;
        this.SwitchTargetedPlayerClientRpc((int) playerControllerB.playerClientId);
        Debug.Log((object) "Turret: Got new player target");
      }
      else
      {
        Debug.Log((object) "Turret: No new player to target; returning to detection mode.");
        this.targetPlayerWithRotation = (PlayerControllerB) null;
        this.RemoveTargetedPlayerClientRpc();
      }
    }
  }

  public PlayerControllerB CheckForPlayersInLineOfSight(float radius = 2f, bool angleRangeCheck = false)
  {
    Vector3 forward = this.aimPoint.forward;
    Vector3 direction = Quaternion.Euler(0.0f, (float) (int) -(double) this.rotationRange / radius, 0.0f) * forward;
    float num = (float) ((double) this.rotationRange / (double) radius * 2.0);
    for (int index = 0; index <= 6; ++index)
    {
      this.shootRay = new Ray(this.centerPoint.position, direction);
      if (Physics.Raycast(this.shootRay, out this.hit, 30f, 1051400, QueryTriggerInteraction.Ignore))
      {
        if (this.hit.transform.CompareTag("Player"))
        {
          PlayerControllerB component = this.hit.transform.GetComponent<PlayerControllerB>();
          if (!((Object) component == (Object) null))
            return angleRangeCheck && (double) Vector3.Angle(component.transform.position + Vector3.up * 1.75f - this.centerPoint.position, this.forwardFacingPos.forward) > (double) this.rotationRange ? (PlayerControllerB) null : component;
          continue;
        }
        if ((this.turretMode == TurretMode.Firing || this.turretMode == TurretMode.Berserk && !this.enteringBerserkMode) && this.hit.transform.tag.StartsWith("PlayerRagdoll"))
        {
          Rigidbody component = this.hit.transform.GetComponent<Rigidbody>();
          if ((Object) component != (Object) null)
            component.AddForce(direction.normalized * 42f, ForceMode.Impulse);
        }
      }
      direction = Quaternion.Euler(0.0f, num / 6f, 0.0f) * direction;
    }
    return (PlayerControllerB) null;
  }

  [ClientRpc]
  public void SwitchRotationClientRpc(bool setRotateRight)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2426770061U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in setRotateRight, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2426770061U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsServer)
      return;
    this.SwitchRotationOnInterval(setRotateRight);
  }

  public void SwitchRotationOnInterval(bool setRotateRight)
  {
    if (this.rotatingRight)
    {
      this.rotatingRight = false;
      this.targetRotation = this.rotationRange;
    }
    else
    {
      this.rotatingRight = true;
      this.targetRotation = -this.rotationRange;
    }
  }

  [ClientRpc]
  public void SwitchTargetedPlayerClientRpc(int playerId, bool setModeToCharging = false)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(866050294U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      bufferWriter.WriteValueSafe<bool>(in setModeToCharging, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 866050294U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsServer)
      return;
    this.targetPlayerWithRotation = StartOfRound.Instance.allPlayerScripts[playerId];
    if (!setModeToCharging)
      return;
    this.SwitchTurretMode(1);
  }

  [ClientRpc]
  public void RemoveTargetedPlayerClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2800017671U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2800017671U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.targetPlayerWithRotation = (PlayerControllerB) null;
  }

  [ClientRpc]
  public void SetToModeClientRpc(int mode)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3335553538U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, mode);
      this.__endSendClientRpc(ref bufferWriter, 3335553538U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsServer)
      return;
    this.SwitchTurretMode(mode);
  }

  private void SwitchTurretMode(int mode) => this.turretMode = (TurretMode) mode;

  public void ToggleTurretEnabled(bool enabled)
  {
    if (this.turretActive == enabled)
      return;
    Debug.Log((object) string.Format("Toggling turret to {0}!", (object) enabled));
    this.ToggleTurretEnabledLocalClient(enabled);
    this.ToggleTurretServerRpc(enabled);
  }

  [ServerRpc(RequireOwnership = false)]
  public void ToggleTurretServerRpc(bool enabled)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2339273208U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in enabled, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 2339273208U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    Debug.Log((object) string.Format("Toggling turret to {0}! serverrpc", (object) enabled));
    this.ToggleTurretClientRpc(enabled);
  }

  [ClientRpc]
  public void ToggleTurretClientRpc(bool enabled)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1135819343U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in enabled, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1135819343U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) string.Format("Toggling turret to {0}! clientrpc", (object) enabled));
    if (this.turretActive == enabled)
      return;
    this.ToggleTurretEnabledLocalClient(enabled);
  }

  private void ToggleTurretEnabledLocalClient(bool enabled)
  {
    Debug.Log((object) string.Format("Setting turret active to {0}!", (object) enabled));
    this.turretActive = enabled;
    this.turretAnimator.SetBool("turretActive", this.turretActive);
    if (enabled)
    {
      this.mainAudio.PlayOneShot(this.turretActivate);
      WalkieTalkie.TransmitOneShotAudio(this.mainAudio, this.turretActivate);
    }
    else
    {
      this.mainAudio.PlayOneShot(this.turretDeactivate);
      WalkieTalkie.TransmitOneShotAudio(this.mainAudio, this.turretDeactivate);
    }
  }

  bool IHittable.Hit(
    int force,
    Vector3 hitDirection,
    PlayerControllerB playerWhoHit,
    bool playHitSFX)
  {
    if (this.turretMode == TurretMode.Berserk || this.turretMode == TurretMode.Firing)
      return false;
    this.SwitchTurretMode(3);
    this.EnterBerserkModeServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
    return true;
  }

  [ServerRpc(RequireOwnership = false)]
  public void EnterBerserkModeServerRpc(int playerWhoTriggered)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4195711963U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoTriggered);
      this.__endSendServerRpc(ref bufferWriter, 4195711963U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.EnterBerserkModeClientRpc(playerWhoTriggered);
  }

  [ClientRpc]
  public void EnterBerserkModeClientRpc(int playerWhoTriggered)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1436540455U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoTriggered);
      this.__endSendClientRpc(ref bufferWriter, 1436540455U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || playerWhoTriggered == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
      return;
    this.SwitchTurretMode(3);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_Turret()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2426770061U, new NetworkManager.RpcReceiveHandler(Turret.__rpc_handler_2426770061)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(866050294U, new NetworkManager.RpcReceiveHandler(Turret.__rpc_handler_866050294)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2800017671U, new NetworkManager.RpcReceiveHandler(Turret.__rpc_handler_2800017671)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3335553538U, new NetworkManager.RpcReceiveHandler(Turret.__rpc_handler_3335553538)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2339273208U, new NetworkManager.RpcReceiveHandler(Turret.__rpc_handler_2339273208)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1135819343U, new NetworkManager.RpcReceiveHandler(Turret.__rpc_handler_1135819343)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4195711963U, new NetworkManager.RpcReceiveHandler(Turret.__rpc_handler_4195711963)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1436540455U, new NetworkManager.RpcReceiveHandler(Turret.__rpc_handler_1436540455)));
  }

  private static void __rpc_handler_2426770061(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool setRotateRight;
    reader.ReadValueSafe<bool>(out setRotateRight, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Turret) target).SwitchRotationClientRpc(setRotateRight);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_866050294(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    bool setModeToCharging;
    reader.ReadValueSafe<bool>(out setModeToCharging, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Turret) target).SwitchTargetedPlayerClientRpc(playerId, setModeToCharging);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2800017671(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Turret) target).RemoveTargetedPlayerClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3335553538(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int mode;
    ByteUnpacker.ReadValueBitPacked(reader, out mode);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Turret) target).SetToModeClientRpc(mode);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2339273208(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool enabled;
    reader.ReadValueSafe<bool>(out enabled, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Turret) target).ToggleTurretServerRpc(enabled);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1135819343(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool enabled;
    reader.ReadValueSafe<bool>(out enabled, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Turret) target).ToggleTurretClientRpc(enabled);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4195711963(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerWhoTriggered;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Turret) target).EnterBerserkModeServerRpc(playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1436540455(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerWhoTriggered;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Turret) target).EnterBerserkModeClientRpc(playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (Turret);
}
