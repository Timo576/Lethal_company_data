// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.Unity_NFGO.NfgoClient
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance.Networking;
using JetBrains.Annotations;
using System;
using Unity.Netcode;

#nullable disable
namespace Dissonance.Integrations.Unity_NFGO
{
  public class NfgoClient : BaseClient<NfgoServer, NfgoClient, NfgoConn>
  {
    private readonly NfgoCommsNetwork _network;
    private NetworkManager _networkManager;
    private byte[] _receiveBuffer = new byte[1024];

    public NfgoClient([NotNull] NfgoCommsNetwork network)
      : base((ICommsNetworkState) network)
    {
      this._network = network;
    }

    public override void Connect()
    {
      this._networkManager = NetworkManager.Singleton;
      this._networkManager.CustomMessagingManager.RegisterNamedMessageHandler("DissonanceToClient", new CustomMessagingManager.HandleNamedMessageDelegate(this.NamedMessageHandler));
      this.Connected();
    }

    public override void Disconnect()
    {
      if ((UnityEngine.Object) this._networkManager != (UnityEngine.Object) null)
      {
        this._networkManager.CustomMessagingManager?.UnregisterNamedMessageHandler("DissonanceToClient");
        this._networkManager = (NetworkManager) null;
      }
      base.Disconnect();
    }

    private void NamedMessageHandler(ulong sender, FastBufferReader stream)
    {
      this.NetworkReceivedPacket(NfgoCommsNetwork.ReadPacket(ref stream, ref this._receiveBuffer));
    }

    protected override void ReadMessages()
    {
    }

    protected override void SendReliable(ArraySegment<byte> packet)
    {
      this._network.SendToServer(packet, true, this._networkManager);
    }

    protected override void SendUnreliable(ArraySegment<byte> packet)
    {
      this._network.SendToServer(packet, false, this._networkManager);
    }
  }
}
