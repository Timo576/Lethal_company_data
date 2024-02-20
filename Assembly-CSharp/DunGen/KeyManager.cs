// Decompiled with JetBrains decompiler
// Type: DunGen.KeyManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [CreateAssetMenu(menuName = "DunGen/Key Manager", order = 700)]
  [Serializable]
  public sealed class KeyManager : ScriptableObject
  {
    private ReadOnlyCollection<Key> keysReadOnly;
    [SerializeField]
    private List<Key> keys = new List<Key>();

    public ReadOnlyCollection<Key> Keys
    {
      get
      {
        if (this.keysReadOnly == null)
          this.keysReadOnly = new ReadOnlyCollection<Key>((IList<Key>) this.keys);
        return this.keysReadOnly;
      }
    }

    public Key CreateKey()
    {
      Key key = new Key(this.GetNextAvailableID());
      key.Name = UnityUtil.GetUniqueName("New Key", this.keys.Select<Key, string>((Func<Key, string>) (x => x.Name)));
      key.Colour = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
      this.keys.Add(key);
      return key;
    }

    public void DeleteKey(int index) => this.keys.RemoveAt(index);

    public Key GetKeyByID(int id)
    {
      return this.keys.Where<Key>((Func<Key, bool>) (x => x.ID == id)).FirstOrDefault<Key>();
    }

    public Key GetKeyByName(string name)
    {
      return this.keys.Where<Key>((Func<Key, bool>) (x => x.Name == name)).FirstOrDefault<Key>();
    }

    public bool RenameKey(int index, string newName)
    {
      if (this.keys[index].Name == newName)
        return false;
      newName = UnityUtil.GetUniqueName(newName, this.keys.Select<Key, string>((Func<Key, string>) (x => x.Name)));
      this.keys[index].Name = newName;
      return true;
    }

    private int GetNextAvailableID()
    {
      int nextAvailableId = 0;
      foreach (Key key in (IEnumerable<Key>) this.keys.OrderBy<Key, int>((Func<Key, int>) (x => x.ID)))
      {
        if (key.ID >= nextAvailableId)
          nextAvailableId = key.ID + 1;
      }
      return nextAvailableId;
    }
  }
}
