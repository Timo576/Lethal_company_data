// Decompiled with JetBrains decompiler
// Type: SpawnableMapObject
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class SpawnableMapObject
{
  public GameObject prefabToSpawn;
  public bool spawnFacingAwayFromWall;
  [Tooltip("Y Axis is the amount to be spawned; X axis should be from 0 to 1 and is randomly picked from.")]
  public AnimationCurve numberToSpawn;
}
