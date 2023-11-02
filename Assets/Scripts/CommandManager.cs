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

    public static CommandManager myInstance { get; private set; }

    private Stack<ICommand> myCommandsBuffer = new Stack<ICommand>();

    private void Awake()
    {
        myInstance = this;
    }

    public void AddMove(ICommand command)
    {
        command.Execute();
        myCommandsBuffer.Push(command);
    }

    public void Undo()
    {
        if (myCommandsBuffer.Count == 0) {
            return;
        }

        var command = myCommandsBuffer.Pop();
        command.Undo();
    }
}
