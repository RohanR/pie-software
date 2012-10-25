/*
 * University of California, Berkeley
 * Pioneers in Engineering, Robotics Organizer.
 * PiER Framework v0.2b - 04/14/11
 * 
 * Changelog:
 * v0.2b
 *  - Split ReflectanceSensor into AnalogReflectanceSensor
 *      and DigitalReflectanceSensor
 *  - Read now returns an int instead of an Object
*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace PiEAPI
{
    public class AnalogPotentiometer : AnalogSensor
    {
        AnalogIn Potentiometer;
        private int update_value = 1; /// Private sensor values. ///
        public static double AngleCalibration = .29326; /// Multiplying the raw output value by .29326 returns the angle in degrees. ///
        public AnalogPotentiometer(int PinNumber)
        {
            this.Potentiometer = GetAnalogPort(PinNumber);
            Potentiometer.SetLinearScale(0, 1023);
        }

        public int GetAngle(ref int value) /// Returns the angle in degrees. ///
        {
            value = (Potentiometer.Read() * AngleCalibration);
        }

        public override void Update() /// Updates the private sensor value. ///
        {
            GetAngle(ref update_value);
        }

        public override int GetUpdate() /// Returns the current private sensor value. ///
        {
            return update_value;
        }
    }
}
