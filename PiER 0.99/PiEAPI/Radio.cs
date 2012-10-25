using System;
using System.IO.Ports;
using Microsoft.SPOT;

namespace PiEAPI
{
    /** This class controls the XBee that communicates with PiEMOS. */
    public class Radio
    {
        /** Incoming packet properties (for receiving data). Must be reflected in PiEMOS configuration. */
        private enum INCOMING_DATA
        {
            IDENT = 0xFE,
            ANALOG_BYTES = 7,
            DIGITAL_BYTES = 1
        }

        /** Outgoing packet properties (for sending data). Must be reflected in PiEMOS configuration. */
        private enum OUTGOING_DATA
        {
            IDENT = 0xFD,
            ANALOG_BYTES = 7,
            DIGITAL_BYTES = 1
        }

        private SerialPort port; /** The serial port to listen and send data on. */ 
        private byte[] buffer; /** The buffer for temporarily storing incoming data. */
        private XBeeInterfaceReceiver receiver; /** Parses the packet received by the XBee and pulls the data out. */
        public IncomingData inData; /** The incoming data parsed by the receiver. */
        public OutgoingData outData; /** The outgoing data to send to PiEMOS. */
        
        public byte fieldTime; /** The field time from the incoming data. */
        public bool canMove; /** Whether or not the robot can move, from the incoming data. */
        public bool isAutonomous; /** Whether or not the robot is autonomous, from the incoming data. */
        public bool isBlue; /** Whether or not the robot is on the Blue Team, from the incoming data. */
        public int[] UIAnalogVals { get; private set; } /** The Analog values from the incoming data. */
        public bool[] UIDigitalVals { get; private set; } /** The Digital values from the incoming data. */
        public long lastUpdate; /** The last time we updated our values with incoming data. */

        public UInt64 interfaceAddress; /** The address of the sender, the PiEMOS instance we are communicating with. */
        public byte[] interfaceAddressBytes; /** The address of the sender split into bytes. */
        private byte frame; /** The frame to send with the outgoing data. */

        public readonly object radioLock = new object(); /** Lock to prevent our data being read while we are updating it. */

        /** Constructor for the Radio class.
         *  @param port The name of the port for the radio to listen and send data on. */
        public Radio(string portName)
        {
            port = new SerialPort(portName, 57600, Parity.None, 8, StopBits.One); // baud rate second argument for SerialPort, try 57600, 19200, 38400, 57600
            port.ReadTimeout = 2; // 2 milliseconds, very short read timeout (default is 500)
            port.Open();

            buffer = new byte[512];
            receiver = new XBeeInterfaceReceiver(this);
            inData = new IncomingData((byte)INCOMING_DATA.IDENT, (int)INCOMING_DATA.ANALOG_BYTES, (int)INCOMING_DATA.DIGITAL_BYTES);
            outData = new OutgoingData((byte)OUTGOING_DATA.IDENT, (int)OUTGOING_DATA.ANALOG_BYTES, (int)OUTGOING_DATA.DIGITAL_BYTES);

            UIAnalogVals = new int[(int)INCOMING_DATA.ANALOG_BYTES];
            UIDigitalVals = new bool[(int)OUTGOING_DATA.DIGITAL_BYTES * 8];
            frame = 0;
        }

        /** This polls the XBee to see if we have a full packet's worth of data, and is meant
         *  to be run periodically by a thread in the Master class.
         *  If we have enough data, we fill a byte array with it. */
        public void PollIncomingData()
        {
            int availableBytes = port.BytesToRead;
            if (availableBytes > 0)
            {
                port.Read(buffer, 0, availableBytes);

                // Only proceed if a full packet was received
                if (receiver.Fill(buffer, availableBytes))
                {
                    // Only proceed if FillData confirms we have the correct ident byte
                    if (inData.FillData(receiver.packet.data))
                    {
                        // Update fields
                        lock (radioLock)
                        {
                            fieldTime = inData.fieldTime;
                            canMove = inData.canMove;
                            isAutonomous = inData.isAutonomous;
                            isBlue = inData.isBlue;
                            for (int i = 0; i < UIAnalogVals.Length; i++)
                            {
                                int ana = (int)inData.analog[i];
                                UIAnalogVals[i] = ana;
                                // Copy incoming data to outgoing data to be echoed back to PIEMOS
                                outData.analog[i] = (byte)ana;
                            }
                            for (int i = 0; i < UIDigitalVals.Length; i++)
                            {
                                bool digi = (bool)inData.digital[i];
                                UIDigitalVals[i] = digi;
                                // Copy incoming data to outgoing data to be echoed back to PIEMOS
                                outData.digital[i] = digi;
                            }
                            // Mark timestamp
                            lastUpdate = DateTime.Now.Ticks;
                        }
                    }
                    receiver.fullPacket = false;
                }
            }
        }

