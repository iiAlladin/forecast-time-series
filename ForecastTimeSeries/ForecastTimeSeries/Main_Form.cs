﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForecastTimeSeries
{
    public partial class Main_Form : Form
    {
        enum ModelType
        {
            SARIMA,
            ANN,
            SARIMA_ANN
        }

        private string m_TrainingDataFile;
        private string m_TestingDataFile;
        private ARIMA ARIMAModel;
        private Neural NeuralModel;
        private ModelType modelType;
        private bool isReadTrainData;
        private bool isReadTestData;

        List<double> _trainDataSeries;
        List<double> _testDataSeries;
        List<double> _dataForTest;
        List<double> _dataForForecast;


        public Main_Form()
        {
            InitializeComponent();
            this.CenterToScreen();
        }

        private void Main_Form_Load(object sender, EventArgs e)
        {
            InitData();
            InitGUI();
        }

        private void InitData()
        {
            _trainDataSeries = new List<double>();
            _testDataSeries = new List<double>();
            _dataForTest = new List<double>();
            _dataForForecast = new List<double>();
            modelType = ModelType.SARIMA_ANN;
            isReadTrainData = false;
            isReadTestData = false;
            ARIMAModel = new ARIMA();
            NeuralModel = new Neural();
        }

        private void InitGUI()
        {
            SettingGUIBeforeChooseData();
            txtConfig1.Text = 0.1.ToString();
            txtConfigEpoches.Text = 1000.ToString();
            txtConfig2.Text = 1.ToString();
            txtConfigErrors.Text = 0.001.ToString();
            comboBoxModel.SelectedIndex = 2;

            labelNumColumnDataTesting.Text = "";
            labelNumColumnDataTraining.Text = "";
            labelNumRowDataTesting.Text = "";
            labelNumRowDataTraining.Text = "";
        }

        private void SettingGUIBeforeChooseData()
        {
            radioBtnAutomaticARIMA.Enabled = false;
            radioBtnManualARIMA.Enabled = false;
            groupBoxARIMAParameter.Enabled = false;

            btnTrainARIMA.Enabled = false;
            btnTestArima.Enabled = false;
            btnForecastARIMA.Enabled = false;
            btnLoadARIMA.Enabled = false;
            btnSaveARIMA.Enabled = false;

            btnPlotDataARIMA.Enabled = false;
            btnResetDataARIMA.Enabled = false;
            btnPlotErrorARIMA.Enabled = false;
            btnCorrelogram.Enabled = false;
            btnPartialCorrelation.Enabled = false;

            groupBoxNetworkConfig.Enabled = false;
            groupBoxAlgorithmConfig.Enabled = false;
            groupBoxNetworkAlgorithm.Enabled = false;

            btnTrainNeural.Enabled = false;
            btnPlotNeural.Enabled = false;
            btnTestNeural.Enabled = false;
            btnForecastNeural.Enabled = false;

            btnForecast.Enabled = false;
            btnTest.Enabled = false;
        }

        private void SettingGUIBeforeARIMAModel()
        {
            radioBtnAutomaticARIMA.Enabled = true;
            radioBtnManualARIMA.Enabled = true;
            if (radioBtnAutomaticARIMA.Checked)
            {
                groupBoxARIMAParameter.Enabled = false;
            }
            else
            {
                groupBoxARIMAParameter.Enabled = true;
            }

            btnTrainARIMA.Enabled = true;
            btnTestArima.Enabled = false;
            btnForecastARIMA.Enabled = false;
            btnLoadARIMA.Enabled = true;
            btnSaveARIMA.Enabled = false;

            btnPlotDataARIMA.Enabled = true;
            btnResetDataARIMA.Enabled = true;
            btnPlotErrorARIMA.Enabled = false;
            btnCorrelogram.Enabled = true;
            btnPartialCorrelation.Enabled = true;
        }

        private void SettingGUIBeforeNeuralNetwork()
        {
            radioBtnAutomaticARIMA.Enabled = true;
            radioBtnManualARIMA.Enabled = true;

            btnTrainARIMA.Enabled = true;
            btnTestArima.Enabled = true;
            btnForecastARIMA.Enabled = true;
            btnLoadARIMA.Enabled = true;
            btnSaveARIMA.Enabled = true;

            btnPlotDataARIMA.Enabled = true;
            btnResetDataARIMA.Enabled = true;
            btnPlotErrorARIMA.Enabled = true;
            btnCorrelogram.Enabled = true;
            btnPartialCorrelation.Enabled = true;

            groupBoxNetworkConfig.Enabled = true;
            groupBoxAlgorithmConfig.Enabled = true;
            groupBoxNetworkAlgorithm.Enabled = true;

            this.txtNumInput.Text = "";
            this.txtNumHidden.Text = "";
            this.txtNumInput.Enabled = true;
            this.txtNumHidden.Enabled = true;

            this.btnNetworkNew.Enabled = true;
            this.btnNetworkLoad.Enabled = true;
            this.btnNetworkSave.Enabled = false;
            this.btnNetworkClear.Enabled = false;

            this.btnPlotNeural.Enabled = true;
            this.btnTrainNeural.Enabled = false;
            this.btnTestNeural.Enabled = false;
            this.btnForecastNeural.Enabled = false;
        }

        #region choose data

        private void btnChooseTrainingData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Open File";
            DialogResult result = openDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                m_TrainingDataFile = openDialog.FileName;
                m_TestingDataFile = openDialog.FileName;
            }
            else
            {
                return;
            }

            System.IO.StreamReader fileInput = null;
            string line = null;
            int numRows = 0;
            int numColumns = 0;
            try
            {
                fileInput = new System.IO.StreamReader(m_TrainingDataFile);
                while ((line = fileInput.ReadLine()) != null)
                {
                    if (numRows == 0)
                    {
                        char[] delimiterChars = { ' ', ',' };
                        List<String> words = new List<string>();
                        words.AddRange(line.Split(delimiterChars));
                        words.RemoveAll(item => "" == item);
                        numColumns = words.Count;
                    }
                    numRows++;
                }
                this.textDataFileTraining.Text = m_TrainingDataFile;
                this.labelNumColumnDataTraining.Text = Convert.ToString(numColumns);
                this.labelNumRowDataTraining.Text = Convert.ToString(numRows);
                this.txtTrainDataColumn.Text = "1";
                this.txtTrainDataFromRow.Text = "1";
                this.txtTrainDataToRow.Text = Convert.ToString((int)(numRows*0.9));
                this.txtTrainDataFromRow.Enabled = true;
                this.txtTrainDataToRow.Enabled = true;

                if (numColumns == 1)
                {
                    this.txtTrainDataColumn.Enabled = false;
                    this.txtTestDataColumn.Enabled = false;
                }
                else
                {
                    this.txtTrainDataColumn.Enabled = true;
                    this.txtTestDataColumn.Enabled = true;
                }
                
                this.textDataFileTesting.Text = m_TrainingDataFile;
                this.labelNumColumnDataTesting.Text = Convert.ToString(numColumns);
                this.labelNumRowDataTesting.Text = Convert.ToString(numRows); ;
                this.txtTestDataColumn.Text = "1";
                this.txtTestDataFromRow.Text = Convert.ToString((int)(numRows * 0.9) + 1);
                this.txtTestDataToRow.Text = Convert.ToString(numRows);
                this.txtTestDataFromRow.Enabled = true;
                this.txtTestDataToRow.Enabled = true;

                isReadTrainData = false;
                isReadTestData = false;
            }
            catch
            {
                MessageBox.Show("File does not found or input is wrong format", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            finally
            {
                if (fileInput != null)
                    fileInput.Close();
            }
        }

        private void btnChooseTestingData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Open File";
            DialogResult result = openDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                m_TestingDataFile = openDialog.FileName;
            }
            else
            {
                return;
            }

            System.IO.StreamReader fileInput = null;
            string line = null;
            int numRows = 0;
            int numColumns = 0;
            try
            {
                fileInput = new System.IO.StreamReader(m_TestingDataFile);
                while ((line = fileInput.ReadLine()) != null)
                {
                    if (numRows == 0)
                    {
                        char[] delimiterChars = { ' ', ',' };
                        List<String> words = new List<string>();
                        words.AddRange(line.Split(delimiterChars));
                        words.RemoveAll(item => "" == item);
                        numColumns = words.Count;
                    }
                    numRows++;
                }

                this.textDataFileTesting.Text = m_TestingDataFile;
                this.labelNumColumnDataTesting.Text = Convert.ToString(numColumns);
                this.labelNumRowDataTesting.Text = Convert.ToString(numRows);
                this.txtTestDataColumn.Text = "1";
                this.txtTestDataFromRow.Text = "1";
                this.txtTestDataToRow.Text = Convert.ToString(numRows);
                this.txtTestDataFromRow.Enabled = true;
                this.txtTestDataToRow.Enabled = true;


                if (numColumns == 1)
                {
                    this.txtTestDataColumn.Enabled = false;
                }
                else
                {
                    this.txtTestDataColumn.Enabled = true;
                }
            }
            catch
            {
                MessageBox.Show("File does not found or input is wrong format", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            finally
            {
                if (fileInput != null)
                    fileInput.Close();
            }
        }

        private bool ReadTrainData()
        {
            _trainDataSeries = new List<double>();
            _dataForForecast = new List<double>();

            System.IO.StreamReader trainingFile = null;
            string lineTrainingFile = null;
            int beginTrainingDataRow = 0;
            int endTrainingDataRow = 0;
            int columnTrainingDataSelected = 0;
            int idxRowTrainingFile = 0;

            try
            {
                beginTrainingDataRow = Convert.ToInt32(this.txtTrainDataFromRow.Text);
                endTrainingDataRow = Convert.ToInt32(this.txtTrainDataToRow.Text);
                columnTrainingDataSelected = Convert.ToInt32(this.txtTrainDataColumn.Text);
            }
            catch
            {
                MessageBox.Show("Input for data file is not correct", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                trainingFile = new System.IO.StreamReader(m_TrainingDataFile);
                while ((lineTrainingFile = trainingFile.ReadLine()) != null)
                {
                    idxRowTrainingFile++;

                    char[] delimiterChars = { ' ', ',' };
                    List<String> words = new List<string>();
                    words.AddRange(lineTrainingFile.Split(delimiterChars));
                    words.RemoveAll(item => "" == item);

                    if (columnTrainingDataSelected > words.Count)
                    {
                        throw new DataException();
                    }

                    if (idxRowTrainingFile >= beginTrainingDataRow && idxRowTrainingFile <= endTrainingDataRow)
                    {
                        _trainDataSeries.Add(Double.Parse(words[columnTrainingDataSelected - 1]));
                        _dataForForecast.Add(Double.Parse(words[columnTrainingDataSelected - 1]));
                    }
                    else
                    {
                        _dataForForecast.Add(Double.Parse(words[columnTrainingDataSelected - 1]));
                    }
                }
            }
            catch
            {
                _trainDataSeries = null;
                if (trainingFile != null)
                    trainingFile.Close();
                MessageBox.Show("Training data file does not found or input is wrong format", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (trainingFile != null)
                    trainingFile.Close();
            }
            return true;
        }

        private bool ReadTestData()
        {
            _testDataSeries = new List<double>();
            _dataForTest = new List<double>();
            System.IO.StreamReader testingFile = null;

            string lineTestingFile = null;
            int beginTestingDataRow = 0;
            int endTestingDataRow = 0;
            int columnTestingDataSelected = 0;
            int idxRowTestingFile = 0;

            try
            {
                beginTestingDataRow = Convert.ToInt32(this.txtTestDataFromRow.Text);
                endTestingDataRow = Convert.ToInt32(this.txtTestDataToRow.Text);
                columnTestingDataSelected = Convert.ToInt32(this.txtTestDataColumn.Text);
            }
            catch
            {
                MessageBox.Show("Input for data file is not correct", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                testingFile = new System.IO.StreamReader(m_TestingDataFile);
                while ((lineTestingFile = testingFile.ReadLine()) != null)
                {
                    idxRowTestingFile++;
                    if (idxRowTestingFile > endTestingDataRow)
                        continue;

                    char[] delimiterChars = { ' ', ',' };
                    List<String> words = new List<string>();
                    words.AddRange(lineTestingFile.Split(delimiterChars));
                    words.RemoveAll(item => "" == item);

                    if (columnTestingDataSelected > words.Count)
                    {
                        throw new DataException();
                    }

                    if (idxRowTestingFile < beginTestingDataRow)
                    {
                        _dataForTest.Add(Double.Parse(words[columnTestingDataSelected - 1]));
                    }
                    else if (idxRowTestingFile <= endTestingDataRow)
                    {
                        _testDataSeries.Add(Double.Parse(words[columnTestingDataSelected - 1]));
                    }
                }
            }
            catch (Exception ex)
            {
                _testDataSeries = null;
                if (testingFile != null)
                    testingFile.Close();
                MessageBox.Show("Testing data file does not found or input is wrong format", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                if (testingFile != null)
                    testingFile.Close();
            }
            return true;

        }

        #endregion choose data

        #region SARIMA model

        private void radioBtnAutomaticARIMA_Click(object sender, EventArgs e)
        {
            groupBoxARIMAParameter.Enabled = false;
            txtRegularDifferencing.Text = "";
            txtSeasonDifferencing.Text = "";
            txtSeasonPartern.Text = "";
            txtARRegular.Text = "";
            txtMARegular.Text = "";
            txtARSeason.Text = "";
            txtMASeason.Text = "";
        }

        private void radioBtnManualARIMA_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxARIMAParameter.Enabled = true;
            txtRegularDifferencing.Text = "0";
            txtSeasonDifferencing.Text = "0";
            txtSeasonPartern.Text = "0";
            txtARRegular.Text = "0";
            txtMARegular.Text = "0";
            txtARSeason.Text = "0";
            txtMASeason.Text = "0";
        }

        private void btnPlotDataARIMA_Click(object sender, EventArgs e)
        {
            ARIMAModel.DrawSeriesData();
        }

        private void btnCorrelogram_Click(object sender, EventArgs e)
        {
            ARIMAModel.DrawAutocorrelation();
        }

        private void btnPartialCorrelation_Click(object sender, EventArgs e)
        {
            ARIMAModel.DrawPartialAutocorrelation();
        }

        private void btnPlotErrorARIMA_Click(object sender, EventArgs e)
        {
            ARIMAModel.DrawErrorData();
        }

        private void btnRemoveSeason_Click(object sender, EventArgs e)
        {
            try
            {
                int regularDifferencingLevel = Int32.Parse(txtRegularDifferencing.Text);
                int seasonDifferencingLevel = Int32.Parse(txtSeasonDifferencing.Text);
                int seasonPartern = Int32.Parse(txtSeasonPartern.Text);
                ARIMAModel.RemoveTrendSeasonality(regularDifferencingLevel, seasonDifferencingLevel, seasonPartern);
            }
            catch
            {
                MessageBox.Show("Input for SARIMA model is not correct", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnManualTrainingARIMA_Click(object sender, EventArgs e)
        {
            int regularDifferencing, pRegular, qRegular, pSeason, qSeason, seasonDifferencing, seasonPartern;
            try
            {
                pRegular = Int32.Parse(txtARRegular.Text);
                regularDifferencing = Int32.Parse(txtRegularDifferencing.Text);
                qRegular = Int32.Parse(txtMARegular.Text);

                pSeason = Int32.Parse(this.txtARSeason.Text);
                seasonDifferencing = Int32.Parse(this.txtSeasonDifferencing.Text);
                qSeason = Int32.Parse(this.txtMASeason.Text);

                seasonPartern = Int32.Parse(this.txtSeasonPartern.Text);
                ARIMAModel.ManualTraining(pRegular, regularDifferencing, qRegular, pSeason, seasonDifferencing, qSeason, seasonPartern);
                List<double> errorARIMASeries;
                ARIMAModel.GetErrorSeries(out errorARIMASeries);
                NeuralModel.SetData(errorARIMASeries);

                SettingGUIBeforeNeuralNetwork();

                string model;
                ARIMAModel.GetModel(out model);
                ARIMA_Model ARIMAResult = new ARIMA_Model();
                ARIMAResult.SetResult(model);
                ARIMAResult.Show();
            }
            catch
            {
                MessageBox.Show("Input for SARIMA model is not correct", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnManualRestoreARIMA_Click(object sender, EventArgs e)
        {
            SettingGUIBeforeARIMAModel();
            groupBoxARIMAParameter.Enabled = true;
            txtRegularDifferencing.Text = "0";
            txtSeasonDifferencing.Text = "0";
            txtSeasonPartern.Text = "0";
            txtARRegular.Text = "0";
            txtMARegular.Text = "0";
            txtARSeason.Text = "0";
            txtMASeason.Text = "0";
            ARIMAModel.InitTraining();        
        }

        private void btnTrainARIMA_Click(object sender, EventArgs e)
        {
            if (_trainDataSeries == null || _trainDataSeries.Count == 0)
            {
                MessageBox.Show("Please set data for training", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ARIMAModel.SetData(_trainDataSeries);
            ARIMAModel.AutomaticTraining();


            List<double> errorARIMASeries;
            ARIMAModel.GetErrorSeries(out errorARIMASeries);
            NeuralModel.SetData(errorARIMASeries);

            radioBtnAutomaticARIMA.Checked = true;
            SettingGUIBeforeNeuralNetwork();

            groupBoxARIMAParameter.Enabled = false;
            txtRegularDifferencing.Text = "";
            txtSeasonDifferencing.Text = "";
            txtSeasonPartern.Text = "";
            txtARRegular.Text = "";
            txtMARegular.Text = "";
            txtARSeason.Text = "";
            txtMASeason.Text = "";

            string model;
            ARIMAModel.GetModel(out model);
            ARIMA_Model ARIMAResult = new ARIMA_Model();
            ARIMAResult.SetResult(model);
            ARIMAResult.Show();


        }

        private void btnTestArima_Click(object sender, EventArgs e)
        {
            List<double> testDataARIMASeries = new List<double>();
            List<double> testResultARIMASeries = new List<double>();
            int numDataForInput = Math.Min(ARIMAModel.GetNumDataForInput(), _dataForTest.Count);
            for (int i = numDataForInput; i > 0; i--)
            {
                testDataARIMASeries.Add(_dataForTest[_dataForTest.Count - i]);
            }
            for (int i = 0; i < _testDataSeries.Count; i++)
            {
                testDataARIMASeries.Add(_testDataSeries[i]);
            }
            ARIMAModel.ComputeTestingResult(testDataARIMASeries, out testResultARIMASeries);

            for (int i = numDataForInput; i > 0; i--)
            {
                testDataARIMASeries.RemoveAt(0);
                testResultARIMASeries.RemoveAt(0);
            }
            Statistic.DrawTwoSeriesTestData(testDataARIMASeries, 0, testResultARIMASeries, 0);
        }

        private void btnForecastARIMA_Click(object sender, EventArgs e)
        {
            List<double> forecastARIMASeries;
            int aHead = 0;
            AHead_Form aHeadDialog = new AHead_Form();

            if (aHeadDialog.ShowDialog() == DialogResult.OK)
            {
                aHead = aHeadDialog.GetAHead();
            }
            aHeadDialog.Dispose();

            if (aHead > 0)
            {
                ARIMAModel.Forecast(_dataForForecast, aHead, out forecastARIMASeries);
                Statistic.DrawForecastSeriesData(_dataForForecast, 0, forecastARIMASeries, 0);
            }
            else
            {
                MessageBox.Show(this, "Please enter input in correct format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadARIMA_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Load SARIMA Config File";
            fileDialog.DefaultExt = "xml";
            DialogResult result = fileDialog.ShowDialog();
            string dataFile = "";
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                dataFile = fileDialog.FileName;
                bool importResult = ARIMAModel.Import(dataFile);
                if (importResult)
                {
                    List<int> model;
                    ARIMAModel.GetModel(out model);
                    this.txtARRegular.Text = model[0].ToString();
                    this.txtRegularDifferencing.Text = model[1].ToString();
                    this.txtMARegular.Text = model[2].ToString();
                    this.txtARSeason.Text = model[3].ToString();
                    this.txtSeasonDifferencing.Text = model[4].ToString();
                    this.txtMASeason.Text = model[5].ToString();
                    this.txtSeasonPartern.Text = model[6].ToString();

                    this.groupBoxARIMAParameter.Enabled = true;

                    btnTestArima.Enabled = true;
                    btnForecastARIMA.Enabled = true;
                    btnLoadARIMA.Enabled = true;
                    btnSaveARIMA.Enabled = true;

                    btnNetworkNew.Enabled = true;
                    btnNetworkLoad.Enabled = true;
                    btnPlotNeural.Enabled = true;

                    List<double> errorARIMASeries;
                    ARIMAModel.GetErrorSeries(out errorARIMASeries);
                    NeuralModel.SetData(errorARIMASeries);
                    SettingGUIBeforeNeuralNetwork();
                }
                else
                {
                    MessageBox.Show("Config file is not correct", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                return;
            }
        }

        private void btnSaveARIMA_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save SARIMA Config File";
            saveDialog.DefaultExt = "xml";
            DialogResult result = saveDialog.ShowDialog();
            string dataFile = "";
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                dataFile = saveDialog.FileName;
                bool exportResult = ARIMAModel.Export(dataFile);
                if (!exportResult)
                {
                    MessageBox.Show("There is an exception. Config doesn't be stored", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                return;
            }
        }

        #endregion SARIMA model

        #region Neural network

        private void btnNetworkNew_Click(object sender, EventArgs e)
        {
            string numInputs = this.txtNumInput.Text;
            string numHiddens = this.txtNumHidden.Text;
            string numOutputs = this.txtNumOutput.Text;

            if (numInputs == "" || numHiddens == "" || numOutputs == "")
            {
                System.Windows.Forms.MessageBox.Show("Please insert the number of Input Nodes, Hidden Nodes, Output Nodes", null, System.Windows.Forms.MessageBoxButtons.OK);
                return;
            }
            try
            {          
                NeuralModel.SettingNeuralNetwork(Int32.Parse(numInputs), Int32.Parse(numHiddens), Int32.Parse(numOutputs));
                System.Windows.Forms.MessageBox.Show("NetWork configuration successfull, You can train it");

                this.txtNumOutput.Enabled = false;
                this.txtNumHidden.Enabled = false;
                this.txtNumInput.Enabled = false;
                this.btnNetworkNew.Enabled = false;
                this.btnNetworkLoad.Enabled = false;
                this.btnNetworkSave.Enabled = true;
                this.btnNetworkClear.Enabled = true;

                this.btnTrainNeural.Enabled = true;
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
        }

        private void btnNetworkLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Load Network Config File";
            fileDialog.DefaultExt = "xml";
            DialogResult result = fileDialog.ShowDialog();
            string dataFile = "";
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                dataFile = fileDialog.FileName;
                Neural temp = Neural.Import(dataFile);
                if (temp == null)
                {
                    System.Windows.Forms.MessageBox.Show("The Input file is not correct format !!!", null, System.Windows.Forms.MessageBoxButtons.OK);
                }
                else
                {
                    NeuralModel.SettingNeuralNetwork(temp.m_iNumInputNodes, temp.m_iNumOutputNodes, temp.m_iNumOutputNodes);

                    this.txtNumInput.Text = NeuralModel.m_iNumInputNodes.ToString();
                    this.txtNumHidden.Text = NeuralModel.m_iNumHiddenNodes.ToString();
                    this.btnNetworkNew.Enabled = false;
                    this.btnNetworkLoad.Enabled = false;
                    this.btnNetworkSave.Enabled = true;
                    this.btnNetworkClear.Enabled = true;

                    this.btnTrainNeural.Enabled = true;
                    this.btnForecastNeural.Enabled = true;
                    this.btnTestNeural.Enabled = true;

                    this.btnForecast.Enabled = true;
                    this.btnTest.Enabled = true;
                }
            }
            else
            {
                return;
            }
        }

        private void btnNetworkSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save Network Config File";
            saveDialog.DefaultExt = "xml";
            DialogResult result = saveDialog.ShowDialog();
            string dataFile = "";
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                dataFile = saveDialog.FileName;
                bool exportResult = Neural.Export(NeuralModel, dataFile);
            }
            else
            {
                return;
            }
        }

        private void btnNetworkClear_Click(object sender, EventArgs e)
        {
            NeuralModel.SettingNeuralNetwork(0, 0, 0);

            this.txtNumInput.Text = "";
            this.txtNumHidden.Text = "";
            this.txtNumInput.Enabled = true;
            this.txtNumHidden.Enabled = true;

            this.btnNetworkNew.Enabled = true;
            this.btnNetworkLoad.Enabled = true;
            this.btnNetworkSave.Enabled = false;
            this.btnNetworkClear.Enabled = false;

            this.btnTrainNeural.Enabled = false;
            this.btnTestNeural.Enabled = false;
            this.btnForecastNeural.Enabled = false;
        }

        private void radioBackPropagation_Click(object sender, EventArgs e)
        {
            groupBoxAlgorithmConfig.Enabled = true;
            groupBoxAlgorithmConfig.Text = "Back Propagation Config";
            labelConfig1.Text = "Learning Rate";
            labelConfig2.Text = "Momemtum";

            txtConfig1.Text = 0.4.ToString();
            txtConfigEpoches.Text = 1000.ToString();
            txtConfig2.Text = 0.2.ToString();
            txtConfigErrors.Text = 0.001.ToString();
        }

        private void radioRPROP_Click(object sender, EventArgs e)
        {
            groupBoxAlgorithmConfig.Enabled = true;
            groupBoxAlgorithmConfig.Text = "Resilient Propagation Config";
            labelConfig1.Text = "Default Update Value";
            labelConfig2.Text = "Max Update Value";

            txtConfig1.Text = 0.1.ToString();
            txtConfigEpoches.Text = 1000.ToString();
            txtConfig2.Text = 1.ToString();
            txtConfigErrors.Text = 0.001.ToString();
        }

        private void btnTrainNeural_Click(object sender, EventArgs e)
        {
            if (radioBackPropagation.Checked)
            {
                double epoch, learningRate, momemtum, residual;
                try
                {
                    epoch = Double.Parse(txtConfigEpoches.Text);
                    learningRate = Double.Parse(txtConfig1.Text);
                    momemtum = Double.Parse(txtConfig2.Text);
                    residual = Double.Parse(txtConfigErrors.Text);

                    NeuralModel.Bp_Run(learningRate, momemtum, epoch, residual);
                    this.btnForecastNeural.Enabled = true;
                    this.btnTestNeural.Enabled = true;

                    this.btnForecast.Enabled = true;
                    this.btnTest.Enabled = true;
                }
                catch
                {
                }

            }
            else if (radioRPROP.Checked)
            {
                double defaultValue, maxValue, epoch, residual;
                try
                {
                    epoch = Double.Parse(txtConfigEpoches.Text);
                    defaultValue = Double.Parse(txtConfig1.Text);
                    maxValue = Double.Parse(txtConfig2.Text);
                    residual = Double.Parse(txtConfigErrors.Text);

                    NeuralModel.Rprop_Run(defaultValue, maxValue, epoch, residual);
                    this.btnForecastNeural.Enabled = true;
                    this.btnTestNeural.Enabled = true;

                    this.btnForecast.Enabled = true;
                    this.btnTest.Enabled = true;
                }
                catch
                {
                }
            }
        }

        private void btnPlotNeural_Click(object sender, EventArgs e)
        {
            NeuralModel.DrawSeriesData();
        }

        private void btnTestNeural_Click(object sender, EventArgs e)
        {
            List<double> testDataARIMASeries = new List<double>();
            List<double> testResultARIMASeries = new List<double>();
            List<double> testDataNeuralSeries = new List<double>();
            List<double> testResultNeuralSeries = new List<double>();
            if (modelType == ModelType.SARIMA_ANN)
            {
                int numDataForInput = Math.Min(ARIMAModel.GetNumDataForInput() + NeuralModel.GetNumDataForInput(), _dataForTest.Count);
                for (int i = numDataForInput; i > 0; i--)
                {
                    testDataARIMASeries.Add(_dataForTest[_dataForTest.Count - i]);
                }
                for (int i = 0; i < _testDataSeries.Count; i++)
                {
                    testDataARIMASeries.Add(_testDataSeries[i]);
                }
                ARIMAModel.ComputeTestingResult(testDataARIMASeries, out testResultARIMASeries);

                int numDataNeural = _testDataSeries.Count + NeuralModel.GetNumDataForInput();
                int begin = 0;
                if (numDataNeural < testResultARIMASeries.Count)
                {
                    begin = testResultARIMASeries.Count - numDataNeural;
                }

                for (int i = begin; i < _testDataSeries.Count + numDataForInput; i++)
                {
                    testDataNeuralSeries.Add(testDataARIMASeries[i] - testResultARIMASeries[i]);
                }

                NeuralModel.ComputeTestingResult(testDataNeuralSeries, out testResultNeuralSeries);
                int end = 0;
                if (testResultNeuralSeries.Count > _testDataSeries.Count)
                {
                    end = testResultNeuralSeries.Count - _testDataSeries.Count;
                }

                for (int i = 0; i < end; i++)
                {
                    testDataNeuralSeries.RemoveAt(0);
                    testResultNeuralSeries.RemoveAt(0);
                }
                Statistic.DrawTwoSeriesTestData(testDataNeuralSeries, 0, testResultNeuralSeries, 0);
            }
            else
            {
                int numDataForInput = Math.Min(NeuralModel.GetNumDataForInput(), _dataForTest.Count);
                for (int i = numDataForInput; i > 0; i--)
                {
                    testDataNeuralSeries.Add(_dataForTest[_dataForTest.Count - i]);
                }
                for (int i = 0; i < _testDataSeries.Count; i++)
                {
                    testDataNeuralSeries.Add(_testDataSeries[i]);
                }

                NeuralModel.ComputeTestingResult(testDataNeuralSeries, out testResultNeuralSeries);
                for (int i = 0; i < numDataForInput; i++)
                {
                    testDataNeuralSeries.RemoveAt(0);
                    testResultNeuralSeries.RemoveAt(0);
                }
                Statistic.DrawTwoSeriesTestData(testDataNeuralSeries, 0, testResultNeuralSeries, 0);
            }


        }

        private void btnForecastNeural_Click(object sender, EventArgs e)
        {
            List<double> forecastResultNeuralSeries = new List<double>();
            List<double> forecastDataNeuralSeries = new List<double>();
            List<double> testResultARIMASeries = new List<double>();
            List<double> testErrorARIMASeries = new List<double>();
            int aHead = 0;
            AHead_Form aHeadDialog = new AHead_Form();

            if (aHeadDialog.ShowDialog() == DialogResult.OK)
            {
                aHead = aHeadDialog.GetAHead();
            }
            aHeadDialog.Dispose();

            if (aHead <= 0)
            {
                MessageBox.Show(this, "Please enter input in correct format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (modelType == ModelType.SARIMA_ANN)
            {
                ARIMAModel.ComputeTestingResult(_dataForForecast, out testResultARIMASeries);
                for (int i = 0; i < _dataForForecast.Count; i++)
                {
                    testErrorARIMASeries.Add(_dataForForecast[i] - testResultARIMASeries[i]);
                }

                for (int i = 0; i < NeuralModel.GetNumDataForInput(); i++)
                {
                    forecastDataNeuralSeries.Add(testErrorARIMASeries[testErrorARIMASeries.Count - NeuralModel.GetNumDataForInput() + i]);
                }

                NeuralModel.Forecast(forecastDataNeuralSeries, aHead, out forecastResultNeuralSeries);
                Statistic.DrawForecastSeriesData(testErrorARIMASeries, 0, forecastResultNeuralSeries, 0);
            }
            else
            {
                NeuralModel.Forecast(_dataForForecast, aHead, out forecastResultNeuralSeries);
                Statistic.DrawForecastSeriesData(_dataForForecast, 0, forecastResultNeuralSeries, 0);                
            }
        }

        #endregion Neural network

        private void btnForecast_Click(object sender, EventArgs e)
        {
            List<double> forecastResultARIMASeries = new List<double>();
            List<double> testResultARIMASeries = new List<double>();
            List<double> testErrorARIMASeries = new List<double>();
            List<double> forecastResultNeuralSeries = new List<double>();
            List<double> forecastDataNeuralSeries = new List<double>();

            List<double> forecastResultSeries = new List<double>();
            int aHead = 0;
            AHead_Form aHeadDialog = new AHead_Form();

            if (aHeadDialog.ShowDialog() == DialogResult.OK)
            {
                aHead = aHeadDialog.GetAHead();
            }
            aHeadDialog.Dispose();

            if (aHead <= 0)
            {
                MessageBox.Show(this, "Please enter input in correct format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ARIMAModel.Forecast(_dataForForecast, aHead, out forecastResultARIMASeries);

            ARIMAModel.ComputeTestingResult(_dataForForecast, out testResultARIMASeries);
            for (int i = 0; i < _dataForForecast.Count; i++)
            {
                testErrorARIMASeries.Add(_dataForForecast[i] - testResultARIMASeries[i]);
            }

            for (int i = 0; i < NeuralModel.GetNumDataForInput(); i++)
            {
                forecastDataNeuralSeries.Add(testErrorARIMASeries[testErrorARIMASeries.Count - NeuralModel.GetNumDataForInput() + i]);
            }

            NeuralModel.Forecast(forecastDataNeuralSeries, aHead, out forecastResultNeuralSeries);
            for (int i = 0; i < forecastResultARIMASeries.Count; i++)
            {
                forecastResultSeries.Add(forecastResultARIMASeries[i] + forecastResultNeuralSeries[i]);
            }


            //Draw forecast result
            chartForecast.Series.Clear();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Color = System.Drawing.Color.Blue;
            series1.IsVisibleInLegend = false;

            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Color = System.Drawing.Color.Red;
            series2.IsVisibleInLegend = false;

            for (int i = 0; i < _dataForForecast.Count; i++)
            {
                series1.Points.AddXY(i + 1, _dataForForecast[i]);
            }
            chartForecast.Series.Add(series1);

            series2.Points.AddXY(_dataForForecast.Count, _dataForForecast[_dataForForecast.Count - 1]);
            for (int i = 0; i < forecastResultSeries.Count; i++)
            {
                series2.Points.AddXY(_dataForForecast.Count + i + 1, forecastResultSeries[i]);
            }
            chartForecast.Series.Add(series2);

            StringBuilder result = new StringBuilder();
            result.Append(String.Format("Forecast data for {0} ahead time\n", aHead));
            for (int i = 0; i < forecastResultSeries.Count; i++)
            {
                result.Append(String.Format("  {0}\t{1}\n", i+1, String.Format("{0:0.###}", forecastResultSeries[i])));
            }

            this.richTextForecast.Text = result.ToString();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            List<double> testDataARIMASeries = new List<double>();
            List<double> testResultARIMASeries = new List<double>();

            List<double> testDataNeuralSeries = new List<double>();
            List<double> testResultNeuralSeries = new List<double>();

            int numDataForInput = Math.Min(ARIMAModel.GetNumDataForInput() + NeuralModel.GetNumDataForInput(), _dataForTest.Count);

            for (int i = numDataForInput; i > 0; i--)
            {
                testDataARIMASeries.Add(_dataForTest[_dataForTest.Count - i]);
            }
            for (int i = 0; i < _testDataSeries.Count; i++)
            {
                testDataARIMASeries.Add(_testDataSeries[i]);
            }

            ARIMAModel.ComputeTestingResult(testDataARIMASeries, out testResultARIMASeries);

            int numDataNeural = _testDataSeries.Count + NeuralModel.GetNumDataForInput();
            int begin = 0;
            if (numDataNeural < testResultARIMASeries.Count)
            {
                begin = testResultARIMASeries.Count - numDataNeural;
            }

            for (int i = begin; i < _testDataSeries.Count + numDataForInput; i++)
            {
                testDataNeuralSeries.Add(testDataARIMASeries[i] - testResultARIMASeries[i]);
            }

            NeuralModel.ComputeTestingResult(testDataNeuralSeries, out testResultNeuralSeries);

            int end = 0;
            if (testDataARIMASeries.Count > _testDataSeries.Count)
            {
                end = testDataARIMASeries.Count - _testDataSeries.Count;
            }

            for (int i = 0; i < end; i++)
            {
                testDataARIMASeries.RemoveAt(0);
                testResultARIMASeries.RemoveAt(0);
            }

            if (testDataNeuralSeries.Count > _testDataSeries.Count)
            {
                end = testDataNeuralSeries.Count - _testDataSeries.Count;
            }
            for (int i = 0; i < end; i++)
            {
                testDataNeuralSeries.RemoveAt(0);
                testResultNeuralSeries.RemoveAt(0);
            }
            for (int i = 0; i < testDataARIMASeries.Count; i++)
            {
                testResultARIMASeries[i] += testResultNeuralSeries[i];
            }
            Statistic.DrawTwoSeriesTestData(testDataARIMASeries, 0, testResultARIMASeries, 0);
        }

        private void comboBoxModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxModel.SelectedIndex == 0)
            {
                if (tabControlModel.TabPages.Contains(tabNeuralNetwork))
                    tabControlModel.TabPages.Remove(tabNeuralNetwork);

                if (!tabControlModel.TabPages.Contains(tabSARIMAModel))
                    tabControlModel.TabPages.Insert(1, tabSARIMAModel);

                if (tabControlModel.TabPages.Contains(tabForecast))
                    tabControlModel.TabPages.Remove(tabForecast);

                modelType = ModelType.SARIMA;
            }
            else if (comboBoxModel.SelectedIndex == 1)
            {
                if (tabControlModel.TabPages.Contains(tabSARIMAModel))
                    tabControlModel.TabPages.Remove(tabSARIMAModel);

                if (!tabControlModel.TabPages.Contains(tabNeuralNetwork))
                    tabControlModel.TabPages.Insert(1, tabNeuralNetwork);

                if (tabControlModel.TabPages.Contains(tabForecast))
                    tabControlModel.TabPages.Remove(tabForecast);

                modelType = ModelType.ANN;
            }
            else
            {
                if (!tabControlModel.TabPages.Contains(tabSARIMAModel))
                    tabControlModel.TabPages.Insert(1, tabSARIMAModel);

                if (!tabControlModel.TabPages.Contains(tabNeuralNetwork))
                    tabControlModel.TabPages.Insert(2, tabNeuralNetwork);

                if (!tabControlModel.TabPages.Contains(tabForecast))
                    tabControlModel.TabPages.Insert(3, tabForecast);

                modelType = ModelType.SARIMA_ANN;
            }
            isReadTestData = false;
            isReadTrainData = false;
        }

        private void tabControlModel_SelectedIndexChanged(object sender, EventArgs e)
        {           
            if (tabControlModel.SelectedTab == tabSARIMAModel)
            {
                if (!isReadTrainData && ReadTrainData())
                {
                    ARIMAModel.SetData(_trainDataSeries);
                    SettingGUIBeforeARIMAModel();
                    isReadTrainData = true;
                }
                else if(!isReadTrainData)
                {
                    tabControlModel.SelectedTab = tabChooseData;
                    return;
                }

                if (!isReadTestData && ReadTestData())
                {
                    isReadTestData = true;
                }
                else if (!isReadTestData)
                {
                    tabControlModel.SelectedTab = tabChooseData;
                    return;
                }
            }
            
            if (tabControlModel.SelectedTab == tabNeuralNetwork && modelType == ModelType.ANN)
            {
                if (!isReadTrainData && ReadTrainData())
                {
                    NeuralModel.SetData(_trainDataSeries);
                    SettingGUIBeforeNeuralNetwork();
                    isReadTrainData = true;
                }
                else if (!isReadTrainData)
                {
                    tabControlModel.SelectedTab = tabChooseData;
                    return;
                }

                if (!isReadTestData && ReadTestData())
                {
                    isReadTestData = true;
                }
                else if (!isReadTestData)
                {
                    tabControlModel.SelectedTab = tabChooseData;
                    return;
                }
            }

            if (tabControlModel.SelectedTab == tabForecast)
            {
                if (!isReadTrainData && ReadTrainData())
                {
                    ARIMAModel.SetData(_trainDataSeries);
                    SettingGUIBeforeARIMAModel();
                    isReadTrainData = true;
                }
                else if (!isReadTrainData)
                {
                    tabControlModel.SelectedTab = tabChooseData;
                    return;
                }

                if (!isReadTestData && ReadTestData())
                {
                    isReadTestData = true;
                }
                else if (!isReadTestData)
                {
                    tabControlModel.SelectedTab = tabChooseData;
                    return;
                }
            }

        }

        private void btnResetDataARIMA_Click(object sender, EventArgs e)
        {
            ARIMAModel.SetData(_trainDataSeries);
            SettingGUIBeforeARIMAModel();
        }

        private void txtTrainDataFromRow_TextChanged(object sender, EventArgs e)
        {
            isReadTrainData = false;
        }

        private void txtTrainDataToRow_TextChanged(object sender, EventArgs e)
        {
            isReadTrainData = false;
        }

        private void txtTrainDataColumn_TextChanged(object sender, EventArgs e)
        {
            isReadTrainData = false;
        }

        private void txtTestDataFromRow_TextChanged(object sender, EventArgs e)
        {
            isReadTestData = false;
        }

        private void txtTestDataToRow_TextChanged(object sender, EventArgs e)
        {
            isReadTestData = false;
        }

        private void txtTestDataColumn_TextChanged(object sender, EventArgs e)
        {
            isReadTestData = false;
        }

    }
}
