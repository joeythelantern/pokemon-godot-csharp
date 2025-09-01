using Game.Core;
using Godot;
using Godot.Collections;
using System.Threading.Tasks;

namespace Game.UI;

public partial class MessageManager : CanvasLayer
{
    public static MessageManager Instance { get; private set; }

    [ExportCategory("Components")]
    [Export]
    public NinePatchRect Box;

    [Export]
    public RichTextLabel Label;

    [ExportCategory("Variables")]
    [Export]
    public bool IsScrolling = false;

    [Export]
    public int Delay = 15;

    [Export]
    public Array<string> Messages;

    public override void _Ready()
    {
        Instance = this;
    }

    public static void PlayText(params string[] payload)
    {
        if (IsReading()) return;
        if (payload.Length == 0) return;

        Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, true);

        Instance.Messages = [.. payload];
        ScrollText();
    }

    public static async void ScrollText()
    {
        if (!IsReading())
            Instance.Box.Visible = true;

        if (Instance.Messages.Count == 0)
        {
            Instance.Box.Visible = false;
            Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, false);
            return;
        }

        Instance.IsScrolling = true;
        Instance.Label.Text = "";

        foreach (char letter in Instance.Messages[0])
        {
            Instance.Label.Text += letter;
            await Task.Delay(Instance.Delay);
        }

        Instance.Messages.RemoveAt(0);
        Instance.IsScrolling = false;
    }

    public static bool IsReading()
    {
        return Instance.Box.Visible;
    }

    public static bool Scrolling()
    {
        return Instance.IsScrolling;
    }

    public static Array<string> GetMessages()
    {
        return Instance.Messages;
    }
}
