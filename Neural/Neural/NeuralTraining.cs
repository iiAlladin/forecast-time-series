﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public enum AlgorithmType { BackPropagation, ResilientPropagation };

    public class NeuralTraining
    {
        public Neural s_Network { get; set; }
        private double[,] Backup_m_arInputHiddenConn;
        private double[,] Backup_m_arHiddenOutputConn;

        private void InitForTrain()
        {
            Backup_m_arInputHiddenConn = new double[s_Network.m_iNumInputNodes + 1, s_Network.m_iNumHiddenNodes];
            Backup_m_arHiddenOutputConn = new double[s_Network.m_iNumHiddenNodes + 1, s_Network.m_iNumOutputNodes];
            BackUp();
        }

        public void Bp_Run(List<double> sampleSeries, List<double> validateSeries, double learnRate, double momentum, double theEpoches = 10000, double residual = 1.0E-5)
        {
            InitForTrain();

            int i, j, k, n;
            int epoch = 0;
            double MAE = Double.MaxValue;
            double LastError = Double.MaxValue;
            List<double> MAError = new List<double>();

            double[,] deltaInputHidden = new double[s_Network.m_iNumInputNodes + 1, s_Network.m_iNumHiddenNodes];
            double[,] deltaHiddenOutput = new double[s_Network.m_iNumHiddenNodes + 1, s_Network.m_iNumOutputNodes];
            double[,] lagDeltaInputHidden = new double[s_Network.m_iNumInputNodes + 1, s_Network.m_iNumHiddenNodes];
            double[,] lagDeltaHiddenOutput = new double[s_Network.m_iNumHiddenNodes + 1, s_Network.m_iNumOutputNodes];

            for (j = 0; j < s_Network.m_iNumHiddenNodes; j++)    // initialize weight-step of Input Hidden connection
            {
                for (i = 0; i <= s_Network.m_iNumInputNodes; i++)
                {
                    deltaInputHidden[i, j] = 0.0;
                    lagDeltaInputHidden[i, j] = 0.0;
                }
            }
            for (k = 0; k < s_Network.m_iNumOutputNodes; k++)   // initialize weight-step of Hidden Output connection
            {
                for (j = 0; j <= s_Network.m_iNumHiddenNodes; j++)
                {
                    deltaHiddenOutput[j, k] = 0.0;
                    lagDeltaHiddenOutput[j, k] = 0.0;
                }
            }

            while (epoch < theEpoches)
            {
                MAE = 0.0;
                for (n = s_Network.m_iNumInputNodes; n < sampleSeries.Count; n++)
                {
                    // forward
                    double[] lstTemp = new double[s_Network.m_iNumInputNodes];
                    for (i = s_Network.m_iNumInputNodes; i > 0; i--)
                    {
                        lstTemp[s_Network.m_iNumInputNodes - i] = sampleSeries[n - i];
                    }
                    s_Network.CalculateOutput(lstTemp);
                    for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
                    {
                        MAE += Math.Abs(sampleSeries.ElementAt(n + k) - s_Network.m_arOutputNodes[k].GetOutput());
                    }

                    // backward
                    /*calculate weight-step for weights connecting from hidden nodes to output nodes*/
                    for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
                    {
                        for (j = 0; j <= s_Network.m_iNumHiddenNodes; j++)
                        {
                            double parDerv = -s_Network.m_arOutputNodes[k].GetOutput() * (1 - s_Network.m_arOutputNodes[k].GetOutput()) * s_Network.m_arHiddenNodes[j].GetOutput() * (sampleSeries.ElementAt(n) - s_Network.m_arOutputNodes[k].GetOutput());
                            deltaHiddenOutput[j, k] = -learnRate * parDerv + momentum * lagDeltaHiddenOutput[j, k];
                            lagDeltaHiddenOutput[j, k] = deltaHiddenOutput[j, k];
                        }
                    }
                    /*calculate weight-step for weights connecting from input nodes to hidden nodes*/
                    for (j = 0; j < s_Network.m_iNumHiddenNodes; j++)
                    {
                        double temp = 0.0;
                        for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
                        {
                            temp += -(sampleSeries.ElementAt(n) - s_Network.m_arOutputNodes[k].GetOutput()) * s_Network.m_arOutputNodes[k].GetOutput() * (1 - s_Network.m_arOutputNodes[k].GetOutput()) * s_Network.m_arHiddenOutputConn[j, k];
                        }
                        for (i = 0; i <= s_Network.m_iNumInputNodes; i++)
                        {
                            double parDerv = s_Network.m_arHiddenNodes[j].GetOutput() * (1 - s_Network.m_arHiddenNodes[j].GetOutput()) * s_Network.m_arInputNodes[i].GetInput() * temp;
                            deltaInputHidden[i, j] = -learnRate * parDerv + momentum * lagDeltaInputHidden[i, j];
                            lagDeltaInputHidden[i, j] = deltaInputHidden[i, j];
                        }
                    }
                    /*updating weight from Input to Hidden*/
                    for (j = 0; j < s_Network.m_iNumHiddenNodes; j++)
                    {
                        for (i = 0; i <= s_Network.m_iNumInputNodes; i++)
                        {
                            s_Network.m_arInputHiddenConn[i, j] += deltaInputHidden[i, j];
                        }
                    }
                    /*updating weight from Hidden to Output*/
                    for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
                    {
                        for (j = 0; j <= s_Network.m_iNumHiddenNodes; j++)
                        {
                            s_Network.m_arHiddenOutputConn[j, k] += deltaHiddenOutput[j, k];
                        }
                    }

                } // end outer for
                MAE = MAE / (sampleSeries.Count - s_Network.m_iNumInputNodes); // caculate mean square error
                if (Math.Abs(MAE - LastError) < residual) // if the Error is not improved significantly, halt training process and rollback
                {
                    RollBack();
                    break;

                }
                else
                { //else backup the current configuration and continue training
                    LastError = MAE;
                    BackUp();
                    MAError.Add(MAE);
                    epoch++;
                }
            }
            /* output training result */

            Train_Result result = new Train_Result();
            result.trainResult.AppendText("Maximum Epochs: " + theEpoches + "\n");
            result.trainResult.AppendText("Training Epoches: " + epoch + "\n");
            result.trainResult.AppendText("Training MAE: " + MAE + "\n");
            result.trainResult.AppendText("Terminated Condition: residual of Error is less than " + residual + "\n");
            result.trainResult.AppendText("Learning Rate: " + learnRate + "\n");
            result.trainResult.AppendText("Momentum Term: " + momentum + "\n");
            result.trainResult.ReadOnly = true;
            result.chart1.Series["MAE"].Color = System.Drawing.Color.Red;
            for (int t = 0; t < MAError.Count; t++)
            {
                result.chart1.Series["MAE"].Points.AddXY(t + 1, MAError.ElementAt(t));
            }
            result.ShowDialog();
        }

        public void Rprop_Run(List<double> sampleSeries, List<double> validateSeries, double defaultDeltaValue = 0.0001, double maxDelta = 50.0, double theEpoches = 10000, double residual = 1.0E-5)
        {
            InitForTrain();

            int n, i, j, k;
            int epoch = 0;
            double MAE = Double.MaxValue;
            double defaultWeightChange = 0.0;
            double defaultGradientValue = 0.0;
            double minDelta = 1.0E-6;
            double maxStep = 1.2;
            double minStep = 0.5;
            double LastError = Double.MaxValue;
            List<double> MAError = new List<double>();

            double[,] weightChangeInputHidden = new double[s_Network.m_iNumInputNodes + 1, s_Network.m_iNumHiddenNodes];
            double[,] deltaInputHidden = new double[s_Network.m_iNumInputNodes + 1, s_Network.m_iNumHiddenNodes];
            double[,] gradientInputHidden = new double[s_Network.m_iNumInputNodes + 1, s_Network.m_iNumHiddenNodes];
            double[,] newGradientInputHidden = new double[s_Network.m_iNumInputNodes + 1, s_Network.m_iNumHiddenNodes];

            double[,] weightChangeHiddenOutput = new double[s_Network.m_iNumHiddenNodes + 1, s_Network.m_iNumOutputNodes];
            double[,] deltaHiddenOutput = new double[s_Network.m_iNumHiddenNodes + 1, s_Network.m_iNumOutputNodes];
            double[,] gradientHiddenOutput = new double[s_Network.m_iNumHiddenNodes + 1, s_Network.m_iNumOutputNodes];
            double[,] newGradientHiddenOutput = new double[s_Network.m_iNumHiddenNodes + 1, s_Network.m_iNumOutputNodes];


            // initialize Input Hidden connection
            for (j = 0; j < s_Network.m_iNumHiddenNodes; j++)
            {
                for (i = 0; i <= s_Network.m_iNumInputNodes; i++)
                {
                    weightChangeInputHidden[i, j] = defaultWeightChange;
                    deltaInputHidden[i, j] = defaultDeltaValue;
                    gradientInputHidden[i, j] = defaultGradientValue;
                    newGradientInputHidden[i, j] = defaultGradientValue;
                }
            }

            // initialize Hidden Output connection
            for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
            {
                for (j = 0; j <= s_Network.m_iNumHiddenNodes; j++)
                {
                    weightChangeHiddenOutput[j, k] = defaultWeightChange;
                    deltaHiddenOutput[j, k] = defaultDeltaValue;
                    gradientHiddenOutput[j, k] = defaultGradientValue;
                    newGradientHiddenOutput[j, k] = defaultGradientValue;
                }
            }

            while (epoch < theEpoches)
            {
                MAE = 0.0;
                //training for each epoch
                for (n = s_Network.m_iNumInputNodes; n < sampleSeries.Count; n++)
                {
                    //forward
                    double[] lstTemp = new double[s_Network.m_iNumInputNodes];
                    for (i = s_Network.m_iNumInputNodes; i > 0; i--)
                    {
                        lstTemp[s_Network.m_iNumInputNodes - i] = sampleSeries[n - i];
                    }
                    s_Network.CalculateOutput(lstTemp);

                    /*calculate abs error*/
                    for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
                    {
                        MAE += Math.Abs(sampleSeries.ElementAt(n) - s_Network.m_arOutputNodes[k].GetOutput());
                    }
                    // backward
                    /*calculate weight-step for weights connecting from hidden nodes to output nodes*/
                    for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
                    {
                        for (j = 0; j <= s_Network.m_iNumHiddenNodes; j++)
                        {
                            newGradientHiddenOutput[j, k] += -s_Network.m_arOutputNodes[k].GetOutput() * (1 - s_Network.m_arOutputNodes[k].GetOutput()) * s_Network.m_arHiddenNodes[j].GetOutput() * (sampleSeries[n] - s_Network.m_arOutputNodes[k].GetOutput());
                        }
                    }
                    /*calculate weight-step for weights connecting from input nodes to hidden nodes*/
                    for (j = 0; j < s_Network.m_iNumHiddenNodes; j++)
                    {
                        double temp = 0.0;
                        for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
                        {
                            temp += -(sampleSeries.ElementAt(n) - s_Network.m_arOutputNodes[k].GetOutput()) * s_Network.m_arOutputNodes[k].GetOutput() * (1 - s_Network.m_arOutputNodes[k].GetOutput()) * s_Network.m_arHiddenOutputConn[j, k];
                        }
                        for (i = 0; i <= s_Network.m_iNumInputNodes; i++)
                        {
                            newGradientInputHidden[i, j] += s_Network.m_arHiddenNodes[j].GetOutput() * (1 - s_Network.m_arHiddenNodes[j].GetOutput()) * s_Network.m_arInputNodes[i].GetOutput() * temp;
                        }
                    }

                } // end outer for

                int sign;
                for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
                {
                    for (j = 0; j <= s_Network.m_iNumHiddenNodes; j++)
                    {
                        sign = Math.Sign(newGradientHiddenOutput[j, k] * gradientHiddenOutput[j, k]);
                        if (sign > 0)
                        {
                            deltaHiddenOutput[j, k] = Math.Min(deltaHiddenOutput[j, k] * maxStep, maxDelta);
                            weightChangeHiddenOutput[j, k] = -Math.Sign(newGradientHiddenOutput[j, k]) * deltaHiddenOutput[j, k];
                            s_Network.m_arHiddenOutputConn[j, k] += weightChangeHiddenOutput[j, k];
                            gradientHiddenOutput[j, k] = newGradientHiddenOutput[j, k];
                        }
                        else if (sign < 0)
                        {
                            deltaHiddenOutput[j, k] = Math.Max(deltaHiddenOutput[j, k] * minStep, minDelta);
                            s_Network.m_arHiddenOutputConn[j, k] -= weightChangeHiddenOutput[j, k]; //restore old value
                            newGradientHiddenOutput[j, k] = 0.0;
                            gradientHiddenOutput[j, k] = newGradientHiddenOutput[j, k];
                        }
                        else
                        {
                            weightChangeHiddenOutput[j, k] = -Math.Sign(newGradientHiddenOutput[j, k]) * deltaHiddenOutput[j, k];
                            s_Network.m_arHiddenOutputConn[j, k] += weightChangeHiddenOutput[j, k];
                            gradientHiddenOutput[j, k] = newGradientHiddenOutput[j, k];
                        }
                        newGradientHiddenOutput[j, k] = 0.0;
                    }
                }

                /*calculate weight-step for weights connecting from input nodes to hidden nodes*/
                for (j = 0; j < s_Network.m_iNumHiddenNodes; j++)
                {
                    for (i = 0; i <= s_Network.m_iNumInputNodes; i++)
                    {
                        sign = Math.Sign(newGradientInputHidden[i, j] * gradientInputHidden[i, j]);
                        if (sign > 0)
                        {
                            deltaInputHidden[i, j] = Math.Min(deltaInputHidden[i, j] * maxStep, maxDelta);
                            weightChangeInputHidden[i, j] = -Math.Sign(newGradientInputHidden[i, j]) * deltaInputHidden[i, j];
                            s_Network.m_arInputHiddenConn[i, j] += weightChangeInputHidden[i, j];
                            gradientInputHidden[i, j] = newGradientInputHidden[i, j];
                        }
                        else if (sign < 0)
                        {
                            deltaInputHidden[i, j] = Math.Max(deltaInputHidden[i, j] * minStep, minDelta);
                            s_Network.m_arInputHiddenConn[i, j] -= weightChangeInputHidden[i, j];
                            newGradientInputHidden[i, j] = 0.0;
                            gradientInputHidden[i, j] = 0.0;
                        }
                        else
                        {
                            weightChangeInputHidden[i, j] = -Math.Sign(newGradientInputHidden[i, j]) * deltaInputHidden[i, j];
                            s_Network.m_arInputHiddenConn[i, j] += weightChangeInputHidden[i, j];
                            gradientInputHidden[i, j] = newGradientInputHidden[i, j];
                        }
                        newGradientInputHidden[i, j] = 0.0;
                    }
                }
                MAE = MAE / (sampleSeries.Count); // caculate mean square error
                if (Math.Abs(MAE - LastError) < residual) // if the Error is not improved significantly, halt training process and rollback
                {
                    RollBack();
                    break;

                }
                else
                { //else backup the current configuration and continue training
                    LastError = MAE;
                    BackUp();
                    MAError.Add(MAE);
                    epoch++;
                }
            }
            /* output training result */
            Train_Result result = new Train_Result();
            result.trainResult.AppendText("Maximum Epochs: " + theEpoches + "\n");
            result.trainResult.AppendText("Training Epoches: " + epoch + "\n");
            result.trainResult.AppendText("Training MAE: " + MAE + "\n");
            result.trainResult.AppendText("Terminated Condition: residual of Error is less than " + residual + "\n");
            result.trainResult.AppendText("Maximum update value: " + maxDelta + "\n");
            result.trainResult.AppendText("default update value: " + defaultDeltaValue + "\n");
            result.trainResult.ReadOnly = true;
            result.chart1.Series["MAE"].Color = System.Drawing.Color.Red;
            for (int t = 0; t < MAError.Count; t++)
            {
                result.chart1.Series["MAE"].Points.AddXY(t + 1, MAError.ElementAt(t));
            }
            result.label1.Text = "Algorithm: RPROP";
            result.ShowDialog();
        }

        public void RollBack()
        {
            int i, j, k;
            for (j = 0; j < s_Network.m_iNumHiddenNodes; j++)
            {
                for (i = 0; i <= s_Network.m_iNumInputNodes; i++)
                {
                    s_Network.m_arInputHiddenConn[i, j] = Backup_m_arInputHiddenConn[i, j];
                }
            }
            for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
            {
                for (j = 0; j <= s_Network.m_iNumHiddenNodes; j++)
                {
                    s_Network.m_arHiddenOutputConn[j, k] = Backup_m_arHiddenOutputConn[j, k];
                }
            }
        }

        public void BackUp()
        {
            int i, j, k;

            for (j = 0; j < s_Network.m_iNumHiddenNodes; j++)
            {
                for (i = 0; i <= s_Network.m_iNumInputNodes; i++)
                {
                    Backup_m_arInputHiddenConn[i, j] = s_Network.m_arInputHiddenConn[i, j];
                }
            }
            for (k = 0; k < s_Network.m_iNumOutputNodes; k++)
            {
                for (j = 0; j <= s_Network.m_iNumHiddenNodes; j++)
                {
                    Backup_m_arHiddenOutputConn[j, k] = s_Network.m_arHiddenOutputConn[j, k];
                }
            }
        }

    }
}
