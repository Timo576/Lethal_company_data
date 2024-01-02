// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.Unity_NFGO.NfgoCommsNetwork
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance.Datastructures;
using Dissonance.Extensions;
using Dissonance.Networking;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

#nullable disable
namespace Dissonance.Integrations.Unity_NFGO
{
  public class NfgoCommsNetwork : BaseCommsNetwork<NfgoServer, NfgoClient, NfgoConn, Unit, Unit>
  {
    private readonly ConcurrentPool<byte[]> _loopbackBuffers = new ConcurrentPool<byte[]>(8, (Func<byte[]>) (() => new byte[1024]));
    private readonly List<ArraySegment<byte>> _loopbackQueueToServer = new List<ArraySegment<byte>>();
    private readonly List<ArraySegment<byte>> _loopbackQueueToClient = new List<ArraySegment<byte>>();

    protected override NfgoClient CreateClient(Unit connectionParameters) => new NfgoClient(this);

    protected override NfgoServer CreateServer(Unit connectionParameters) => new NfgoServer(this);

    protected override void Update()
    {
      if (this.IsInitialized)
      {
        bool flag = NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient;
        bool isServer1 = NetworkManager.Singleton.IsServer;
        if ((!NetworkManager.Singleton.isActiveAndEnabled ? 0 : (flag | isServer1 ? 1 : 0)) != 0)
        {
          bool isServer2 = NetworkManager.Singleton.IsServer;
          bool isClient = NetworkManager.Singleton.IsClient;
          if (this.Mode.IsServerEnabled() != isServer2 || this.Mode.IsClientEnabled() != isClient)
          {
            if (isServer2 & isClient)
              this.RunAsHost(Unit.None, Unit.None);
            else if (isServer2)
              this.RunAsDedicatedServer(Unit.None);
            else if (isClient)
              this.RunAsClient(Unit.None);
          }
        }
        else if (this.Mode != NetworkMode.None)
        {
          this.Stop();
          this._loopbackQueueToClient.Clear();
          this._loopbackQueueToServer.Clear();
        }
        if (this.Client != null)
        {
          foreach (ArraySegment<byte> data in this._loopbackQueueToClient)
          {
            if (data.Array != null)
            {
              this.Client.NetworkReceivedPacket(data);
              this._loopbackBuffers.Put(data.Array);
            }
          }
        }
        this._loopbackQueueToClient.Clear();
        if (this.Server != null)
        {
          foreach (ArraySegment<byte> data in this._loopbackQueueToServer)
          {
            if (data.Array != null)
            {
              this.Server.NetworkReceivedPacket(new NfgoConn(NetworkManager.Singleton.LocalClientId), data);
              this._loopbackBuffers.Put(data.Array);
            }
          }
        }
        this._loopbackQueueToServer.Clear();
      }
      base.Update();
    }

    internal void SendToServer(ArraySegment<byte> packet, bool reliable, [NotNull] NetworkManager netManager)
    {
      if (packet.Array == null)
        throw new ArgumentException("packet is null");
      if (netManager.IsHost)
      {
        this._loopbackQueueToServer.Add(packet.CopyToSegment<byte>(this._loopbackBuffers.Get()));
      }
      else
      {
        using (FastBufferWriter messageStream = NfgoCommsNetwork.WritePacket(packet))
          netManager.CustomMessagingManager.SendNamedMessage("DissonanceToServer", 0UL, messageStream, reliable ? NetworkDelivery.ReliableSequenced : NetworkDelivery.Unreliable);
      }
    }

    internal void SendToClient(
      ArraySegment<byte> packet,
      NfgoConn client,
      bool reliable,
      [NotNull] NetworkManager netManager)
    {
      if (packet.Array == null)
        throw new ArgumentException("packet is null");
      if ((long) netManager.LocalClientId == (long) client.ClientId)
      {
        this._loopbackQueueToClient.Add(packet.CopyToSegment<byte>(this._loopbackBuffers.Get()));
      }
      else
      {
        if (!reliable && !netManager.ConnectedClients.ContainsKey(client.ClientId))
          return;
        using (FastBufferWriter messageStream = NfgoCommsNetwork.WritePacket(packet))
          netManager.CustomMessagingManager.SendNamedMessage("DissonanceToClient", client.ClientId, messageStream, reliable ? NetworkDelivery.ReliableSequenced : NetworkDelivery.Unreliable);
      }
    }

    private static FastBufferWriter WritePacket(ArraySegment<byte> packet)
    {
      FastBufferWriter fastBufferWriter = new FastBufferWriter(packet.Count + 4, Allocator.Temp);
      fastBufferWriter.WriteValueSafe<uint>((uint) packet.Count, new FastBufferWriter.ForPrimitives());
      fastBufferWriter.WriteBytesSafe(packet.Array, packet.Count, packet.Offset);
      return fastBufferWriter;
    }

    internal static ArraySegment<byte> ReadPacket(ref FastBufferReader reader, [CanBeNull] ref byte[] buffer)
    {
      uint num1;
      reader.ReadValueSafe<uint>(out num1, new FastBufferWriter.ForPrimitives());
      if (buffer == null || (long) buffer.Length < (long) num1)
        buffer = new byte[(int) Math.Max(1024U, num1)];
      for (int index = 0; (long) index < (long) num1; ++index)
      {
        byte num2;
        reader.ReadByteSafe(out num2);
        buffer[index] = num2;
      }
      return new ArraySegment<byte>(buffer, 0, (int) num1);
    }
  }
}
