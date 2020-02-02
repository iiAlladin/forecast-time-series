using System;
using System.IO;
using System.Xml;
using System.Data;
using System.Drawing;
using System.Security;
using System.Windows.Forms.DataVisualization.Charting;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForecastTimeSeries {

    internal class Statistic {
        // Methods
        public static void ComputeAutocorrelation(List<double> series, out List<double> listAutocorrelation)
        {
            listAutocorrelation = new List<double>();
            int num = ((series.Count / 4) > 50) ? (series.Count / 4) : 50;
            double num2 = 0.0;
            for (int i = 0; i < series.Count; i++)
            {
                num2 += series[i];
            }
            num2 /= (double)series.Count;
            double num4 = 0.0;
            for (int j = 0; j < series.Count; j++)
            {
                num4 += Math.Pow(series[j] - num2, 2.0);
            }
            num4 /= (double)series.Count;
            listAutocorrelation.Add(1.0);
            int num6 = 1;
            while (num6 < num)
            {
                double item = 0.0;
                int num8 = 0;
                while (true)
                {
                    if (num8 >= (series.Count - num6))
                    {
                        item /= series.Count * num4;
                        listAutocorrelation.Add(item);
                        num6++;
                        break;
                    }
                    item += (series[num8] - num2) * (series[num8 + num6] - num2);
                    num8++;
                }
            }
        }

        public static void ComputeAutocorrelation(List<double> series, int startIndex, out List<double> listAutocorrelation)
        {
            List<double> list = new List<double>();
            for (int i = startIndex; i < series.Count; i++)
            {
                list.Add(series[i]);
            }
            ComputeAutocorrelation(list, out listAutocorrelation);
        }

        public static void ComputeConfidenceLimit(List<double> listAutocorrelation, int dataSize, out List<double> listConfidenceLimit)
        {
            listConfidenceLimit = new List<double>();
            int num = 0;
            while (num < listAutocorrelation.Count)
            {
                double item = 0.0;
                int num3 = 0;
                while (true)
                {
                    if (num3 >= num)
                    {
                        item = Math.Sqrt((1.0 + (2.0 * item)) / ((double)dataSize));
                        listConfidenceLimit.Add(item);
                        num++;
                        break;
                    }
                    item += Math.Pow(listAutocorrelation[num3], 2.0);
                    num3++;
                }
            }
        }

        public static void ComputeDifference(ref List<double> series, ref int startIndex, int regularDifferencingLevel, int seasonDifferencingLevel, int seasonPartern)
        {
            int num = 0;
            while (num < regularDifferencingLevel)
            {
                startIndex++;
                int num2 = series.Count - 1;
                while (true)
                {
                    if (num2 < startIndex)
                    {
                        num++;
                        break;
                    }
                    series[num2] -= series[num2 - 1];
                    num2--;
                }
            }
            int num3 = 0;
            while (num3 < seasonDifferencingLevel)
            {
                startIndex += seasonPartern;
                int num4 = series.Count - 1;
                while (true)
                {
                    if (num4 < startIndex)
                    {
                        num3++;
                        break;
                    }
                    series[num4] -= series[num4 - seasonPartern];
                    num4--;
                }
            }
        }

        public static double ComputeMAE(List<double> errorSeries)
        {
            double num = 0.0;
            foreach (double num2 in errorSeries)
            {
                num += Math.Abs(num2);
            }
            return (num / ((double)errorSeries.Count));
        }

        public static double ComputeMAE(List<double> processSeries, List<double> testSeries)
        {
            double num = 0.0;
            for (int i = 0; i < processSeries.Count; i++)
            {
                num += Math.Abs((double)(processSeries[i] - testSeries[i]));
            }
            return (num / ((double)processSeries.Count));
        }

        public static double ComputeMAPE(List<double> processSeries, List<double> testSeries)
        {
            double num = 0.0;
            for (int i = 1; i < processSeries.Count; i++)
            {
                double d = Math.Abs((double)((processSeries[i] - testSeries[i]) / processSeries[i]));
                if (double.IsNaN(d))
                {
                    d = 1.0;
                }
                num += Math.Abs(Math.Min(d, 1.0));
            }
            return ((num * 100.0) / ((double)processSeries.Count));
        }

        public static double ComputeMSE(List<double> errorSeries)
        {
            double num = 0.0;
            foreach (double num2 in errorSeries)
            {
                num += Math.Pow(num2, 2.0);
            }
            return (num / ((double)errorSeries.Count));
        }

        public static double ComputeMSE(List<double> processSeries, List<double> testSeries)
        {
            double num = 0.0;
            for (int i = 0; i < processSeries.Count; i++)
            {
                num += Math.Pow(processSeries[i] - testSeries[i], 2.0);
            }
            return (num / ((double)processSeries.Count));
        }

        public static void ComputePartialAutocorrelation(List<double> listAutocorrelation, out List<double> listPartialAutocorrelation)
        {
            int count = listAutocorrelation.Count;
            int num2 = (count * (count + 1)) / 2;
            listPartialAutocorrelation = new List<double>();
            for (int i = 0; i < num2; i++)
            {
                listPartialAutocorrelation.Add(0.0);
            }
            int num4 = 1;
            while (num4 < count)
            {
                double num5 = 0.0;
                double num7 = 0.0;
                int num8 = 1;
                while (true)
                {
                    if (num8 > (num4 - 1))
                    {
                        num5 = listAutocorrelation[num4] - num7;
                        num7 = 0.0;
                        int num9 = 1;
                        while (true)
                        {
                            if (num9 > (num4 - 1))
                            {
                                SetPartialCorrelationAt(listPartialAutocorrelation, num4, num4, num5 / (1.0 - num7));
                                int num10 = 1;
                                while (true)
                                {
                                    if (num10 >= num4)
                                    {
                                        num4++;
                                        break;
                                    }
                                    num7 = GetPartialCorrelationAt(listPartialAutocorrelation, num4 - 1, num10) - (GetPartialCorrelationAt(listPartialAutocorrelation, num4, num4) * GetPartialCorrelationAt(listPartialAutocorrelation, num4 - 1, num4 - num10));
                                    SetPartialCorrelationAt(listPartialAutocorrelation, num4, num10, num7);
                                    num10++;
                                }
                                break;
                            }
                            num7 += GetPartialCorrelationAt(listPartialAutocorrelation, num4 - 1, num9) * listAutocorrelation[num9];
                            num9++;
                        }
                        break;
                    }
                    num7 += GetPartialCorrelationAt(listPartialAutocorrelation, num4 - 1, num8) * listAutocorrelation[num4 - num8];
                    num8++;
                }
            }
            List<double> list = new List<double>();
            for (int j = 1; j <= count; j++)
            {
                list.Add(GetPartialCorrelationAt(listPartialAutocorrelation, j, j));
            }
            listPartialAutocorrelation.Clear();
            for (int k = 0; k < list.Count; k++)
            {
                listPartialAutocorrelation.Add(list[k]);
            }
        }

        public static double ComputeSSE(List<double> errorSeries)
        {
            double num = 0.0;
            foreach (double num2 in errorSeries)
            {
                num += Math.Pow(num2, 2.0);
            }
            return num;
        }

        public static double ComputeSSE(List<double> processSeries, List<double> testSeries)
        {
            double num = 0.0;
            for (int i = 0; i < processSeries.Count; i++)
            {
                num += Math.Pow(processSeries[i] - testSeries[i], 2.0);
            }
            return num;
        }

        public static void DrawAutocorrelation(List<double> listAutocorrelation, List<double> listConfidenceLimit, bool isPACF = false)
        {
            Plot_Form form = new Plot_Form();
            if (!isPACF)
            {
                form.chart1.Titles["Title1"].Text = "Autocorrelation Function";
                form.chart1.ChartAreas["ChartArea1"].Axes[0].Title = "Lag";
                form.chart1.ChartAreas["ChartArea1"].Axes[1].Title = "ACF";
                form.chart1.Series[0].Name = "ACF";
            }
            else
            {
                form.chart1.Titles["Title1"].Text = "Partial Autocorrelation Function";
                form.chart1.ChartAreas["ChartArea1"].Axes[0].Title = "Lag";
                form.chart1.ChartAreas["ChartArea1"].Axes[1].Title = "PACF";
                form.chart1.Series[0].Name = "PACF";
            }
            int count = listAutocorrelation.Count;
            form.chart1.ChartAreas[0].AxisX.Interval = Math.Ceiling((double)((1.0 * count) / 20.0));
            Series item = new Series
            {
                ChartArea = "ChartArea1",
                ChartType = SeriesChartType.Line,
                Color = Color.Red,
                IsVisibleInLegend = false
            };
            Series series2 = new Series
            {
                ChartArea = "ChartArea1",
                ChartType = SeriesChartType.Line,
                Color = Color.Red,
                IsVisibleInLegend = false
            };
            Series series3 = new Series
            {
                ChartArea = "ChartArea1",
                ChartType = SeriesChartType.Line,
                Color = Color.Black,
                IsVisibleInLegend = false
            };
            int num2 = 0;
            if (isPACF)
            {
                num2 = 1;
            }
            for (int i = 0; i < listAutocorrelation.Count; i++)
            {
                Series series4 = new Series
                {
                    ChartArea = "ChartArea1",
                    ChartType = SeriesChartType.Line,
                    Color = Color.Blue
                };
                series4.Points.AddXY((double)(num2 + i), 0.0);
                series4.Points.AddXY((double)(num2 + i), listAutocorrelation[i]);
                series4.IsVisibleInLegend = false;
                form.chart1.Series.Add(series4);
                item.Points.AddXY((double)(num2 + i), listConfidenceLimit[i]);
                series2.Points.AddXY((double)(num2 + i), -listConfidenceLimit[i]);
                series3.Points.AddXY((double)i, 0.0);
            }
            form.chart1.Series.Add(item);
            form.chart1.Series.Add(series2);
            form.chart1.Series.Add(series3);
            form.Show();
        }

        public static void DrawForecastSeriesData(List<double> firstSeries, int firstStartIndex, List<double> secondSeries, int secondStartIndex)
        {
            Forecast_Form form = new Forecast_Form();
            Series item = new Series
            {
                ChartArea = "ChartArea1",
                ChartType = SeriesChartType.Line,
                Color = Color.Blue,
                IsVisibleInLegend = false
            };
            Series series2 = new Series
            {
                ChartArea = "ChartArea1",
                ChartType = SeriesChartType.Line,
                Color = Color.Red,
                IsVisibleInLegend = false
            };
            form.chart1.ChartAreas[0].AxisX.Interval = Math.Ceiling((double)((1.0 * (firstSeries.Count - firstStartIndex)) / 20.0));
            form.chart1.Titles["Title1"].Text = "Time series";
            form.chart1.Series["Data"].Color = Color.Blue;
            for (int i = firstStartIndex; i < firstSeries.Count; i++)
            {
                item.Points.AddXY((double)(i + 1), firstSeries[i]);
            }
            form.chart1.Series.Add(item);
            series2.Points.AddXY((double)firstSeries.Count, firstSeries[firstSeries.Count - 1]);
            for (int j = secondStartIndex; j < secondSeries.Count; j++)
            {
                series2.Points.AddXY((double)(((j + 1) + firstSeries.Count) - secondStartIndex), secondSeries[j]);
            }
            form.chart1.Series.Add(series2);
            StringBuilder builder = new StringBuilder();
            builder.Append($"Forecast data for {secondSeries.Count} ahead time    ");
            for (int k = 0; k < secondSeries.Count; k++)
            {
                builder.Append($"  {k + 1}	{$"{secondSeries[k]:0.###}"}    ");
        }
            form.setDataResult(builder.ToString());
            form.Show();
        }

        public static void DrawPartialAutocorrelation(List<double> listPartialAutocorrelation, double confidenceLimit)
        {
            List<double> listConfidenceLimit = new List<double>();
            for (int i = 0; i < listPartialAutocorrelation.Count; i++)
            {
                listConfidenceLimit.Add(confidenceLimit);
            }
            DrawAutocorrelation(listPartialAutocorrelation, listConfidenceLimit, true);
        }

        public static void DrawSeriesData(List<double> series, int startIndex)
        {
            Plot_Form form = new Plot_Form
            {
                chart1 = {
                ChartAreas = { [0] = {
                    AxisX = { Interval = Math.Ceiling((double) ((1.0 * (series.Count - startIndex)) / 20.0)) },
                    //Text = "Time series",
                    //Color = Color.Blue
                } },
                Titles = { [0] = {
                    //AxisX = { Interval = Math.Ceiling((double) ((1.0 * (series.Count - startIndex)) / 20.0)) },
                    Text = "Time series",
                    //Color = Color.Blue
                } },
                Series = { [0] = {
                    //AxisX = { Interval = Math.Ceiling((double) ((1.0 * (series.Count - startIndex)) / 20.0)) },
                    //Text = "Time series",
                    Color = Color.Blue
                } }
            }
            };
            for (int i = startIndex; i < series.Count; i++)
            {
                form.chart1.Series["Data"].Points.AddXY((double)(i + 1), series.ElementAt<double>(i));
            }
            form.Show();
        }

        public static void DrawTwoSeriesTestData(List<double> dataSeries, int firstStartIndex, List<double> testSeries, int secondStartIndex)
        {
            double num = ComputeMAE(dataSeries, testSeries);
            double num2 = ComputeMSE(dataSeries, testSeries);
            double num3 = ComputeMAPE(dataSeries, testSeries);
            Test_Form form = new Test_Form();
            form.textBox1.AppendText("Mean Absolute Error MAE =  " + $"{num:0.000}" + "\n");
            form.textBox1.AppendText("Mean Square Error MSE =  " + $"{num2:0.000}" + "\n");
            form.textBox1.AppendText("Mean absolute percentage Error MAPE =  " + $"{num3:0.000}" + "\n");
            form.textBox1.ReadOnly = true;
            for (int i = firstStartIndex; i < dataSeries.Count; i++)
            {
                form.chart1.Series["Observations"].Points.AddXY((double)((i + 1) - firstStartIndex), dataSeries[i]);
            }
            for (int j = secondStartIndex; j < testSeries.Count; j++)
            {
                form.chart1.Series["Computations"].Points.AddXY((double)((j + 1) - secondStartIndex), testSeries[j]);
            }
            form.Show();
        }

        public static double GetPartialCorrelationAt(List<double> listPartialCorrelation, int i, int j)
        {
            int num = 0;
            if (i > 1)
            {
                num = (((i * (i - 1)) / 2) + i) - j;
            }
            return listPartialCorrelation[num];
        }

        public static void RevertDifference(ref List<double> series, ref int startIndex, int regularDifferencingLevel, int seasonDifferencingLevel, int seasonPartern)
        {
            int num = 0;
            while (num < seasonDifferencingLevel)
            {
                int num2 = startIndex;
                while (true)
                {
                    if (num2 >= series.Count)
                    {
                        startIndex -= seasonPartern;
                        num++;
                        break;
                    }
                    series[num2] += series[num2 - seasonPartern];
                    num2++;
                }
            }
            int num3 = 0;
            while (num3 < regularDifferencingLevel)
            {
                int num4 = startIndex;
                while (true)
                {
                    if (num4 >= series.Count)
                    {
                        startIndex--;
                        num3++;
                        break;
                    }
                    series[num4] += series[num4 - 1];
                    num4++;
                }
            }
        }

        public static void SetPartialCorrelationAt(List<double> listPartialCorrelation, int i, int j, double value)
        {
            int num = 0;
            if (i > 1)
            {
                num = (((i * (i - 1)) / 2) + i) - j;
            }
            listPartialCorrelation[num] = value;
        }

        public static void WriteSeries(List<double> series, string filename)
        {
            StreamWriter writer = new StreamWriter(filename, false);
            foreach (double num in series)
            {
                writer.WriteLine(num);
            }
            writer.Flush();
            writer.Close();
        }
    }

    
    }



