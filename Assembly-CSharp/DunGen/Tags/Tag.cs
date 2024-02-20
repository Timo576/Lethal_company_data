// Decompiled with JetBrains decompiler
// Type: DunGen.Tags.Tag
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DunGen.Tags
{
  [Serializable]
  public sealed class Tag : IEqualityComparer<Tag>
  {
    [SerializeField]
    private int id = -1;

    public int ID
    {
      get => this.id;
      set => this.id = value;
    }

    public string Name
    {
      get => DunGenSettings.Instance.TagManager.TryGetNameFromID(this.id);
      set => DunGenSettings.Instance.TagManager.TryRenameTag(this.id, value);
    }

    public Tag(int id) => this.id = id;

    public Tag(string name) => DunGenSettings.Instance.TagManager.TagExists(name, out this.id);

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      Tag y = obj as Tag;
      return !(y == (Tag) null) && this.Equals(this, y);
    }

    public override int GetHashCode() => this.id;

    public override string ToString()
    {
      return string.Format("[{0}] {1}", (object) this.id, (object) DunGenSettings.Instance.TagManager.TryGetNameFromID(this.id));
    }

    public int GetHashCode(Tag tag) => this.id;

    public bool Equals(Tag x, Tag y)
    {
      if (x == (Tag) null && y == (Tag) null)
        return true;
      return !(x == (Tag) null) && !(y == (Tag) null) && x.id == y.id;
    }

    public static bool operator ==(Tag a, Tag b)
    {
      if ((object) a == null && (object) b == null)
        return true;
      return (object) a != null && (object) b != null && a.id == b.id;
    }

    public static bool operator !=(Tag a, Tag b)
    {
      if (a == (Tag) null && b == (Tag) null)
        return false;
      return a == (Tag) null && b != (Tag) null || a.id != b.id;
    }
  }
}
