using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
namespace ForecastTimeSeries {
    partial class Forecast_Form {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.richTextForecast = new System.Windows.Forms.RichTextBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextForecast
            // 
            this.richTextForecast.Location = new System.Drawing.Point(666, 13);
            this.richTextForecast.Name = "richTextForecast";
            this.richTextForecast.Size = new System.Drawing.Size(122, 403);
            this.richTextForecast.TabIndex = 0;
            this.richTextForecast.Text = "";
            // 
            // chart1
            // 
            chartArea.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea);
            legend.Name = "Legend1";
            this.chart1.Legends.Add(legend);
            this.chart1.Location = new System.Drawing.Point(13, 13);
            this.chart1.Name = "chart1";
            series.ChartArea = "ChartArea1";
            series.Legend = "Legend1";
            series.Name = "Data";
            this.chart1.Series.Add(series);
            this.chart1.Size = new System.Drawing.Size(637, 403);
            this.chart1.TabIndex = 1;
            this.chart1.Text = "chart";

            Title title = new Title();
            this.chart1.Titles.Add(title);

            // 
            // Forecast_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.richTextForecast);
            this.Name = "Forecast_Form";
            this.Text = "Forecast_Form";
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextForecast;
        public System.Windows.Forms.DataVisualization.Charting.Chart chart1;
    }
}