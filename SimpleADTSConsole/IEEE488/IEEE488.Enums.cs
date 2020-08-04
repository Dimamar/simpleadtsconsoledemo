using System;

namespace WrapLib.IEEE488
{
  #region Timeout
  public enum Timeout : short
  { 
    TNONE,
    T10us,
    T30us,
    T100us,
    T300us,
    T1ms,
    T3ms,
    T10ms,
    T30ms,
    T100ms,
    T300ms,
    T1s,
    T3s,
    T10s,
    T30s,
    T100s,
    T300s,
    T1000s,
  }  
  #endregion

  //#region IbaOption
  //public enum IbaOption : short
  //{ 
  //  IbaPAD = 0x0001,
  //  IbaSAD = 0x0002,
  //  IbaTMO = 0x0003,
  //  IbaEOT = 0x0004,
  //  IbaPPC = 0x0005,
  //  IbaREADDR = 0x0006,
  //  IbaAUTOPOLL = 0x0007,
  //  IbaCICPROT = 0x0008,
  //  IbaIRQ = 0x0009,
  //  IbaSC = 0x000A,
  //  IbaSRE = 0x000B,
  //  IbaEOSrd = 0x000C,
  //  IbaEOSwrt = 0x000D,
  //  IbaEOScmp = 0x000E,
  //  IbaEOSchar = 0x000F,
  //  IbaPP2 = 0x0010,
  //  IbaTIMING = 0x0011,
  //  IbaDMA = 0x0012,
  //  IbaReadAdjust = 0x0013,
  //  IbaWriteAdjust = 0x014,
  //  IbaSendLLO = 0x0017,
  //  IbaSPollTime = 0x0018,
  //  IbaPPollTime = 0x0019,
  //  IbaEndBitIsNormal = 0x001A,
  //  IbaUnAddr = 0x001B,
  //  IbaHSCableLength = 0x001F,
  //  IbaIst = 0x0020,
  //  IbaRsv = 0x0021,
  //  IbaLON = 0x0022,
  //  IbaSerialNumber = 0x0023,
  //}  
  //#endregion

  //#region IbcOption
  //public enum IbcOption : short
  //{
  //  IbcPAD = 0x0001,
  //  IbcSAD = 0x0002,
  //  IbcTMO = 0x0003,
  //  IbcEOT = 0x0004,
  //  IbcPPC = 0x0005,
  //  IbcREADDR = 0x0006,
  //  IbcAUTOPOLL = 0x0007,
  //  IbcCICPROT = 0x0008,
  //  IbcIRQ = 0x0009,
  //  IbcSC = 0x000A,
  //  IbcSRE = 0x000B,
  //  IbcEOSrd = 0x000C,
  //  IbcEOSwrt = 0x000D,
  //  IbcEOScmp = 0x000E,
  //  IbcEOSchar = 0x000F,
  //  IbcPP2 = 0x0010,
  //  IbcTIMING = 0x0011,
  //  IbcDMA = 0x0012,
  //  IbcReadAdjust = 0x0013,
  //  IbcWriteAdjust = 0x014,
  //  IbcSendLLO = 0x0017,
  //  IbcSPollTime = 0x0018,
  //  IbcPPollTime = 0x0019,
  //  IbcEndBitIsNormal = 0x001A,
  //  IbcUnAddr = 0x001B,
  //  IbcHSCableLength = 0x001F,
  //  IbcIst = 0x0020,
  //  IbcRsv = 0x0021,
  //  IbcLON = 0x0022,
  //}
  //#endregion

  //#region IEE488Status
  //[Flags()]
  //public enum IEE488Status : ushort
  //{ 
  //  ERR = (1<<15),
  //  TIMO = (1<<14),
  //  END = (1<<13),
  //  SRQI = (1<<12),
  //  RQS = (1<<11),
  //  CMPL = (1<<8),
  //  LOK = (1<<7),
  //  REM = (1<<6),
  //  CIC = (1<<5),
  //  ATN = (1<<4),
  //  TACS = (1<<3),
  //  LACS = (1<<2),
  //  DTAS = (1<<1),
  //  DCAS = (1<<0),
  //}
  //#endregion
}
