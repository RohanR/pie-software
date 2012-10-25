/*
 * University of California, Berkeley
 * Pioneers in Engineering, Robotics Organizer.
 * PiER Framework v0.41 - 4/5/12
 * 
*/

using System;
using PiEAPI;
using Microsoft.SPOT;
using GHIElectronics.NETMF.System;

namespace StudentPiER
{
    public class StudentCode : RobotCode
    {
        // Variables
        /// <summary>
        /// This is your robot
        /// </summary>
        private Robot robot;

        /// <summary>
        /// These variables are your motor controllers
        /// </summary>
        private PolarBear i2cR;
        private PolarBear i2cL;

        
        /*flag used to determine what kind of drive style
         * 0 => tank drive
         * 1 => arcade style
        */
        private const int driveType = 1;

        /* Flag used to determine which joystick you wish to use to control the robot. This is only applicable to arcade style drive.
         * If you use tank drive style, this flag values does not matter.
         * 0 => left joystick
         * 1 => right joystick
         */
        private const int joystick = 1;

        /// <summary>
        /// Main method which initializes the robot, and starts
        /// it running.
        /// </summary>       
        public static void Main()
        {
            // Initialize robot
            Robot robot = new Robot("1", "COM4");
            robot.Auton(false);
            Debug.Print("Code loaded successfully!");
            Master master = new Master(new StudentCode(robot));
            master.RunCode();
            
        }

        // Constructor
        public StudentCode(Robot robot)
        {
            this.robot = robot;
            i2cR = new PolarBear(robot, 0x0C); //0x0A is the right motor on drivetrain by default
            i2cL = new PolarBear(robot, 0x0E); //0x0B is the left motor on drivetrain by default
        }

        // Gets the robot associated with this StudentCode
        public Robot getRobot()
        {
            return this.robot;
        }

        /// <summary>
        /// The robot will call this method every time it needs to run the user-controlled student code
        /// The StudentCode should basically treat this as a chance to read all the new PiEMOS analog/digital values
        /// and then use them to update the actuator states
        /// </summary>
        public void UserControlledCode()
        {
            // always first set brake to 0 to move the motors
            i2cL.brakeAmount = 0;
            i2cR.brakeAmount = 0;
            // Observe two values from the PiEMOS interface (like left and right joysticks) and map them directly to the speeds of the motors
            // PiEMOS interface values will be between 0 and 255, but I'm centering motor speed = 0 at 128 (halfway between 0 and 255), so that I
            // can get negative and positive speeds.
            // Because I2CMotorController's motorSpeed only accepts between -100 and 100, I have to map the values to that range.
            // Ex. PiEMOS Interface value of 255 --> (255 - 128) * 100 / 128 = 99.23 (basically 100, the highest forward motor speed)
            // EX. PiEMOS Interface value of 0 --> (0 - 128) * 100 / 128 = -100 (the highest backward motor speed)
            // The nice thing is that this will automatically change the motor speed to things like joystick values when the joysticks are moved

            //arcade drive 

            //uncomment for arcade drive
            //  float[] driveVals = new float[2];
          //  driveVals = dr.getMotorVals(robot.UIAnalogVals[0], robot.UIAnalogVals[1], robot.UIAnalogVals[2], robot.UIAnalogVals[3]);
           // i2cL.motorSpeed = driveVals[0];
           // i2cR.motorSpeed = driveVals[1];

            i2cR.velocity = ((int)(robot.UIAnalogVals[1] - 128) * 2);  //comment out for arcade drive
            robot.radio.outData.analog[0] = (byte)(i2cR.velocity/2 + 128);
            i2cL.velocity = ((int)-(robot.UIAnalogVals[3] - 128) * 2); //comment out for arcade drive
            robot.radio.outData.analog[1] = (byte)(i2cL.velocity/2 + 128);

            
            

            /*
            // Observe a certain button being pressed on PiEMOS interface, if true (meaning "if pressed"), then brake
            if (robot.UIDigitalVals[0])
            {
                // treat this as a gradual brake. every time this method is called (very often!), it will slowly add more braking
                // until it reaches the max braking of 10, at which it remains at 10.
                if (i2cL.brakeAmount < 10)
                {
                    i2cL.brakeAmount = i2cL.brakeAmount + (float).1; //let's represent fractions with floats. always cast decimals to float
                    i2cR.brakeAmount = i2cR.brakeAmount + (float).1;
                }
                else
                {
                    i2cL.brakeAmount = 10;
                    i2cR.brakeAmount = 10;
                }
            }
            // at any point, if the button is released, turn braking off
            else
            {
                i2cL.motorBrake = 0;
                i2cR.motorBrake = 0;
            }*/
            
            // This is useful for debugging:
            /*
            Debug.Print("UI: " + robot.UIAnalogVals[0] + " " + robot.UIAnalogVals[1] + " " + robot.UIAnalogVals[2] + " " + robot.UIAnalogVals[3]);
            Debug.Print("canMove: " + robot.canMove);
            Debug.Print("i2cL:" + i2cL.motorSpeed);
            Debug.Print("i2cR:" + i2cR.motorSpeed);*/
            
            
        }

        /// <summary>
        /// The robot will call this method every time it needs to run the autonomous student code
        /// The StudentCode should basically treat this as a chance to change motors and servos based on
        /// non user-controlled input like sensors. But you don't need sensors, as this example demonstrates.
        /// </summary>
        public void AutonomousCode()
        {
            //Debug.Print("Auton");
            /*
            //always set motorBrake to 0 to move motors.
            i2cL.motorBrake = 0;
            i2cR.motorBrake = 0;

            //move one wheel full forward and one wheel full backward. Circling like a crazy person is fun.
            i2cL.motorSpeed = 100;
            i2cR.motorSpeed = -100;

            */

        }
    }
}
