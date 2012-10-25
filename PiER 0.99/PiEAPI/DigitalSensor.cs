/*
 * University of California, Berkeley
 * Pioneers in Engineering, Robotics Organizer.
 * PiER Framework v2.a - 04/08/11
*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace PiEAPI
{
    /// <summary>
    /// Generic class for digital sensors
    /// </summary>
    public class DigitalSensor : Hardware, Sensor //Basic class for digital sensors
    {
        /// <summary>
        /// Object encapsulating digital input port.
        /// </summary>
        InputPort Digital;

        /// <summary>
        /// Locally store last read value.
        /// </summary>
        private bool Value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="robot">Robot object</param>
        public DigitalSensor(Robot robot) : base (robot)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="robot">Robot object</param>
        public DigitalSensor(int Pin, Robot robot) : base (robot)
        {
            Digital = GetPort(Pin);
            addToGlobalList();
        }

        /// <summary>
        /// Return the last read value in an array.
        /// </summary>
        /// <returns>Array with returned value.</returns>
        public Array getValues()
        {
            return new bool[] { Value };
        }

        /// <summary>
        /// Update the locally stored data variable.
        /// </summary>
        public void Update()
        {
            bool Data = Digital.Read();
            Value = Data;
        }

        /// <summary>
        /// TODO: IMPLEMENT
        /// </summary>
        public void addToGlobalList()
        {
        }

        /// <summary>
        /// Get the corresponding InputPort object
        /// </summary>
        /// <param name="num">Port number</param>
        /// <returns>InputPort object</returns>
        public virtual InputPort GetPort(int num) //Generic pin-getting method
        {

            if (num == 0)
            {
                return new InputPort((Cpu.Pin)FEZ_Pin.Digital.IO57, false, Port.ResistorMode.Disabled);
            }
            else if (num == 1)
            {
                return new InputPort((Cpu.Pin)FEZ_Pin.Digital.IO55, false, Port.ResistorMode.Disabled);
            }
            else if (num == 2)
            {
                return new InputPort((Cpu.Pin)FEZ_Pin.Digital.IO53, false, Port.ResistorMode.Disabled);
            }
            else if (num == 3)
            {
                return new InputPort((Cpu.Pin)FEZ_Pin.Digital.IO51, false, Port.ResistorMode.Disabled);
            }
            else if (num == 4)
            {
                return new InputPort((Cpu.Pin)FEZ_Pin.Digital.IO58, false, Port.ResistorMode.Disabled);
            }
            else if (num == 5)
            {
                return new InputPort((Cpu.Pin)FEZ_Pin.Digital.IO56, false, Port.ResistorMode.Disabled);
            }
            else if (num == 6)
            {
                return new InputPort((Cpu.Pin)FEZ_Pin.Digital.IO54, false, Port.ResistorMode.Disabled);
            }
            else
            {
                return new InputPort((Cpu.Pin)FEZ_Pin.Digital.IO52, false, Port.ResistorMode.Disabled);
            }

        }
    }
}
