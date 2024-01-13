// Decompiled with JetBrains decompiler
// Type: RelayJoinData
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
public struct RelayJoinData
{
  public string JoinCode;
  public string IPv4Address;
  public ushort Port;
  public Guid AllocationID;
  public byte[] AllocationIDBytes;
  public byte[] ConnectionData;
  public byte[] HostConnectionData;
  public byte[] Key;
}
