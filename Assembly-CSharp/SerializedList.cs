// Decompiled with JetBrains decompiler
// Type: SerializedList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;

#nullable disable
internal struct SerializedList : INetworkSerializable
{
  public ulong[] Value;

  void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
  {
    serializer.SerializeValue<ulong>(ref this.Value, new FastBufferWriter.ForPrimitives());
  }
}
