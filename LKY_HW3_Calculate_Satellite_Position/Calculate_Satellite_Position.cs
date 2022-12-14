/*
 *      [Principle of Navigation] Copyright (C) 2022 SJTU Inc.
 *      
 *      FileName : Calculate_Satellite_Position.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */


using LKY_Calculate_Satellite_Position.Common;
using LKY_Calculate_Satellite_Position.Lib;
using System;
using System.Threading;

namespace LKY_Calculate_Satellite_Position
{
    internal class Calculate_Satellite_Position
    {
        static void Main(string[] args)
        {
            //欢迎话术
            new Log("******** Homework #3: Satellite Position   ********", Log.LogType.Welcome, Log.ArrowType.None);
            new Log("******** Developer: liukaiyuan@sjtu.edu.cn ********", Log.LogType.Welcome, Log.ArrowType.None);
            new Log("******** Student Number: 122413930100      ********", Log.LogType.Welcome, Log.ArrowType.None);
            new Log("", Log.LogType.Welcome, Log.ArrowType.None);

            // -------------- Calculate Satellite Position Begin --------------

            new Log("Demands: Orbit calculation Download the Yuma and Rinex files in the canvas, and " +
                            "calculate satellite PRN 01 's position in both orbital and ECEF coordinates.\n", Log.LogType.Welcome, Log.ArrowType.None);

            new Log("※※※ Part1, Using Rinex files calculate:\n", Log.LogType.Welcome, Log.ArrowType.None);

            //延时下
            Thread.Sleep(2000);

            //计算卫星位置【Rinex】
            new lib_OrbitalFormula.ByRinex(1);

            new Log("\n※※※ Part2, Using Yuma files calculate:\n", Log.LogType.Welcome, Log.ArrowType.None);

            //计算卫星位置【Yuma】
            new lib_OrbitalFormula.ByYuma(1);

            // -------------- Calculate Satellite Position End --------------

            //test_time();

            new Log("\n\nDone. 按任意键退出运行。", Log.LogType.Welcome, Log.ArrowType.None);
            Console.ReadKey();
        }

        /// <summary>
        /// 仅用于测试
        /// </summary>
        private static void test_time()
        {
            int yuma_sec = 61440;
            int rinex_sec = 79200;
            DateTime rinex_datetime = new DateTime(2022, 10, 02, 22, 00, 00);
            DateTime yuma_datetime = rinex_datetime.AddSeconds(-1f * (rinex_sec - yuma_sec));
            new Log(yuma_datetime.ToString());
        }
    }



}
