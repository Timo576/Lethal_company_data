// Decompiled with JetBrains decompiler
// Type: DunGen.RandomPrefab
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DunGen
{
  [AddComponentMenu("DunGen/Random Props/Random Prefab")]
  public class RandomPrefab : RandomProp
  {
    [AcceptGameObjectTypes(GameObjectFilter.Asset)]
    public GameObjectChanceTable Props = new GameObjectChanceTable();
    public bool ZeroPosition = true;
    public bool ZeroRotation = true;

    public override void Process(RandomStream randomStream, Tile tile)
    {
      if (this.Props.Weights.Count <= 0)
        return;
      GameObject original = this.Props.GetRandom(randomStream, tile.Placement.IsOnMainPath, tile.Placement.NormalizedDepth, (GameObject) null, true, true).Value;
      GameObject gameObject = Object.Instantiate<GameObject>(original);
      gameObject.transform.parent = this.transform;
      gameObject.transform.localPosition = !this.ZeroPosition ? original.transform.localPosition : Vector3.zero;
      if (this.ZeroRotation)
        gameObject.transform.localRotation = Quaternion.identity;
      else
        gameObject.transform.localRotation = original.transform.localRotation;
    }
  }
}
