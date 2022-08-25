using MathCore.Generated.MVVM;

namespace ClassLibTests;

public partial class LibViewModel
{
    [NotifyProperty]
    private string? _Title;

    [Command]
    private void OnCommand() { }

    public void Test()
    {
        Title = "123";
    }
}