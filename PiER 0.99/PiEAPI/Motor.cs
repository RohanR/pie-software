using System;
using Microsoft.SPOT;

namespace PiEAPI
{
    public interface Motor
    {
        //speed sets the PWM value for the motor.
        //speed should be in the range [-255, 255] (this matches the Polar Bear Motor Controller).
        // By convention, a positive velocity is a counterclockwise rotation of the motor axle.
        //This convection is necessary for the Driver class
        // If a positive value results in a clockwise motion, then negate the reverseVelocity property
        int velocity
        {
            get;
            set;
        }

        //If true this will negate the actual velocity (clockwise becomes counter clockwise)
        bool reverseVelocity
        {
            get;
            set;
        }

        //The actual value sent to the motor controller after scaling and stopZone adjustments
        int actualVelocity
        {
            get;
        }

        // brakeAmount is the amount of braking applied when velocity==0
        // brakeAmount is in the range [0, 255] with 0 being no braking and 255  
        //being full braking.
        int brakeAmount
        {
            get;
            set;
        }

        //Limits the maximum forward speed of the motor. Value of actualSpeed is linearly scaled.
        // 255 = no effect
        // maxVelocity > minVelocity
        int maxVelocity
        {
            get;
            set;
        }

        //Limits the maximum backwards speed of the motor. Value of actualSpeed is linearly scaled.
        // -255 = no effect
        // maxVelocity > minVelocity
         int minVelocity
        {
            get;
            set;
        }

        //Minimum forward seed at which the motor starts moving
        //actualSpeed is scaled linearly to avoid the upperStopZone (except for 0)
        // Should be [0, 255]
         int upperStopZone
        {
            get;
            set;
        }

        //Minimum backwards speed at which the motor starts moving
        //actualSpeed is scaled linearly to avoid the upperStopZone (except for 0)
        //should be [-255,0]
         int lowerStopZone
        {
            get;
            set;
        }

        //Motor state is a class that has fields that span the state of Motor
        //setValues is used to set all properties of Motor at once and returns the actual state
         MotorState setValues(MotorState state);

        //returns the current state of Motor
         MotorState getValues();
    }
}
