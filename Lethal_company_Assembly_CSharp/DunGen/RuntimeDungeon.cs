// Decompiled with JetBrains decompiler
// Type: DunGen.RuntimeDungeon
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DunGen
{
  [AddComponentMenu("DunGen/Runtime Dungeon")]
  public class RuntimeDungeon : MonoBehaviour
  {
    public DungeonGenerator Generator = new DungeonGenerator();
    public bool GenerateOnStart = true;
    public GameObject Root;

    protected virtual void Start()
    {
      if (!this.GenerateOnStart)
        return;
      this.Generate();
    }

    public void Generate()
    {
      if ((Object) this.Root != (Object) null)
        this.Generator.Root = this.Root;
      if (this.Generator.IsGenerating)
        return;
      this.Generator.Generate();
    }
  }
}
