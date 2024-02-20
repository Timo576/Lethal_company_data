// Decompiled with JetBrains decompiler
// Type: LevelGenerationManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen;
using UnityEngine;

#nullable disable
public class LevelGenerationManager : MonoBehaviour
{
  public RuntimeDungeon dungeonGenerator;
  private StartOfRound playersManager;
  private RoundManager roundManager;

  private void Awake()
  {
    this.roundManager = Object.FindObjectOfType<RoundManager>();
    this.playersManager = Object.FindObjectOfType<StartOfRound>();
    if ((Object) this.playersManager != (Object) null)
    {
      this.dungeonGenerator.Generator.ShouldRandomizeSeed = false;
      this.dungeonGenerator.Generator.Seed = this.playersManager.randomMapSeed;
      this.dungeonGenerator.Generate();
    }
    else
      Debug.Log((object) "PLAYERS MANAGER WAS NOT FOUND FROM OTHER SCENE!");
  }

  private void Update()
  {
  }
}
