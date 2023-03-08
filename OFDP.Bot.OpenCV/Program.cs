// See https://aka.ms/new-console-template for more information

using OFDP.Bot.OpenCV;
using OFDP.Bot.OpenCV.WinApi;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Rect = OpenCvSharp.Rect;

Console.WriteLine("Running ...");

using var wins = new Windows();

using var t = new ResourcesTracker();

Mat ReadMaterial(string fileName)
{
    var mat = t.T(new Mat(fileName));
    return t.T(mat.CvtColor(ColorConversionCodes.BGR2GRAY));
}

string currentDir = Environment.CurrentDirectory;
Console.WriteLine(currentDir);
// var capture = t.T(new VideoCapture(Path.Join(currentDir, "res", "train.mp4")));
var redZone = ReadMaterial(Path.Join(currentDir, "res", "red.png"));
var blueZone = ReadMaterial(Path.Join(currentDir, "res", "blue.png"));
var windowSrc = t.T(new Window("src"));
var windowDst = t.T(new Window("dst"));

const string windowName = "One Finger Death Punch";
var window = new WindowManager(windowName);
var rect = window.Rectangle;
var screen = new Screenshooter(rect);
while (true)
{
    using var tt = new ResourcesTracker();
    using var screenshot = screen.MakeScreenshot();
    var frame = tt.T(screenshot.ToMat());
    if (frame.Empty())
        break;

    windowSrc.Image = frame;
    
    var grayFrame = tt.T(frame.CvtColor(ColorConversionCodes.BGR2GRAY));

    void DetectTemplateOnFrame(Mat f, Mat grayTemplate, Scalar color, double threshold, out Point? location)
    {
        location = null;
        const double maxThreshold = 1;
        
        var result = tt.T(new Mat(
            f.Rows - grayTemplate.Rows + 1,
            f.Cols - grayTemplate.Cols + 1,
            MatType.CV_32FC1));

        Cv2.MatchTemplate(f, grayTemplate, result, TemplateMatchModes.CCoeffNormed);
        Cv2.Threshold(result, result, threshold, maxThreshold, ThresholdTypes.Tozero);
        
        while (true)
        {
            Cv2.MinMaxLoc(result, out var minVal, out double maxVal, out var minLoc, out Point maxLoc);

            if (maxLoc.Y < 450 &&  maxVal >= threshold)
            {
                //Setup the rectangle to draw
                location = new Point(maxLoc.X, maxLoc.Y);
                var r = new Rect(location.Value, new Size(grayTemplate.Width, grayTemplate.Height));
                Console.WriteLine(
                    $"MinVal={minVal.ToString()} MaxVal={maxVal.ToString()} MinLoc={minLoc.ToString()} MaxLoc={maxLoc.ToString()} Rect={r.ToString()}");

                //Draw a rectangle of the matching area
                Cv2.Rectangle(frame, r, color, 3);
                
                Cv2.PutText(frame, $"{maxVal * 100 : 0}%", r.TopLeft, HersheyFonts.Italic, 0.5, Scalar.Green);

                //Fill in the res Mat so you don't find the same area again in the MinMaxLoc
                Cv2.FloodFill(result, maxLoc, new Scalar(0), out _, new Scalar(0.1), new Scalar(1.0));
            }
            else
                break;
        }
    }

    const double threshold = .87;
    DetectTemplateOnFrame(grayFrame, redZone, Scalar.LimeGreen, threshold, out var pointR);
    DetectTemplateOnFrame(grayFrame, blueZone, Scalar.Blue, threshold, out var pointL);
    if (pointR is not null)
    {
        Console.WriteLine("KICK RIGHT");
        window.TrySendClick(false);
    }
    else if (pointL is not null)
    {
        Console.WriteLine("KICK LEFT");
        window.TrySendClick(true);
    }

    windowSrc.Image = frame;
    Cv2.WaitKey(1);
}