/*
 *      [Principle of Navigation] Copyright (C) 2022 SJTU Inc.
 *      
 *      FileName : Time_Convert.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using System;

namespace LKY_Time_Convert
{
    internal class Time_Convert
    {
        static void Main(string[] args)
        {
            //欢迎话术
            Console.WriteLine("******** Homework #1: Time Conversion      ********");
            Console.WriteLine("******** Developer: liukaiyuan@sjtu.edu.cn ********");
            Console.WriteLine("******** Student Number: 122413930100      ********");
            Console.WriteLine();

            DateTime time_shanghai = new DateTime(2022, 9, 19, 10, 0, 0);
            Console.WriteLine("转换上海时间（" + time_shanghai.ToString() +  "）:\n");

            //转换至UTC时间，东八区时间-8小时，得到UTC时间
            DateTime UTC = time_shanghai.AddHours(-8);
            Console.WriteLine("---> UTC 时间：" + UTC.ToString());

            //先转换至TAI时间，根据2017年的跳秒资料，TAI = UTC + 37s
            DateTime TAI = UTC.AddSeconds(37);
            //TAI转为GPS时间，GPS = TAI - 19s
            DateTime GPST = TAI.AddSeconds(-19);

            //Console.WriteLine("---> GPS 时间：" + GPS.ToString());
            Console.WriteLine("---> GPS 时间：" + Get_GNSS_Time(GNSS_Type.GPS, GPST));

            //GPS转为BDS时间，BDT = GPST - 14s
            DateTime BDT = GPST.AddSeconds(-14);
            Console.WriteLine("---> BDS 时间：" + Get_GNSS_Time(GNSS_Type.BDS, BDT));

            Console.WriteLine("\nDone. 可按任意键退出运行。");
            Console.ReadKey();
        }

        /// <summary>
        /// 获取GNSS格式的时间
        /// </summary>
        /// <returns></returns>
        private static string Get_GNSS_Time(GNSS_Type type, DateTime dateTime)
        {
            DateTime startTime;     //定义GNSS的起始发射时间

            //依据不同的GNSS来指定起始时间
            if (type == GNSS_Type.GPS)
            {
                startTime = new DateTime(1980, 1, 6);
            }
            else
            {
                startTime = new DateTime(2006, 1, 1);
            }

            ///转换为 Week Number 和 Seconds Of Week
            TimeSpan Day_Diff = dateTime.Subtract(startTime);      //计算时间差值
            int Weeks_Diff = (int)Day_Diff.TotalDays / 7 % 1024;     //计算周数差值，并基于1024周清零

            int WeekOfSec = (int)Day_Diff.TotalSeconds % (7 * 24 * 3600);     //计算 week of seconds

            return Weeks_Diff + " weeks, " + WeekOfSec + " s";
        }

        /// <summary>
        /// 定义一个GNSS类型
        /// </summary>
        enum GNSS_Type
        { 
            GPS,    //GPS类型
            BDS     //北斗
        }
    }
}
