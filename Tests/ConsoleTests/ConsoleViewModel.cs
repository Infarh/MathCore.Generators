
using MathCore.Generated.MVVM;

namespace ConsoleTests;

public partial class ConsoleViewModel
{
    [NotifyProperty]
    private string? _Title;

    //[Command(CanExecuteMethodName = nameof(CanTestCommandExecute))]
    private void OnTestCommandExecuted(object p) { }

    private bool CanTestCommandExecute(object? p) => p is not null;

    //[Command]
    //private void OnTest2CommandExecuted() { }

    public void Test()
    {
        //LambdaCommand cmd;
        //Title = "123";
        //TestCommand.Execute(null);
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
    //public CorrectCommand(Action<object> OnExecute) { }

    public CorrectCommand(Action<object> OnExecute, Func<object?,bool>? CanExecute = null)
    {

    }

    public bool CanExecute(object? parameter) => throw new NotImplementedException();

    public void Execute(object? parameter) => throw new NotImplementedException();

    public event EventHandler? CanExecuteChanged;
}