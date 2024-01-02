// Decompiled with JetBrains decompiler
// Type: DunGen.DunGenSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Tags;
using System;
using UnityEngine;

#nullable disable
namespace DunGen
{
  public sealed class DunGenSettings : ScriptableObject
  {
    private static DunGenSettings instance;
    [SerializeField]
    private DoorwaySocket defaultSocket;
    [SerializeField]
    private TagManager tagManager = new TagManager();

    public static DunGenSettings Instance => DunGenSettings.GetOrCreateInstance();

    private static DunGenSettings GetOrCreateInstance()
    {
      if ((UnityEngine.Object) DunGenSettings.instance == (UnityEngine.Object) null)
        DunGenSettings.instance = Resources.Load<DunGenSettings>("DunGen Settings");
      return !((UnityEngine.Object) DunGenSettings.instance == (UnityEngine.Object) null) ? DunGenSettings.instance : throw new Exception("No instance of DunGen settings was found.");
    }

    public DoorwaySocket DefaultSocket => this.defaultSocket;

    public TagManager TagManager => this.tagManager;
  }
}
