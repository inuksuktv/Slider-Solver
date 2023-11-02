using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public static CommandManager Instance => myInstance;
    private static CommandManager myInstance;

    private Stack<ICommand> commands = new();

    private void Awake()
    {
        myInstance = this;
    }

    public void AddCommand(ICommand command)
    {
        command.Execute();
        commands.Push(command);
    }

    public void Undo()
    {
        if (commands.Count == 0) {
            return;
        }

        ICommand command = commands.Pop();
        command.Undo();
    }
}
