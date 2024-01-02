// Decompiled with JetBrains decompiler
// Type: DunGen.Tags.TagManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DunGen.Tags
{
  [Serializable]
  public sealed class TagManager : ISerializationCallbackReceiver
  {
    private Dictionary<int, string> tags = new Dictionary<int, string>();
    [SerializeField]
    private List<int> keys = new List<int>();
    [SerializeField]
    private List<string> values = new List<string>();

    public int TagCount => this.tags.Count;

    public string TryGetNameFromID(int id)
    {
      string nameFromId = (string) null;
      this.tags.TryGetValue(id, out nameFromId);
      return nameFromId;
    }

    public bool TagExists(string name, out int id)
    {
      foreach (KeyValuePair<int, string> tag in this.tags)
      {
        if (tag.Value == name)
        {
          id = tag.Key;
          return true;
        }
      }
      id = -1;
      return false;
    }

    public bool TryRenameTag(int id, string newName)
    {
      string str;
      if (!this.tags.TryGetValue(id, out str))
        return false;
      if (str == newName)
        return true;
      if (this.TagExists(newName, out int _))
        return false;
      this.tags[id] = newName;
      return true;
    }

    public int AddTag(string tagName)
    {
      tagName = this.GetUnusedTagName(tagName);
      int num = 0;
      foreach (int key in this.tags.Keys)
        num = Mathf.Max(num, key + 1);
      this.tags[num] = tagName;
      return num;
    }

    private string GetUnusedTagName(string desiredTagName)
    {
      bool flag = false;
      foreach (KeyValuePair<int, string> tag in this.tags)
      {
        if (tag.Value == desiredTagName)
        {
          flag = true;
          break;
        }
      }
      if (!flag)
        return desiredTagName;
      int num = 2;
      string name = desiredTagName + " " + num.ToString();
      while (this.TagExists(name, out int _))
      {
        name = desiredTagName + " " + num.ToString();
        ++num;
      }
      return name;
    }

    public bool RemoveTag(int id)
    {
      if (!this.tags.ContainsKey(id))
        return false;
      this.tags.Remove(id);
      return true;
    }

    public int[] GetTagIDs()
    {
      int[] array = new int[this.tags.Count];
      int index = 0;
      foreach (int key in this.tags.Keys)
      {
        array[index] = key;
        ++index;
      }
      Array.Sort<int>(array);
      return array;
    }

    public void OnAfterDeserialize()
    {
      this.tags = new Dictionary<int, string>();
      for (int index = 0; index < this.keys.Count; ++index)
        this.tags[this.keys[index]] = this.values[index];
      this.keys.Clear();
      this.values.Clear();
    }

    public void OnBeforeSerialize()
    {
      this.keys = new List<int>();
      this.values = new List<string>();
      foreach (KeyValuePair<int, string> tag in this.tags)
      {
        this.keys.Add(tag.Key);
        this.values.Add(tag.Value);
      }
    }
  }
}
