using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;

using NATUPNPLib;
using UPNPLib;

namespace ChineseChess
{
	public partial class View : Form
	{
		public int[] r = new int[10] { 30, 84, 137, 191, 244, 298, 351, 405, 458, 512 };
		public int[] c = new int[9] { 217, 270, 323, 377, 430, 484, 538, 591, 645 };

		public int[] rev_r = new int[800];
		public int[] rev_c = new int[800];

		public int[] rm = new int[9];
		public int[] cm = new int[8];

		public PictureBox[,] chessBoard = new PictureBox[10, 9];		

		public Point _p;
		public Point p;
		public int _x, _y, x, y;

		private bool hold = false;
		private bool isMove = false;
        private bool isRed = false;
        private bool blackTempMoveFlag = false;

        private NetworkStream networkStream = null;

		public View()
		{
			InitializeComponent();
            
            SearchRouter();
			InitializePosition();
		}

        public delegate void UpdateDelegate(string text);

        public static ManualResetEvent tcpClientConnected = new ManualResetEvent(false);
        public static ManualResetEvent connectDone = new ManualResetEvent(false);

        /// <summary>
        /// ����·����
        /// </summary>
        public void SearchRouter()
        {
            UPnPDeviceFinder finder = new UPNPLib.UPnPDeviceFinderClass();
            string deviceType = "upnp:rootdevice";
            UPNPLib.IUPnPDevices allDevice = new UPNPLib.UPnPDevicesClass();
            allDevice = finder.FindByType(deviceType, 0);

            UPNPLib.IUPnPDevice device = new UPNPLib.UPnPDeviceClass();

            foreach (UPNPLib.UPnPDevice p in allDevice)
            {
                if (p.Type.Equals("urn:schemas-upnp-org:device:InternetGatewayDevice:1"))
                {
                    device = p;
                    break;
                }
            }
            labelRouterFind.Text = "�ѷ���·������";
            labelRouterFind.ForeColor = Color.Blue;
        }

        public void Update(string text)
        {
            if (this.InvokeRequired)
            {
                UpdateDelegate updateDelegate = new UpdateDelegate(Update);
                this.BeginInvoke(updateDelegate, new object[] { text });
                return;
            }
            //richTextBox1.Text += "Friend: " + txt + "\t\n";
            //richTextBox1.ScrollToCaret();

            int[] result = Model.DecodeMove(text);
            UpdateView(result);
            isRed = true;
        }

        public class receiveClass
        {
            private NetworkStream ns;
            private View.UpdateDelegate updateDelg = null;

            public receiveClass(NetworkStream networkStream, View.UpdateDelegate delg)
            {
                this.ns = networkStream;
                this.updateDelg = delg;
            }

