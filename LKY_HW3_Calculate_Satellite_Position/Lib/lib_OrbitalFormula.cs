/*
 *      [Principle of Navigation] Copyright (C) 2022 SJTU Inc.
 *      
 *      FileName : lib_OrbitalFormula.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using LKY_Calculate_Satellite_Position.Common;
using System;
using static LKY_Calculate_Satellite_Position.Common.Log;
using static LKY_Calculate_Satellite_Position.Lib.lib_CoordinateOS;
using static LKY_Calculate_Satellite_Position.Lib.lib_YumaOS;
using static LKY_Calculate_Satellite_Position.Lib.lib_RinexOS;

namespace LKY_Calculate_Satellite_Position.Lib
{
    /// <summary>
    /// 计算卫星的轨道公式
    /// </summary>
    public class lib_OrbitalFormula
    {
        /// <summary>
        /// 定义:万有引力常数G与地球总质量的乘积
        /// </summary>
        public static readonly double GM = 3.986005 * Math.Pow(10, 14);


        /// <summary>
        /// 使用 Rinex 计算
        /// </summary>
        public class ByRinex
        {
            /// <summary>
            /// 重载，计算Rinex模式下的卫星位置
            /// </summary>
            /// <param name="PRN_NO">GPS卫星的编号</param>
            public ByRinex(int PRN_NO)
            {
                //先获取 Rinex 信息
                RinexInfo rinex = lib_RinexOS.IO.Get_Latest_PRN_PhyInfo(PRN_NO);

                new Log("启动 计算 Rinex PRN-" + PRN_NO.ToString() + " 卫星轨道信息 ......", Log.LogType.Calculate, ArrowType.Full);

                //计算平均角速度
                ///计算参考时刻平均角速度
                double n0 = Math.Sqrt(GM) / Math.Pow(rinex.sqrtA, 3);
                ///基于摄动改正数，得出观测时的修正后的平均角速度
                double n = n0 + rinex.Delta_n;
                new Log("平均角速度n      = " + n.ToString());

                //计算平近点角
                ///先计算时钟偏移量
                double detlaT = rinex.DeltaT_a0 + rinex.DeltaT_a1 * (rinex.TOE - rinex.TOC_GPST[1]) + rinex.DeltaT_a2 * Math.Pow((rinex.TOE - rinex.TOC_GPST[1]), 2);
                ///计算修正后的时间
                double T = rinex.TOE - detlaT;
                ///计算相对参考时间的差值
                double Tk = T - rinex.TOC_GPST[1];
                ///计算平近点角
                double M = rinex.M0 + n * Tk;
                new Log("平近点角M        = " + M.ToString());

                //计算偏近点角
                double E = 0;
                ///迭代收敛
                for (int loop = 1; loop <= new Random().Next(6, 10); loop++)
                {
                    E = M + rinex.e * Math.Sin(E);
                    string loop_info = "偏近点角E        ... 迭代第 " + loop.ToString() + " 次";
                    if (loop != 1)
                    {
                        loop_info = loop_info.Replace("偏近点角E", "         ");
                        new Log(loop_info, LogType.Display, ArrowType.NoneFormat);
                    }
                    else
                    {
                        new Log(loop_info);
                    }
                }
                new Log("偏近点角E        = " + E.ToString());

                //计算真近点角
                double Vk = (Math.Cos(E) - rinex.e) / (Math.Sin(E) * Math.Sqrt(1 - Math.Pow(rinex.e, 2)));
                new Log("真近点角Vk       = " + Vk.ToString());

                //计算升交距角
                double u = rinex.omega + Vk;
                new Log("升交距角u        = " + u.ToString());

                //计算摄动改正项
                ///升交距角摄动
                double Su = rinex.cuc * Math.Cos(2 * u) + rinex.cus * Math.Sin(2 * u);
                new Log("升交距角摄动Su   = " + Su.ToString());
                ///卫星矢径摄动
                double Sr = rinex.crc * Math.Cos(2 * u) + rinex.Crs * Math.Sin(2 * u);
                new Log("卫星矢径摄动Sr   = " + Sr.ToString());
                ///卫星轨道倾角摄动
                double Si = rinex.cic * Math.Cos(2 * u) + rinex.cis * Math.Sin(2 * u);
                new Log("轨道倾角摄动Si   = " + Si.ToString());

                //基于三个摄动项，计算实际值
                ///升交距角
                double Uk = u + Su;
                new Log("升交距角Uk       = " + Uk.ToString());
                ///辅助圆半径
                double Rk = Math.Pow(rinex.sqrtA, 2) * (1 - rinex.e * Math.Cos(E)) + Sr;        //卫星轨道长半径，用长半轴长来使用。
                new Log("辅助半径Rk       = " + Rk.ToString());
                ///卫星轨道倾角
                double Ik = rinex.i0 + Si + rinex.IDOT * Tk;
                new Log("轨道倾角Ik       = " + Ik.ToString());

                //计算卫星轨道的坐标系中的坐标                
                double X = Rk * Math.Cos(Uk);
                double Y = Rk * Math.Sin(Uk);
                new Log("轨道坐标(XOY)    = (" + X.ToString() + ", " + Y.ToString() + ")");

                new Log("计算 Rinex PRN-" + PRN_NO.ToString() + " 卫星椭圆轨道坐标，完成。\n", LogType.Message, ArrowType.Tick);

                //计算发射时刻的升交点经度
                ///定义地球自转角速度
                double We = 7.29211567 * Math.Pow(10, -5);
                double L = rinex.OMEGA + rinex.OMEGA_DOT * Tk - We * (Tk + rinex.TOE);      //减去坐标系的旋转、卫星的旋转
                new Log("发射时升交点经度L = " + L.ToString());

                //计算ECEF坐标
                double X_ecef = X * Math.Cos(L) - Y * Math.Cos(Ik) * Math.Sin(L);
                double Y_ecef = X * Math.Sin(L) + Y * Math.Cos(Ik) * Math.Cos(L);
                double Z_ecef = Y * Math.Sin(Ik);
                new Log("轨道坐标(ECEF)    = (" + X_ecef.ToString() + ", " + Y_ecef.ToString() + ", " + Z_ecef.ToString() + ")");
                new Log("计算 Rinex PRN-" + PRN_NO.ToString() + " 卫星ECEF坐标，完成。\n", LogType.Message, ArrowType.Tick);

                //计算LLA坐标，用于验证ECEF准确性
                double[] ECEF = { X_ecef, Y_ecef, Z_ecef };
                double[] LLA = ECEF2LLA(ECEF);
                new Log("轨道坐标(LLA)     = (" + LLA[0].ToString() + "°, " + LLA[1].ToString() + "°, " + LLA[2].ToString() + "m)");
                new Log("计算 Rinex PRN-" + PRN_NO.ToString() + " 卫星LLA坐标，完成。\n", LogType.Message, ArrowType.Tick);
            }
        }

        /// <summary>
        /// 使用 Yuma 计算
        /// </summary>
        public class ByYuma
        {
            /// <summary>
            /// 重载，计算Yuma模式下的卫星位置
            /// </summary>
            /// <param name="PRN_NO">GPS卫星的编号</param>
            public ByYuma(int PRN_NO)
            {
                //先获取 Yuma 信息
                YumaInfo yuma = lib_YumaOS.IO.Get_Latest_PRN_PhyInfo(PRN_NO);

                new Log("启动 计算 Yuma PRN-" + PRN_NO.ToString() + " 卫星轨道信息 ......", Log.LogType.Calculate, ArrowType.Full);

                //计算平均角速度
                ///计算参考时刻平均角速度
                double n0 = Math.Sqrt(GM) / Math.Pow(yuma.sqrtA, 3);
                
                /*///基于摄动改正数，得出观测时的修正后的平均角速度
                double n = n0 + yuma.Delta_n;*/
                new Log("平均角速度n      = " + n0.ToString());

                //计算平近点角
                ///先计算时钟偏移量
                double detlaT = yuma.DeltaT_a0 /*+ yuma.DeltaT_a1 * (yuma.TOE - yuma.TOC_GPST[1]) + yuma.DeltaT_a2 * Math.Pow((yuma.TOE - yuma.TOC_GPST[1]), 2)*/;
                /*///计算修正后的时间
                double T = yuma.TOE - detlaT;
                ///计算相对参考时间的差值
                double Tk = T - yuma.TOC_GPST[1];*/
                ///计算平近点角
                double M = yuma.M0 + n0 * detlaT;
                new Log("平近点角M        = " + M.ToString());

                //计算偏近点角
                double E = 0;
                ///迭代收敛
                for (int loop = 1; loop <= new Random().Next(6, 10); loop++)
                {
                    E = M + yuma.e * Math.Sin(E);
                    string loop_info = "偏近点角E        ... 迭代第 " + loop.ToString() + " 次";
                    if (loop != 1)
                    {
                        loop_info = loop_info.Replace("偏近点角E", "         ");
                        new Log(loop_info, LogType.Display, ArrowType.NoneFormat);
                    }
                    else
                    {
                        new Log(loop_info);
                    }
                }
                new Log("偏近点角E        = " + E.ToString());

                //计算真近点角
                double Vk = (Math.Cos(E) - yuma.e) / (Math.Sin(E) * Math.Sqrt(1 - Math.Pow(yuma.e, 2)));
                new Log("真近点角Vk       = " + Vk.ToString());

                //计算升交距角
                double u = yuma.omega + Vk;
                new Log("升交距角u        = " + u.ToString());

                //基于三个摄动项，计算实际值
                ///升交距角
                double Uk = u + 0;
                new Log("升交距角Uk       = " + Uk.ToString());
                ///辅助圆半径
                double Rk = Math.Pow(yuma.sqrtA, 2) * (1 - yuma.e * Math.Cos(E)) + 0;        //卫星轨道长半径，用长半轴长来使用。
                new Log("辅助半径Rk       = " + Rk.ToString());
                ///卫星轨道倾角
                double Ik = yuma.i0 /*+ Si + yuma.IDOT * Tk*/;
                new Log("轨道倾角Ik       = " + Ik.ToString());

                //计算卫星轨道的坐标系中的坐标                
                double X = Rk * Math.Cos(Uk);
                double Y = Rk * Math.Sin(Uk);
                new Log("轨道坐标(XOY)    = (" + X.ToString() + ", " + Y.ToString() + ")");

                new Log("计算 Yuma PRN-" + PRN_NO.ToString() + " 卫星椭圆轨道坐标，完成。\n", LogType.Message, ArrowType.Tick);

                //计算发射时刻的升交点经度
                ///定义地球自转角速度
                double We = 7.29211567 * Math.Pow(10, -5);
                double L = yuma.OMEGA + yuma.OMEGA_DOT * n0 - We * (n0 /*+ yuma.TOE*/);      //减去坐标系的旋转、卫星的旋转
                new Log("发射时升交点经度L = " + L.ToString());

                //计算ECEF坐标
                double X_ecef = X * Math.Cos(L) - Y * Math.Cos(Ik) * Math.Sin(L);
                double Y_ecef = X * Math.Sin(L) + Y * Math.Cos(Ik) * Math.Cos(L);
                double Z_ecef = Y * Math.Sin(Ik);
                new Log("轨道坐标(ECEF)    = (" + X_ecef.ToString() + ", " + Y_ecef.ToString() + ", " + Z_ecef.ToString() + ")");
                new Log("计算 Yuma PRN-" + PRN_NO.ToString() + " 卫星ECEF坐标，完成。\n", LogType.Message, ArrowType.Tick);

                //计算LLA坐标，用于验证ECEF准确性
                double[] ECEF = { X_ecef, Y_ecef, Z_ecef };
                double[] LLA = ECEF2LLA(ECEF);
                new Log("轨道坐标(LLA)     = (" + LLA[0].ToString() + "°, " + LLA[1].ToString() + "°, " + LLA[2].ToString() + "m)");
                new Log("计算 Yuma PRN-" + PRN_NO.ToString() + " 卫星LLA坐标，完成。\n", LogType.Message, ArrowType.Tick);

            }
        }
    }
}
