using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;
using System.Collections;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace PiEAPI
{
    public class I2CBus
    {
        private I2CDevice.Configuration oConfig;
        private I2CDevice i2cd;
        private I2CDevice.I2CTransaction[] xActions1 = new I2CDevice.I2CTransaction[1];
        public I2CBus()
        {
            oConfig = new I2CDevice.Configuration(0x0B, 100);
            i2cd=new I2CDevice(oConfig);
        }
        public void Execute(I2CDevice.I2CTransaction[] xActions, int timeout, I2CDevice.Configuration config)
        {
            lock (this)
            {
                //Debug.Print("Locked Execute");
                i2cd.Config = config;
                for (int d = 0; d < xActions.Length; d++)
                {
                    xActions1[0] = xActions[d];
                    i2cd.Execute(xActions1, timeout);
                    //Thread.Sleep(10);

                    /*if (i2cd.Execute(xActions1, timeout) == 0)
                    {
                        Debug.Print("I2C Transaction Failed");
                    }
                    else
                    {
                        Debug.Print("I2C success "+xActions.Length.ToString());
                    }*/
                }
            }
        }
    }
}
