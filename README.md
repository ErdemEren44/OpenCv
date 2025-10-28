# C# OpenCV (OpenCvSharp) 

Bu proje, C# ve OpenCvSharp kullanarak kamera görüntüsü alma, kenar algılama (Canny), yüz tespiti (Haar Cascade) ve göz tespiti örneği içerir. Kamera yoksa proje kökünde `input.jpg` varsa onu işler ve pencerelerde gösterir.

## Gereksinimler
- .NET SDK (7/8/9)
- Windows (OpenCvSharp4.runtime.win yerel OpenCV DLL’lerini getirir)
- Visual Studio 2022/2025 veya VS Code 

## Çalıştırma
### Komut satırı
```
dotnet build
dotnet run
```
- Çıkmak için `q` veya `ESC`.
- Görüntüyü kaydetmek için `s` tuşuna basın (kamera modunda `snapshot_YYYYMMDD_HHMMSS.png` kaydeder).
- Bulanıklaştırmayı aç/kapatmak için `b` tuşu.
- Kamera yoksa `input.jpg` ekleyip çalıştırabilirsiniz.

### Visual Studio ile
1. Visual Studio’yu açın.
2. `File > Open > Project/Solution` ile `OpenCv.csproj` açın.
3. `OpenCv` projesini başlangıç olarak ayarlayın.
4. `F5` (Debug) veya `Ctrl+F5` ile çalıştırın.

## Yüz ve Göz Tespiti (CascadeClassifier)
- Proje, `haarcascade_frontalface_default.xml` ve `haarcascade_eye.xml` dosyalarını otomatik indirir. İnternet yoksa dosyaları proje köküne manuel ekleyin.
- Yüz tespiti parametreleri: `scaleFactor=1.1`, `minNeighbors=4`, `minSize=30x30`.
- Gözler, yüz ROI içinde aranır (`minSize=15x15`) ve mavi dikdörtgen ile işaretlenir; yüzler kırmızı dikdörtgen ile gösterilir.
- Gri görüntü üzerinde `EqualizeHist` uygulanır; ayrıca kamera önizlemesinde `b` ile isteğe bağlı 7x7 Gaussian blur uygulanabilir.

## Proje Yapısı
- `Program.cs`: Kamera, kenar, yüz ve göz tespiti akışı; `s` ile görüntü kaydı; `b` ile blur.
- `OpenCv.csproj`: Paketler ve hedef çerçeve.

## Notlar
- Paketler: `OpenCvSharp4`, `OpenCvSharp4.runtime.win`.
- Pencere açılmıyorsa sistem grafik sürücü kısıtlamaları olabilir; alternatif olarak `Cv2.ImWrite("output.png", edges);` ile dosya çıktısı alabilirsiniz.
