// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.Unity_NFGO.NfgoConn
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
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
