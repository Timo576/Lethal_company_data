// Decompiled with JetBrains decompiler
// Type: DunGen.StringUtil
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Text.RegularExpressions;

#nullable disable
namespace DunGen
{
  public static class StringUtil
  {
    private static Regex capitalLetterPattern = new Regex("([A-Z])");

    public static string SplitCamelCase(string input)
    {
      return StringUtil.capitalLetterPattern.Replace(input, " $1").Trim();
    }
  }
}
