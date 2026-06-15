using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApplication8
{
    public partial class Form1 : Form
    {
        private readonly string connectionString = "server=server;user=user;pwd=pwd;database=imagenes;";

        private int ventana = 20;
        private int promR, promG, promB;
        private int cr, cg, cb; 
        private int xyR, xyG, xyB;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT descripcion FROM textura", con))
            {
                con.Open();
                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        comboBox1.Items.Add(lector["descripcion"].ToString());
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
                pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null) return;
            using (Bitmap bmp = new Bitmap(pictureBox1.Image))
            {
                promR = 0; promG = 0; promB = 0;
                int x = e.X;
                int y = e.Y;
                int mitadVentana = ventana / 2;

                int startX = Math.Max(0, x - mitadVentana);
                int endX = Math.Min(bmp.Width, x + mitadVentana);
                int startY = Math.Max(0, y - mitadVentana);
                int endY = Math.Min(bmp.Height, y + mitadVentana);

                int conteoPixeles = 0;
                for (int i = startX; i < endX; i++)
                {
                    for (int j = startY; j < endY; j++)
                    {
                        Color cp = bmp.GetPixel(i, j);
                        promR += cp.R;
                        promG += cp.G;
                        promB += cp.B;
                        conteoPixeles++;
                    }
                }
                if (conteoPixeles > 0)
                {
                    promR /= conteoPixeles;
                    promG /= conteoPixeles;
                    promB /= conteoPixeles;

                    double sumSqR = 0, sumSqG = 0, sumSqB = 0;

                    for (int i = startX; i < endX; i++)
                    {
                        for (int j = startY; j < endY; j++)
                        {
                            Color cp = bmp.GetPixel(i, j);
                            sumSqR += Math.Pow(cp.R - promR, 2);
                            sumSqG += Math.Pow(cp.G - promG, 2);
                            sumSqB += Math.Pow(cp.B - promB, 2);
                        }
                    }

                    cr = (int)Math.Sqrt(sumSqR / conteoPixeles);
                    cg = (int)Math.Sqrt(sumSqG / conteoPixeles);
                    cb = (int)Math.Sqrt(sumSqB / conteoPixeles);
                }

                Color pixelCentral = bmp.GetPixel(x, y);
                xyR = pixelCentral.R;
                xyG = pixelCentral.G;
                xyB = pixelCentral.B;

                textBox1.Text = "R: " + promR.ToString() + " | Tex: " + cr.ToString();
                textBox2.Text = "G: " + promG.ToString() + " | Tex: " + cg.ToString();
                textBox3.Text = "B: " + promB.ToString() + " | Tex: " + cb.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) return;
            ProcesarYMostrarImagen(promR, promG, promB, cr, cg, cb);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox4.Text)) return;

            comboBox1.Items.Add(textBox4.Text);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string sql = "INSERT INTO textura (r, g, b, descripcion, cr, cg, cb) VALUES (@r, @g, @b, @desc, @cr, @cg, @cb)";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@r", promR);
                    cmd.Parameters.AddWithValue("@g", promG);
                    cmd.Parameters.AddWithValue("@b", promB);
                    cmd.Parameters.AddWithValue("@desc", textBox4.Text);
                    cmd.Parameters.AddWithValue("@cr", cr);
                    cmd.Parameters.AddWithValue("@cg", cg);
                    cmd.Parameters.AddWithValue("@cb", cb);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null || pictureBox1.Image == null) return;

            int R = 0, G = 0, B = 0;
            int CR = 0, CG = 0, CB = 0;
            string descripcion = comboBox1.SelectedItem.ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string sql = "SELECT r, g, b, cr, cg, cb FROM textura WHERE descripcion = @desc";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@desc", descripcion);
                    con.Open();
                    using (SqlDataReader lector = cmd.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            R = Convert.ToInt32(lector["r"]);
                            G = Convert.ToInt32(lector["g"]);
                            B = Convert.ToInt32(lector["b"]);

                            if (lector["cr"] != DBNull.Value) CR = Convert.ToInt32(lector["cr"]);
                            if (lector["cg"] != DBNull.Value) CG = Convert.ToInt32(lector["cg"]);
                            if (lector["cb"] != DBNull.Value) CB = Convert.ToInt32(lector["cb"]);
                        }
                    }
                }
            }

            ProcesarYMostrarImagen(R, G, B, CR, CG, CB);
        }

        private void ProcesarYMostrarImagen(int targetR, int targetG, int targetB, int targetCR, int targetCG, int targetCB)
        {
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Bitmap bmp1 = new Bitmap(bmp.Width, bmp.Height);

            int pR, pG, pB;
            int pCR, pCG, pCB;
            int area = ventana * ventana;

            for (int i = 0; i <= bmp.Width - ventana; i += ventana)
            {
                for (int j = 0; j <= bmp.Height - ventana; j += ventana)
                {
                    pR = 0; pG = 0; pB = 0;

                    for (int k = i; k < i + ventana; k++)
                    {
                        for (int l = j; l < j + ventana; l++)
                        {
                            Color cp = bmp.GetPixel(k, l);
                            pR += cp.R;
                            pG += cp.G;
                            pB += cp.B;
                        }
                    }
                    pR /= area;
                    pG /= area;
                    pB /= area;

                    double sumSqR = 0, sumSqG = 0, sumSqB = 0;
                    for (int k = i; k < i + ventana; k++)
                    {
                        for (int l = j; l < j + ventana; l++)
                        {
                            Color cp = bmp.GetPixel(k, l);
                            sumSqR += Math.Pow(cp.R - pR, 2);
                            sumSqG += Math.Pow(cp.G - pG, 2);
                            sumSqB += Math.Pow(cp.B - pB, 2);
                        }
                    }
                    pCR = (int)Math.Sqrt(sumSqR / area);
                    pCG = (int)Math.Sqrt(sumSqG / area);
                    pCB = (int)Math.Sqrt(sumSqB / area);

                    int tolColor = 50;   // Tolerancia equilibrada para color
                    int tolTextura = 15; // Tolerancia equilibrada para rugosidad

                    bool coincideColor = Math.Abs(targetR - pR) < tolColor &&
                                         Math.Abs(targetG - pG) < tolColor &&
                                         Math.Abs(targetB - pB) < tolColor;

                    bool coincideTextura = Math.Abs(targetCR - pCR) < tolTextura &&
                                           Math.Abs(targetCG - pCG) < tolTextura &&
                                           Math.Abs(targetCB - pCB) < tolTextura;

                    for (int k = i; k < i + ventana; k++)
                    {
                        for (int l = j; l < j + ventana; l++)
                        {
                            if (coincideColor && coincideTextura)
                            {
                                bmp1.SetPixel(k, l, Color.Black);
                            }
                            else
                            {
                                bmp1.SetPixel(k, l, bmp.GetPixel(k, l));
                            }
                        }
                    }
                }
            }

            if (pictureBox2.Image != null) pictureBox2.Image.Dispose();
            pictureBox2.Image = bmp1;
            bmp.Dispose();
        }
    }
}