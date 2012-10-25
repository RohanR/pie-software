
/*
 * University of California, Berkeley
 * Pioneers in Engineering, Robotics Organizer.
 * PiER Framework v0.2b - 04/14/11
 * 
 * Changelog:
 * v0.2b
 *  - Move teleoperated/autonomous checking from StudentCode.cs to robot threads
 *  - Added heartbeat timeout
*/

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;
using System.Collections;
using GHIElectronics.NETMF.FEZ;


namespace PiEAPI
{
    public class Robot
    {
        /// <summary>
        /// List of actuator controllers on this robot
        /// </summary>
        public ArrayList actuators;
        /// <summary>
        // List of sensors on this robot       
        /// </summary>
        public ArrayList sensors;
        /// <summary>
        /// List of open communication ports on this robot
        /// </summary>      
        public ArrayList ports;
        /// <summary>
        /// This robot's radio
        /// </summary>
        public Radio radio;

        /// <summary>
        /// current match time in seconds property with private set and public get.
        /// </summary>
        public int fieldTime { get; private set; }
        /// <summary>
        /// teamID property with private set and public get.
        /// </summary>
        public string teamID { get; private set; }
        /// <summary>
        /// scores property that returns all teams scores.
        /// </summary>
        public int[] scores { get; private set; }
        /// <summary>
        /// winningScore property that returns the score needed to win.
        /// </summary>
        public int winningScore { get; private set; }
        /// <summary>
        /// canMove property that returns true if the robot can move.
        /// </summary>
        public bool canMove { get; private set; }
        /// <summary>
        /// isAutonomous property that returns true if robot is in autonomous mode.
        /// </summary>
        public bool isAutonomous { get; private set; }
        /// <summary>
        /// isAutonomous property that returns true if robot is blue team.
        /// </summary>
        public bool isBlue { get; private set; }
        /// <summary>
        /// UIAnalogVals property that returns the interface's analog values.
        /// </summary> 
        public int[] UIAnalogVals { get; private set; }
        /// <summary>
        /// UIDigitalVals property that returns the interface's digital values.
        /// </summary>
        public bool[] UIDigitalVals { get; private set; }

        // Heartbeat Timer
        private long heartbeatTimer;
        private static long heartbeatPeriod = 2 * 10000000;
        /// <summary>
        /// I2C Device used by all I2C components on robot
        /// </summary>
        public I2CBus i2c;
        /// <summary>
        /// Port representation of the yellow LED pin.
        /// </summary>
        public OutputPort yellowLED;
        /// <summary>
        /// Port representation of the red LED pin.
        /// </summary>
        public OutputPort redLED;

        public OutputPort digSens6;
        public OutputPort digSens7;

        /// <summary>
        /// Robot Constructor: initilizes variables
        /// </summary>
        /// <param name="teamID">Team ID</param>
        /// <param name="radioComPort">Radio Com Port</param>
        public Robot(string teamID, String radioComPort)
        {
            actuators = new ArrayList();
            sensors = new ArrayList();
            ports = new ArrayList();
            canMove = true;
            isAutonomous = false;
            this.teamID = teamID;
            radio = new Radio(radioComPort);

            i2c = new I2CBus();

            heartbeatTimer = DateTime.Now.Ticks;

            // Set the team color for the shiftBrite
            // Still to be implemented in 0.1c


            // Make a deep copy of the UI values so they don't change mid-update
            UIAnalogVals = (int[])radio.UIAnalogVals.Clone();
            UIDigitalVals = (bool[])radio.UIDigitalVals.Clone();
        }

        // Set isAutonomous
        public void Auton(bool b)
        {
            isAutonomous = b;
        }

        /// <summary>
        /// Robot Thread constantly calls this to update the actuator
        /// values or kill them if necessary.
        /// </summary>        
        public void Run()
        {
            bool prevCanMove = canMove;

            lock (radio.radioLock)
            {
                // Make a deep copy of the radio values so they don't change mid-update
                UIAnalogVals = (int[])radio.UIAnalogVals.Clone();
                UIDigitalVals = (bool[])radio.UIDigitalVals.Clone();
                fieldTime = radio.fieldTime;
                canMove = radio.canMove;
                isAutonomous = radio.isAutonomous;
                isBlue = radio.isBlue;
                heartbeatTimer = radio.lastUpdate;
            }

            // Check heartbeat time
            if (DateTime.Now.Ticks - heartbeatTimer > heartbeatPeriod)
            {
                canMove = false;
            }

            //Update Team Color
            if (isBlue)
            {
                digSens6.Write(true);
                digSens7.Write(false);
            }
            else
            {
                digSens6.Write(false);
                digSens7.Write(true);
            }

            //specifically revive actuators if field wanted to revive actuators.
            if (!prevCanMove && canMove)
            {
                foreach (Actuator act in actuators)
                {
                    act.Revive(); // update each actuator's state using its actuator controller's "update actuators" method.
                }
            }


            // kill or revive actuators based on the canMove bool
            if (canMove == true)
            {
                foreach (Actuator act in actuators)
                {
                    act.Write(); // update each actuator's state using its actuator controller's "update actuators" method.
                }
            }
            else
            {
                foreach (Actuator act in actuators)
                {
                    act.Kill(); // if actuator is already killed, this will not do anything
                }
            }

        }

        /// <summary>
        /// Kills actuators; can be called by StudentCode or Robot thread
        /// </summary>
        public void KillActuators()
        {
            foreach (Actuator act in actuators)
            {
                act.Kill();
            }
        }

        /// <summary>
        /// Revives actuators; can be called by StudentCode or Robot thread
        /// </summary>
        public void ReviveActuators()
        {
            foreach (Actuator act in actuators)
            {
                act.Revive();
            }
        }

    }
}