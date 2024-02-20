// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.Unity_NFGO.NfgoPlayerModified
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using JetBrains.Annotations;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
namespace Dissonance.Integrations.Unity_NFGO
{
  [RequireComponent(typeof (NetworkObject))]
  public class NfgoPlayerModified : NetworkBehaviour, IDissonancePlayer
  {
    private static readonly Log Log = Logs.Create(LogCategory.Network, "NfgoPlayer");
    private DissonanceComms _comms;
    private Transform _transform;
    private string _playerIdString;
    private readonly NetworkVariable<FixedString128Bytes> _playerId = new NetworkVariable<FixedString128Bytes>(new FixedString128Bytes(""));
    private bool hasStartedTracking;

    [NotNull]
    private Transform Transform
    {
      get
      {
        if ((UnityEngine.Object) this._transform == (UnityEngine.Object) null)
          this._transform = this.transform;
        return this._transform;
      }
    }

    public Vector3 Position => this.Transform.position;

    public Quaternion Rotation => this.Transform.rotation;

    public bool IsTracking { get; private set; }

    public string PlayerId
    {
      get
      {
        if (this._playerIdString == null || !this._playerId.Value.Equals(this._playerIdString))
          this._playerIdString = this._playerId.Value.ToString();
        return this._playerIdString;
      }
    }

    public NetworkPlayerType Type
    {
      get
      {
        if (!((UnityEngine.Object) this._comms == (UnityEngine.Object) null))
        {
          FixedString128Bytes fixedString128Bytes = this._playerId.Value;
          if (!fixedString128Bytes.IsEmpty)
          {
            fixedString128Bytes = this._playerId.Value;
            return !fixedString128Bytes.Equals(this._comms.LocalPlayerName) ? NetworkPlayerType.Remote : NetworkPlayerType.Local;
          }
        }
        return NetworkPlayerType.Unknown;
      }
    }

    public override void OnDestroy()
    {
      if ((UnityEngine.Object) this._comms != (UnityEngine.Object) null)
        this._comms.LocalPlayerNameChanged -= new Action<string>(this.OnLocalPlayerIdChanged);
      this._playerId.OnValueChanged -= new NetworkVariable<FixedString128Bytes>.OnValueChangedDelegate(this.OnNetworkVariablePlayerIdChanged<FixedString128Bytes>);
    }

    public void VoiceChatTrackingStart()
    {
      this._comms = UnityEngine.Object.FindObjectOfType<DissonanceComms>();
      if ((UnityEngine.Object) this._comms == (UnityEngine.Object) null)
        throw NfgoPlayerModified.Log.CreateUserErrorException("cannot find DissonanceComms component in scene", "not placing a DissonanceComms component on a game object in the scene", "https://placeholder-software.co.uk/dissonance/docs/Basics/Quick-Start-UNet-HLAPI.html", "A6A291D8-5B53-417E-95CD-EC670637C532");
      if (!this.hasStartedTracking)
        this._playerId.OnValueChanged += new NetworkVariable<FixedString128Bytes>.OnValueChangedDelegate(this.OnNetworkVariablePlayerIdChanged<FixedString128Bytes>);
      if (this.gameObject.GetComponent<PlayerControllerB>().isPlayerControlled && this.IsOwner)
      {
        if (this._comms.LocalPlayerName != null)
          this.SetNameServerRpc(this._comms.LocalPlayerName);
        if (!this.hasStartedTracking)
          this._comms.LocalPlayerNameChanged += new Action<string>(this.OnLocalPlayerIdChanged);
      }
      else if (!this._playerId.Value.IsEmpty)
        this.StartTracking();
      this.hasStartedTracking = true;
    }

    [ServerRpc]
    public void SetNameServerRpc(string playerName)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(2623869394U, serverRpcParams, RpcDelivery.Reliable);
        bool flag = playerName != null;
        bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
        if (flag)
          bufferWriter.WriteValueSafe(playerName);
        this.__endSendServerRpc(ref bufferWriter, 2623869394U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this._playerId.Value = (FixedString128Bytes) playerName;
    }

    private void OnLocalPlayerIdChanged(string _)
    {
      if (this.IsTracking)
        this.StopTracking();
      if (this.gameObject.GetComponent<PlayerControllerB>().isPlayerControlled && this.IsOwner)
        this.SetNameServerRpc(this._comms.LocalPlayerName);
      this.StartTracking();
    }

    private void OnNetworkVariablePlayerIdChanged<T>(T previousvalue, T newvalue)
    {
      if (this.IsTracking)
        this.StopTracking();
      this.StartTracking();
    }

    private void StartTracking()
    {
      if (this.IsTracking)
        throw NfgoPlayerModified.Log.CreatePossibleBugException("Attempting to start player tracking, but tracking is already started", "4C2E74AA-CA09-4F98-B820-F2518A4E87D2");
      if (!((UnityEngine.Object) this._comms != (UnityEngine.Object) null))
        return;
      this._comms.TrackPlayerPosition((IDissonancePlayer) this);
      this.IsTracking = true;
    }

    private void StopTracking()
    {
      if (!this.IsTracking)
        throw NfgoPlayerModified.Log.CreatePossibleBugException("Attempting to stop player tracking, but tracking is not started", "BF8542EB-C13E-46FA-A8A0-B162F188BBA3");
      if (!((UnityEngine.Object) this._comms != (UnityEngine.Object) null))
        return;
      this._comms.StopTracking((IDissonancePlayer) this);
      this.IsTracking = false;
    }

    protected override void __initializeVariables()
    {
      if (this._playerId == null)
        throw new Exception("NfgoPlayerModified._playerId cannot be null. All NetworkVariableBase instances must be initialized.");
      this._playerId.Initialize((NetworkBehaviour) this);
      this.__nameNetworkVariable((NetworkVariableBase) this._playerId, "_playerId");
      this.NetworkVariableFields.Add((NetworkVariableBase) this._playerId);
      base.__initializeVariables();
    }

    [RuntimeInitializeOnLoadMethod]
    internal static void InitializeRPCS_NfgoPlayerModified()
    {
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2623869394U, new NetworkManager.RpcReceiveHandler(NfgoPlayerModified.__rpc_handler_2623869394)));
    }

    private static void __rpc_handler_2623869394(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        bool flag;
        reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
        string s = (string) null;
        if (flag)
          reader.ReadValueSafe(out s);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((NfgoPlayerModified) target).SetNameServerRpc(s);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    protected internal override string __getTypeName() => nameof (NfgoPlayerModified);
  }
}
