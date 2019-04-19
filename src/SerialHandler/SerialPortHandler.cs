using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SerialHandler
{
    /// <summary>
    /// Serial handler for local serial port.
    /// </summary>
    /// <remarks>
    /// <para></para>
    /// <para>Simple Usage:</para>
    /// <code>
    /// // create serial handler
    /// SerialHandler spHandler = new SerialHandler("COM1", 9600, IsReport, DequeueResponse);
    /// spHandler.OnReport += SpHandler_OnReport;
    /// 
    /// //Start serial handler
    /// spHandler.Start();
    /// //...
    /// //Stop serial handler
    /// spHandler.Stop();
    /// //...
    /// //do something 
    /// byte[] Cmd_Req = null;
    /// byte[] Cmd_Res = new byte[CMD_RES.Length];
    /// int ret = spHandler.SendCommand(Cmd_Req, Cmd_Req, 1000);
    /// 
    /// if (ret > 0 && DataEqual(Cmd_Res, CMD_RES))
    /// {
    ///     //create data, do something A about data
    /// }
    /// else
    /// {
    ///     //do something B
    /// }
    /// //...
    /// 
    /// bool IsReport(byte[] data)
    /// {
    ///     //Determines whether the command is Cmd_Reprot
    ///     return true;
    /// }
    ///     
    /// byte[] DequeueResponse(Queue<byte> quReceive)
    /// {
    ///     byte[] response = null;
    ///     //Dequeuing a valid response based protocol rules
    ///     return response;
    /// }
    /// private void SpHandler_OnReport(byte[] data)
    /// {
    ///     if (DataEqual(Cmd_Report, CMD_REPORT))
    ///     {
    ///         //do something X about data then save data
    ///     }
    /// }
    /// 
    /// </code>
    /// </remarks>
    public class SerialPortHandler
    {
        /// <summary>
        /// The instance of <see cref="SerialPort"/>.
        /// </summary>
        SerialPort _serialPort;
        /// <summary>
        /// The thread of read data from serial prot.
        /// </summary>
        Thread _thrdRead;
        /// <summary>
        /// The thread of data process.
        /// </summary>
        Thread _thrdHandle;
        /// <summary>
        /// The signaled of runing.
        /// </summary>
        ManualResetEvent _runingEvent;
        /// <summary>
        /// Save all data read from the serial port
        /// </summary>
        Queue<byte> _quReceiveBuff;
        /// <summary>
        /// Save the response of the last command
        /// </summary>
        Queue<byte[]> _quCmdRespone;
        /// <summary>
        /// The singaled of command response.
        /// </summary>
        AutoResetEvent _cmdResponseReset;
        /// <summary>
        /// The state sending command.
        /// </summary>
        bool _isSending;
        /// <summary>
        /// A method to determine whether a byte[] is a spontaneous report of a serial port.
        /// </summary>
        Predicate<byte[]> _reportPredicate;
        /// <summary>
        /// Dequeuing a command from the received data queue method.
        /// </summary>
        Func<Queue<byte>, byte[]> _dequeueFunc;

        /// <summary>
        ///  Initializes a new instance of the <see cref="SerialPortHandler"/> class.
        /// </summary>
        /// <param name="portName">The port to use (for example, COM1).<see cref="SerialPort"/></param>
        /// <param name="baudRate">The baud rate.<see cref="SerialPort"/></param>
        /// <param name="reportPredicate">A method to determine whether a byte[] is a spontaneous report of a serial port.Dequeuing a command from the received data queue method</param>
        /// <param name="dequeueFunc">Dequeuing a command from the received data queue method.</param>
        public SerialPortHandler(string portName, int baudRate, Predicate<byte[]> reportPredicate, Func<Queue<byte>, byte[]> dequeueFunc)
           : this(reportPredicate, dequeueFunc)
        {
            this._serialPort = new SerialPort(portName, baudRate);
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="SerialPortHandler"/> class.
        /// </summary>
        /// <param name="portName">The port to use (for example, COM1).<see cref="SerialPort"/></param>
        /// <param name="baudRate">The baud rate.<see cref="SerialPort"/></param>
        /// <param name="parity">One of the <see cref="SerialPort.Parity"/> values.<seealso cref="SerialPort"/></param>
        /// <param name="dataBits">The data bits value.<see cref="SerialPort"/></param>
        /// <param name="stopBits">One of the <see cref="SerialPort.StopBits"/> values.<seealso cref="SerialPort"/></param>
        /// <param name="reportPredicate">A method to determine whether a byte[] is a spontaneous report of a serial port.Dequeuing a command from the received data queue method</param>
        /// <param name="dequeueFunc">Dequeuing a command from the received data queue method.</param>
        public SerialPortHandler(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Predicate<byte[]> reportPredicate, Func<Queue<byte>, byte[]> dequeueFunc)
          : this(reportPredicate, dequeueFunc)
        {
            this._serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        }

        private SerialPortHandler(Predicate<byte[]> reportPredicate, Func<Queue<byte>, byte[]> dequeueFunc)
        {
            _quCmdRespone = new Queue<byte[]>();
            _quReceiveBuff = new Queue<byte>();
            _reportPredicate = reportPredicate;
            _dequeueFunc = dequeueFunc;
        }

        /// <summary>
        /// Data reprot event.
        /// </summary>
        /// 
        /// <remarks>Called when the serial interface is actively reporting data.</remarks>
        /// 
        /// <para><note>Used <see cref="DataReport.BeginInvoke(object, EventArgs, AsyncCallback, object)"/></note></para>
        public event DataReportEventHandler DataReport;

        /// <summary>
        /// Read error event.
        /// </summary>
        /// 
        /// <remarks>Called when the serial interface is actively reporting data.</remarks>
        /// 
        /// <para><note>Used <see cref="ReadError.BeginInvoke(object, EventArgs, AsyncCallback, object)"/></note></para>
        public event ReadErrorEventHandler ReadError;


        /// <summary>
        /// Data handle error event.
        /// </summary>
        /// 
        /// <remarks>Called when the serial interface is actively reporting data.</remarks>
        /// 
        /// <para><note>Used <see cref="DataHandleError.BeginInvoke(object, EventArgs, AsyncCallback, object)"/></note></para>
        public event DataHandleErrorEventHandler DataHandleError;

        /// <summary>
        /// State of the serial port.
        /// </summary>
        public bool IsOpen
        {
            get { return this._serialPort == null ? false : this._serialPort.IsOpen; }
        }

        /// <summary>
        /// State of the serial handler.
        /// </summary>
        /// 
        /// <remarks>Current state of video source object - running or not.</remarks>
        /// 
        public bool IsRunning
        {
            get { return _runingEvent != null && !_runingEvent.WaitOne(1); }
        }

        /// <summary>
        /// Send a <see cref="byte[]"/> command
        /// </summary>
        /// <param name="sendData">command buff</param>
        /// <param name="receivedData">REF: response of command</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait serial port response.</param>
        /// <returns>count of response, -1: failure, -2: port is busy</returns>
        public SendResult SendCommand(byte[] sendData, ref byte[] receivedData, int millisecondsTimeout)
        {
            if (_isSending)
                return SendResult.Busing;
            try
            {
                _isSending = true;
                _cmdResponseReset.Reset();
                this._serialPort.Write(sendData, 0, sendData.Length);
                int ret = 0;
                receivedData = ReadCommandResponse(millisecondsTimeout);
                ret = receivedData == null ? -1 : receivedData.Length;
                if (receivedData != null)
                    return SendResult.Success;
                else
                    return SendResult.NoResponse;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Send command is failure：{0}", ex.Message), ex);
            }
            finally
            {
                _isSending = false;
            }
        }

        /// <summary>
        /// Start serial handler.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            if (this._serialPort == null || this._serialPort.IsOpen)
                return false;
            this._serialPort.Open();
            _runingEvent = new ManualResetEvent(true);
            _cmdResponseReset = new AutoResetEvent(false);
            _thrdRead = new Thread(new ThreadStart(Read));
            _thrdHandle = new Thread(new ThreadStart(DataHandling));
            _thrdRead.Start();
            _thrdHandle.Start();
            return true;
        }

        /// <summary>
        /// Stop serial handler
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            _runingEvent?.Reset();
            if (_thrdRead != null && _thrdRead.IsAlive)
            {
                _thrdRead.Join();
                _thrdRead = null;
            }
            if (_thrdHandle != null && _thrdRead.IsAlive)
            {
                _thrdHandle.Join();
                _thrdHandle = null;
            }
            if (this._serialPort != null && this._serialPort.IsOpen)
            {
                this._serialPort.Close();
            }
            _cmdResponseReset?.Close();
            _cmdResponseReset = null;
            _runingEvent?.Close();
            _runingEvent = null;
            _quCmdRespone?.Clear();
            _quCmdRespone = null;
            _quReceiveBuff?.Clear();
            _quReceiveBuff = null;
            return true;
        }

        /// <summary>
        /// Read the response of the last command.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        private byte[] ReadCommandResponse(int millisecondsTimeout)
        {
            byte[] buffer = null;
            if (_cmdResponseReset.WaitOne(millisecondsTimeout, false))
                buffer = _quCmdRespone.Dequeue();
            return buffer;
        }

        /// <summary>
        /// Read data from serial port.
        /// </summary>
        private void Read()
        {
            while (_runingEvent.WaitOne(1))
            {
                try
                {

                    if (this._serialPort == null || !this._serialPort.IsOpen || this._serialPort.BytesToRead == 0)
                    {
                        SpinWait.SpinUntil(() => this._serialPort != null && this._serialPort.IsOpen && this._serialPort.BytesToRead > 0, 10);
                        continue;
                    }
                    byte[] data = new byte[this._serialPort.BytesToRead];
                    this._serialPort.Read(data, 0, data.Length);
                    Array.ForEach(data, b =>
                    {
                        if (_quReceiveBuff != null)
                            _quReceiveBuff.Enqueue(b);
                    });
                }
                catch (InvalidOperationException)
                {
                    if (!_runingEvent.WaitOne(10) || this._serialPort == null)
                        return;
                    else
                        this._serialPort.Open();
                }
                catch (Exception ex)
                {
                    var errorDescription = $"An error occurred in the reading processing: {ex.Message}";
                    this.ReadError?.BeginInvoke(this, new ReadErrorEventArgs(errorDescription, new Exception(errorDescription, ex)), null, null);
                }
            }
        }

        /// <summary>
        /// Data processing
        /// </summary>
        private void DataHandling()
        {
            while (_runingEvent.WaitOne(1))
            {
                try
                {
                    if (_quReceiveBuff.Count == 0)
                    {
                        SpinWait.SpinUntil(() => _quReceiveBuff.Count > 0, 10);
                        continue;
                    }
                    byte[] data = _dequeueFunc(_quReceiveBuff);
                    if (data == null || data.Length == 0)
                    {
                        SpinWait.SpinUntil(() => false, 10);
                        continue;
                    }

                    if (_reportPredicate(data))
                        DataReport?.BeginInvoke(this, new DataReportEventArgs(data), null, null);   //If the data is spontaneously reported by the serial port, the DataReport event is called
                    else
                    {                                                                               //If the command response returned by the serial port, join the command response queue
                        if (_quCmdRespone.Count > 0)
                            _quCmdRespone.Clear();                                                  //The queue is cleared to ensure that if a command timed out does not affect subsequent command results

                        _quCmdRespone.Enqueue(data);
                        _cmdResponseReset.Set();
                    }
                }
                catch (Exception ex)
                {
                    var errorDescription = $"An error occurred in the data processing: {ex.Message}";
                    this.DataHandleError?.BeginInvoke(this, new DataHandleErrorEventArgs(errorDescription, new Exception(errorDescription, ex)), null, null);
                }
            }
        }
    }
}
