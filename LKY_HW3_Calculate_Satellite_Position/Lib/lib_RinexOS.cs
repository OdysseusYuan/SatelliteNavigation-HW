/*
 *      [Principle of Navigation] Copyright (C) 2022 SJTU Inc.
 *      
 *      FileName : lib_RinexOS.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using LKY_Calculate_Satellite_Position.Common;
using System;
using System.IO;
using System.Text;
using static LKY_Calculate_Satellite_Position.Common.Log;
using static LKY_Calculate_Satellite_Position.Lib.lib_TimeCovert;

namespace LKY_Calculate_Satellite_Position.Lib
{
    /// <summary>
    /// 对 Rinex 文件读写的类库
    /// </summary>
    public class lib_RinexOS
    {
        /// <summary>
        /// Rinex 的结构体，用于存储各种物理量信息。
        /// </summary>
        public struct RinexInfo
        {
            //时钟时间
            /// <summary>
            /// TOC_GPST格式的时间，weeks + week of seconds
            /// </summary>
            public int[] TOC_GPST;
            public int TOC_year;
            public int TOC_month;
            public int TOC_day;
            public int TOC_hour;
            public int TOC_minute;
            public float TOC_second;

            /// <summary>
            /// 卫星钟差
            /// </summary>
            public double DeltaT_a0;

            /// <summary>
            /// 卫星 钟速
            /// </summary>
            public double DeltaT_a1;

            /// <summary>
            /// 卫星 钟漂
            /// </summary>
            public double DeltaT_a2;

            //第2行每列开始
            /// <summary>
            /// 数据龄期
            /// </summary>
            public double IODE;

            /// <summary>
            /// 由精密星历计算得到的卫星平均角速度
            /// </summary>
            public double Crs;

            /// <summary>
            /// 按给定参数计算所得的平均角度速度之差
            /// </summary>
            public double Delta_n;

            /// <summary>
            /// 参考时刻的平近点角
            /// </summary>
            public double M0;

            //第3行每列开始

            public double cuc;

            /// <summary>
            /// 轨道偏心率
            /// </summary>
            public double e;


            public double cus;

            /// <summary>
            /// 轨道长半轴的平方根
            /// </summary>
            public double sqrtA;


            //第4行每列开始
            /// <summary>
            /// 星历参考时刻
            /// </summary>
            public double TOE;


            public double cic;

            /// <summary>
            /// 参考时刻的升交点赤经
            /// </summary>
            public double OMEGA;


            public double cis;

            //第5行每列开始
            /// <summary>
            /// 参考时刻的轨道倾角
            /// </summary>
            public double i0;


            public double crc;

            /// <summary>
            /// 近地点角距
            /// </summary>
            public double omega;

            /// <summary>
            /// 升交点赤经变化率
            /// </summary>
            public double OMEGA_DOT;


            //第6行每列开始
            /// <summary>
            /// 轨道倾角变化率
            /// </summary>
            public double IDOT;

        }


        /// <summary>
        /// 读取文本类
        /// </summary>
        public class IO
        {
            /// <summary>
            /// Rinex 文件名
            /// </summary>
            const string Rinex_filename = "brdc2750.22n";       //brdc2750.22n   PRN-11_Rinex_OnlyTest.txt

            /// <summary>
            /// 读取 Rinex 文件
            /// </summary>
            /// <returns></returns>
            private static string Reading()
            {
                string Rinex_Filepath = Environment.CurrentDirectory + $"\\Attachments\\{Rinex_filename}";
                if (File.Exists(Rinex_Filepath))
                {
                    new Log($"读取 Rinex 文件 {Rinex_filename}，路径：" + Rinex_Filepath.Replace(Environment.CurrentDirectory, "") + " ......", Log.LogType.Calculate, ArrowType.Full);
                    string file_info = File.ReadAllText(Rinex_Filepath, Encoding.UTF8);
                    //new Log("读取 brdc2750.22n，" + "完成。");
                    return file_info;
                }
                else
                {
                    new Log($"{Rinex_filename} 文件不存在，预期路径：" + Rinex_Filepath.Replace(Environment.CurrentDirectory, "") + "，请拷贝对应文件到该路径！", LogType.Error);
                    return null;
                }
            }

            /// <summary>
            /// 获取不同 PRN 编号的历书信息（原始信息）
            /// </summary>
            /// <returns></returns>
            private static string Get_Latest_PRN_GlobalInfo(int PRN_Number)
            {
                string Rinex_Context = Reading();                                       //获取全部Rinex文件内容

                //文件不存在时，返回
                if (string.IsNullOrEmpty(Rinex_Context))
                {
                    return null;
                }

                new Log($"读取 Rinex 文件 {Rinex_filename}，完成。\n", LogType.Message, ArrowType.Tick);

                int Header_Ending_Index = Rinex_Context.IndexOf("END OF HEADER");       //找到历书头文件截止的位置
                if (Header_Ending_Index > -1)
                {
                    Rinex_Context = Rinex_Context.Remove(0, Header_Ending_Index + "END OF HEADER       \n".Length + 1);     //获得综合矩阵信息

                    //将综合矩阵信息拆分为每行
                    string[] Rinex_Context_Arr = Rinex_Context.Split('\n');

                    string result = "";                 //获得的最终满足条件的PRN序号的信息
                    string now_mar = "";                //建立一个8行矩阵值
                    for (int now_mar_number = 0; now_mar_number < Rinex_Context_Arr.Length; now_mar_number++)      //只要获取的报文组编号小于总组数，就遍历查找
                    {
                        //每次叠加新一行读取的结果
                        now_mar = now_mar + Rinex_Context_Arr[now_mar_number] + "\n";

                        //每8行，记为一个矩阵
                        if ((now_mar_number + 1) % 8 == 0)
                        {
                            //如果是查找的PRN序号，则暂存，随着每次遍历，获得最后一次查到的PRN信息
                            if (PRN_Number == int.Parse(now_mar.Substring(0, 2)))
                            {
                                result = now_mar;
                            }
                            now_mar = "";               //当前矩阵已达到8行，清空，记录下一个。
                        }
                    }

                    //new Log("PRN-" + PRN_Number.ToString() + " 最新矩阵信息：\n" + result);

                    return result;
                }
                else
                {
                    new Log($"{Rinex_filename} 文件标头为非标准格式，请使用正确的文件格式！", LogType.Error);
                    return null;
                }
            }

            /// <summary>
            /// 用于将 Rinex 的特殊数字表示法 转换为 C 支持的浮点
            /// </summary>
            /// <param name="RinexMath">RinexMath数值必须是19位 ±x.xxxxxxxxD±xx 的格式</param>
            /// <returns></returns>
            private static double RinexMath2Double(string RinexMath)
            {
                //先截断后面四个字符，即：D±xx
                double result = double.Parse(RinexMath.Substring(0, RinexMath.Length - 4));

                //分割D元素得到10的正负x次方
                int Ex = int.Parse(RinexMath.Split('D')[1]);

                //计算最终真实的浮点值
                result = result * Math.Pow(10, Ex);

                return result;
            }


            /// <summary>
            /// 获取 PRN 特定序号最新的物理信息（已处理过的结构体值）
            /// </summary>
            /// <param name="PRN_Number"></param>
            /// <returns></returns>
            public static RinexInfo Get_Latest_PRN_PhyInfo(int PRN_Number)
            {
                //先获取最新的 PRN 需要序号的矩阵信息。
                string PRN_Info = Get_Latest_PRN_GlobalInfo(PRN_Number);

                string[] prn_info = PRN_Info.Split('\n');   //拆分为每行单独计算

                new Log("解析 Rinex PRN-" + PRN_Number.ToString() + " 数据 ......", Log.LogType.Calculate, ArrowType.Full);

                //赋值拆解的物理量
                RinexInfo rinex_info;

                //时钟时间
                rinex_info.TOC_year = int.Parse(prn_info[0].Substring(3, 2)) + 2000;
                rinex_info.TOC_month = int.Parse(prn_info[0].Substring(6, 2));
                rinex_info.TOC_day = int.Parse(prn_info[0].Substring(9, 2));
                rinex_info.TOC_hour = int.Parse(prn_info[0].Substring(12, 2));
                rinex_info.TOC_minute = int.Parse(prn_info[0].Substring(15, 2));
                rinex_info.TOC_second = float.Parse(prn_info[0].Substring(18, 4));

                //标准化生成一个TOC时间
                rinex_info.TOC_GPST = Get_GNSS_Time(GNSS_Type.GPS, new DateTime(rinex_info.TOC_year, rinex_info.TOC_month, rinex_info.TOC_day,
                    rinex_info.TOC_hour, rinex_info.TOC_minute, (int)rinex_info.TOC_second));
                new Log("获得 时间TOC（GPST格式） =       " + rinex_info.TOC_GPST[0].ToString() + "w, " + rinex_info.TOC_GPST[1].ToString() + "s");

                //卫星钟差
                rinex_info.DeltaT_a0 = RinexMath2Double(prn_info[0].Substring(22, 19));
                new Log("获得 卫星钟差DeltaT_a0 =         " + rinex_info.DeltaT_a0.ToString());

                //卫星钟速
                rinex_info.DeltaT_a1 = RinexMath2Double(prn_info[0].Substring(41, 19));
                new Log("获得 卫星钟速DeltaT_a1 =         " + rinex_info.DeltaT_a1.ToString());

                //卫星钟漂
                rinex_info.DeltaT_a2 = RinexMath2Double(prn_info[0].Substring(60, 19));
                new Log("获得 卫星钟漂DeltaT_a2 =         " + rinex_info.DeltaT_a2.ToString());

                //第2行每列开始
                //数据龄期
                rinex_info.IODE = RinexMath2Double(prn_info[1].Substring(3, 19));
                new Log("获得 数据龄期IODE =              " + rinex_info.IODE.ToString());

                rinex_info.Crs = RinexMath2Double(prn_info[1].Substring(22, 19));
                new Log("获得 crs =                       " + rinex_info.Crs.ToString());

                //按给定参数计算所得的平均角度速度之差
                rinex_info.Delta_n = RinexMath2Double(prn_info[1].Substring(41, 19));
                new Log("获得 平均角度速度之差Delta_n =   " + rinex_info.Delta_n.ToString());

                //参考时刻的平近点角
                rinex_info.M0 = RinexMath2Double(prn_info[1].Substring(60, 19));
                new Log("获得 参考时刻的平近点角M0 =      " + rinex_info.M0.ToString());


                //第3行每列开始
                rinex_info.cuc = RinexMath2Double(prn_info[2].Substring(3, 19));
                new Log("获得 cuc =                       " + rinex_info.cuc.ToString());

                //轨道偏心率
                rinex_info.e = RinexMath2Double(prn_info[2].Substring(22, 19));
                new Log("获得 轨道偏心率e =               " + rinex_info.e.ToString());

                rinex_info.cus = RinexMath2Double(prn_info[2].Substring(41, 19));
                new Log("获得 cus =                       " + rinex_info.cus.ToString());

                //轨道长半轴的平方根
                rinex_info.sqrtA = RinexMath2Double(prn_info[2].Substring(60, 19));
                new Log("获得 轨道长半轴的平方根sqrtA =   " + rinex_info.sqrtA.ToString());


                //第4行每列开始
                //星历参考时刻
                rinex_info.TOE = RinexMath2Double(prn_info[3].Substring(3, 19));
                new Log("获得 星历参考时刻TOE =           " + rinex_info.TOE.ToString());

                rinex_info.cic = RinexMath2Double(prn_info[3].Substring(22, 19));
                new Log("获得 cic =                       " + rinex_info.cic.ToString());

                //参考时刻的升交点赤经
                rinex_info.OMEGA = RinexMath2Double(prn_info[3].Substring(41, 19));
                new Log("获得 参考时刻的升交点赤经OMEGA = " + rinex_info.OMEGA.ToString());

                rinex_info.cis = RinexMath2Double(prn_info[3].Substring(60, 19));
                new Log("获得 cis =                       " + rinex_info.cis.ToString());


                //第5行每列开始
                //参考时刻的轨道倾角
                rinex_info.i0 = RinexMath2Double(prn_info[4].Substring(3, 19));
                new Log("获得 参考时刻的轨道倾角i0 =      " + rinex_info.i0.ToString());

                rinex_info.crc = RinexMath2Double(prn_info[4].Substring(22, 19));
                new Log("获得 crc =                       " + rinex_info.crc.ToString());

                //近地点角距
                rinex_info.omega = RinexMath2Double(prn_info[4].Substring(41, 19));
                new Log("获得 近地点角距omega =           " + rinex_info.omega.ToString());

                //升交点赤经变化率
                rinex_info.OMEGA_DOT = RinexMath2Double(prn_info[4].Substring(60, 19));
                new Log("获得 升交点赤经变化率OMEGA_DOT = " + rinex_info.OMEGA_DOT.ToString());


                //第6行每列开始
                //轨道倾角变化率
                rinex_info.IDOT = RinexMath2Double(prn_info[5].Substring(3, 19));
                new Log("获得 轨道倾角变化率IDOT =        " + rinex_info.IDOT.ToString());

                new Log("解析 Rinex PRN-" + PRN_Number.ToString() + " 数据，完成。\n", LogType.Message, ArrowType.Tick);

                return rinex_info;
            }
        }
    }
}
