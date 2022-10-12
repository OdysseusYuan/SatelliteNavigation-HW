/*
 *      [Principle of Navigation] Copyright (C) 2022 SJTU Inc.
 *      
 *      FileName : Log.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using System;
using System.Threading;

namespace LKY_Calculate_Satellite_Position.Common
{
    /// <summary>
    /// 输出日志的类库
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Log输出模式
        /// </summary>
        public enum LogType
        {
            /// <summary>
            /// 运算模式
            /// </summary>
            Calculate,

            /// <summary>
            /// 一般消息通知
            /// </summary>
            Message,

            /// <summary>
            /// 严重错误
            /// </summary>
            Error,

            /// <summary>
            /// 用于呈现过程
            /// </summary>
            Display,

            /// <summary>
            /// 用于欢迎话术
            /// </summary>
            Welcome,
        }

        /// <summary>
        /// Log箭头类型
        /// </summary>
        public enum ArrowType
        {
            /// <summary>
            /// 无箭头
            /// </summary>
            None,

            /// <summary>
            /// 无箭头，但对齐格式
            /// </summary>
            NoneFormat,

            /// <summary>
            /// 完整箭头
            /// </summary>
            Full,

            /// <summary>
            /// 省略双横线
            /// </summary>
            Half,

            /// <summary>
            /// 打钩号类型
            /// </summary>
            Tick,
        }

        /// <summary>
        /// 自定义日志输出格式
        /// </summary>
        /// <param name="print_info"></param>
        /// <param name="logType"></param>
        /// <param name="arrowType"></param>
        public Log(string print_info, LogType logType = LogType.Display, ArrowType arrowType = ArrowType.Half)
        {
            //更改箭头
            string Arrow;
            if (arrowType == ArrowType.Half)
            {
                Arrow = "      >> ";
            }
            else if (arrowType == ArrowType.Full)
            {
                Arrow = "======>> ";
            }
            else if (arrowType == ArrowType.Tick)
            {
                Arrow = "   √ >> ";
            }
            else if (arrowType == ArrowType.NoneFormat)
            {
                Arrow = "         ";
            }
            else
            {
                Arrow = "";
            }

            //依据不同的类别，更改颜色
            if (logType == LogType.Calculate)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }
            else if (logType == LogType.Display)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
            }
            else if (logType == LogType.Message)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            else if (logType == LogType.Error)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }
            else if (logType == LogType.Welcome)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.WriteLine(Arrow + print_info);

            bool ForDemo = true;
            //预览模式下，增加一个延时效果，方便看数据
            if (ForDemo)
            {
                if (logType == LogType.Calculate)
                {
                    Thread.Sleep(new Random().Next(3000, 4000));
                }
                else if (logType == LogType.Display)
                {
                    Thread.Sleep(new Random().Next(20, 50));
                }
                else if (logType == LogType.Message)
                {
                    Thread.Sleep(new Random().Next(1500, 2500));
                }
                else if (logType == LogType.Error)
                {
                    //Thread.Sleep(0);
                }
                else if (logType == LogType.Welcome)
                {
                    Thread.Sleep(20);
                }
            }
        }
    }
}
