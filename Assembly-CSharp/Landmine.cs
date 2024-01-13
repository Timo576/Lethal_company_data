// Decompiled with JetBrains decompiler
// Type: Landmine
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class Landmine : NetworkBehaviour, IHittable
{
  private bool mineActivated = true;
  public bool hasExploded;
  public ParticleSystem explosionParticle;
  public Animator mineAnimator;
  public AudioSource mineAudio;
  public AudioSource mineFarAudio;
  public AudioClip mineDetonate;
  public AudioClip mineTrigger;
  public AudioClip mineDetonateFar;
  public AudioClip mineDeactivate;
  public AudioClip minePress;
  private bool sendingExplosionRPC;
  private RaycastHit hit;
  private RoundManager roundManager;
  private float pressMineDebounceTimer;
  private bool localPlayerOnMine;

  private void Start() => this.StartCoroutine(this.StartIdleAnimation());

  private void Update()
  {
    if ((double) this.pressMineDebounceTimer > 0.0)
      this.pressMineDebounceTimer -= Time.deltaTime;
    if (!this.localPlayerOnMine || !GameNetworkManager.Instance.localPlayerController.teleportedLastFrame)
      return;
    this.localPlayerOnMine = false;
    this.TriggerMineOnLocalClientByExiting();
  }

  public void ToggleMine(bool enabled)
  {
    if (this.mineActivated == enabled)
      return;
    this.mineActivated = enabled;
    if (!enabled)
    {
      this.mineAudio.PlayOneShot(this.mineDeactivate);
      WalkieTalkie.TransmitOneShotAudio(this.mineAudio, this.mineDeactivate);
    }
    this.ToggleMineServerRpc(enabled);
  }

  [ServerRpc(RequireOwnership = false)]
  public void ToggleMineServerRpc(bool enable)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2763604698U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in enable, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 2763604698U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ToggleMineClientRpc(enable);
  }

  [ClientRpc]
  public void ToggleMineClientRpc(bool enable)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3479956057U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in enable, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 3479956057U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.ToggleMineEnabledLocalClient(enable);
  }

  public void ToggleMineEnabledLocalClient(bool enabled)
  {
    if (this.mineActivated == enabled)
      return;
    this.mineActivated = enabled;
    if (enabled)
      return;
    this.mineAudio.PlayOneShot(this.mineDeactivate);
    WalkieTalkie.TransmitOneShotAudio(this.mineAudio, this.mineDeactivate);
  }

  private IEnumerator StartIdleAnimation()
  {
    this.roundManager = Object.FindObjectOfType<RoundManager>();
    if (!((Object) this.roundManager == (Object) null))
    {
      if (this.roundManager.BreakerBoxRandom != null)
        yield return (object) new WaitForSeconds((float) this.roundManager.BreakerBoxRandom.NextDouble() + 0.5f);
      this.mineAnimator.SetTrigger("startIdle");
      this.mineAudio.pitch = Random.Range(0.9f, 1.1f);
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (this.hasExploded || (double) this.pressMineDebounceTimer > 0.0)
      return;
    Debug.Log((object) string.Format("Trigger entered mine: {0}; {1}; {2}", (object) other.tag, (object) other.CompareTag("Player"), (object) (bool) (other.CompareTag("PhysicsProp") ? 1 : (other.tag.StartsWith("PlayerRagdoll") ? 1 : 0))));
    if (other.CompareTag("Player"))
    {
      PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
      if ((Object) component != (Object) GameNetworkManager.Instance.localPlayerController || !((Object) component != (Object) null) || component.isPlayerDead)
        return;
      this.localPlayerOnMine = true;
      this.pressMineDebounceTimer = 0.5f;
      this.PressMineServerRpc();
    }
    else
    {
      if (!other.CompareTag("PhysicsProp") && !other.tag.StartsWith("PlayerRagdoll"))
        return;
      if ((bool) (Object) other.GetComponent<DeadBodyInfo>())
      {
        if ((Object) other.GetComponent<DeadBodyInfo>().playerScript != (Object) GameNetworkManager.Instance.localPlayerController)
          return;
      }
      else if ((bool) (Object) other.GetComponent<GrabbableObject>() && !other.GetComponent<GrabbableObject>().NetworkObject.IsOwner)
        return;
      this.pressMineDebounceTimer = 0.5f;
      this.PressMineServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void PressMineServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4224840819U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 4224840819U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PressMineClientRpc();
  }

  [ClientRpc]
  public void PressMineClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2652432181U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2652432181U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.pressMineDebounceTimer = 0.5f;
    this.mineAudio.PlayOneShot(this.minePress);
    WalkieTalkie.TransmitOneShotAudio(this.mineAudio, this.minePress);
  }

  private void OnTriggerExit(Collider other)
  {
    if (this.hasExploded || !this.mineActivated)
      return;
    Debug.Log((object) ("Object leaving mine trigger, gameobject name: " + other.gameObject.name));
    Debug.Log((object) string.Format("Trigger exited mine: {0}; ({1} / {2}) {3}; {4}", (object) other.tag, (object) other.gameObject.name, (object) other.transform.name, (object) other.CompareTag("Player"), (object) (bool) (other.CompareTag("PhysicsProp") ? 1 : (other.tag.StartsWith("PlayerRagdoll") ? 1 : 0))));
    if (other.CompareTag("Player"))
    {
      PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
      if (!((Object) component != (Object) null) || component.isPlayerDead || (Object) component != (Object) GameNetworkManager.Instance.localPlayerController)
        return;
      this.localPlayerOnMine = false;
      this.TriggerMineOnLocalClientByExiting();
    }
    else
    {
      if (!other.tag.StartsWith("PlayerRagdoll") && !other.CompareTag("PhysicsProp"))
        return;
      if ((bool) (Object) other.GetComponent<DeadBodyInfo>())
      {
        if ((Object) other.GetComponent<DeadBodyInfo>().playerScript != (Object) GameNetworkManager.Instance.localPlayerController)
          return;
      }
      else if ((bool) (Object) other.GetComponent<GrabbableObject>() && !other.GetComponent<GrabbableObject>().NetworkObject.IsOwner)
        return;
      this.TriggerMineOnLocalClientByExiting();
    }
  }

  private void TriggerMineOnLocalClientByExiting()
  {
    if (this.hasExploded)
      return;
    this.SetOffMineAnimation();
    this.sendingExplosionRPC = true;
    this.ExplodeMineServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void ExplodeMineServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3032666565U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3032666565U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ExplodeMineClientRpc();
  }

  [ClientRpc]
  public void ExplodeMineClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(456724201U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 456724201U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (this.sendingExplosionRPC)
      this.sendingExplosionRPC = false;
    else
      this.SetOffMineAnimation();
  }

  public void SetOffMineAnimation()
  {
    this.hasExploded = true;
    this.mineAnimator.SetTrigger("detonate");
    this.mineAudio.PlayOneShot(this.mineTrigger, 1f);
  }

  private IEnumerator TriggerOtherMineDelayed(Landmine mine)
  {
    if (!mine.hasExploded)
    {
      mine.mineAudio.pitch = Random.Range(0.75f, 1.07f);
      mine.hasExploded = true;
      yield return (object) new WaitForSeconds(0.2f);
      mine.SetOffMineAnimation();
    }
  }

  public void Detonate()
  {
    this.mineAudio.pitch = Random.Range(0.93f, 1.07f);
    this.mineAudio.PlayOneShot(this.mineDetonate, 1f);
    Landmine.SpawnExplosion(this.transform.position + Vector3.up, killRange: 5.7f, damageRange: 6.4f);
  }

  public static void SpawnExplosion(
    Vector3 explosionPosition,
    bool spawnExplosionEffect = false,
    float killRange = 1f,
    float damageRange = 1f)
  {
    Debug.Log((object) "Spawning explosion at pos: {explosionPosition}");
    if (spawnExplosionEffect)
      Object.Instantiate<GameObject>(StartOfRound.Instance.explosionPrefab, explosionPosition, Quaternion.Euler(-90f, 0.0f, 0.0f), RoundManager.Instance.mapPropsContainer.transform).SetActive(true);
    float num1 = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, explosionPosition);
    if ((double) num1 < 14.0)
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
    else if ((double) num1 < 25.0)
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
    Collider[] colliderArray = Physics.OverlapSphere(explosionPosition, 6f, 2621448, QueryTriggerInteraction.Collide);
    for (int index = 0; index < colliderArray.Length; ++index)
    {
      float num2 = Vector3.Distance(explosionPosition, colliderArray[index].transform.position);
      if ((double) num2 <= 4.0 || !Physics.Linecast(explosionPosition, colliderArray[index].transform.position + Vector3.up * 0.3f, 256, QueryTriggerInteraction.Ignore))
      {
        if (colliderArray[index].gameObject.layer == 3)
        {
          PlayerControllerB component = colliderArray[index].gameObject.GetComponent<PlayerControllerB>();
          if ((Object) component != (Object) null && component.IsOwner)
          {
            if ((double) num2 < (double) killRange)
            {
              Vector3 bodyVelocity = (component.gameplayCamera.transform.position - explosionPosition) * 80f / Vector3.Distance(component.gameplayCamera.transform.position, explosionPosition);
              component.KillPlayer(bodyVelocity, causeOfDeath: CauseOfDeath.Blast);
            }
            else if ((double) num2 < (double) damageRange)
              component.DamagePlayer(50);
          }
        }
        else if (colliderArray[index].gameObject.layer == 21)
        {
          Landmine componentInChildren = colliderArray[index].gameObject.GetComponentInChildren<Landmine>();
          if ((Object) componentInChildren != (Object) null && !componentInChildren.hasExploded && (double) num2 < 6.0)
          {
            Debug.Log((object) "Setting off other mine");
            componentInChildren.StartCoroutine(componentInChildren.TriggerOtherMineDelayed(componentInChildren));
          }
        }
        else if (colliderArray[index].gameObject.layer == 19)
        {
          EnemyAICollisionDetect componentInChildren = colliderArray[index].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
          if ((Object) componentInChildren != (Object) null && componentInChildren.mainScript.IsOwner && (double) num2 < 4.5)
            componentInChildren.mainScript.HitEnemyOnLocalClient(6);
        }
      }
    }
    int num3 = ~LayerMask.GetMask("Room");
    int layerMask = ~LayerMask.GetMask("Colliders");
    foreach (Component component1 in Physics.OverlapSphere(explosionPosition, 10f, layerMask))
    {
      Rigidbody component2 = component1.GetComponent<Rigidbody>();
      if ((Object) component2 != (Object) null)
        component2.AddExplosionForce(70f, explosionPosition, 10f);
    }
  }

  public bool MineHasLineOfSight(Vector3 pos)
  {
    return !Physics.Linecast(this.transform.position, pos, out this.hit, 256);
  }

  bool IHittable.Hit(
    int force,
    Vector3 hitDirection,
    PlayerControllerB playerWhoHit = null,
    bool playHitSFX = false)
  {
    this.SetOffMineAnimation();
    this.sendingExplosionRPC = true;
    this.ExplodeMineServerRpc();
    return true;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_Landmine()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2763604698U, new NetworkManager.RpcReceiveHandler(Landmine.__rpc_handler_2763604698)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3479956057U, new NetworkManager.RpcReceiveHandler(Landmine.__rpc_handler_3479956057)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4224840819U, new NetworkManager.RpcReceiveHandler(Landmine.__rpc_handler_4224840819)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2652432181U, new NetworkManager.RpcReceiveHandler(Landmine.__rpc_handler_2652432181)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3032666565U, new NetworkManager.RpcReceiveHandler(Landmine.__rpc_handler_3032666565)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(456724201U, new NetworkManager.RpcReceiveHandler(Landmine.__rpc_handler_456724201)));
  }

  private static void __rpc_handler_2763604698(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool enable;
    reader.ReadValueSafe<bool>(out enable, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Landmine) target).ToggleMineServerRpc(enable);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3479956057(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool enable;
    reader.ReadValueSafe<bool>(out enable, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Landmine) target).ToggleMineClientRpc(enable);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4224840819(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Landmine) target).PressMineServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2652432181(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Landmine) target).PressMineClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3032666565(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Landmine) target).ExplodeMineServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_456724201(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Landmine) target).ExplodeMineClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (Landmine);
}
