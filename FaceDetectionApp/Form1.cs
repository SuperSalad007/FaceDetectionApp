using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace FaceDetectionApp
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private CascadeClassifier _faceCascade;
        private CascadeClassifier _eyeCascade;

        public Form1()
        {
            InitializeComponent();

            // Загружаем каскады Хаара
            _faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");
            _eyeCascade = new CascadeClassifier("haarcascade_eye.xml");

            // Простая проверка наличия файлов (без IsEmpty)
            if (!System.IO.File.Exists("haarcascade_frontalface_default.xml") ||
                !System.IO.File.Exists("haarcascade_eye.xml"))
            {
                MessageBox.Show("Внимание: XML-файлы каскадов не найдены в папке с программой (bin/Debug).");
            }
        }

        // Обработчик кнопки "Старт"
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_capture == null)
            {
                _capture = new VideoCapture(0); // 0 - встроенная веб-камера
                timer1.Start(); // Включаем таймер
            }
        }

        // Обработчик кнопки "Стоп"
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_capture != null)
            {
                timer1.Stop();
                _capture.Dispose();
                _capture = null;
                pictureBox1.Image = null; // Очищаем экран
            }
        }

        // Таймер: срабатывает каждые 33 мс
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_capture == null) return;

            using (Mat frame = new Mat())
            {
                _capture.Read(frame); // Читаем кадр с камеры

                if (!frame.IsEmpty)
                {
                    using (Mat grayFrame = new Mat())
                    {
                        // 1. Делаем кадр серым для алгоритма
                        CvInvoke.CvtColor(frame, grayFrame, ColorConversion.Bgr2Gray);

                        // 2. Ищем лица
                        Rectangle[] faces = _faceCascade.DetectMultiScale(grayFrame, 1.1, 5, Size.Empty);

                        foreach (Rectangle face in faces)
                        {
                            // Рисуем зеленую рамку вокруг лица
                            CvInvoke.Rectangle(frame, face, new MCvScalar(0, 255, 0), 2);

                            // 3. Ищем глаза внутри лица
                            using (Mat faceRegion = new Mat(grayFrame, face))
                            {
                                Rectangle[] eyes = _eyeCascade.DetectMultiScale(faceRegion, 1.1, 3, Size.Empty);

                                foreach (Rectangle eye in eyes)
                                {
                                    // Пересчитываем координаты глаз
                                    Rectangle eyeRect = new Rectangle(face.X + eye.X, face.Y + eye.Y, eye.Width, eye.Height);
                                    // Рисуем синюю рамку вокруг глаз
                                    CvInvoke.Rectangle(frame, eyeRect, new MCvScalar(255, 0, 0), 2);
                                }
                            }
                        }
                    }
                    // 4. Выводим картинку на форму
                    pictureBox1.Image = frame.ToBitmap();
                }
            }
        }
    }
}