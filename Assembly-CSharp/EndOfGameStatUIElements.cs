// Decompiled with JetBrains decompiler
// Type: EndOfGameStatUIElements
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
[Serializable]
public class EndOfGameStatUIElements
{
  public TextMeshProUGUI quotaNumerator;
  public TextMeshProUGUI quotaDenominator;
  public TextMeshProUGUI[] playerNamesText;
  public Image[] playerStates;
  public Sprite aliveIcon;
  public Sprite deceasedIcon;
  public Sprite missingIcon;
  public TextMeshProUGUI[] playerNotesText;
  public TextMeshProUGUI gradeLetter;
  public Image allPlayersDeadOverlay;
  public TextMeshProUGUI penaltyAddition;
  public TextMeshProUGUI penaltyTotal;
  public TextMeshProUGUI challengeCollectedText;
  public TextMeshProUGUI challengeRankText;
}
