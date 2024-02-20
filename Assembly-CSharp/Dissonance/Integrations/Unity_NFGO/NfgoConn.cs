// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.Unity_NFGO.NfgoConn
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace Dissonance.Integrations.Unity_NFGO
{
  public readonly struct NfgoConn : IEquatable<NfgoConn>
  {
    public readonly ulong ClientId;

    public NfgoConn(ulong id) => this.ClientId = id;

    public bool Equals(NfgoConn other) => (long) this.ClientId == (long) other.ClientId;

    public override bool Equals(object obj) => obj is NfgoConn other && this.Equals(other);

    public override int GetHashCode() => this.ClientId.GetHashCode();

    public static bool operator ==(NfgoConn left, NfgoConn right) => left.Equals(right);

    public static bool operator !=(NfgoConn left, NfgoConn right) => !left.Equals(right);
  }
}
