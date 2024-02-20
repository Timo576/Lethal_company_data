// Decompiled with JetBrains decompiler
// Type: DunGen.Tags.TagContainer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;

#nullable disable
namespace DunGen.Tags
{
  [Serializable]
  public sealed class TagContainer : IEnumerable<Tag>, IEnumerable
  {
    public List<Tag> Tags = new List<Tag>();

    public bool HasTag(Tag tag) => this.Tags.Contains(tag);

    public bool HasAnyTag(params Tag[] tags)
    {
      foreach (Tag tag in tags)
      {
        if (this.HasTag(tag))
          return true;
      }
      return false;
    }

    public bool HasAnyTag(TagContainer tags)
    {
      foreach (Tag tag in tags)
      {
        if (this.HasTag(tag))
          return true;
      }
      return false;
    }

    public bool HasAllTags(params Tag[] tags)
    {
      bool flag = true;
      foreach (Tag tag in tags)
      {
        if (!this.HasTag(tag))
        {
          flag = false;
          break;
        }
      }
      return flag;
    }

    public bool HasAllTags(TagContainer tags)
    {
      bool flag = true;
      foreach (Tag tag in tags)
      {
        if (!this.HasTag(tag))
        {
          flag = false;
          break;
        }
      }
      return flag;
    }

    public IEnumerator<Tag> GetEnumerator() => (IEnumerator<Tag>) this.Tags.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.Tags.GetEnumerator();
  }
}
