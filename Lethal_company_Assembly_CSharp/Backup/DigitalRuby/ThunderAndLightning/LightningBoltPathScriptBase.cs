// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningBoltPathScriptBase
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public abstract class LightningBoltPathScriptBase : LightningBoltPrefabScriptBase
  {
    [Header("Lightning Path Properties")]
    [Tooltip("The game objects to follow for the lightning path")]
    public List<GameObject> LightningPath;
    private readonly List<GameObject> currentPathObjects = new List<GameObject>();

    protected List<GameObject> GetCurrentPathObjects()
    {
      this.currentPathObjects.Clear();
      if (this.LightningPath != null)
      {
        foreach (GameObject gameObject in this.LightningPath)
        {
          if ((Object) gameObject != (Object) null && gameObject.activeInHierarchy)
            this.currentPathObjects.Add(gameObject);
        }
      }
      return this.currentPathObjects;
    }

    protected override LightningBoltParameters OnCreateParameters()
    {
      LightningBoltParameters parameters = base.OnCreateParameters();
      parameters.Generator = LightningGenerator.GeneratorInstance;
      return parameters;
    }
  }
}
