using System;
using System.IO;
using System.Net.Http;
using OpenCvSharp;

class Program
{
    private const string FaceCascadeFile = "haarcascade_frontalface_default.xml";
    private const string FaceCascadeUrl = "https://raw.githubusercontent.com/opencv/opencv/master/data/haarcascades/haarcascade_frontalface_default.xml";

    private const string EyeCascadeFile = "haarcascade_eye.xml";
    private const string EyeCascadeUrl = "https://raw.githubusercontent.com/opencv/opencv/master/data/haarcascades/haarcascade_eye.xml";

    static void Main(string[] args)
    {
        Console.WriteLine("OpenCvSharp C# örneği: Kamera, kenar, yüz ve göz algılama");
        Console.WriteLine("Çıkmak için pencerede 'q' ya da ESC; kaydetmek için 's'.");

        EnsureFile(FaceCascadeFile, FaceCascadeUrl);
        EnsureFile(EyeCascadeFile, EyeCascadeUrl);

        using var faceCascade = new CascadeClassifier(FaceCascadeFile);
        using var eyeCascade = new CascadeClassifier(EyeCascadeFile);
        if (faceCascade.Empty()) Console.WriteLine("Uyarı: Yüz cascade yüklenemedi, yüz tespiti devre dışı.");
        if (eyeCascade.Empty()) Console.WriteLine("Uyarı: Göz cascade yüklenemedi, göz tespiti devre dışı.");

        // Önce kamerayı dene
        using var capture = new VideoCapture(0);
        if (!capture.IsOpened())
        {
            Console.WriteLine("Kamera açılamadı. 'input.jpg' varsa onu işleyeceğim.");
            if (!File.Exists("input.jpg"))
            {
                Console.WriteLine("input.jpg bulunamadı. Kamerayı bağlayın ya da bir görsel ekleyin.");
                return;
            }

            using var img = Cv2.ImRead("input.jpg", ImreadModes.Color);
            if (img.Empty())
            {
                Console.WriteLine("Görsel okunamadı.");
                return;
            }

            using var gray = new Mat();
            using var edges = new Mat();
            Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(gray, gray);
            Cv2.GaussianBlur(gray, gray, new Size(3, 3), 0);
            Cv2.Canny(gray, edges, 70, 140);

            if (!faceCascade.Empty())
            {
                var faces = faceCascade.DetectMultiScale(gray, 1.1, 4, HaarDetectionTypes.ScaleImage, new Size(30, 30));
                foreach (var face in faces)
                {
                    Cv2.Rectangle(img, face, Scalar.Red, 2);
                    if (!eyeCascade.Empty())
                    {
                        using var faceRoiGray = new Mat(gray, face);
                        var eyes = eyeCascade.DetectMultiScale(faceRoiGray, 1.1, 3, HaarDetectionTypes.ScaleImage, new Size(15, 15));
                        foreach (var eye in eyes)
                        {
                            var eyeRect = new Rect(face.X + eye.X, face.Y + eye.Y, eye.Width, eye.Height);
                            Cv2.Rectangle(img, eyeRect, Scalar.Blue, 2);
                        }
                    }
                }
                Cv2.PutText(img, $"Yuz sayisi: {faces.Length}", new Point(10, 25), HersheyFonts.HersheySimplex, 0.7, Scalar.Yellow, 2);
            }

            Cv2.ImShow("Giriş", img);
            Cv2.ImShow("Kenarlar", edges);
            Cv2.WaitKey();
            return;
        }

        using var frame = new Mat();
        using var window = new Window("Kamera");
        using var windowEdges = new Window("Kenarlar");
        bool applyBlur = false;
        Console.WriteLine("Kameradan görüntü alınıyor. Çıkmak için 'q' ya da ESC, kaydetmek için 's', bulanıklaştırma için 'b'.");

        while (true)
        {
            capture.Read(frame);
            if (frame.Empty())
            {
                Console.WriteLine("Kare alınamadı, çıkılıyor.");
                break;
            }

            using var gray = new Mat();
            using var edges = new Mat();
            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(gray, gray);
            Cv2.GaussianBlur(gray, gray, new Size(3, 3), 0);
            Cv2.Canny(gray, edges, 70, 140);

            if (!faceCascade.Empty())
            {
                var faces = faceCascade.DetectMultiScale(gray, 1.1, 4, HaarDetectionTypes.ScaleImage, new Size(30, 30));
                foreach (var face in faces)
                {
                    Cv2.Rectangle(frame, face, Scalar.Red, 2);
                    if (!eyeCascade.Empty())
                    {
                        using var faceRoiGray = new Mat(gray, face);
                        var eyes = eyeCascade.DetectMultiScale(faceRoiGray, 1.1, 3, HaarDetectionTypes.ScaleImage, new Size(15, 15));
                        foreach (var eye in eyes)
                        {
                            var eyeRect = new Rect(face.X + eye.X, face.Y + eye.Y, eye.Width, eye.Height);
                            Cv2.Rectangle(frame, eyeRect, Scalar.Blue, 2);
                        }
                    }
                }
                Cv2.PutText(frame, $"Yuz sayisi: {faces.Length}", new Point(10, 25), HersheyFonts.HersheySimplex, 0.7, Scalar.Yellow, 2);
            }

            if (applyBlur)
            {
                Cv2.GaussianBlur(frame, frame, new Size(7, 7), 0);
            }

            window.ShowImage(frame);
            windowEdges.ShowImage(edges);

            var key = Cv2.WaitKey(1);
            if (key == 'q' || key == 27) // ESC
                break;
            if (key == 's' || key == 'S')
            {
                var fileName = $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                Cv2.ImWrite(fileName, frame);
                Console.WriteLine($"Kaydedildi: {fileName}");
            }
            if (key == 'b' || key == 'B')
            {
                applyBlur = !applyBlur;
                Console.WriteLine(applyBlur ? "Bulanıklaştırma: Açık" : "Bulanıklaştırma: Kapalı");
            }
        }

        capture.Release();
        Cv2.DestroyAllWindows();
    }

    static void EnsureFile(string file, string url)
    {
        try
        {
            if (File.Exists(file))
            {
                Console.WriteLine($"Cascade bulundu: {file}");
                return;
            }

            Console.WriteLine($"Cascade indiriliyor: {file}...");
            using var client = new HttpClient();
            var bytes = client.GetByteArrayAsync(url).Result;
            File.WriteAllBytes(file, bytes);
            Console.WriteLine("Cascade indirildi ve kaydedildi.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cascade indirilemedi ({file}): {ex.Message}");
        }
    }
}
