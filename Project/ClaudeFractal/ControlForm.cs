using System;
using System.Windows.Forms;

namespace ClaudeFractal
{
    public partial class ControlForm : Form
    {
        private Form1 _mainForm;

        public ControlForm(Form1 mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
        }

        public void ControlForm_Load()
        {
            // Set initial values for the controls
            textBoxXMin.Text = _mainForm.XMin.ToString();
            textBoxXMax.Text = _mainForm.XMax.ToString();
            textBoxYMin.Text = _mainForm.YMin.ToString();
            textBoxYMax.Text = _mainForm.YMax.ToString();
            textBoxMaxIterations.Text = _mainForm.MaxIterations.ToString();
        }

        private void buttonApply_Click_1(object sender, EventArgs e)
        {
            // Update the properties of the main form
            _mainForm.XMin = Decimal.Parse(textBoxXMin.Text);
            _mainForm.XMax = Decimal.Parse(textBoxXMax.Text);
            _mainForm.YMin = Decimal.Parse(textBoxYMin.Text);
            _mainForm.YMax = Decimal.Parse(textBoxYMax.Text);
            _mainForm.MaxIterations = int.Parse(textBoxMaxIterations.Text);

            // Refresh the main form to redraw the fractal
            _mainForm.RenderMandelbrot(_mainForm.ClientSize.Width,_mainForm.ClientSize.Height);
        }

        private void buttonIn_Click(object sender, EventArgs e)
        {
            _mainForm.Zoom(1);
            ControlForm_Load();
        }

        private void buttonOut_Click(object sender, EventArgs e)
        {
            _mainForm.Zoom(0);
            ControlForm_Load();
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            _mainForm.Movement(1);
            ControlForm_Load();
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            _mainForm.Movement(0);
            ControlForm_Load();
        }

        private void buttonLeft_Click(object sender, EventArgs e)
        {
            _mainForm.Movement(3);
            ControlForm_Load();
        }

        private void buttonRight_Click(object sender, EventArgs e)
        {
            _mainForm.Movement(2);
            ControlForm_Load();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            _mainForm.XMin = -2;
            _mainForm.XMax = 2;
            _mainForm.YMin = -2;
            _mainForm.YMax = 2;
            _mainForm.MaxIterations = 100;

            ControlForm_Load();
            
            _mainForm.RenderMandelbrot(_mainForm.ClientSize.Width,_mainForm.ClientSize.Height);
        }
    }
}