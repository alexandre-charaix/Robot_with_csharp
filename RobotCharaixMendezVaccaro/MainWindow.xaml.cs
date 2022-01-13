using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtendedSerialPort;
using System.Windows.Threading;


namespace RobotCharaixMendezVaccaro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ReliableSerialPort serialPort1;
        DispatcherTimer timerAffichage;

        Robot robot = new Robot();


        public MainWindow()
        {
            InitializeComponent();
            serialPort1 = new ReliableSerialPort("COM15", 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Open();


            // Timer
            timerAffichage = new DispatcherTimer();
            timerAffichage.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerAffichage.Tick += TimerAffichage_Tick;
            timerAffichage.Start();
        }

        private void TimerAffichage_Tick(object sender, EventArgs e)
        {

            if (robot.byteListReceived.Count > 0)
            {

                for (int i = 0; i < robot.byteListReceived.Count; i++)
                {
                    //textBoxReception.Text += "0x" + DecodeMessage(robot.byteListReceived.Dequeue())+ " ";
                    DecodeMessage(robot.byteListReceived.Dequeue()) ;
                }
            }
        }

        public void SerialPort1_DataReceived(object sender, DataReceivedArgs e)
        {

            foreach (byte b in e.Data)
            {
                robot.byteListReceived.Enqueue(b);
            }
        }

        private void SendMessage()
        {
            if (textBoxEmission.Text != "")
            {
                serialPort1.WriteLine(textBoxEmission.Text);
                textBoxEmission.Text = "";
            }

        }


        private void buttonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
            if (buttonEnvoyer.Background != Brushes.RoyalBlue)
            {
                buttonEnvoyer.Background = Brushes.RoyalBlue;
            }
            else
            {
                buttonEnvoyer.Background = Brushes.Beige;
            }
            SendMessage();
        }


        private void TextBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {

            //byte[] byteList = new byte[20];
            //for(int i=0; i<=19; i++)
            //{
            //    byteList[i] = (byte)(2*i);
            //    serialPort1.Write(byteList[i].ToString());
            //}
            byte[] array = Encoding.ASCII.GetBytes(textBoxEmission.Text);
            UartEncodeAndSendMessage(0x0080, array.Length, array);

        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxReception.Text = "";
        }
        public byte CalculateChecksum(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte checksum = 0;
            checksum ^= 0xFE;
            checksum ^= (byte)(msgFunction);
            checksum ^= (byte)(msgPayloadLength);
            foreach (byte b in msgPayload)
            {
                checksum ^= b;
            }

            return checksum;

        }

        void UartEncodeAndSendMessage(int msgFunction, int msgPayloadLength, byte[] msgPayload)
        {
            byte[] Message = new byte[msgPayloadLength + 6];
            Message[0] = 0xFE;
            Message[1] = (byte)(msgFunction >> 8);
            Message[2] = (byte)(msgFunction);
            Message[3] = (byte)(msgPayloadLength >> 8);
            Message[4] = (byte)(msgPayloadLength);
            for (int i = 0; i < msgPayloadLength; i++)
            {
                Message[5 + i] = msgPayload[i];
            }
            Message[msgPayloadLength + 5] = CalculateChecksum(msgFunction, msgPayloadLength, msgPayload);
            serialPort1.Write(Message, 0, Message.Length);
        }


        public enum StateReception
        {
            Waiting,
            FunctionMSB,
            FunctionLSB,
            PayloadLengthMSB,
            PayloadLengthLSB,
            Payload,
            CheckSum
        }

        StateReception rcvState = StateReception.Waiting;
        int msgDecodedFunction = 0;
        int msgDecodedPayloadLength = 0;
        byte[] msgDecodedPayload;
        int msgDecodedPayloadIndex = 0;
        private void DecodeMessage(byte c)
        {
            switch (rcvState)
            {
                case StateReception.Waiting:
                    if (c == 0xFE)
                        rcvState = StateReception.FunctionMSB;
                    break;

                case StateReception.FunctionMSB:
                    msgDecodedFunction = c;
                    msgDecodedFunction = msgDecodedFunction >> 8;
                    rcvState = StateReception.FunctionLSB;
                    break;

                case StateReception.FunctionLSB:
                    msgDecodedFunction = (msgDecodedFunction | c);
                    rcvState = StateReception.PayloadLengthMSB;
                    break;

                case StateReception.PayloadLengthMSB:
                    msgDecodedPayloadLength = c;
                    msgDecodedPayloadLength = msgDecodedPayloadLength >> 8;
                    rcvState = StateReception.PayloadLengthLSB;
                    break;

                case StateReception.PayloadLengthLSB:
                    msgDecodedPayloadLength = (msgDecodedPayloadLength | c);
                    rcvState = StateReception.Payload;
                    if (msgDecodedPayloadLength == 0)
                        rcvState = StateReception.CheckSum;
                    else
                        rcvState = StateReception.Payload;
                    break;

                case StateReception.Payload:
                    msgDecodedPayload[msgDecodedPayloadIndex++] = c;
                    if (msgDecodedPayloadLength == msgDecodedPayloadIndex)
                        rcvState = StateReception.CheckSum;
                    else
                        rcvState = StateReception.Payload;
                    break;

                case StateReception.CheckSum:
                    if (CalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload) == c)
                    {
                        Console.WriteLine("CheckSum OK");

                    }

                    break;
                default:
                    rcvState = StateReception.Waiting;
                    break;
            }

        }


    }
}