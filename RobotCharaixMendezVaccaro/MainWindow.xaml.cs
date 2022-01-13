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
                    textBoxReception.Text += "0x"+robot.byteListReceived.Dequeue().ToString("X2")+" ";
                }
            }
        }

        public void SerialPort1_DataReceived(object sender, DataReceivedArgs e)
        {
            
            foreach(var b in e.Data)
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
            byte[] byteList = new byte[20];
            for(int i=0; i<=19; i++)
            {
                byteList[i] = (byte)(2*i);
                serialPort1.Write(byteList[i].ToString());
            }

        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            textBoxReception.Text = "";
        }
    }
}
