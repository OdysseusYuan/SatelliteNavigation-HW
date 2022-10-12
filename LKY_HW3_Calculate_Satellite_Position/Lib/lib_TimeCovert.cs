/*
 *      [Principle of Navigation] Copyright (C) 2022 SJTU Inc.
 *      
 *      FileName : lib_TimeCovert.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using LKY_Calculate_Satellite_Position.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LKY_Calculate_Satellite_Position.Lib
{
    /// <summary>
    /// 时间转换的类库
    /// </summary>
    public class lib_TimeCovert
    {
        /// <summary>
        /// 定义一个GNSS类型
        /// </summary>
        public enum GNSS_Type
        {
            GPS,    //GPS类型
            BDS     //北斗
        }

        /// <summary>
        /// 获取GNSS格式的时间，返回[weeks, week of seconds]
        /// </summary>
        /// <returns></returns>
        public static int [] Get_GNSS_Time(GNSS_Type type, DateTime endingTime)
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
            TimeSpan Day_Diff = endingTime.Subtract(startTime);      //计算时间差值
            int Weeks_Diff = (int)Day_Diff.TotalDays / 7 % 1024;     //计算周数差值，并基于1024周清零

            int WeekOfSec = (int)Day_Diff.TotalSeconds % (7 * 24 * 3600);     //计算 week of seconds

            int[] GPST = { Weeks_Diff, WeekOfSec };

            return GPST;
        }

        
    }
}
