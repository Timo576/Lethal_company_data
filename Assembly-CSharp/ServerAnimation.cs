// Decompiled with JetBrains decompiler
// Type: ServerAnimation
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;

#nullable disable
public struct ServerAnimation : INetworkSerializable
{
  public string animationString;
  public NetworkObjectReference animatorObj;
  public bool isTrigger;
  public bool setTrue;

  public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
  {
    serializer.SerializeValue<NetworkObjectReference>(ref this.animatorObj, new FastBufferWriter.ForNetworkSerializable());
    serializer.SerializeValue<bool>(ref this.isTrigger, new FastBufferWriter.ForPrimitives());
    if (this.isTrigger)
      return;
    serializer.SerializeValue<bool>(ref this.setTrue, new FastBufferWriter.ForPrimitives());
  }
}
