using System;
using Microsoft.SPOT;

namespace PiEAPI
{
    /// <summary>
    /// Interface for all sensors plus I2CMotorController and I2CEncoder. Will probably change interface name.
    /// </summary>
    public interface Sensor
    {
        /// <summary>
        /// IMPORTANT: EVERY CLASS INHERITING THIS CLASS MUST CALL THIS METHOD ON THE LAST LINE OF THE CONSTRUCTOR
        /// </summary>
        void addToGlobalList();

        /// <summary>
        /// get the most recently stored values
        /// IMPORTANT: EVERY CLASS INHERITING THIS CLASS MUST STORE THEIR MOST RECENTLY READ DATA IN PRIVATE FIELDS
        /// </summary>
        Array getValues();

        /// <summary>
        /// read the data and set Values to be whatever you read
        /// </summary>
        void Update();
    }
}
