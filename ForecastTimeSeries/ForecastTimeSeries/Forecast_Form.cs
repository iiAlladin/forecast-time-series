using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace ForecastTimeSeries {
    public partial class Forecast_Form : Form {
        public Forecast_Form() {
            this.InitializeComponent();
            base.CenterToScreen();
        }
        public void setDataResult(string data) {
            this.richTextForecast.Text = data;
        }

    }
}
