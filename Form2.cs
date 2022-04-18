using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        public int getTimer()
        {
            int number = (int)numericUpDown1.Value;
            return number;
        }
        public void setTimer(int time) { numericUpDown1.Value = time; }
        public int getXaxis()
        {
            int number = (int)numericUpDown2.Value;
            return number;
        }
        public void setXaxis(int xAxis) { numericUpDown2.Value = xAxis; }
        public int getYaxis()
        {
            int number = (int)numericUpDown3.Value;
            return number;
        }
        public void setYaxis(int yAxis) { numericUpDown3.Value = yAxis; }
    }
}
