using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace STM32SerialCommunication
{
  class Program
  {
    static SerialPort? serialPort;
    static string buffer = string.Empty;

    static void Main(string[] args)
    {
      serialPort = new SerialPort
      {
        PortName = "/dev/tty.usbserial-AU06JCS7", // 使用するポート名に置き換えてください
        BaudRate = 115200,
        Parity = Parity.None,
        DataBits = 8,
        StopBits = StopBits.One,
        Handshake = Handshake.None,
        ReadTimeout = 500,
        WriteTimeout = 500
      };

      serialPort.DataReceived += SerialPort_DataReceived;

      try
      {
        serialPort.Open();
        Console.WriteLine("Port opened successfully!");

        while (true)
        {
          Console.Write("Enter a message to send (type 'ESC' to send ESC key): ");
          string? input = Console.ReadLine();

          if (input.Equals("ESC", StringComparison.OrdinalIgnoreCase))
          {
            // 'ESC'と入力された場合、ESCキーの制御文字（ASCII 27）を送信
            serialPort.Write(new byte[] { 27 }, 0, 1);
            Console.WriteLine("ESC key sent successfully!");
          }
          else
          {
            if (input == "end") break;

            // メッセージの末尾にキャリッジリターンを追加
            serialPort.Write(input + "\r");
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
        if (serialPort.IsOpen)
        {
          serialPort.Close();
          Console.WriteLine("Port closed successfully!");
        }
      }
    }

    private static async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      try
      {
        string receivedData = await Task.Run(() =>
        {
          try
          {
            return serialPort.ReadExisting();
          }
          catch (TimeoutException tex)
          {
            Console.WriteLine($"Timeout Error: {tex.Message}");
            return string.Empty; // タイムアウトの場合は空の文字列を返す
          }
        });

        if (!string.IsNullOrEmpty(receivedData))
        {
          buffer += receivedData;

          // 受信データを処理する
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
      while (buffer.Contains("\r") | buffer.Contains(">"))
      {
        int index;
        if (!buffer.Contains("\r"))
        {
          index = buffer.IndexOf(">");
        }
        else
        {
          index = buffer.IndexOf("\r");
        }

        string completeMessage = buffer.Substring(0, index + 1);
        buffer = buffer.Substring(index + 1);

        // 受信データの処理例
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Write($"{completeMessage}");
      }
    }
  }
}
