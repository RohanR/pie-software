/*
 * University of California, Berkeley
 * Pioneers in Engineering, Robotics Organizer.
 * PiER Framework v2.a - 04/08/11
*/

namespace PiEAPI
{
    /// <summary>
    /// A base class for all hardware elements e.g. sensors, actuators...
    /// Stores the robot that all hardware elements are attached to
    /// </summary>
    public class Hardware
    {
        /// <summary>
        /// The field that stores the robot corresponding to a particular hardware element
        /// </summary>
        public Robot robot;

        /// <summary>
        /// Instantiates an object of class Hardware. This is intended to be called from subclasses during instantiation
        /// so that each hardware element knows which robot it is attached to
        /// </summary>
        /// <param name="robot">The robot that is creating its sensor/actuator object</param>
        public Hardware(Robot robot)
        {
            this.robot = robot;
        }
    }
}