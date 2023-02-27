using OpenCvSharp;

namespace OFDP.Bot.OpenCV;

public class Windows : IDisposable
{
    private List<Window> wins = new();

    public void Show(Mat mat)
    {
        Window win = new Window("Window " + wins.Count);
        win.Image = mat;
        wins.Add(win);
    }

    public void Dispose()
    {
        wins.ForEach(x=>x.Dispose());
    }
}