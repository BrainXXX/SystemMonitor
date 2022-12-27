using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemMonitor
{
    public partial class Form1 : Form
    {
        // Регистрируем класс KeyboardHook
        KeyboardHook hook = new KeyboardHook();

        // Блок для определения позиции курсора на экране
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        private struct POINT
        {
            public int X;
            public int Y;
        }

        private POINT ShowMousePosition()
        {
            GetCursorPos(out POINT point);
            return point;
        }

        // Блок для перемещения позиции курсора на экране и для клика ЛКМ
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_MOVE = 0x01; //Movement occurred.
        private const uint MOUSEEVENTF_LEFTDOWN = 0x02; //The left button is down.
        private const uint MOUSEEVENTF_LEFTUP = 0x04; //The left button is up.

        int count = 0;

        public Form1()
        {
            InitializeComponent();

            Text = "SystemMonitor v27.12.2022";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            AllowTransparency = true;
            Opacity = 0.9;
            BackColor = Color.AliceBlue;


            groupBox1.Text = "GlobalPosition";
            label1.Text = "X:";
            label2.Text = "";
            label3.Text = "Y:";
            label4.Text = "";

            groupBox2.Text = "JobPosition";
            label5.Text = "X:";
            label6.Text = "Interval:";
            label7.Text = "Y:";
            textBox1.Text = "";
            textBox2.Text = "500";
            textBox3.Text = "";
            button1.Text = "Run [Alt+F1]";

            label8.Text = count.ToString();
            label9.Text = "Count Click:";
            label10.Text = "[Alt+F2] - Stop; [Alt+F3] - Get position;";
            button2.Text = "Close";

            // Включаем таймер для отображения текущих координат мыши
            timer1.Enabled = true;
            timer1.Interval = 100;

            // Регистрация события, запускающегося после ввода зарегистрированной комбинации клавиш
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(Hook_KeyPressed);
            // Регистрация комбинаций клавиш
            hook.RegisterHotKey(SystemMonitor.ModifierKeys.Alt, Keys.F1);
            hook.RegisterHotKey(SystemMonitor.ModifierKeys.Alt, Keys.F2);
            hook.RegisterHotKey(SystemMonitor.ModifierKeys.Alt, Keys.F3);
        }

        private void Hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (((ModifierKeys & Keys.Alt) == Keys.Alt) && (e.Key == Keys.F1))
            {
                Button1_Click(sender, e);
            }

            if (((ModifierKeys & Keys.Alt) == Keys.Alt) && (e.Key == Keys.F2))
            {
                StopTimer();
            }

            if (((ModifierKeys & Keys.Alt) == Keys.Alt) && (e.Key == Keys.F3))
            {
                GetCurrentCursorPosition();
            }
        }

        private void GetCurrentCursorPosition()
        {
            POINT cursorPosition = ShowMousePosition();

            if (label2.Text != "" && label4.Text != "")
            {
                textBox1.Text = cursorPosition.X.ToString();
                textBox3.Text = cursorPosition.Y.ToString();
            }
        }

        private async Task LeftMouseClick(string _Xposition, string _Yposition)
        {
            int Xposition = Int32.Parse(_Xposition);
            int Yposition = Int32.Parse(_Yposition);

            await Task.Delay(100);
            SetCursorPos(Xposition, Yposition);
            await Task.Delay(100);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            await Task.Delay(100);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
            await Task.Delay(100);
            mouse_event(MOUSEEVENTF_MOVE, 1, 1, 0, IntPtr.Zero);
        }

        private void StopTimer()
        {
            timer2.Enabled = false;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            POINT cursorPosition = ShowMousePosition();

            if (label2.Text != cursorPosition.X.ToString() || label4.Text != cursorPosition.Y.ToString())
            {
                label2.Text = cursorPosition.X.ToString();
                label4.Text = cursorPosition.Y.ToString();
            }
        }

        private async void Timer2_Tick(object sender, EventArgs e)
        {
            await LeftMouseClick(textBox1.Text, textBox3.Text);

            count++;
            label8.Text = count.ToString();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "")
                {
                    count = 0;
                    timer2.Enabled = true;
                    timer2.Interval = Int32.Parse(textBox2.Text);
                }
                else
                {
                    timer2.Enabled = false;
                    MessageBox.Show("Не заданы параметры для курсора.", "Warning!!!");
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Source + ": " + error.ToString());
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            hook.Dispose();
        }
    }
}