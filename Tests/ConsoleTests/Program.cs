
using System.Windows.Input;

var type = typeof(TestClass);
var ii = type.GetInterface("System.Windows.Input.ICommand");


Console.WriteLine("End.");
Console.ReadLine();

class TestClass : ICommand
{
    public bool CanExecute(object? parameter) => throw new NotImplementedException();

    public void Execute(object? parameter) => throw new NotImplementedException();

    public event EventHandler? CanExecuteChanged;
}