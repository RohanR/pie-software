/*
 * University of California, Berkeley
 * Pioneers in Engineering, Robotics Organizer.
 * PiER Framework v0.2b - 04/14/11
 * 
 * Changelog:
 * v0.2b
 *  - Split ReflectanceSensor into AnalogReflectanceSensor
 *      and DigitalReflectanceSensora
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
     * A class for analog sensors.
     * This class inherits Sensor class.
     */
    public class AnalogSensor : Hardware, Sensor //Generic class for analog sensors
    {
        AnalogIn Analog;
        private double value; /**< value of this sensor */

        /**
         * The constructor of AnalogSensor.
         * It initiates the value scale, and add itself to the global list of sensors.
         * 
         * @param PinNumber    the number of pin where this sensor is attached.
         * @param robot        the robot to which this sensor belongs.
         */
        public AnalogSensor(int PinNumber, Robot robot) : base(robot)
        {
            this.Analog = GetAnalogPort(PinNumber);
            this.Analog.SetLinearScale(0, 1023);
            this.value = 0.0d;
            this.addToGlobalList();
        }

        /**
         * addToGlobalList() adds this sensor to the list of sensors in base.robot.
         */
        public void addToGlobalList()
        {
            base.robot.sensors.Add(this);
        }

        /**
         * Update() reads the value of this sensor and stores it into the value field.
         */
        public void Update()
        {
            this.value = Analog.Read();
        }

        /**
         * getValues() returns the data stored in value field.
         * 
         * @return an array containing the updated value of the sensor.
         */
        public Array getValues()
        {
            double[] values = { this.value };
            return values;
        }

        /**
         * GetAnalogPort() creates an AnalogIn object which holds the information of the pin it
         * is attached to.
         * 
         * @param num the number of the pin this sensor is attached to.
         * @return an AnalogIn object holding the information of the pin.
         */
        public AnalogIn GetAnalogPort(int num) //Generic pin-getting method for analog ports
        {
            if (num == 0)
            {
                return new AnalogIn(AnalogIn.Pin.Ain0);
            }
            else if (num == 1)
            {
                return new AnalogIn(AnalogIn.Pin.Ain1);
            }
            else if (num == 2)
            {
                return new AnalogIn(AnalogIn.Pin.Ain2);
            }
            else if (num == 3)
            {
                return new AnalogIn(AnalogIn.Pin.Ain3);
            }
            else if (num == 4)
            {
                return new AnalogIn(AnalogIn.Pin.Ain4);
            }
            else
            {
                return new AnalogIn(AnalogIn.Pin.Ain5);
            }
        }

    }
}
