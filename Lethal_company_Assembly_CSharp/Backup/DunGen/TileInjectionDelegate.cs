﻿// Decompiled with JetBrains decompiler
// Type: DunGen.TileInjectionDelegate
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

#nullable disable
namespace DunGen
{
  public delegate void TileInjectionDelegate(
    RandomStream randomStream,
    ref List<InjectedTile> tilesToInject);
}