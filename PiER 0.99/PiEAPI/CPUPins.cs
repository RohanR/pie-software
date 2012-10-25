using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;

/// <summary>
/// This is the abstraction for the CPU Pins used in Master.cs; if you want
/// to change/add the pins, do it here.
/// </summary>
namespace PiEAPI
{
    /// <summary>
    /// This is the general interface for CPU Pins.
    /// </summary>
    public interface CPUPins
    {
        Cpu.Pin yellowLEDPin { get; }
        Cpu.Pin redLEDPin { get; }
        Cpu.Pin digSens6Pin { get; }
        Cpu.Pin digSens7Pin { get; }
    }

    /// <summary>
    /// This is the specific class for the Panda II's pins.
    /// </summary>
    public class PandaTwoCPUPins
    {
        public Cpu.Pin yellowLEDPin { get { return (Cpu.Pin)FEZ_Pin.Digital.IO59; } }
        public Cpu.Pin redLEDPin { get { return (Cpu.Pin)FEZ_Pin.Digital.IO60; } }
        public Cpu.Pin digSens6Pin { get { return (Cpu.Pin)FEZ_Pin.Digital.IO63; } }
        public Cpu.Pin digSens7Pin { get { return (Cpu.Pin)FEZ_Pin.Digital.IO65; } }
    }
}
