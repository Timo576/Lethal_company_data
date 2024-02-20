// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.SingleLineClampAttribute
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class SingleLineClampAttribute : SingleLineAttribute
  {
    public SingleLineClampAttribute(string tooltip, double minValue, double maxValue)
      : base(tooltip)
    {
      this.MinValue = minValue;
      this.MaxValue = maxValue;
    }

    public double MinValue { get; private set; }

    public double MaxValue { get; private set; }
  }
}
