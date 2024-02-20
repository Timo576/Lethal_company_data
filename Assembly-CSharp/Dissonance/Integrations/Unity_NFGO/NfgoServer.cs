// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.Unity_NFGO.NfgoServer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance.Networking;
using System;
using Unity.Netcode;

#nullable disable
namespace Dissonance.Integrations.Unity_NFGO
{
  public class NfgoServer : BaseServer<NfgoServer, NfgoClient, NfgoConn>
  {
    private readonly NfgoCommsNetwork _network;
    private byte[] _receiveBuffer = new byte[1024];
    private NetworkManager _networkManager;

    public NfgoServer(NfgoCommsNetwork network) => this._network = network;

    public override void Connect()
    {
      this._networkManager = NetworkManager.Singleton;
      this._networkManager.OnClientDisconnectCallback += new Action<ulong>(this.Disconnected);
      this._networkManager.CustomMessagingManager.RegisterNamedMessageHandler("DissonanceToServer", new CustomMessagingManager.HandleNamedMessageDelegate(this.NamedMessageHandler));
      base.Connect();
    }

    public override void Disconnect()
    {
      if ((UnityEngine.Object) this._networkManager != (UnityEngine.Object) null)
      {
        this._networkManager.OnClientDisconnectCallback -= new Action<ulong>(this.Disconnected);
        this._networkManager.CustomMessagingManager?.UnregisterNamedMessageHandler("DissonanceToServer");
        this._networkManager = (NetworkManager) null;
      }
      base.Disconnect();
    }

    private void Disconnected(ulong client) => this.ClientDisconnected(new NfgoConn(client));

    private void NamedMessageHandler(ulong sender, FastBufferReader stream)
    {
      int length = stream.Length;
      if (this._receiveBuffer.Length < length)
        Array.Resize<byte>(ref this._receiveBuffer, length);
      ArraySegment<byte> data = NfgoCommsNetwork.ReadPacket(ref stream, ref this._receiveBuffer);
      this.NetworkReceivedPacket(new NfgoConn(sender), data);
    }

    protected override void ReadMessages()
    {
    }

    protected override void SendReliable(NfgoConn destination, ArraySegment<byte> packet)
    {
      if ((UnityEngine.Object) this._networkManager == (UnityEngine.Object) null)
        return;
      this._network.SendToClient(packet, destination, true, this._networkManager);
    }

    protected override void SendUnreliable(NfgoConn destination, ArraySegment<byte> packet)
    {
      if ((UnityEngine.Object) this._networkManager == (UnityEngine.Object) null)
        return;
      this._network.SendToClient(packet, destination, false, this._networkManager);
    }
  }
}
