using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Sockets;

namespace ChineseChess
{
	public class Model
	{
        private static byte[] buffer = new byte[8192];

		internal static Point Locate(View view, Point p)
		{
			//View view = new View();

			int x=0, y=0;

			for (int j = 0; j < 8; j++)
			{
			    if (p.X <= view.cm[j])
			    {
			        x = view.c[j];
			        break;
			    }
				x = view.c[8];
			}

			for (int i = 0; i < 9; i++)
			{
				if (p.Y <= view.rm[i])
				{
					y = view.r[i];
					break;
				}
				y = view.r[9];
			}

			Point result = new Point(x,y);
			return result;
		}

		internal static void DoMove(View view, PictureBox pictureBox, int x1, int y1, int x2, int y2)
		{
			view.chessBoard[y1, x1] = null;
			pictureBox.Location = new Point(view.c[x2], view.r[y2]);
                
            if (view.chessBoard[y2, x2] != null)  // 吃子的情况
            {
                view.chessBoard[y2, x2].Enabled = false;
                view.chessBoard[y2, x2].Visible = false;
            }
            view.chessBoard[y2, x2] = pictureBox;			
		}

        internal static void SendMove(NetworkStream networkStream, int id, int x1, int y1, int x2, int y2)
        {
            try
            {
                buffer = Encoding.Unicode.GetBytes(id.ToString() + "_" + x1.ToString() + "_" + y1.ToString() + "_" + x2.ToString() + "_" + y2.ToString());
                //textBox1.Text = "";
                //string msg1 = Encoding.Unicode.GetString(buffer);
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

        /// <summary>
        /// 对接收到的对方的移动进行解码，转换成自己界面中的对方的移动
        /// </summary>
        /// <param name="text"> 信息</param>
        /// <returns> 解码后的数组</returns>
        internal static int[] DecodeMove(string text)
        {
            int[] result = new int[5];
            string[] array = text.Split('_');
            result[0] = int.Parse(array[0]);
            result[1] = 8 - int.Parse(array[1]);
            result[2] = 9 - int.Parse(array[2]);
            result[3] = 8 - int.Parse(array[3]);
            result[4] = 9 - int.Parse(array[4]);

            return result;
        }

		private static int GetType(PictureBox pictureBox)
		{
			if (pictureBox.Name.Contains("jiang"))
				return 0;
			else if (pictureBox.Name.Contains("shi"))
				return 1;
			else if (pictureBox.Name.Contains("xiang"))
				return 2;
			else if (pictureBox.Name.Contains("ma"))
				return 3;
			else if (pictureBox.Name.Contains("ju"))
				return 4;
			else if (pictureBox.Name.Contains("pao"))
				return 5;
			else
				return 6;
		}

        internal static bool CheckJiang(View view, int Rjiang_x, int Rjiang_y, int Bjiang_x, int Bjiang_y)
        {
            if (Rjiang_x == Bjiang_x)
            {
                for (int i = Bjiang_y + 1; i < Rjiang_y; i++)
                {
                    if (view.chessBoard[i, Rjiang_x] != null)  // 如果中间有棋子，说明没有将对将
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

		internal static bool CanMove(View view, int id, int x1, int y1, int x2, int y2)
		{
            if (view.chessBoard[y1, x1].Name.Contains("R")
                && view.chessBoard[y2, x2] != null
                && view.chessBoard[y2, x2].Name.Contains("R"))
            {               
                // 如果自己棋子吃自己棋子
                return false;               
            }            
            else
            {
			    switch (id)
			    { 
				    case 0:  // 将和帅的移动
                        if (x1 == x2 || y1 == y2)  // 垂直移动或水平移动
                        {
                            if (x1 == x2)  // 垂直移动
                            {
                                if (Math.Abs(y2 - y1) != 1)  // 不是移动一格，错误
                                {
                                    return false;
                                }
                                if (y2 < 7)  // 超出米子格，错误
                                {
                                    return false;
                                }
                                return true;  // 不是以上两种情况，可以移动
                            }
                            else  // 水平移动
                            {
                                if (Math.Abs(x2 - x1) != 1)  // 不是移动一格，错误
                                {
                                    return false;
                                }
                                if (x2 < 3 || x2 > 5)  // 超出米子格，错误
                                {
                                    return false;
                                }
                                return true;  // 不是以上两种情况，可以移动
                            }
                        }
                        else  // 不是垂直移动或水平移动，则不能移动
                        {
                            return false;
                        }

				    case 1:  // 士的移动
                        if (x2 > 2 && x2 < 6 && y2 > 6)  // 如果在米字范围内
                        {
                            if (Math.Abs(x2 - x1) == 1 && Math.Abs(y2 - y1) == 1)
                            {
                                return true; // 如果斜向移动一格，正确
                            }
                            else
                            {
                                return false; // 如果不是斜向移动一格，错误
                            }
                        }
                        else   // 如果在米字范围外，错误
                        {
                            return false;
                        }

				    case 2:  // 相的移动
                        if ((x2 == 0 || x2 == 2 || x2 == 4 || x2 == 6 || x2 == 8) && 
                            (y2 == 5 || y2 == 7 || y2 == 9))  // 如果在相该出现的范围内
                        {
                            if (Math.Abs(x2 - x1) == 2 && Math.Abs(y2 - y1) == 2) // 如果斜向移动两格
                            {
                                if (view.chessBoard[(y1 + y2) / 2, (x1 + x2) / 2] == null)
                                {
                                    return true;  // 如果没有障碍，正确
                                }
                                else
                                {
                                    return false;  // 如果有障碍，错误
                                }
                            }
                            else   // 如果不是斜向移动两格，错误
                            {
                                return false;
                            }
                        }
                        else   // 如果不在范围内，错误
                        {
                            return false;
                        }

				    case 3:  // 马的移动
                        if (Math.Abs(x1 - x2) == 1 && Math.Abs(y1 - y2) == 2) // 如果是竖的日字移动
                        {
                            if (view.chessBoard[(y1 + y2) / 2, x1] == null) // 如果没有障碍，正确
                            {
                                return true;
                            }
                            else  // 如果有障碍，错误
                            {
                                return false;
                            }
                        }
                        else if (Math.Abs(y1 - y2) == 1 && Math.Abs(x1 - x2) == 2) // 如果是横的日字移动
                        {
                            if (view.chessBoard[y1, (x1 + x2) / 2] == null) // 如果没有障碍，正确
                            {
                                return true;
                            }
                            else  // 如果有障碍，错误
                            {
                                return false;
                            }
                        }
                        else  // 如果不是日字移动，错误
                        {
                            return false;
                        }

				    case 4:  // 车的移动
                        if (x1 == x2 || y1 == y2)  // 如果是直线移动
                        {
                            if (x1 == x2)  // 如果是垂直移动
                            {
                                if (y2 > y1)  // 如果从上往下移动
                                {
                                    for (int i = y1 + 1; i < y2; i++)
                                    {
                                        if (view.chessBoard[i, x1] != null)  // 如果中间有棋子，就退出
                                        {
                                            return false;
                                        }
                                    }
                                }
                                else  // 如果从下往上移动
                                {
                                    for (int i = y2 + 1; i < y1; i++)
                                    {
                                        if (view.chessBoard[i, x1] != null)  // 如果中间有棋子，就退出
                                        {
                                            return false;
                                        }
                                    }
                                }
                                return true;  // 中间没有棋子，可以移动
                            }
                            else  // 如果水平移动
                            {
                                if (x2 > x1)  // 如果从左往右移动
                                {
                                    for (int i = x1 + 1; i < x2; i++)
                                    {
                                        if (view.chessBoard[y1, i] != null)  // 如果中间有棋子，就退出
                                        {
                                            return false;
                                        }
                                    }
                                }
                                else  // 如果从右往左移动
                                {
                                    for (int i = x2 + 1; i < x1; i++)
                                    {
                                        if (view.chessBoard[y1, i] != null)  // 如果中间有棋子，就退出
                                        {
                                            return false;
                                        }
                                    }
                                }
                                return true;  // 中间没有棋子，可以移动
                            }
                        }
                        else  // 如果不是直线移动，就出错
                        {
                            return false;
                        }

				    case 5:  // 炮的移动
                        if (x1 == x2 || y1 == y2) // 如果是直线移动
                        {
                            if (x1 == x2)  // 如果是垂直移动
                            {                                
                                if (view.chessBoard[y2, x2] == null)   // 如果只是移动
                                {
                                    if (y2 > y1)  // 如果从上往下移动
                                    {
                                        for (int i = y1 + 1; i < y2; i++)
                                        {
                                            if (view.chessBoard[i, x1] != null)  // 如果中间有棋子，就退出
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    else  // 如果从下往上移动
                                    {
                                        for (int i = y2 + 1; i < y1; i++)
                                        {
                                            if (view.chessBoard[i, x1] != null)  // 如果中间有棋子，就退出
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    return true;  // 中间没有棋子，可以移动
                                }
                                else // 如果是吃掉黑方棋子
                                {
                                    if (y2 > y1)  // 如果从上往下移动
                                    {
                                        int count = 0;
                                        for (int i = y1 + 1; i < y2; i++)
                                        {
                                            if (view.chessBoard[i, x1] != null)
                                            {
                                                count++;
                                            }
                                        }
                                        if (count == 1)  // 跳过一个棋子，正确
                                        {
                                            return true;
                                        }
                                        else  // 没有跳过棋子，或者跳过超过一个棋子，错误
                                        {
                                            return false;
                                        }
                                    }
                                    else  // 如果从下往上移动
                                    {
                                        int count = 0;
                                        for (int i = y2 + 1; i < y1; i++)
                                        {
                                            if (view.chessBoard[i, x1] != null)
                                            {
                                                count++;
                                            }
                                        }
                                        if (count == 1)  // 跳过一个棋子，正确
                                        {
                                            return true;
                                        }
                                        else  // 没有跳过棋子，或者跳过超过一个棋子，错误
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                            else  // 如果水平移动
                            {                                
                                if (view.chessBoard[y2, x2] == null)  // 如果只是移动
                                {
                                    if (x2 > x1)  // 如果从左往右移动
                                    {
                                        for (int i = x1 + 1; i < x2; i++)
                                        {
                                            if (view.chessBoard[y1, i] != null)  // 如果中间有棋子，就退出
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    else  // 如果从右往左移动
                                    {
                                        for (int i = x2 + 1; i < x1; i++)
                                        {
                                            if (view.chessBoard[y1, i] != null)  // 如果中间有棋子，就退出
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    return true;  // 中间没有棋子，可以移动
                                }
                                else // 如果是吃掉黑方棋子
                                {
                                    if (x2 > x1)  // 如果从左往右移动
                                    {
                                        int count = 0;
                                        for (int i = x1 + 1; i < x2; i++)
                                        {
                                            if (view.chessBoard[y1, i] != null)
                                            {
                                                count++;
                                            }
                                        }
                                        if (count == 1)  // 跳过一个棋子，正确
                                        {
                                            return true;
                                        }
                                        else  // 没有跳过棋子，或者跳过超过一个棋子，错误
                                        {
                                            return false;
                                        }
                                    }
                                    else  // 如果从右往左移动
                                    {
                                        int count = 0;
                                        for (int i = x2 + 1; i < x1; i++)
                                        {
                                            if (view.chessBoard[y1, i] != null)
                                            {
                                                count++;
                                            }
                                        }
                                        if (count == 1)  // 跳过一个棋子，正确
                                        {
                                            return true;
                                        }
                                        else  // 没有跳过棋子，或者跳过超过一个棋子，错误
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                        else  // 如果不是直线移动，就退出
                        {
                            return false;
                        }

				    case 6:  // 兵的移动
                        if (y1 > 4)     // 如果在自己的河界内
                        {
                            if (x1 == x2 && (y1 - y2) == 1) // 如果往前移动一步，正确
                            {
                                return true;
                            }
                            else  // 如果不是这么做，错误
                            {
                                return false;
                            }
                        }
                        else   // 如果在黑方的河界内
                        {
                            if (y2 == y1 && Math.Abs(x1 - x2) == 1) // 如果横向移动一步，正确
                            {
                                return true;
                            }
                            else if ((y1 - y2) == 1 && x1 == x2) // 如果往前移动一步，正确
                            {
                                return true;
                            }
                            else   // 如果不是以上两种情况，错误
                            {
                                return false;
                            }
                        }
                    default:   // 其它情况，错误
                        return false;
			    }	
            }
		}

	}
}