        /** Send the data in our outgoing data array back to PiEMOS. */
        public void SendOutgoingData()
        {
            //Debug.Print("Sending outgoing data");
            byte[] data = outData.GetData();
            byte[] outPacket = OutgoingPacket(interfaceAddressBytes, frame, data);
            frame++;
            if (frame > 126) frame = 0;
            port.Write(outPacket, 0, outPacket.Length);
        }

        /** Make an outgoing packet to send back to PiEMOS.
         *  @param addr The address of the PiEMOS instance we are sending to.
         *  @param frameId The id of the frame to send.
         *  @param data The data to send.
         *  @return The outgoing packet we'll actually send. */
        public byte[] OutgoingPacket(byte[] addr, byte frameId, byte[] data)
        {
            if (addr == null)
            {
                addr = new byte[8];
                //Debug.Print("Missing address bytes");
            }
            int i;
            byte[] packet = new byte[data.Length + 15];
            packet[0] = 0x7E;
            packet[1] = 0x00;
            packet[2] = (byte)(data.Length + 11);
            packet[3] = 0x00;
            packet[4] = frameId;
            for (i = 5; i < 13; i++)
            {
                packet[i] = addr[i - 5];
            }

            packet[13] = 0x00;

            for (i = 14; i < 14 + data.Length; i++)
            {
                packet[i] = data[i - 14];
            }

            byte checksum = 0;
            for (i = 3; i < packet.Length - 1; i++)
            {
                checksum += packet[i];
            }

            checksum = (byte)(0xFF - checksum);
            packet[packet.Length - 1] = checksum;
            return packet;
        }
    }

    /** Class to represent the incoming data we parse out of the XBee packet. */
    public class IncomingData
    {
        private byte ident; /** The identification byte expected at the beginning of the data. */
        public byte[] analog; /** The analog data received. */
        public bool[] digital; /** The digital data received. */
        private ushort digitalByteCount; /** The number of digial bytes received. */
        public byte fieldTime; /** The field time of the current game. */
        public bool canMove; /** Whether or not our robot is allowed to move. */
        public bool isAutonomous; /** Whether or not our robot is in autonomous mode. */
        public bool isBlue; /** Whether or not our robot is on the blue team. */

        /** Constructor for the IncomingData class.
         *  @param identByte The expected identification byte.
         *  @param analogByteCount The expected number of analog bytes to receive.
         *  @param digitalByteCount The expected number of digital bytes to receive. */
        public IncomingData(byte identByte, int analogByteCount, ushort digitalByteCount)
        {
            this.digitalByteCount = digitalByteCount;
            digital = new bool[digitalByteCount * 8];
            analog = new byte[analogByteCount];
            ident = identByte;
        }

        /** Fills our data arrays with incoming data.
         *  @param bytes The incoming data.
         *  @return True if the incoming identification byte matches our expected byte, false otherwise. */
        public bool FillData(byte[] bytes)
        {
            // First byte is identification (needs to match expected ident)
            if (bytes[0] != ident) return false;
            // Second byte is field time
            fieldTime = bytes[1];
            // Third byte contains canMove, isAutonomous, and isBlue booleans
            canMove = (bytes[2] & 0x01) > 0;
            isAutonomous = (bytes[2] & 0x02) > 0;
            isBlue = (bytes[2] & 0x04) > 0;

            // Fill analog bytes, starting with the sixth byte
            int i;
            for (i = 0; i < analog.Length; i++)
            {
                analog[i] = bytes[i + 5];
            }

            // Fill digital bits, converting incoming bytes to bits
            int bit = 0;
            for (; i < analog.Length + digitalByteCount; i++)
            {
                for (int j = 0; j < 8; j++, bit++)
                {
                    digital[bit] = (bytes[i + 5] & (1 << j)) > 0;
                }
            }
            return true;
        }
    }

