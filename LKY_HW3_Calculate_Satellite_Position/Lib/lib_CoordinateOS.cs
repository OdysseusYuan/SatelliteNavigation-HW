/*
 *      [Principle of Navigation] Copyright (C) 2022 SJTU Inc.
 *      
 *      FileName : lib_CoordinateOS.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using System;

namespace LKY_Calculate_Satellite_Position.Lib
{
    public class lib_CoordinateOS
    {
        /// <summary>
        /// ECEF转LLA
        /// </summary>
        /// <param name="ecef"></param>
        /// <returns></returns>
        public static double[] ECEF2LLA(double[] ecef)
        {
            double a = 6378137;             //半径
            double e = 8.1819190842622e-2;  // 离心率

            double a2 = Math.Pow(a, 2);
            double e2 = Math.Pow(e, 2);

            double x = ecef[0];
            double y = ecef[1];
            double z = ecef[2];

            double b = Math.Sqrt(a2 * (1 - e2));
            double b2 = Math.Pow(b, 2);
            double ep = Math.Sqrt((a2 - b2) / b2);
            double p = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            double th = Math.Atan2(a * z, b * p);

            double lon = Math.Atan2(y, x);
            double lat = Math.Atan2((z + Math.Pow(ep, 2) * b * Math.Pow(Math.Sin(th), 3)), (p - e2 * a * Math.Pow(Math.Cos(th), 3)));
            double N = a / (Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(lat), 2)));
            double alt = p / Math.Cos(lat) - N;

            lon = lon % (2 * Math.PI);

            double[] ret = { lat * 180 / Math.PI, lon * 180 / Math.PI, alt };       //转换为角度制

            return ret;
        }
    }
}
