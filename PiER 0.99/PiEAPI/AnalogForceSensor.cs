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
    /**
     * A class for analog force sensors used to measure pressure.
     * It inherits update() and GetAnalogPort() from super class AnalogSensor.
     */
    public class AnalogForceSensor : AnalogSensor
    {
        
        AnalogIn ForceSensor;
        private double value;
        /**
         * A constructor for this class. It sets the pin number in order to read 
         * values and set the linear scale.
         * 
         * @param PinNumber    the num of pin where this sensor is attached to.
         * @param robot        the robot to which this sensor belongs.
         */
        public AnalogForceSensor(int PinNumber, Robot robot) : base(PinNumber, robot)
        {
            this.ForceSensor = GetAnalogPort(PinNumber);
            this.ForceSensor.SetLinearScale(0, 1023);
        }

        /**
         * Update() reads the value of this sensor and stores it into the value field.
         */
        new public void Update()
        {
            this.value = ForceSensor.Read();
        }

        /**
         * getValues() returns the data stored in value field.
         * 
         * @return an array containing the updated value of the sensor.
         */
        new public Array getValues()
        {
            double[] values = { this.value };
            return values;
        }
    }
}
