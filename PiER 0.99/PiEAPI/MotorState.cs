using System;
using Microsoft.SPOT;

namespace PiEAPI
{
    public class MotorState
    {
        public int brakeAmount;

        public int velocity;
        public bool reverseVelocity;

        public int maxVelocity;
        public int minVelocity;

        public int upperStopZone;
        public int lowerStopZone;

        public MotorState(int brakeAmountVal, int velocityVal, bool reverseVelocityVal, int maxVelocity, int minVelocity, int upperStopZone, int lowerStopZone)
        {
            this.brakeAmount = brakeAmountVal;

            this.velocity = velocityVal;
            this.reverseVelocity = reverseVelocityVal;

            this.maxVelocity = maxVelocity;
            this.minVelocity = minVelocity;

            this.upperStopZone = upperStopZone;
            this.lowerStopZone = lowerStopZone;
        }

        public MotorState()
        {
            // TODO: Complete member initialization
        }
    }
}
