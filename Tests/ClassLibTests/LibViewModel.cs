using MathCore.Generated.MVVM;

namespace ClassLibTests;

public partial class LibViewModel
{
    [NotifyProperty]
    private string? _Title;

    //[Command(CommandName = "TestCommand123")]
    private void OnTestCommandExecuted() { }

    public void Test()
    {
        Title = "123";
        //TestCommand123.Execute(null);
    }
}

class Error1Command
{

}

//class Error2Command : ICommand
//{

//}

//interface ICommand
//{

//}

//class CorrectCommand : System.Windows.Input.ICommand
//{
//    public bool CanExecute(object? parameter) => throw new NotImplementedException();

//    public void Execute(object? parameter) => throw new NotImplementedException();

//    public event EventHandler? CanExecuteChanged;
//}