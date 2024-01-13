// Decompiled with JetBrains decompiler
// Type: ServerAudio
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;

#nullable disable
public struct ServerAudio : INetworkSerializable
{
  public NetworkObjectReference audioObj;
  public bool oneshot;
  public bool looped;

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
  {
    serializer.SerializeValue<NetworkObjectReference>(ref this.audioObj, new FastBufferWriter.ForNetworkSerializable());
    serializer.SerializeValue<bool>(ref this.oneshot, new FastBufferWriter.ForPrimitives());
    if (this.oneshot)
      return;
    serializer.SerializeValue<bool>(ref this.looped, new FastBufferWriter.ForPrimitives());
  }
}
