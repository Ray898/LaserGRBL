﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaserGRBL.ComWrapper
{
	class UsbSerial : IComWrapper
	{
		private System.IO.Ports.SerialPort com = new System.IO.Ports.SerialPort();
		private string mPortName;
		private int mBaudRate;

		public void Configure(params object[] param)
		{
			mPortName = (string)param[0];
			mBaudRate = (int)param[1];
		}

		public void Open()
		{
			if (!com.IsOpen)
			{
				try
				{
					com.DataBits = 8;
					com.Parity = System.IO.Ports.Parity.None;
					com.StopBits = System.IO.Ports.StopBits.One;
					com.Handshake = System.IO.Ports.Handshake.None;
					com.PortName = mPortName;
					com.BaudRate = mBaudRate;
					com.NewLine = "\n";
					com.WriteTimeout = 1000; //se si blocca in write

					//log("com", string.Format("Open {0} @ {1} baud", com.PortName.ToUpper(), com.BaudRate));
					Logger.LogMessage("OpenCom", "Open {0} @ {1} baud", com.PortName.ToUpper(), com.BaudRate);

					com.Open();
					com.DiscardOutBuffer();
					com.DiscardInBuffer();
				}
				catch (System.IO.IOException ioex)
				{
					if (char.IsDigit(mPortName[mPortName.Length - 1]) && char.IsDigit(mPortName[mPortName.Length - 2])) //two digits port like COM23
					{
						//FIX https://github.com/arkypita/LaserGRBL/issues/31
						com.PortName = mPortName.Substring(0, mPortName.Length - 1); //remove last digit and try again
						Logger.LogMessage("OpenCom", "Retry open {0} @ {1} baud", com.PortName.ToUpper(), com.BaudRate);

						com.Open();
						com.DiscardOutBuffer();
						com.DiscardInBuffer();
					}
					else
					{
						throw ioex;
					}
				}
			}
		}

		public void Close(bool auto)
		{
			if (com.IsOpen)
			{

				//log("com", string.Format("Close {0} [{1}]", com.PortName.ToUpper(), auto ? "CORE" : "USER"));
				Logger.LogMessage("CloseCom", "Close {0} [{1}]", com.PortName.ToUpper(), auto ? "CORE" : "USER");
				com.DiscardOutBuffer();
				com.DiscardInBuffer();
				com.Close();
			}
		}

		public bool IsOpen
		{get { return com.IsOpen; }}

		public void Write(byte b)
		{
			//log("tx", string.Format("[0x{0:X}]", b));
			com.Write(new byte[]{ b } , 0, 1);
		}

		public void Write(string text)
		{
			//log("tx", text);
			com.Write(text);
		}

		int logcnt = 0;
		
		public string ReadLineBlocking()
		{
			return com.ReadLine();
			//string rv = com.ReadLine();
			//log("rx", rv);
			//return rv;

		} //la lettura della com è bloccante per natura

		public bool HasData()
		{ return com.BytesToRead > 0; }

		private void log(string operation, string line)
		{
			line = line.Replace("\r", "\\r");
			line = line.Replace("\n", "\\n");
			try { System.IO.File.AppendAllText(System.IO.Path.Combine(GrblCore.DataPath, string.Format("comlog.txt", operation)), string.Format("{0:00000000}\t{1:00000}\t{2}\t{3}\r\n", Tools.TimingBase.TimeFromApplicationStartup().TotalMilliseconds, logcnt++, operation, line)); }
			catch { }
		}
	}
}
