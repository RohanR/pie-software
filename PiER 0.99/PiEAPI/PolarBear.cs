using System;
using System.Threading;
using System.Text;
using System.IO.Ports;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;

namespace PiEAPI
{
    public class PolarBear : MotorBase, Actuator
    {
        private const int maxBraking = 255;
        private ushort deviceAddress;
        public I2CDevice.Configuration conDeviceA;
        //private I2CDevice I2CA;
        private I2CDevice.I2CTransaction[] xActions = new I2CDevice.I2CTransaction[1];
        private byte[] sendbuffer = new byte[3] { 0x01, 1, 0 };
        private bool canMove;


        public PolarBear(Robot robo, ushort deviceAdd) : base(robo)
        {
            //attach to robot, allowing itself to be updated by the Robot thread when called
            
            robot.actuators.Add(this);
            deviceAddress = deviceAdd;

            //initialize the motor as not turning
            canMove = true;
            velocity = 0;
            brakeAmount = 0;
            upperStopZone = 0;
            lowerStopZone = 0;
            minVelocity = -255;
            maxVelocity = 255;

            //create I2C Device object representing both devices on our bus
            conDeviceA = new I2CDevice.Configuration(deviceAddress, 100);
        }

        /// <summary>
        /// As part of the Actuator interface, this allows the operating state of the motor to be set among drive, reverse and brake
        /// </summary>
        public void Write()
        {
            //long now = DateTime.Now.Ticks;
            //Debug.Print("Time since: " + (now - this.lastTicks));
            //this.lastTicks = now;
            if (canMove) // if the motor is supposed to be moving, then execute the code to set the speed
            {
                int pwm = actualVelocity;
                Debug.Print(pwm.ToString());
                if (pwm > 0)
                {
                    sendbuffer[1] = (byte)(1);
                    sendbuffer[2] = (byte)(pwm);
                }
                else if (pwm < 0)
                {
                    sendbuffer[1] = (byte)(0);
                    sendbuffer[2] = (byte)(-1 * pwm);
                }
                else
                {
                    sendbuffer[1] = (byte)(2);
                    sendbuffer[2] = (byte)(brakeAmount);
                }
            }
            else // if students want to brake, don't change motor speed, brake instead
            {
                sendbuffer[1] = (byte)(2);
                sendbuffer[2] = (byte)(maxBraking); // For whatever maxBraking is supposed to be
            }
            //Debug.Print(sendbuffer[2].ToString());
            xActions[0] = I2CDevice.CreateWriteTransaction(sendbuffer);
            robot.i2c.Execute(xActions, 200, conDeviceA);
        }

        /// <summary>
        /// Stops the motor and prevents it from driving until ReviveActuators() is called.
        /// StudentCode can use this for fail-safety measures, etc.
        /// </summary>
        public void Kill()
        {
            if (canMove)
            {
                velocity = 0;
                canMove = false;

                sendbuffer[1] = (byte)(2);
                sendbuffer[2] = (byte)(maxBraking);
                xActions[0] = I2CDevice.CreateWriteTransaction(sendbuffer);
                robot.i2c.Execute(xActions, 200, conDeviceA);
            }
        }

        /// <summary>
        /// StudentCode can use this to revive after killing. This will not affect disables sent
        /// from the field or PiEMOS.
        /// </summary>
        public void Revive()
        {
            if (robot.canMove && !this.canMove)
            {
                canMove = true;
            }
        }
    }
}
