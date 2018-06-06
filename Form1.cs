using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ByteMatrix = com.google.zxing.common.ByteMatrix;

namespace QRCode
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 生成条形码
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            lbshow.Text = "";
            Regex rg = new Regex("^[0-9]{13}$");
            if (!rg.IsMatch(txtMsg.Text))
            {
                lbshow.Text = "请输入13位数字！";
                return;
            }

            try
            {
                //本例子采用EAN_13编码
                com.google.zxing.MultiFormatWriter mutiWriter = new com.google.zxing.MultiFormatWriter();
                ByteMatrix bm = mutiWriter.encode(txtMsg.Text, com.google.zxing.BarcodeFormat.EAN_13, 363, 150);
                Bitmap img = bm.ToBitmap();
                pictureBox1.Image = img;

                //自动保存图片到当前目录
                string filename = GetSaveImagePath("EAN13");
                img.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                lbshow.Text = "图片已保存到：" + filename;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            lbshow.Text = "";
            if (string.IsNullOrEmpty(txtMsg.Text))
            {
                lbshow.Text = "请输入内容！";
                return;
            }

            try
            {
                //构造二维码写码器
                com.google.zxing.MultiFormatWriter mutiWriter = new com.google.zxing.MultiFormatWriter();
                ByteMatrix bm = mutiWriter.encode(txtMsg.Text, com.google.zxing.BarcodeFormat.QR_CODE, 300, 300);
                Bitmap img = bm.ToBitmap();

                pictureBox1.Image = img;

                //自动保存图片到当前目录
                string filename = GetSaveImagePath("QR");
                img.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                lbshow.Text = "图片已保存到：" + filename;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// 选择图片
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "图片文件|*.bmp;*.jpg;*.png;*.ico";
            if (openFileDialog1.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                pictureBox2.ImageLocation = openFileDialog1.FileName;
            }
        }

        /// <summary>
        /// 解码操作
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            lbshow.Text = "";
            if (pictureBox2.Image == null)
            {
                lbshow.Text = "未导入图片！";
                return;
            }

            try
            {
                //构建解码器
                com.google.zxing.MultiFormatReader mutiReader = new com.google.zxing.MultiFormatReader();
                Bitmap img = (Bitmap)pictureBox2.Image; //(Bitmap)Bitmap.FromFile(opFilePath);
                com.google.zxing.LuminanceSource ls = new RGBLuminanceSource(img, img.Width, img.Height);
                com.google.zxing.BinaryBitmap bb = new com.google.zxing.BinaryBitmap(new com.google.zxing.common.HybridBinarizer(ls));

                //注意  必须是Utf-8编码
                Hashtable hints = new Hashtable();
                hints.Add(com.google.zxing.EncodeHintType.CHARACTER_SET, "UTF-8");
                com.google.zxing.Result r = mutiReader.decode(bb, hints);
                txtmsg2.Text = r.Text;
                lbshow.Text = "解码成功！";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 选择二维码中间图片
        /// </summary>
        private void QRMiddleImg_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "图片文件|*.bmp;*.jpg;*.png;*.ico";
            if (openFileDialog1.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                Image img = Image.FromFile(openFileDialog1.FileName);
                QRMiddleImg.Image = img;
            }
        }

        /// <summary>
        /// 生成中间带图片的二维码
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            lbshow.Text = "";
            if (string.IsNullOrEmpty(txtMsg.Text))
            {
                lbshow.Text = "请输入内容！";
                return;
            }

            try
            {
                //构造二维码写码器
                com.google.zxing.MultiFormatWriter mutiWriter = new com.google.zxing.MultiFormatWriter();
                Hashtable hint = new Hashtable();
                hint.Add(com.google.zxing.EncodeHintType.CHARACTER_SET, "UTF-8");
                hint.Add(com.google.zxing.EncodeHintType.ERROR_CORRECTION,
                    com.google.zxing.qrcode.decoder.ErrorCorrectionLevel.H);
                //生成二维码
                ByteMatrix bm = mutiWriter.encode(txtMsg.Text, com.google.zxing.BarcodeFormat.QR_CODE, 300, 300, hint);
                Bitmap img = bm.ToBitmap();

                //要插入到二维码中的图片
                Image middlImg = QRMiddleImg.Image;
                //获取二维码实际尺寸（去掉二维码两边空白后的实际尺寸）
                System.Drawing.Size realSize = mutiWriter.GetEncodeSize(txtMsg.Text,
                    com.google.zxing.BarcodeFormat.QR_CODE, 300, 300);
                //计算插入图片的大小和位置
                int middleImgW = Math.Min((int)(realSize.Width / 3.5), middlImg.Width);
                int middleImgH = Math.Min((int)(realSize.Height / 3.5), middlImg.Height);
                int middleImgL = (img.Width - middleImgW) / 2;
                int middleImgT = (img.Height - middleImgH) / 2;

                //将img转换成bmp格式，否则后面无法创建 Graphics对象
                Bitmap bmpimg = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmpimg))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.DrawImage(img, 0, 0);
                }

                //在二维码中插入图片
                System.Drawing.Graphics MyGraphic = System.Drawing.Graphics.FromImage(bmpimg);
                //白底
                MyGraphic.FillRectangle(Brushes.White, middleImgL, middleImgT, middleImgW, middleImgH);
                MyGraphic.DrawImage(middlImg, middleImgL, middleImgT, middleImgW, middleImgH);

                pictureBox1.Image = bmpimg;

                //自动保存图片到当前目录
                string filename = GetSaveImagePath("QR");
                bmpimg.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                lbshow.Text = "图片已保存到：" + filename;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 获取保存路径
        /// </summary>
        private string GetSaveImagePath(string name)
        {
            string path = Environment.CurrentDirectory + "\\Image";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return $"{path}\\{name}_{DateTime.Now.Ticks}.jpg";
        }
    }
}
