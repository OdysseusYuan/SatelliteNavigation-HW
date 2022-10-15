/*
 *      [Principle of Navigation] Copyright (C) 2022 SJTU Inc.
 *      
 *      FileName : lib_YumaOS.cs
 *      Developer: liukaiyuan@sjtu.edu.cn (Odysseus.Yuan)
 */

using LKY_Calculate_Satellite_Position.Common;
using System;
using System.IO;
using System.Text;
using static LKY_Calculate_Satellite_Position.Common.Log;

namespace LKY_Calculate_Satellite_Position.Lib
{
    /// <summary>
    /// 对 Yuma 文件读写的类库
    /// </summary>
    public class lib_YumaOS
    {
        /// <summary>
        /// Yuma 的结构体，用于存储各种物理量信息。
        /// </summary>
        public struct YumaInfo
        {
            /// <summary>
            /// 轨道偏心率
            /// </summary>
            public double e;

            /// <summary>
            /// 历书基准时间（周内秒）
            /// </summary>
            public int WOS;

            /// <summary>
            /// 参考时刻的轨道倾角
            /// </summary>
            public double i0;

            /// <summary>
            /// 升交点赤经变化率
            /// </summary>
            public double OMEGA_DOT;

            /// <summary>
            /// 轨道长半轴的平方根
            /// </summary>
            public double sqrtA;

            /// <summary>
            /// 参考时刻的升交点赤经
            /// </summary>
            public double OMEGA;

            /// <summary>
            /// 近地点角距
            /// </summary>
            public double omega;

            /// <summary>
            /// 参考时刻的平近点角
            /// </summary>
            public double M0;

            /// <summary>
            /// 卫星钟差
            /// </summary>
            public double DeltaT_a0;

            /// <summary>
            /// 卫星钟速
            /// </summary>
            public double DeltaT_a1;

            /// <summary>
            /// GPS周数
            /// </summary>
            public int week;
        }


        /// <summary>
        /// 读取文本类
        /// </summary>
        public class IO
        {
            /// <summary>
            /// Yuma 文件名
            /// </summary>
            const string Yuma_filename = "almanac.yuma.week0182.061440.txt";        //almanac.yuma.week0182.061440.txt      PRN-11_Yuma_OnlyTest.txt

            /// <summary>
            /// 读取 Yuma 文件
            /// </summary>
            /// <returns></returns>
            private static string Reading()
            {
                string Yuma_Filepath = Environment.CurrentDirectory + "\\Attachments\\" + Yuma_filename;
                if (File.Exists(Yuma_Filepath))
                {
                    new Log($"读取 Yuma 文件 {Yuma_filename}，路径：" + Yuma_Filepath.Replace(Environment.CurrentDirectory, "") + " ......", Log.LogType.Calculate, ArrowType.Full);
                    string file_info = File.ReadAllText(Yuma_Filepath, Encoding.UTF8);
                    //new Log("读取 brdc2750.22n，" + "完成。");
                    return file_info;
                }
                else
                {
                    new Log($"{Yuma_filename} 文件不存在，预期路径：" + Yuma_Filepath.Replace(Environment.CurrentDirectory, "") + "，请拷贝对应文件到该路径！", LogType.Error);
                    return null;
                }
            }

            /// <summary>
            /// 获取不同 PRN 编号的历书信息（原始信息）
            /// </summary>
            /// <returns></returns>
            private static string Get_Latest_PRN_GlobalInfo(int PRN_Number)
            {
                string Yuma_Context = Reading();                                       //获取全部Yuma文件内容

                //文件不存在时，返回
                if (string.IsNullOrEmpty(Yuma_Context))
                {
                    return null;
                }

                new Log($"读取 Yuma 文件 {Yuma_filename}，完成。\n", LogType.Message, ArrowType.Tick);

                //判断是否是Yuma标准格式
                if (Yuma_Context.Contains("almanac for"))
                {
                    //将综合矩阵信息拆分为每行
                    string[] Yuma_Context_Arr = Yuma_Context.Split('\n');

                    string result = "";                 //获得的最终满足条件的PRN序号的信息
                    string now_mar = "";                //建立一个15行矩阵值
                    for (int now_mar_number = 0; now_mar_number < Yuma_Context_Arr.Length; now_mar_number++)      //只要获取的报文组编号小于总组数，就遍历查找
                    {
                        //每次叠加新一行读取的结果
                        now_mar = now_mar + Yuma_Context_Arr[now_mar_number] + "\n";

                        //每15行，记为一个矩阵
                        if ((now_mar_number + 1) % 15 == 0)
                        {
                            //如果是查找的PRN序号，则暂存，随着每次遍历，获得最后一次查到的PRN信息
                            int now_prn_ID_index = now_mar.IndexOf("almanac for PRN-");
                            int now_prn_ID = int.Parse(now_mar.Substring(now_prn_ID_index + "almanac for PRN-".Length, 2));     //截取获得ID号

                            if (PRN_Number == now_prn_ID)
                            {
                                result = now_mar;
                            }
                            now_mar = "";               //当前矩阵已达到15行，清空，记录下一个。
                        }
                    }

                    //new Log("PRN-" + PRN_Number.ToString() + " 最新矩阵信息：\n" + result);

                    return result;
                }
                else
                {
                    new Log($"{Yuma_filename} 文件标头为非标准格式，请使用正确的文件格式！", LogType.Error);
                    return null;
                }
            }

