
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using System.Runtime.CompilerServices;

/// <summary>
/// This is the main class of the robot. It starts program threads and manages device communication.
/// When in doubt, do not change this code!
/// </summary>
namespace PiEAPI
{
    public class Master
    {
        #region Private Variables
        /// <summary>
        /// The student code to run.  StudentCode implements RobotCode, which is basically
        /// a wrapper class that allows us to do threading in Master.cs while keeping the Main
        /// method in StudentCode.cs.
        /// </summary>
        private RobotCode student;

        /// <summary>
        /// The robot on which to run the code.
        /// </summary>
        private Robot robot;
        #endregion

        #region Timers
        /// <summary>
        /// Timer that controls userControlled code.
        /// </summary>
        private static Timer userControlledTimer;
        /// <summary>
        /// Timer that controls Radio telemetry code
        /// (i.e. sending data to the radio.)
        /// </summary>
        private static Timer rfTeleTimer;
        #endregion

        #region Threads
        /// <summary>
        /// Thread that controls autonomous code.
        /// </summary>
        private Thread autonomousThread;
        /// <summary>
        /// Thread that controls Radio polling code 
        /// (i.e. getting data from the radio.)
        /// </summary>
        private Thread rfPollThread;
        #endregion

        #region Constants
        /// <summary>
        /// The wait time (period) value for the robot supervisor thread, in milliseconds.
        /// Specify zero (0) to indicate that this thread should be suspended to allow other waiting threads to execute.
        /// </summary>
        private const int ROBOT_WAIT_TIME = 0;
        /// <summary>
        /// The wait time (period) value for the userControlled thread, in milliseconds.
        /// Specify zero (0) to indicate that this thread should be suspended to allow other waiting threads to execute,
        /// which in turn means that a normal Thread should be created instead of a Timer.
        /// </summary>
        private const int USER_CONTROLLED_WAIT_TIME = 75;
        /// <summary>
        /// The wait time (period) value for the autonomous thread, in milliseconds.
        /// Specify zero (0) to indicate that this thread should be suspended to allow other waiting threads to execute,
        /// which in turn means that a normal Thread should be created instead of a Timer.
        /// </summary>
        private const int AUTONOMOUS_WAIT_TIME = 0;
        /// <summary>
        /// The wait time (period) value for the rfPoll thread, in milliseconds.
        /// Specify zero (0) to indicate that this thread should be suspended to allow other waiting threads to execute,
        /// which in turn means that a normal Thread should be created instead of a Timer.
        /// </summary>
        private const int RFPOLL_WAIT_TIME = 0;
        /// <summary>
        /// The wait time (period) value for the rfTele thread, in milliseconds.
        /// Specify zero (0) to indicate that this thread should be suspended to allow other waiting threads to execute,
        /// which in turn means that a normal Thread should be created instead of a Timer.
        /// </summary>
        private const int RFTELE_WAIT_TIME = 100;
        #endregion

        #region Accessors
        /// <summary>
        /// Returns the robot controlled by this student code.
        /// </summary>
        public Robot Robot { get { return robot; } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructor to be called by Student Code.
        /// Creates the radio's and student code's threads.
        /// </summary>
        public Master(RobotCode s)
        {
            /// <summary>
            /// Makes this robot run the student's code.
            /// </summary>
            student = s;
            robot = student.getRobot();
            
            // Create and set radio thread for polling.
            rfPollThread = new Thread(RunRFPoll);
            rfPollThread.Priority = ThreadPriority.AboveNormal;

            // Create and set student code thread for autonomous
            autonomousThread = new Thread(RunAutonomous);
            autonomousThread.Priority = ThreadPriority.BelowNormal;

            // Initialize CPU Pin interface
            PandaTwoCPUPins PandaTwoCPUPin = new PandaTwoCPUPins();
            
            // Initialize LEDs.
            robot.yellowLED = new OutputPort(PandaTwoCPUPin.yellowLEDPin, false);
            robot.redLED = new OutputPort(PandaTwoCPUPin.redLEDPin, false);

            //Initialize blue and yellow team lights.  digSens6 and digSens7
            //correspond to the actual pin numbers.
            robot.digSens6 = new OutputPort(PandaTwoCPUPin.digSens6Pin, false);
            robot.digSens7 = new OutputPort(PandaTwoCPUPin.digSens7Pin, false);

            // Disable garbage collection messages.
            Debug.EnableGCMessages(false);

            // Run the robot once to initialize isAutonomous and isEnabled values
            // along with all other information received by polling the radio once.
            robot.Run();
        }

        /// <summary>
        /// Begins running the threads, and then runs the robot thread indefinitely.
        /// </summary>
        public void RunCode()
        {
            // Set robot thread priority to AboveNormal.
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

            // Create an AutoResetEvent.  We will pass this to the Timer constructors
            // below, and we can use it to change the start time/period of the Timers
            // if needed (e.g. if an event fires, change period to 100 ms.)
            AutoResetEvent autoEvent = new AutoResetEvent(false);

            // Start the threads and timers as follows:
            // If the wait time of a thread is 0, make a regular thread that implements the
            // appropriate RunX method, because it doesn't make sense to have a timer with period 0.
            // Else, make a timer with a TimerCallback corresponding to the 
            // appropriate RunXTimer method, AutoEvent as the trigger event,
            // 0 as the start time (i.e. Timer starts immediately), and wait time
            // as the period (how often the Timer executes.)

            // Start userControlled timer.
            TimerCallback userControlledCallback = new TimerCallback(RunUserControlledTimer);
            userControlledTimer = new Timer(userControlledCallback, autoEvent, 0, USER_CONTROLLED_WAIT_TIME);

            // Start autonomous thread.
            autonomousThread.Start();

            // Start rfPoll thread.
            rfPollThread.Start();
        
            // Start rfTele timer.
            TimerCallback rfTeleCallback = new TimerCallback(RunRFTeleTimer);
            rfTeleTimer = new Timer(rfTeleCallback, autoEvent, 0, RFTELE_WAIT_TIME);

            // Run robot supervisor code.
            while (true)
            {
                robot.Run();
                Thread.Sleep(ROBOT_WAIT_TIME);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Continually run student teleoperated code with yielding.
        /// This is the method for the timer.  The Synchronized option
        /// means that only one thread can use this method at a time.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void RunUserControlledTimer(Object stateInfo)
        {
            if (!robot.isAutonomous)
            {
                student.UserControlledCode();
            }
        }

        /// <summary>
        /// Continually run student autonomous code with yielding.
        /// This is the method for the thread.
        /// </summary>
        private void RunAutonomous()
        {
            while (true)
            {
                if (robot.isAutonomous)
                {
                    student.AutonomousCode();
                    Thread.Sleep(0);
                }
            }
        }

        /// <summary>
        /// Continually run poll code in the radio class with yielding.
        /// This is the method for the thread.
        /// </summary>
        private void RunRFPoll()
        {
            while (true)
            {
                robot.radio.PollIncomingData();
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Continually run telemetry code in the radio class with yielding.
        /// This is the method for the timer.  The Synchronized option
        /// means that only one thread can use this method at a time.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void RunRFTeleTimer(Object stateInfo)
        {
            robot.radio.SendOutgoingData();
        }
        #endregion
    }
}
