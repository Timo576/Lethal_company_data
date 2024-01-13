// Decompiled with JetBrains decompiler
// Type: PlayerListSlot
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
[Serializable]
public class PlayerListSlot
{
  public GameObject slotContainer;
  public GameObject volumeSliderContainer;
  public GameObject KickUserButton;
  public bool isConnected;
  public TextMeshProUGUI usernameHeader;
  public Slider volumeSlider;
  public ulong playerSteamId;
}
