// Decompiled with JetBrains decompiler
// Type: DunGen.Adapters.BaseAdapter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DunGen.Adapters
{
  public abstract class BaseAdapter : MonoBehaviour
  {
    public int Priority;
    protected DungeonGenerator dungeonGenerator;

    public virtual bool RunDuringAnalysis { get; set; }

    protected virtual void OnEnable()
    {
      RuntimeDungeon component = this.GetComponent<RuntimeDungeon>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
      {
        this.dungeonGenerator = component.Generator;
        this.dungeonGenerator.RegisterPostProcessStep(new Action<DungeonGenerator>(this.OnPostProcess), this.Priority);
        this.dungeonGenerator.Cleared += new Action(this.Clear);
      }
      else
        Debug.LogError((object) ("[DunGen Adapter] RuntimeDungeon component is missing on GameObject '" + this.gameObject.name + "'. Adapters must be attached to the same GameObject as your RuntimeDungeon component"));
    }

    protected virtual void OnDisable()
    {
      if (this.dungeonGenerator == null)
        return;
      this.dungeonGenerator.UnregisterPostProcessStep(new Action<DungeonGenerator>(this.OnPostProcess));
      this.dungeonGenerator.Cleared -= new Action(this.Clear);
    }

    private void OnPostProcess(DungeonGenerator generator)
    {
      if (generator.IsAnalysis && !this.RunDuringAnalysis)
        return;
      this.Run(generator);
    }

    protected virtual void Clear()
    {
    }

    protected abstract void Run(DungeonGenerator generator);
  }
}
