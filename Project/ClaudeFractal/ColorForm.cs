using System;
using System.Windows.Forms;

namespace ClaudeFractal
{
    public partial class ColorForm : Form
    {
        private Form1 _mainForm;
        
        public ColorForm(Form1 mainForm)
        {
            InitializeComponent();
            _mainForm = mainForm;
        }

        public void ColorForm_Load()
        {
            textBoxRed.Text = _mainForm.redPart.ToString();
            textBoxGreen.Text = _mainForm.greenPart.ToString();
            textBoxBlue.Text = _mainForm.bluePart.ToString();
            textBoxBright.Text = _mainForm.brightness.ToString();
        }
        private void buttonApply_Click(object sender, EventArgs e)
        {
            _mainForm.redPart = Convert.ToInt32(textBoxRed.Text);
            _mainForm.greenPart = Convert.ToInt32(textBoxGreen.Text);
            _mainForm.bluePart = Convert.ToInt32(textBoxBlue.Text);
            _mainForm.brightness = Convert.ToInt32(textBoxBright.Text);
            
            _mainForm.RenderMandelbrot(_mainForm.ClientSize.Width,_mainForm.ClientSize.Height);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            _mainForm.redPart = 255;
            _mainForm.greenPart = 0;
            _mainForm.bluePart = 255;
            _mainForm.brightness = 20;
            
            ColorForm_Load();
            
            _mainForm.RenderMandelbrot(_mainForm.ClientSize.Width,_mainForm.ClientSize.Height);
        }
    }
}