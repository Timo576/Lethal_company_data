// Decompiled with JetBrains decompiler
// Type: Anomaly
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public abstract class Anomaly : NetworkBehaviour
{
  public AnomalyType anomalyType;
  public float initialInstability = 10f;
  public float difficultyMultiplier;
  public float normalizedHealth;
  public NetworkObject thisNetworkObject;
  public float maxHealth;
  [HideInInspector]
  public float health;
  [HideInInspector]
  public float removingHealth;
  [HideInInspector]
  public float usedInstability;
  public RoundManager roundManager;
  [Header("Misc properties")]
  public bool raycastToSurface;
  private bool addingInstability;

  public virtual void Start()
  {
    this.roundManager = UnityEngine.Object.FindObjectOfType<RoundManager>(false);
    this.thisNetworkObject = this.gameObject.GetComponent<NetworkObject>();
    this.addingInstability = true;
    if (!this.roundManager.hasInitializedLevelRandomSeed)
      this.roundManager.AnomalyValuesRandom = new System.Random();
    float time = (float) this.roundManager.AnomalyValuesRandom.NextDouble();
    Debug.Log((object) string.Format("the random number is {0}", (object) time));
    Debug.Log((object) string.Format("the anomaly capture base difficulty {0}", (object) this.anomalyType.captureDifficulty));
    this.difficultyMultiplier = this.anomalyType.captureDifficulty * this.anomalyType.difficultyVariance.Evaluate(time);
    this.maxHealth = this.anomalyType.anomalyMaxHealth;
    this.health = this.maxHealth;
  }

  public virtual void AnomalyDespawn(bool removedByPatcher = false)
  {
    if (!this.IsServer)
    {
      this.DespawnAnomalyServerRpc();
    }
    else
    {
      this.addingInstability = false;
      this.gameObject.GetComponent<NetworkObject>().Despawn();
      this.roundManager.SpawnedAnomalies.Remove(this);
      this.roundManager.SpawnedAnomalies.TrimExcess();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void DespawnAnomalyServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3450772816U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3450772816U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !this.gameObject.GetComponent<NetworkObject>().IsSpawned)
      return;
    this.AnomalyDespawn();
  }

  public virtual void Update()
  {
    if ((double) this.removingHealth > 0.0)
    {
      this.health -= this.removingHealth * Time.deltaTime;
      if (this.IsServer && (double) this.health <= 0.0)
        this.AnomalyDespawn(true);
    }
    else
    {
      this.health = Mathf.Clamp(this.health += Time.deltaTime * 1.5f, this.anomalyType.anomalyMaxHealth / 3f, this.anomalyType.anomalyMaxHealth);
      if (this.IsServer && this.addingInstability)
      {
        if ((double) this.usedInstability <= (double) this.initialInstability)
          this.usedInstability += Time.deltaTime;
        else
          this.usedInstability += Time.deltaTime / 3f;
      }
    }
    this.normalizedHealth = Mathf.Abs((float) ((double) this.anomalyType.anomalyMaxHealth / (double) this.health - 1.0));
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_Anomaly()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3450772816U, new NetworkManager.RpcReceiveHandler(Anomaly.__rpc_handler_3450772816)));
  }

  private static void __rpc_handler_3450772816(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Anomaly) target).DespawnAnomalyServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (Anomaly);
}
