using System;
using System.Windows.Input;

namespace Shipwreck.HlsDownloader
{
    public class Command : ICommand
    {
        public Command(Action executed)
        {
            _Executed = executed;
        }

        private readonly Action _Executed;

        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }

        bool ICommand.CanExecute(object parameter)
            => true;

        public void Execute(object parameter)
            => _Executed();
    }
}