// Decompiled with JetBrains decompiler
// Type: ServerAnimAndAudio
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;

#nullable disable
public struct ServerAnimAndAudio : INetworkSerializable
{
  public string animationString;
  public NetworkObjectReference animatorObj;
  public NetworkObjectReference audioObj;

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
  {
    serializer.SerializeValue(ref this.animationString);
    serializer.SerializeValue<NetworkObjectReference>(ref this.animatorObj, new FastBufferWriter.ForNetworkSerializable());
    serializer.SerializeValue<NetworkObjectReference>(ref this.audioObj, new FastBufferWriter.ForNetworkSerializable());
  }
}
