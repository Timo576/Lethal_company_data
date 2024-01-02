// Decompiled with JetBrains decompiler
// Type: CurrentLevelValues
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class CurrentLevelValues
{
  public int difficultyLevel;
  [Header("Scrap-collecting difficulty")]
  public int minScrap;
  public int maxScrap;
  public int minTotalScrapValue;
  public int maxTotalScrapValue;
  [Space(5f)]
  public float levelSizeMultiplier = 0.6f;
}
