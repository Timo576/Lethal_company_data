// Decompiled with JetBrains decompiler
// Type: DunGen.Tags.TagPair
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace DunGen.Tags
{
  [Serializable]
  public sealed class TagPair
  {
    public Tag TagA;
    public Tag TagB;

    public TagPair()
    {
    }

    public TagPair(Tag a, Tag b)
    {
      this.TagA = a;
      this.TagB = b;
    }

    public override string ToString()
    {
      return string.Format("{0} <-> {1}", (object) this.TagA.Name, (object) this.TagB.Name);
    }

    public bool Matches(Tag a, Tag b, bool twoWay)
    {
      if (twoWay)
      {
        if (a == this.TagA && b == this.TagB)
          return true;
        return a == this.TagB && b == this.TagA;
      }
      return a == this.TagA && b == this.TagB;
    }
  }
}