            public void receiveThread()
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = ns.Read(buffer, 0, 8192)) != 0)
                {
                    string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                    updateDelg.BeginInvoke(msg, null, null);
                }
            }
        }

        private byte[] buffer = new byte[8192];
        private TcpClient tcpClient;

        /// <summary>
        /// ��ʼ���������ӵ�λ��
        /// </summary>
		public void InitializePosition()
		{
			rev_r[30] = 0;
			rev_r[84] = 1;
			rev_r[137] = 2;
			rev_r[191] = 3;
			rev_r[244] = 4;
			rev_r[298] = 5;
			rev_r[351] = 6;
			rev_r[405] = 7;
			rev_r[458] = 8;
			rev_r[512] = 9;

            rev_c[217] = 0;
            rev_c[270] = 1;
            rev_c[323] = 2;
            rev_c[377] = 3;
            rev_c[430] = 4;
            rev_c[484] = 5;
            rev_c[538] = 6;
            rev_c[591] = 7;
            rev_c[645] = 8;

            // ���������PictureBox��

            // �ڳ�
            pictureBox_Bju.Location = new Point(c[8], r[0]); chessBoard[0, 8] = pictureBox_Bju;

            // ����
            pictureBox_Bma.Location = new Point(c[7], r[0]); chessBoard[0, 7] = pictureBox_Bma;

            // ����
            pictureBox_Bxiang.Location = new Point(c[6], r[0]); chessBoard[0, 6] = pictureBox_Bxiang;

            // ��ʿ
            pictureBox_Bshi.Location = new Point(c[5], r[0]); chessBoard[0, 5] = pictureBox_Bshi;

            // �ڽ�
            pictureBox_Bjiang.Location = new Point(c[4], r[0]); chessBoard[0, 4] = pictureBox_Bjiang;

            // ��ʿ1
            pictureBox_Bshi1.Location = new Point(c[3], r[0]); chessBoard[0, 3] = pictureBox_Bshi1;

            // ����1
            pictureBox_Bxiang1.Location = new Point(c[2], r[0]); chessBoard[0, 2] = pictureBox_Bxiang1;

            // ����1
            pictureBox_Bma1.Location = new Point(c[1], r[0]); chessBoard[0, 1] = pictureBox_Bma1;

            // �ڳ�1
            pictureBox_Bju1.Location = new Point(c[0], r[0]); chessBoard[0, 0] = pictureBox_Bju1;

            // ����
            pictureBox_Bpao.Location = new Point(c[7], r[2]); chessBoard[2, 7] = pictureBox_Bpao;
            pictureBox_Bpao1.Location = new Point(c[1], r[2]); chessBoard[2, 1] = pictureBox_Bpao1;

            // ����
            pictureBox_Bbing.Location = new Point(c[8], r[3]); chessBoard[3, 8] = pictureBox_Bbing;
            pictureBox_Bbing1.Location = new Point(c[6], r[3]); chessBoard[3, 6] = pictureBox_Bbing1;
            pictureBox_Bbing2.Location = new Point(c[4], r[3]); chessBoard[3, 4] = pictureBox_Bbing2;
            pictureBox_Bbing3.Location = new Point(c[2], r[3]); chessBoard[3, 2] = pictureBox_Bbing3;
            pictureBox_Bbing4.Location = new Point(c[0], r[3]); chessBoard[3, 0] = pictureBox_Bbing4;

            // ���
            pictureBox_Rbing.Location = new Point(c[0], r[6]); chessBoard[6, 0] = pictureBox_Rbing;
            pictureBox_Rbing1.Location = new Point(c[2], r[6]); chessBoard[6, 2] = pictureBox_Rbing1;
            pictureBox_Rbing2.Location = new Point(c[4], r[6]); chessBoard[6, 4] = pictureBox_Rbing2;
            pictureBox_Rbing3.Location = new Point(c[6], r[6]); chessBoard[6, 6] = pictureBox_Rbing3;
            pictureBox_Rbing4.Location = new Point(c[8], r[6]); chessBoard[6, 8] = pictureBox_Rbing4;

            // ����
            pictureBox_Rpao.Location = new Point(c[1], r[7]); chessBoard[7, 1] = pictureBox_Rpao;
            pictureBox_Rpao1.Location = new Point(c[7], r[7]); chessBoard[7, 7] = pictureBox_Rpao1;

            // �쳵
            pictureBox_Rju.Location = new Point(c[0], r[9]); chessBoard[9, 0] = pictureBox_Rju;

            // ����
            pictureBox_Rma.Location = new Point(c[1], r[9]); chessBoard[9, 1] = pictureBox_Rma;

            // ����
            pictureBox_Rxiang.Location = new Point(c[2], r[9]); chessBoard[9, 2] = pictureBox_Rxiang;

            // ����
            pictureBox_Rshi.Location = new Point(c[3], r[9]); chessBoard[9, 3] = pictureBox_Rshi;

            // ��˧
            pictureBox_Rjiang.Location = new Point(c[4], r[9]); chessBoard[9, 4] = pictureBox_Rjiang;

            // ����1
            pictureBox_Rshi1.Location = new Point(c[5], r[9]); chessBoard[9, 5] = pictureBox_Rshi1;

            // ����1
            pictureBox_Rxiang1.Location = new Point(c[6], r[9]); chessBoard[9, 6] = pictureBox_Rxiang1;

            // ����1
            pictureBox_Rma1.Location = new Point(c[7], r[9]); chessBoard[9, 7] = pictureBox_Rma1;

            // �쳵1
            pictureBox_Rju1.Location = new Point(c[8], r[9]); chessBoard[9, 8] = pictureBox_Rju1;

			for (int i = 0; i < 9; i++)
			{
				rm[i] = (r[i] + r[i + 1]) / 2;      // row margin
			}
			for (int j = 0; j < 8; j++)
			{
				cm[j] = (c[j] + c[j + 1]) / 2;      // column margin
			}

            isRed = true;
		}

        private void UpdateView(int[] result)
        {
            switch (result[0])
            { 
                case 0:
                    Model.DoMove(this, pictureBox_Bjiang, result[1], result[2], result[3], result[4]);
                    break;
                case 1:
                    Model.DoMove(this, pictureBox_Bshi, result[1], result[2], result[3], result[4]);
                    break;
                case 2:
                    Model.DoMove(this, pictureBox_Bshi1, result[1], result[2], result[3], result[4]);
                    break;
                case 3:
                    Model.DoMove(this, pictureBox_Bxiang, result[1], result[2], result[3], result[4]);
                    break;
                case 4:
                    Model.DoMove(this, pictureBox_Bxiang1, result[1], result[2], result[3], result[4]);
                    break;
                case 5:
                    Model.DoMove(this, pictureBox_Bma, result[1], result[2], result[3], result[4]);
                    break;
                case 6:
                    Model.DoMove(this, pictureBox_Bma1, result[1], result[2], result[3], result[4]);
                    break;
                case 7:
                    Model.DoMove(this, pictureBox_Bju, result[1], result[2], result[3], result[4]);
                    break;
                case 8:
                    Model.DoMove(this, pictureBox_Bju1, result[1], result[2], result[3], result[4]);
                    break;
                case 9:
                    Model.DoMove(this, pictureBox_Bpao, result[1], result[2], result[3], result[4]);
                    break;
                case 10:
                    Model.DoMove(this, pictureBox_Bpao1, result[1], result[2], result[3], result[4]);
                    break;
                case 11:
                    Model.DoMove(this, pictureBox_Bbing, result[1], result[2], result[3], result[4]);
                    break;
                case 12:
                    Model.DoMove(this, pictureBox_Bbing1, result[1], result[2], result[3], result[4]);
                    break;
                case 13:
                    Model.DoMove(this, pictureBox_Bbing2, result[1], result[2], result[3], result[4]);
                    break;
                case 14:
                    Model.DoMove(this, pictureBox_Bbing3, result[1], result[2], result[3], result[4]);
                    break;
                case 15:
                    Model.DoMove(this, pictureBox_Bbing4, result[1], result[2], result[3], result[4]);
                    break;
                default:
                    break;
            }
        }

        // �����˳�
        private void View_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// ģ���ɫ�����ƶ�
        /// </summary>
        private void BlackTempMove()
        {
            if (pictureBox_Bjiang.Enabled == false)
            {
                MessageBox.Show("��Ӯ�ˣ�");
                return;
            }
            if (!blackTempMoveFlag)
            {
                Model.DoMove(this, pictureBox_Bjiang, 4, 0, 4, 1);
                blackTempMoveFlag = true;
            }
            else
            {
                Model.DoMove(this, pictureBox_Bjiang, 4, 1, 4, 0);
                blackTempMoveFlag = false;
            }
            isRed = true;
        }

        // �쳵���
        private void pictureBox_Rju_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rju.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rju.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 4, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rju, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 7, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rju, x, y, _x, _y);
                    }                    
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rju, _x, _y, _x, _y);
                }
            }
        }

        // �쳵�ƶ�
        private void pictureBox_Rju_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rju.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rju.Location = new Point(pictureBox_Rju.Location.X + x1, pictureBox_Rju.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // �쳵1���
        private void pictureBox_Rju1_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rju1.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rju1.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 4, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rju1, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 8, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rju1, x, y, _x, _y);
                    }                    
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rju1, _x, _y, _x, _y);
                }
            }
        }

        // �쳵1�ƶ�
        private void pictureBox_Rju1_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rju1.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rju1.Location = new Point(pictureBox_Rju1.Location.X + x1, pictureBox_Rju1.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ������
        private void pictureBox_Rma_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rma.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rma.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 3, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rma, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 5, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rma, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rma, _x, _y, _x, _y);
                }
            }
        }

        // �����ƶ�
        private void pictureBox_Rma_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rma.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rma.Location = new Point(pictureBox_Rma.Location.X + x1, pictureBox_Rma.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ����1���
        private void pictureBox_Rma1_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rma1.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rma1.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 3, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rma1, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 6, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rma1, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rma1, _x, _y, _x, _y);
                }
            }
        }

        // ����1�ƶ�
        private void pictureBox_Rma1_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rma1.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rma1.Location = new Point(pictureBox_Rma1.Location.X + x1, pictureBox_Rma1.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ������
        private void pictureBox_Rxiang_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rxiang.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rxiang.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 2, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rxiang, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 3, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rxiang, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rxiang, _x, _y, _x, _y);
                }
            }
        }

        // �����ƶ�
        private void pictureBox_Rxiang_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rxiang.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rxiang.Location = new Point(pictureBox_Rxiang.Location.X + x1, pictureBox_Rxiang.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ����1���
        private void pictureBox_Rxiang1_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rxiang1.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rxiang1.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 2, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rxiang1, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 4, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rxiang1, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rxiang1, _x, _y, _x, _y);
                }
            }
        }

        // ����1�ƶ�
        private void pictureBox_Rxiang1_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rxiang1.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rxiang1.Location = new Point(pictureBox_Rxiang1.Location.X + x1, pictureBox_Rxiang1.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ���˵��
        private void pictureBox_Rshi_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rshi.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rshi.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 1, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rshi, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 1, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rshi, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rshi, _x, _y, _x, _y);
                }
            }
        }

        // �����ƶ�
        private void pictureBox_Rshi_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rshi.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rshi.Location = new Point(pictureBox_Rshi.Location.X + x1, pictureBox_Rshi.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ����1���
        private void pictureBox_Rshi1_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rshi1.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rshi1.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 1, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rshi1, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 2, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rshi1, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rshi1, _x, _y, _x, _y);
                }
            }
        }

        // ����1�ƶ�
        private void pictureBox_Rshi1_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rshi1.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rshi1.Location = new Point(pictureBox_Rshi1.Location.X + x1, pictureBox_Rshi1.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ��˧���
        private void pictureBox_Rjiang_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rjiang.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rjiang.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 0, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rjiang, _x, _y, x, y);
                    if (Model.CheckJiang(this, x, y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 0, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rjiang, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rjiang, _x, _y, _x, _y);
                }
            }
        }

        // ��˧�ƶ�
        private void pictureBox_Rjiang_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rjiang.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rjiang.Location = new Point(pictureBox_Rjiang.Location.X + x1, pictureBox_Rjiang.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ���ڵ��
        private void pictureBox_Rpao_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rpao.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rpao.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 5, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rpao, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 9, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rpao, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rpao, _x, _y, _x, _y);
                }
            }
        }

        // �����ƶ�
        private void pictureBox_Rpao_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rpao.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rpao.Location = new Point(pictureBox_Rpao.Location.X + x1, pictureBox_Rpao.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ����1���
        private void pictureBox_Rpao1_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rpao1.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rpao1.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 5, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rpao1, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 10, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rpao1, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rpao1, _x, _y, _x, _y);
                }
            }
        }

        // ����1�ƶ�
        private void pictureBox_Rpao1_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rpao1.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rpao1.Location = new Point(pictureBox_Rpao1.Location.X + x1, pictureBox_Rpao1.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ������
        private void pictureBox_Rbing_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rbing.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rbing.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 6, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rbing, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 11, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rbing, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rbing, _x, _y, _x, _y);
                }
            }
        }
        
        // ����ƶ�
        private void pictureBox_Rbing_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rbing.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rbing.Location = new Point(pictureBox_Rbing.Location.X + x1, pictureBox_Rbing.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ���1���
        private void pictureBox_Rbing1_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rbing1.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rbing1.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 6, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rbing1, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 12, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rbing1, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rbing1, _x, _y, _x, _y);
                }
            }
        }

        // ���1�ƶ�
        private void pictureBox_Rbing1_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rbing1.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rbing1.Location = new Point(pictureBox_Rbing1.Location.X + x1, pictureBox_Rbing1.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ���2���
        private void pictureBox_Rbing2_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rbing2.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rbing2.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 6, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rbing2, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 13, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rbing2, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rbing2, _x, _y, _x, _y);
                }
            }
        }

        // ���2�ƶ�
        private void pictureBox_Rbing2_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rbing2.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rbing2.Location = new Point(pictureBox_Rbing2.Location.X + x1, pictureBox_Rbing2.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ���3���
        private void pictureBox_Rbing3_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rbing3.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rbing3.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 6, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rbing3, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 14, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rbing3, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rbing3, _x, _y, _x, _y);
                }
            }
        }

        // ���3�ƶ�
        private void pictureBox_Rbing3_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rbing3.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rbing3.Location = new Point(pictureBox_Rbing3.Location.X + x1, pictureBox_Rbing3.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ���4���
        private void pictureBox_Rbing4_MouseClick(object sender, MouseEventArgs e)
        {
            if (hold == false && isRed)
            {
                hold = true;
                _p = pictureBox_Rbing4.Location;

                _x = rev_c[_p.X];
                _y = rev_r[_p.Y];

                p = Cursor.Position;
            }

            if (hold == true && isMove == true)
            {
                hold = false;
                isMove = false;
                Point point = Model.Locate(this, pictureBox_Rbing4.Location);
                x = rev_c[point.X];
                y = rev_r[point.Y];

                Point RjiangPoint = Model.Locate(this, pictureBox_Rjiang.Location);
                int Rjiang_x = rev_c[RjiangPoint.X];
                int Rjiang_y = rev_r[RjiangPoint.Y];

                Point BjiangPoint = Model.Locate(this, pictureBox_Bjiang.Location);
                int Bjiang_x = rev_c[BjiangPoint.X];
                int Bjiang_y = rev_r[BjiangPoint.Y];

                if (Model.CanMove(this, 6, _x, _y, x, y))
                {
                    Model.DoMove(this, pictureBox_Rbing4, _x, _y, x, y);
                    if (Model.CheckJiang(this, Rjiang_x, Rjiang_y, Bjiang_x, Bjiang_y))
                    {
                        Model.SendMove(networkStream, 15, _x, _y, x, y);
                        isRed = false;
                    }
                    else
                    {
                        Model.DoMove(this, pictureBox_Rbing4, x, y, _x, _y);
                    }
                }
                else
                {
                    Model.DoMove(this, pictureBox_Rbing4, _x, _y, _x, _y);
                }
            }
        }

        // ���4�ƶ�
        private void pictureBox_Rbing4_MouseMove(object sender, MouseEventArgs e)
        {
            if (hold == true)
            {
                isMove = true;
                pictureBox_Rbing4.BringToFront();
                int x1 = Cursor.Position.X - p.X;
                int y1 = Cursor.Position.Y - p.Y;
                pictureBox_Rbing4.Location = new Point(pictureBox_Rbing4.Location.X + x1, pictureBox_Rbing4.Location.Y + y1);
                p = Cursor.Position;
            }
        }

        // ���ڲ��Եķ�����Ϣ�İ�ť
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                buffer = Encoding.Unicode.GetBytes("����һ����Ϣ��");
                //textBox1.Text = "";
                string msg1 = Encoding.Unicode.GetString(buffer);
                //richTextBox1.Text += "Me:      " + msg1 + "\t\n";
                //richTextBox1.ScrollToCaret();
                lock (networkStream)
                {
                    networkStream.Write(buffer, 0, buffer.Length);
                }
                //textBox1.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ��ʼ��Ϸ
        private void gameStart_Click(object sender, EventArgs e)
        {
            IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
            //if (RoutMap.addPortMappingFlag == true)
            //{
            try
            {
                toolStripStatusLabel1.Text = "Listening...";
                //TcpListener tcpListener = new TcpListener(ipAddress, Convert.ToInt32(RoutMap.locPort));
                TcpListener tcpListener = new TcpListener(ipAddress, Convert.ToInt32("10005"));
                tcpListener.Start();

                tcpClientConnected.Reset();
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), tcpListener);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //}
        }

        /// <summary>
        /// �������˵�TcpClient�ص�����
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient tcpClient = listener.EndAcceptTcpClient(ar);
            
            toolStripStatusLabel1.Text = "Connection Established!";

            // ��TcpClient�л�ȡNetworkStream
            networkStream = tcpClient.GetStream();

            lock (networkStream)
            {
                receiveClass rc = new receiveClass(networkStream, new UpdateDelegate(Update));
                Thread thread = new Thread(new ThreadStart(rc.receiveThread));
                thread.IsBackground = true;
                thread.Start();
            }

            // ��ֹtcpClientConnected
            tcpClientConnected.Set();
        }

        // ����
        private void send_Click(object sender, EventArgs e)
        {
            //if (RoutMap.addPortMappingFlag == true)
            //{
            try
            {
                tcpClient = new TcpClient();
                connectDone.Reset();
                tcpClient.BeginConnect(IPAddress.Parse("101.244.189.87"),
                    10005,
                    new AsyncCallback(ConnectCallback),
                    tcpClient);
                connectDone.WaitOne();
                toolStripStatusLabel1.Text = "Connected!";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            networkStream = tcpClient.GetStream();
            lock (networkStream)
            {
                receiveClass rc = new receiveClass(networkStream, new UpdateDelegate(Update));
                Thread thread = new Thread(new ThreadStart(rc.receiveThread));
                thread.IsBackground = true;
                thread.Start();
            }
            //}
        }

        /// <summary>
        /// �ͻ������ӳɹ��Ļص�����
        /// </summary>
        /// <param name="ar"> �첽����״̬</param>
        private static void ConnectCallback(IAsyncResult ar)
        {
            TcpClient client = (TcpClient)ar.AsyncState;
            client.EndConnect(ar);
            connectDone.Set();
        }
	}
}