    /** Class to represent the outgoing data to send back to PiEMOS. */
    public class OutgoingData
    {
        private byte ident; /** The identification byte to send at the beginning of the data. */
        public byte[] analog; /** The analog data to send. */
        public bool[] digital; /** The digital data to send. */
        private ushort digitalByteCount; /** The number of digial bytes to send. */

        /** Constructor for the OutgoingData class.
         *  @param identByte The identification byte to send.
         *  @param analogByteCount The number of analog bytes to send.
         *  @param digitalByteCount The number of digital bytes to send. */
        public OutgoingData(byte identByte, int analogByteCount, ushort digitalByteCount)
        {
            this.digitalByteCount = digitalByteCount;
            digital = new bool[digitalByteCount * 8];
            analog = new byte[analogByteCount];
            ident = identByte;
        }

        /** Puts our data into a single array to send back to PiEMOS.
         *  @return The array of data to send. */
        public byte[] GetData()
        {
            byte[] data = new byte[analog.Length + digital.Length + 1];
            data[0] = ident;
            for (int i = 1; i < analog.Length + 1; i++)
            {
                data[i] = analog[i - 1];
            }

            int idx = analog.Length + 1;
            for (int i = 0; i < digitalByteCount; i++)
            {
                data[idx] = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (digital[(8 * i) + j])
                    {
                        data[idx] += (byte)(1 << j);
                    }
                }
                idx++;
            }
            return data;
        }
    }

    /** Class to parse the incoming XBee packet and put it into a data array we can deal with. */
    public class XBeeInterfaceReceiver
    {
        /** Possible states of the receiver. */
        enum RXState { IDLE, LEN, COMMAND, SRC64, SRC16, RSSI, OPTIONS, DATA, CHECKSUM, UNKNOWN, ERROR }
        /** Possible XBee commands we might receive. */
        enum XBeeCommands { RX64 = 0x80, RX16 = 0x81, TX = 0x89 }
        private RXState state; /** Current state of the receiver. */
        public int bytesRead; /** Number of bytes read. */
        public XBeeRXPacket packet; /** The XBee packet to parse. */
        public Boolean fullPacket; /** Whether or not we received a full packet. */
        private Radio parent; /** The radio instance we're parsing packets for. */

        /** Constructor for the XBeeInterfaceReceiver class.
         *  @param p The parent Radio instance we're parsing packets for. */
        public XBeeInterfaceReceiver(Radio p)
        {
            state = (int)RXState.IDLE;
            packet = new XBeeRXPacket();
            parent = p;
        }

        /** Parse a packet received by the XBee.
         *  @param bytes The bytes of the incoming packet (in a 512 byte buffer array).
         *  @param len The actual number of bytes in the incoming packet.
         *  @return True if we actually received a full packet, False otherwise. */
        public Boolean Fill(byte[] bytes, int len)
        {
            byte b;
            int i;
            for (i = 0; i < len; i++)
            {
                b = bytes[i];
                switch (state)
                {
                    case RXState.IDLE:
                        if (b == 0x7E)
                        {
                            bytesRead = 0;
                            state = RXState.LEN;
                        }
                        break;

                    case RXState.LEN:
                        if (bytesRead == 0)
                        {
                            packet.length = (uint)b << 8;
                        }
                        else if (bytesRead == 1)
                        {
                            packet.length |= b;
                        }
                        bytesRead++;
                        if (bytesRead == 2)
                        {
                            bytesRead = 0;
                            state = RXState.COMMAND;
                        }
                        break;

                    case RXState.COMMAND:
                        if (b == (byte)XBeeCommands.RX64)
                        {
                            packet.command = b;
                            packet.data = new byte[packet.length - 11];
                            packet.src64 = 0;
                            state = RXState.SRC64;
                        }
                        else if (b == (byte)XBeeCommands.TX)
                        {
                            bytesRead = 0;
                            state = RXState.UNKNOWN;
                        }
                        else
                        {
                            bytesRead = 0;
                            Debug.Print("UNKNOWN PACKET!! " + b);
                            state = RXState.UNKNOWN;
                        }
                        break;

                    case RXState.SRC64:
                        //Read 64bit Sender's Address
                        packet.src64Bytes[bytesRead] = b;
                        packet.src64 += (UInt64)b << (8 * bytesRead);

                        bytesRead++;
                        if (bytesRead == 8)
                        {
                            bytesRead = 0;
                            state = RXState.RSSI;

                            //get first interface packet and store 64bit address
                            if (parent.interfaceAddress == 0)
                            {
                                parent.interfaceAddress = packet.src64;
                                parent.interfaceAddressBytes = packet.src64Bytes;
                                //parent.telemetryThread.Start();
                            }
                            else
                            {
                                //ignore interface packets from the wrong address
                                if (parent.interfaceAddress != packet.src64)
                                {
                                    //state = (int)RXState.UNKNOWN;
                                }
                            }

                        }
                        break;

                    case RXState.SRC16:
                        //Read 16bit Sender's Address
                        packet.src16 = (ushort)((packet.src16 << 8) | b);
                        bytesRead++;

                        if (bytesRead == 2)
                        {
                            bytesRead = 0;
                            state = RXState.RSSI;
                        }
                        break;

                    case RXState.RSSI:
                        packet.rssi = b;
                        state = RXState.OPTIONS;
                        break;

                    case RXState.OPTIONS:
                        packet.options = b;
                        state = RXState.DATA;
                        break;

                    case RXState.DATA:
                        //The beef
                        packet.data[bytesRead] = b;
                        bytesRead++;
                        if (bytesRead == packet.data.Length)
                        {
                            state = RXState.CHECKSUM;
                        }
                        break;

                    case RXState.CHECKSUM:
                        fullPacket = true;
                        state = RXState.IDLE;
                        break;

                    case RXState.UNKNOWN:
                        //ignore all bytes in packet before trying to parse again (all XBee API functions generate a confirmation packet sent as a packet)
                        //We dont care about most of them.
                        //We can get information about the current XBee module (like requesting its serial number or coordinator or network status)
                        bytesRead++;
                        if (bytesRead == packet.length)
                        {
                            state = RXState.IDLE;
                            bytesRead = 0;
                        }
                        break;
                }
            }

            if (fullPacket) return true;
            return false;
        }
    }

    /** Class to represent an incoming XBee packet. */
    public class XBeeRXPacket
    {
        public uint length; /* The length of the packet. */
        public byte command; /* The command from the packet. */
        public byte[] src64Bytes; /** The address of the PiEMOS instance that sent the packet. */
        public UInt64 src64; /** The 64-bit sender's address field of the packet. */
        public UInt16 src16; /** The 16-bit sender's address field of the packet. */
        public byte rssi; /** The received single strength indicator field of the packet. */
        public byte options; /** The options field of the packet. */
        public byte[] data; /** The data we actually care about in the packet. */
        public byte checksum; /** The packet's checksum, to ensure our data is not corrupted. */

        /** Default constructor for an XBeeRXPacket. Initializes just the sender's address byte array. */
        public XBeeRXPacket()
        {
            src64Bytes = new byte[8];
        }

        /** Constructor for an XBeeRXPacket that takes all of its fields as arguments.
         *  @param aLength Received length of the packet.
         *  @param aCommand Received command of the packet.
         *  @param aSrc64 Received 64-bit sender's address.
         *  @param aRssi Received rssi field.
         *  @param aOptions Received options.
         *  @param aData Received data that we actually care about.
         *  @param aChecksum Received checksum to check for corruption. */
        public XBeeRXPacket(uint aLength, byte aCommand, UInt64 aSrc64, byte aRssi, byte aOptions, byte[] aData, byte aChecksum)
        {
            length = aLength;
            command = aCommand;
            src64 = aSrc64;
            //src16 = aSrc16;
            rssi = aRssi;
            options = aOptions;
            data = aData;
            checksum = aChecksum;
            src64Bytes = new byte[8];
        }
    }
}
