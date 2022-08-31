using MathCore.Generated.MVVM;
using MathCore.Generated.MVVM.Commands;

namespace ConsoleTests;

public partial class ConsoleViewModel
{
    //[NotifyProperty]
    private string? _Title;

    //[Command(CommandName = "TestCommand123"/*, CommandType = typeof(CorrectCommand)*/)]
    private void OnTestCommandExecuted() { }

    [Command]
    private void OnTest2CommandExecuted() { }

    public void Test()
    {
        LambdaCommand cmd;
        //Title = "123";
        //TestCommand123.Execute(null);
    }
}

class Error1Command
{

}

class Error2Command : ICommand
{

}

interface ICommand
{

}

class CorrectCommand : System.Windows.Input.ICommand
{
    public bool CanExecute(object? parameter) => throw new NotImplementedException();

    public void Execute(object? parameter) => throw new NotImplementedException();

    public event EventHandler? CanExecuteChanged;
}