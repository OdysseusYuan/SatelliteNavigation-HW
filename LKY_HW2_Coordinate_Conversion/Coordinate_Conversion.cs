/*
 *      [Principle of Navigation] Copyright (C) 2022 SJTU Inc.
 *      
 *      FileName : Coordinate_Conversion.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using System;

namespace LKY_Coordinate_Conversion
{
    internal class Coordinate_Conversion
    {
        static void Main(string[] args)
        {
            //欢迎话术
            Console.WriteLine("******** Homework #2: Coordinate Conversion  ********");
            Console.WriteLine("******** Developer: liukaiyuan@sjtu.edu.cn   ********");
            Console.WriteLine("******** Student Number: 122413930100        ********");
            Console.WriteLine();

            // -------------- Coordinate Conversion Begin --------------

            Console.WriteLine("※※※ Part1: Coordinate conversion: find a point A on SJTU campus, marked on the Baidu/Google " +
                "Map to obtain the geodetic coordinate(l,l,h), and convert it to ECEF, and ECI (optional).\n");
            //定义交大空天院LLA
            Coordinate.LLA sjtuA_lla;
            sjtuA_lla.longitude = 121.44124568;
            sjtuA_lla.latitude = 31.02546975;
            sjtuA_lla.altitude = 5.26;

            Console.WriteLine("选取位置A：上海交通大学（闵行校区）空天学院 门口：");
            Console.WriteLine("----- > " + Coordinate.Type.LLA.ToString() + "  坐标：("
                + sjtuA_lla.longitude.ToString() + ", " + sjtuA_lla.latitude.ToString() + ", " + sjtuA_lla.altitude.ToString() + ")");


            //转为ECEF坐标
            Coordinate.ECEF sjtuA_ecef = Coordinate.LLA2ECEF(sjtuA_lla);
            Console.WriteLine("----- > " + Coordinate.Type.ECEF.ToString() + " 坐标：("
                + sjtuA_ecef.x + ", " + sjtuA_ecef.y + ", " + sjtuA_ecef.z + ")");
            ///ECEF基准值：-2857998.38, -1718988.29, 5418464.12

            //转为ENU坐标(以天安门为基准)
            Coordinate.LLA tam_lla;
            tam_lla.longitude = 116.39127910;
            tam_lla.latitude = 39.90713686;
            tam_lla.altitude = 49.81;

            Coordinate.ENU sjtuA_enu = Coordinate.ECEF2ENU(tam_lla, sjtuA_ecef);
            Console.WriteLine("----- > " + Coordinate.Type.ENU.ToString() + "  坐标：("
                + sjtuA_enu.East + ", " + sjtuA_enu.North + ", " + sjtuA_enu.Up + ") BY 天安门");
            Console.WriteLine("----- > " + Coordinate.Type.ENU.ToString() + "  距离："
                + (Coordinate.GetVectorLen(sjtuA_enu.East, sjtuA_enu.North, sjtuA_enu.Up)) + " m");



            Console.WriteLine("\n\n※※※ Part2: Given a point B (121.455899°, 31.036321°，100m), " +
                "calculate the ENU of point B relative to your own Point A. Plot the skymap marked with  elevation and azimuth. \n");
            Console.WriteLine("转换位置B：(121.455899°, 31.036321°，100m)：");

            //定义B点LLA坐标
            Coordinate.LLA sjtuB_lla;
            sjtuB_lla.longitude = 121.455899;
            sjtuB_lla.latitude = 31.036321;
            sjtuB_lla.altitude = 100d;

            Coordinate.ENU sjtuB_enu = Coordinate.ECEF2ENU(sjtuA_lla, Coordinate.LLA2ECEF(sjtuB_lla));        //以A为目标，获取enu
            Console.WriteLine("----- > " + Coordinate.Type.ENU.ToString() + " 坐标：("
                + sjtuB_enu.East + ", " + sjtuB_enu.North + ", " + sjtuB_enu.Up + ") BY 位置A");
            Console.WriteLine("----- > " + Coordinate.Type.ENU.ToString() + " 距离："
                + (Coordinate.GetVectorLen(sjtuB_enu.East, sjtuB_enu.North, sjtuB_enu.Up)) + " m");

            //获取SkyMap
            double az = Math.Atan(sjtuB_enu.East / sjtuB_enu.North);
            double el = Math.Asin(sjtuB_enu.Up / Math.Sqrt(Math.Pow(sjtuB_enu.East, 2) + Math.Pow(sjtuB_enu.North, 2) + Math.Pow(sjtuB_enu.Up, 2)));
            Console.WriteLine("----- > " + "SkyMap  ：(" + "az: "
                + az + "°, " + "el: " + el + "°)");


            // -------------- Coordinate Conversion End --------------

            Console.WriteLine("\n\nDone. 可按任意键退出运行。");
            Console.ReadKey();
        }



    }

    /// <summary>
    /// 不同坐标系的定义
    /// </summary>
    class Coordinate
    {
        /// <summary>
        /// 计算向量模长
        /// </summary>
        /// <returns></returns>
        public static double GetVectorLen(double x, double y, double z)
        {
            double result = Math.Sqrt(x * x + y * y + z * z);

            return result;
        }

        /// <summary>
        /// LLA转ECEF
        /// </summary>
        /// <param name="lla"></param>
        /// <returns></returns>
        public static ECEF LLA2ECEF(LLA lla)
        {
            double a = 6378137.0;                //椭球体长半径
            double f = 1.0 / 298.257223565;      //极扁率

            double e2 = f * (2 - f);            //计算e²

            //弧度制转换，非常重要
            double deg2rad = Math.PI / 180.0;
            double lat = lla.latitude * deg2rad;
            double lon = lla.longitude * deg2rad;

            double N = a / Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(lat), 2));   //计算N

            ECEF result;
            result.x = (N + lla.altitude) * Math.Cos(lat) * Math.Cos(lon);
            result.y = (N + lla.altitude) * Math.Cos(lat) * Math.Sin(lon);
            result.z = (N * (1 - e2) + lla.altitude) * Math.Sin(lat);

            return result;
        }

        /// <summary>
        /// ECEF转ENU
        /// 基于 byFromLLA 为基准点，将目标点的 ecef 转换为 ENU坐标
        /// </summary>
        /// <param name="ecef"></param>
        /// <returns></returns>
        public static ENU ECEF2ENU(LLA byFromLLA, ECEF ecef)
        {
            double deltaX = ecef.x - LLA2ECEF(byFromLLA).x;
            double deltaY = ecef.y - LLA2ECEF(byFromLLA).y;
            double deltaZ = ecef.z - LLA2ECEF(byFromLLA).z;

            ///Console.WriteLine(deltaX.ToString() + ", " + deltaY.ToString() + ", " + deltaZ.ToString());

            double lon = byFromLLA.longitude;
            double lat = byFromLLA.latitude;


            ENU result;
            result.East = -Math.Sin(lon) * deltaX + Math.Cos(lon) * deltaY;
            result.North = -Math.Sin(lat) * Math.Cos(lon) * deltaX - Math.Sin(lat) * Math.Sin(lon) * deltaY + Math.Cos(lat) * deltaZ;
            result.Up = Math.Cos(lat) * Math.Cos(lon) * deltaX + Math.Cos(lat) * Math.Sin(lon) * deltaY + Math.Sin(lat) * deltaZ;

            return result;
        }

        /// <summary>
        /// 坐标系类型
        /// </summary>
        public enum Type
        {
            LLA,
            ECEF,
            ENU,
            ECI
        }

        /// <summary>
        /// LLA坐标系信息
        /// </summary>
        public struct LLA
        {
            public double longitude;  //经度
            public double latitude;   //纬度
            public double altitude;   //高度
        }

        /// <summary>
        /// ECEF坐标系信息
        /// </summary>
        public struct ECEF
        {
            public double x;
            public double y;
            public double z;
        }

        /// <summary>
        /// ENU坐标系信息
        /// </summary>
        public struct ENU
        {
            public double East;
            public double North;
            public double Up;
        }
    }
}
