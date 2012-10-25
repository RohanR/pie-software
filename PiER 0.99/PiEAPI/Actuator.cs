using System;
using Microsoft.SPOT;

namespace PiEAPI
{
    interface Actuator
    {

        //IMPORTANT: Classes that implement Actuator should use properties for  setting values.
        //Implementations should also must have a GetValues() method 
        //that will return a collection of the current state of the actuator, 
        //and a SetValues(*args) method that can set the Actuator state at once.

        //This interface is very similar to ActuatorController in PiER versions <=0.41.

        //The software framework will call this method to actually update values 
        //(eg. send the I2C motor values to the motor controller). 
        //This method will hopefully be called on a regular basis.
        void Write();

        //stop all actuation (eg. stop and brake motors)
        void Kill();

        //Undo kill
        void Revive();
    }

}
