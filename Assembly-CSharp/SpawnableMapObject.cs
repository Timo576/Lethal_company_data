// Decompiled with JetBrains decompiler
// Type: SpawnableMapObject
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
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
