﻿// Decompiled with JetBrains decompiler
// Type: DunGen.CharacterTileChangedEvent
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace DunGen
{
  public delegate void CharacterTileChangedEvent(
    DungenCharacter character,
    Tile previousTile,
    Tile newTile);
}