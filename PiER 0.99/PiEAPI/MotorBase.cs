using System;
using Microsoft.SPOT;

namespace PiEAPI
{
    public class MotorBase : Hardware, Motor
    {
        private MotorState state = new MotorState();

        public MotorBase() : base(null)
        { 
        }

        public MotorBase(Robot robot) : base(robot)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public int velocity
        {
            get
            {
                return state.velocity;
            }
            set
            {
                state.velocity = limitValue(value, -255, 255);
            }
        }

        public bool reverseVelocity
        {
            get
            {
                return state.reverseVelocity;
            }
            set
            {
                state.reverseVelocity = value;
            }
        }

        /// <summary>
        /// Computes what the value of velocity should be based on the parameters given.
        /// If reverseVelocityVal is true, the actual veloctity should be negative of the velocity argument
        /// </summary>
        /// <returns> Returns a value to be used as actual velocity. Defines the characteristic input/output relationship. </returns>
        private int generateRawVelocity()
        {
            // TODO: Add greater sensitivity control at low speeds
            if (state.reverseVelocity)
            {
                return -1 * state.velocity;
            }
            else
            {
                return state.velocity;
            }
        }

        public int actualVelocity
        {
            get
            {
                return generateRawVelocity();
            }
        }

        public int brakeAmount
        {
            get
            {
                return state.brakeAmount;
            }
            set
            {
                state.brakeAmount = limitValue(value, 0, 255);
            }
        }

        public int maxVelocity
        {
            get
            {
                return state.maxVelocity;
            }
            set
            {
                state.maxVelocity = limitValue(value, -255, 255);
            }
        }

        public int minVelocity
        {
            get
            {
                return state.minVelocity;
            }
            set
            {
                state.minVelocity = limitValue(value, -255, 255);
            }
        }

        public int upperStopZone
        {
            get
            {
                return state.upperStopZone;
            }
            set
            {
                state.upperStopZone = limitValue(value, -255, 255);
            }
        }

        public int lowerStopZone
        {
            get
            {
                return state.lowerStopZone;
            }
            set
            {
                state.lowerStopZone = limitValue(value, -255, 255);
            }
        }

        public MotorState setValues(MotorState state)
        {
            this.velocity = state.velocity;
            this.reverseVelocity = state.reverseVelocity;
            this.maxVelocity = state.maxVelocity;
            this.minVelocity = state.minVelocity;
            this.upperStopZone = state.upperStopZone;
            this.lowerStopZone = state.lowerStopZone;
            this.brakeAmount = state.brakeAmount;
            return getValues();
        }

        public MotorState getValues()
        {
            return new MotorState(brakeAmount, velocity, reverseVelocity, maxVelocity, minVelocity, upperStopZone, lowerStopZone);
        }

        private int limitValue(int input, int low, int high)
        {
            if (input > high)
            {
                Debug.Print("Warning: You are setting a Motor property to a value that is too high. You are trying to set it to " + input + " but the maximum value is " + high + " this property will be set to " + high);
                return high;
            }
            else if (input < low)
            {
                Debug.Print("Warning: You are setting a Motor property to a value that is too low. You are trying to set it to " + input + " but the minimum value is " + low + " this property will be set to " + low);
                return low;
            }
            else
            {
                return input;
            }
        }
    }
}