            /// <summary>
            /// 用于将 Yuma 的特殊数字表示法 转换为 C 支持的浮点
            /// </summary>
            /// <param name="YumaMath"></param>
            /// <returns></returns>
            private static double YumaMath2Double(string YumaMath)
            {
                double result;

                //先判断是否存在历书专用的 E±xxx 字符
                if (YumaMath.Contains("E"))
                {
                    //存在E进行转换
                    //先截断后面5个字符，即：E±xxx
                    result = double.Parse(YumaMath.Substring(0, YumaMath.Length - 5));

                    //分割E元素得到10的正负x次方
                    int Ex = int.Parse(YumaMath.Split('E')[1]);

                    //计算最终真实的浮点值
                    result = result * Math.Pow(10, Ex);
                }
                else
                {
                    //不存在E的时候，直接返回原值
                    result = double.Parse(YumaMath);
                }

                return result;
            }

            /// <summary>
            /// 通过输入Yuma键，获得对应的值
            /// 本质原理：取中间文本。
            /// </summary>
            /// <param name="Key"></param>
            /// <param name="full_str">原始文本</param>
            /// <returns></returns>
            private static string GetYumaValue(string Key, string full_str)
            {
                if (full_str.Contains(Key))
                {
                    //如果存在要查询的键，则继续
                    int start_index = full_str.IndexOf(Key);                                           //先找到Key所在index起始位置
                    string right_content = full_str.Substring(start_index + Key.Length);               //截断后，获得Key之后（不含Key）右侧的全部内容
                    string value_str = right_content.Split('\n')[0];                                   //使用换行符分割为数组，取第0个值，即为初步value
                    string result = value_str.Replace(" ", "").Replace(":", "").Replace("\r", "");      //替换冒号、空格、光标移位符之后得到最终的值
                    return result;
                }
                else
                {
                    //不存在键，返回null
                    return null;
                }
            }


            /// <summary>
            /// 获取 PRN 特定序号最新的物理信息（已处理过的结构体值）
            /// </summary>
            /// <param name="PRN_Number"></param>
            /// <returns></returns>
            public static YumaInfo Get_Latest_PRN_PhyInfo(int PRN_Number)
            {
                //先获取最新的 PRN 需要序号的矩阵信息。
                string PRN_Info = Get_Latest_PRN_GlobalInfo(PRN_Number);

                new Log("解析 Yuma PRN-" + PRN_Number.ToString() + " 数据 ......", Log.LogType.Calculate, ArrowType.Full);

                //赋值拆解的物理量
                YumaInfo yuma_info;

                //轨道偏心率
                yuma_info.e = YumaMath2Double(GetYumaValue("Eccentricity", PRN_Info));
                new Log("获得 轨道偏心率e               = " + yuma_info.e.ToString());

                //历书基准时间（周内秒）
                yuma_info.week = (int)YumaMath2Double(GetYumaValue("week", PRN_Info));
                yuma_info.WOS = (int)YumaMath2Double(GetYumaValue("Time of Applicability(s)", PRN_Info));
                new Log("获得 历书基准时间WO            = " + $"{yuma_info.week}w, {yuma_info.WOS}s");

                //参考时刻的轨道倾角
                yuma_info.i0 = YumaMath2Double(GetYumaValue("Orbital Inclination(rad)", PRN_Info));
                new Log("获得 参考时刻的轨道倾角i0      = " + yuma_info.i0.ToString());

                //升交点赤经变化率
                yuma_info.OMEGA_DOT = YumaMath2Double(GetYumaValue("Rate of Right Ascen(r/s)", PRN_Info));
                new Log("获得 升交点赤经变化率OMEGA_DOT = " + yuma_info.OMEGA_DOT.ToString());

                //轨道长半轴的平方根
                yuma_info.sqrtA = YumaMath2Double(GetYumaValue("SQRT(A)  (m 1/2)", PRN_Info));
                new Log("获得 轨道长半轴的平方根sqrtA   = " + yuma_info.sqrtA.ToString());

                //参考时刻的升交点赤经
                yuma_info.OMEGA = YumaMath2Double(GetYumaValue("Right Ascen at Week(rad)", PRN_Info));
                new Log("获得 参考时刻的升交点赤经OMEGA = " + yuma_info.OMEGA.ToString());

                //近地点角距
                yuma_info.omega = YumaMath2Double(GetYumaValue("Argument of Perigee(rad)", PRN_Info));
                new Log("获得 近地点角距omega           = " + yuma_info.omega.ToString());

                //参考时刻的平近点角
                yuma_info.M0 = YumaMath2Double(GetYumaValue("Mean Anom(rad)", PRN_Info));
                new Log("获得 参考时刻的平近点角M0      = " + yuma_info.M0.ToString());

                //卫星钟差
                yuma_info.DeltaT_a0 = YumaMath2Double(GetYumaValue("Af0(s)", PRN_Info));
                new Log("获得 卫星钟差DeltaT_a0         = " + yuma_info.DeltaT_a0.ToString());

                //卫星钟速
                yuma_info.DeltaT_a1 = YumaMath2Double(GetYumaValue("Af1(s/s)", PRN_Info));
                new Log("获得 卫星钟速DeltaT_a1         = " + yuma_info.DeltaT_a1.ToString());

                new Log("解析 Yuma PRN-" + PRN_Number.ToString() + " 数据，完成。\n", LogType.Message, ArrowType.Tick);

                return yuma_info;
            }

        }
    }
}
