// Decompiled with JetBrains decompiler
// Type: AnomalyType
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
[CreateAssetMenu(menuName = "ScriptableObjects/AnomalyType", order = 1)]
public class AnomalyType : ScriptableObject
{
  public string anomalyName;
  [Space(10f)]
  [Header("Capturing")]
  public float anomalyMaxHealth;
  [Range(0.0f, 1f)]
  public float captureDifficulty;
  public AnimationCurve difficultyVariance = AnimationCurve.Linear(0.0f, 1f, 1f, 1f);
  [Header("Spawning")]
  public AnimationCurve probabilityCurve;
  public int[] spawnableEnemies;
  public GameObject anomalyPrefab;
}
