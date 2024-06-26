//Style Guide: https://google.github.io/styleguide/csharp-style.html

using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace STM32SerialCommunication
{
  class Program
  {
    static SerialPort? _serialPort;
    static string _buffer = string.Empty;

    static void Main(string[] args)
    {
      _serialPort = new SerialPort
      {
        PortName = "/dev/tty.usbserial-AU06JCS7",
        BaudRate = 115200,
        Parity = Parity.None,
        DataBits = 8,
        StopBits = StopBits.One,
        Handshake = Handshake.None,
        ReadTimeout = 500,
        WriteTimeout = 500
      };

      _serialPort.DataReceived += SerialPortDataReceived;

      try
      {
        _serialPort.Open();
        Console.WriteLine("Port opened successfully!");

        while (true)
        {
          Console.Write("Enter a message to send (type 'ESC' to send ESC key): ");
          string? input = Console.ReadLine();

          if (input.Equals("ESC", StringComparison.OrdinalIgnoreCase))
          {
            _serialPort.Write(new byte[] { 27 }, 0, 1);
            Console.WriteLine("ESC key sent successfully!");
          }
          else
          {
            if (input == "end") break;
            _serialPort.Write(input + "\r");
            Console.WriteLine("Data sent successfully!");
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }
      finally
      {
        if (_serialPort.IsOpen)
        {
          _serialPort.Close();
          Console.WriteLine("Port closed successfully!");
        }
      }
    }

    private static async void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      try
      {
        string receivedData = await Task.Run(() =>
        {
          try
          {
            return _serialPort.ReadExisting();
          }
          catch (TimeoutException tex)
          {
            Console.WriteLine($"Timeout Error: {tex.Message}");
            return string.Empty;
          }
        });

        if (!string.IsNullOrEmpty(receivedData))
        {
          _buffer += receivedData;
          ProcessReceivedData();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Receive Error: {ex.Message}");
      }
    }

    private static void ProcessReceivedData()
    {
      while (_buffer.Contains("\r") | _buffer.Contains(">"))
      {
        int index;
        if (!_buffer.Contains("\r"))
        {
          index = _buffer.IndexOf(">");
        }
        else
        {
          index = _buffer.IndexOf("\r");
        }

        string completeMessage = _buffer.Substring(0, index + 1);
        _buffer = _buffer.Substring(index + 1);

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        // completeMessage = completeMessage.Replace('?', '@');
        Console.Write($"{completeMessage}");
      }
    }
  }
}
