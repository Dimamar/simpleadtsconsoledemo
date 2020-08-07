using System;

namespace SimpleADTSConsole.Interfaces.IEEE488
{
  public class IEEE488Exception : Exception
  {
    public IEEE488Exception(string message)
      : base(message)
    {

    }
  }
}