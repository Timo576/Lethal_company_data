﻿// Decompiled with JetBrains decompiler
// Type: AllItemsList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
[CreateAssetMenu(menuName = "ScriptableObjects/ItemsList", order = 2)]
public class AllItemsList : ScriptableObject
{
  public List<Item> itemsList = new List<Item>();
}