// Decompiled with JetBrains decompiler
// Type: DunGen.StringUtil
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
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
