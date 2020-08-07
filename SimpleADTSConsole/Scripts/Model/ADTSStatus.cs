using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleADTSConsole
{
    /// <summary>
    /// Маска состояния условных тригеров ADTS
    /// </summary>
    [Flags]
    public enum ADTSStatus
    {
        /// <summary>
        /// Set after 15 seconds (to give pressure time to settle)
        /// after both bits 10 and 8 (if not in Pt only mode) become set.
        /// </summary>
        /// <remarks>
        /// Bit 1 should generally be used to indicate that all pressures are stable before taking readings,
        /// rather than bits 8 or 10 as bit 1 includes stabilization time.
        /// </remarks>
        StableAtAimValue = 1 << 1,
        SafeAtGround = 1 << 2,

        /// <summary>
        /// This is set to 1 if bit 9 is set and bit 11 is set.
        /// </summary>
        Ramping = 1 << 3,

        /// <summary>
        /// Set immediately the aim set-point is reached.
        /// </summary>
        PsAtSetPointAndInControlMode = 1 << 8,
        PsRampingAndAchievingRate = 1 << 9,

        /// <summary>
        /// Set immediately the aim set-point is reached.
        /// </summary>
        PtAtSetPointAndInControlMode = 1 << 10,
        PtRampingAndAchievingRate = 1 << 11,
    }
}
